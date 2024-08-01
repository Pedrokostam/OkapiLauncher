using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Channels;
using System.Timers;
using System.Windows.Threading;
using AuroraVisionLauncher.Contracts.Services;
using AuroraVisionLauncher.Core.Models.Apps;
using AuroraVisionLauncher.Models;
using AuroraVisionLauncher.Models.Messages;
using CommunityToolkit.Mvvm.Messaging;

namespace AuroraVisionLauncher.Services
{
    public class ProcessManagerService : IProcessManagerService, IRecipient<AppProcessChangedMessage>
    {
        /// <summary>
        /// Each records holds a list of all apps that share the process name.
        /// Since the process name is the key, it is quite fast to look it up
        /// ExecutablePath - simpleprocesses
        /// </summary>
        private readonly Dictionary<string, HashSet<SimpleProcess>> _dictionary = new(StringComparer.OrdinalIgnoreCase);
        private readonly IMessenger _messenger;
        private readonly IAvAppFacadeFactory _avAppFacadeFactory;
        private readonly System.Timers.Timer _timer;
        private DateTime _lastUpdate;
        /// <summary>
        /// How much time at minimum there must to the next scheduled updated to force it.
        /// </summary>
        private static readonly TimeSpan _gracePeriod = TimeSpan.FromMilliseconds(300);
        private static readonly TimeSpan _timerPeriod = TimeSpan.FromMilliseconds(2000);
        /// <summary>
        /// Above this threshold updated should not be forced.
        /// </summary>
        private static readonly TimeSpan _recheckThreshold = _timerPeriod - _gracePeriod;
        public FreshAppProcesses GetCurrentState => new FreshAppProcesses(_dictionary);

        public ProcessManagerService(IMessenger messenger, IAvAppFacadeFactory avAppFacadeFactory)
        {
            _messenger = messenger;
            _avAppFacadeFactory = avAppFacadeFactory;
            _timer = new(_timerPeriod.TotalMilliseconds);
            _timer.Elapsed += _timer_Elapsed;
            _timer.AutoReset = true;
            Update(_avAppFacadeFactory.AvApps);
            _timer.Start();
            _messenger.RegisterAll(this);

        }

        private readonly object _lock = new();

        private void _timer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            Update(_avAppFacadeFactory.AvApps);
        }



        private void Update(IReadOnlyCollection<IAvApp> apps)
        {
            //var full = Stopwatch.StartNew();
            if (Monitor.TryEnter(_lock))
            {
                try
                {
                    Update_Impl(apps);
                }
                finally
                {
                    Monitor.Exit(_lock);
                }
            }
            //Trace.WriteLine($"FULL_{full.Elapsed.TotalMilliseconds}");
        }

        private void Update_Impl(IReadOnlyCollection<IAvApp> apps)
        {
            var rawProcesses = Process.GetProcesses();
            var stateDict = GetNewStateDict(rawProcesses, apps);
            var missings = _dictionary.Keys.ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (var kvp in stateDict)
            {
                UpdateOneExe(kvp);
                missings.Remove(kvp.Key);
            }

            foreach (var missing in missings)
            {
                if (_dictionary.TryGetValue(missing, out var value))
                {
                    value.Clear();
                }
            }
            foreach (var proc in rawProcesses)
            {
                proc.Dispose();
            }
            _messenger.Send<FreshAppProcesses>(GetCurrentState);
            _lastUpdate = DateTime.UtcNow;
        }

        private void UpdateSingle(IAvApp app)
        {
            if (Monitor.TryEnter(_lock))
            {
                try
                {
                    UpdateSingle_Impl(app);
                }
                finally
                {
                    Monitor.Exit(_lock);
                }
            }
        }

        private void UpdateSingle_Impl(IAvApp app)
        {
            var rawProcesses = Process.GetProcessesByName(app.ProcessName);
            List<SimpleProcess> simples = new List<SimpleProcess>(rawProcesses.Length);

            foreach (var proc in rawProcesses)
            {
                simples.Add(new SimpleProcess(proc, _messenger, app.Path));
                proc.Dispose();
            }
            UpdateOneExe(app.Path, simples);
            _messenger.Send<FreshAppProcesses>(GetCurrentState);
        }

        private Dictionary<string, List<SimpleProcess>> GetNewStateDict(Process[] rawProcesses, IReadOnlyCollection<IAvApp> apps)
        {
            var full = Stopwatch.StartNew();
            int count = apps.Count;
            var stateDict = new Dictionary<string, List<SimpleProcess>>(count, StringComparer.OrdinalIgnoreCase);
            var processNames = new HashSet<string>(count, StringComparer.OrdinalIgnoreCase);
            foreach (var app in apps)
            {
                processNames.Add(app.ProcessName);
                stateDict[app.Path] = [];
            }
            foreach (var process in rawProcesses)
            {
                if (!(processNames.Contains(process.ProcessName) && process.MainModule?.FileName is string filepath))
                {
                    continue;
                }
                var sp = new SimpleProcess(process, _messenger, filepath);
                stateDict[filepath].Add(sp);
            }
            Debug.WriteLine($"DICTING: {full.Elapsed.TotalMilliseconds}");
            return stateDict;
        }

        private void UpdateOneExe(KeyValuePair<string, List<SimpleProcess>> exePathGroup)
        {
            UpdateOneExe(exePathGroup.Key, exePathGroup.Value);
        }
        private void UpdateOneExe(string filepath, IEnumerable<SimpleProcess> newProcesses)
        {
            if (!_dictionary.TryGetValue(filepath, out var set))
            {
                set = [];
                _dictionary[filepath] = set;
            }
            var freshSimples = newProcesses.ToHashSet();
            foreach (var newProc in freshSimples)
            {
                if (set.TryGetValue(newProc, out var oldProc))
                {
                    oldProc.UpdateFrom(newProc);
                }
                else
                {
                    set.Add(newProc);
                }
            }
            set.IntersectWith(freshSimples);
        }

        public void Receive(AppProcessChangedMessage message)
        {
            var diff = DateTime.UtcNow - _lastUpdate;
            if (diff > _recheckThreshold)
            {
                // It's gonna be updated son enough, no need to force it
                return;
            }
            if (_avAppFacadeFactory.TryGetAppByPath(message.AppPath, out var app))
            {
                UpdateSingle(app);
            }
        }
    }
}
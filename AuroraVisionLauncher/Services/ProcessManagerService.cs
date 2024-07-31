using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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

        public FreshAppProcesses GetCurrentState => new FreshAppProcesses(_dictionary);

        public ProcessManagerService(IMessenger messenger, IAvAppFacadeFactory avAppFacadeFactory)
        {
            _messenger = messenger;
            _avAppFacadeFactory = avAppFacadeFactory;
            _timer = new(2000);
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


        private void Update(IEnumerable<IAvApp> apps)
        {
            var st = Stopwatch.StartNew();
            if (Monitor.TryEnter(_lock))
            {
                try
                {
                    var changedApps = new List<string>();
                    var processNames = apps.Select(x => x.ProcessName).ToHashSet(StringComparer.OrdinalIgnoreCase);
                    // Cleared all previous records, we dont want old data
                    // it may be better to instantiate the dict here, worth checking
                    //_dictionary.Clear();
                    //PopulateDictionary(apps);
                    var rawProcesses = Process.GetProcesses();
                    var groupedByExePath = rawProcesses
                        .Where(x => processNames.Contains(x.ProcessName) && x.MainModule?.FileName is not null)
                        .GroupBy(x => x.MainModule!.FileName!, x => new SimpleProcess(x, _messenger), StringComparer.OrdinalIgnoreCase);

                    var missings = _dictionary.Keys.ToHashSet(StringComparer.OrdinalIgnoreCase);

                    foreach (var exePathGroup in groupedByExePath)
                    {
                        UpdateOneExe(exePathGroup);
                        missings.Remove(exePathGroup.Key);
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
                    _messenger.Send<FreshAppProcesses>(new FreshAppProcesses(_dictionary));
                }
                finally
                {
                    Monitor.Exit(_lock);
                }
            }
            Debug.WriteLine(st.Elapsed.TotalMilliseconds);
        }

        private void UpdateOneExe(IGrouping<string, SimpleProcess> exePathGroup)
        {
            if (!_dictionary.TryGetValue(exePathGroup.Key, out var set))
            {
                set = [];
                _dictionary[exePathGroup.Key] = set;
            }
            var freshSimples = exePathGroup.ToHashSet();
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
            int prevCount = set.Count;
            set.IntersectWith(freshSimples);
        }

        public void Receive(AppProcessChangedMessage message)
        {
            _timer.Stop();
            Update(_avAppFacadeFactory.AvApps);
            _timer.Start();
        }
    }
}
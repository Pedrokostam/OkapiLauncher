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
            var full = Stopwatch.StartNew();
            var st = new Stopwatch();
            if (Monitor.TryEnter(_lock))
            {
                try
                {
                    st.Restart();
                    var changedApps = new List<string>();
                    var processNames = apps.Select(x => x.ProcessName).ToHashSet(StringComparer.OrdinalIgnoreCase);
                    st.Stop();
                    Debug.WriteLine($"Names: {st.Elapsed.TotalMilliseconds}");
                    // Cleared all previous records, we dont want old data
                    // it may be better to instantiate the dict here, worth checking
                    //_dictionary.Clear();
                    //PopulateDictionary(apps);
                    st.Restart();
                    var rawProcesses = Process.GetProcesses();
                    st.Stop();
                    Debug.WriteLine($"Processes: {st.Elapsed.TotalMilliseconds}");
                    //List<IGrouping<string, SimpleProcess>> groupedByExePath = NewMethod(processNames, rawProcesses);
                    var groupedByExePath = NewMethod2(processNames, rawProcesses, apps);
                    var missings = _dictionary.Keys.ToHashSet(StringComparer.OrdinalIgnoreCase);

                    foreach (var exePathGroup in groupedByExePath)
                    {
                        UpdateOneExe(exePathGroup);
                        missings.Remove(exePathGroup.Key);
                    }

                    st.Restart();
                    foreach (var missing in missings)
                    {
                        if (_dictionary.TryGetValue(missing, out var value))
                        {
                            value.Clear();
                        }
                    }
                    st.Stop();
                    Debug.WriteLine($"Clearing: {st.Elapsed.TotalMilliseconds}");
                    st.Restart();
                    foreach (var proc in rawProcesses)
                    {
                        proc.Dispose();
                    }
                    st.Stop();
                    Debug.WriteLine($"Disposing: {st.Elapsed.TotalMilliseconds}");
                    st.Restart();
                    _messenger.Send<FreshAppProcesses>(new FreshAppProcesses(_dictionary));
                    st.Stop();
                    Debug.WriteLine($"Messaging: {st.Elapsed.TotalMilliseconds}");
                }
                finally
                {
                    Monitor.Exit(_lock);
                }
            }
            Debug.WriteLine(st.Elapsed.TotalMilliseconds);
            Trace.WriteLine($"FULL_{full.Elapsed.TotalMilliseconds}");
        }

        private List<IGrouping<string, SimpleProcess>> NewMethod(HashSet<string> processNames, Process[] rawProcesses)
        {
            var full = Stopwatch.StartNew();
            var q = rawProcesses
                                    .Where(x => processNames.Contains(x.ProcessName) && x.MainModule?.FileName is not null)
                                    .GroupBy(x => x.MainModule!.FileName!, x => new SimpleProcess(x, _messenger), StringComparer.OrdinalIgnoreCase).ToList();
            Debug.WriteLine($"Grouping: {full.Elapsed.TotalMilliseconds}");
            return q;
        }

        private Dictionary<string, List<SimpleProcess>> NewMethod2(HashSet<string> processNames, Process[] rawProcesses, IEnumerable<IAvApp> apps)
        {
            var full = Stopwatch.StartNew();
            var d = new Dictionary<string, List<SimpleProcess>>(StringComparer.OrdinalIgnoreCase);
            foreach (var app in apps)
            {
                d[app.Path] = [];
            }
            foreach (var process in rawProcesses)
            {
                if (!(processNames.Contains(process.ProcessName) && process.MainModule?.FileName is string filepath))
                {
                    continue;
                }
                var sp = new SimpleProcess(process, _messenger,filepath);
                d[filepath].Add(sp);
            }
            Debug.WriteLine($"DICTING: {full.Elapsed.TotalMilliseconds}");
            return d;
        }

        private void UpdateOneExe(KeyValuePair<string, List<SimpleProcess>> exePathGroup)
        {
            if (!_dictionary.TryGetValue(exePathGroup.Key, out var set))
            {
                set = [];
                _dictionary[exePathGroup.Key] = set;
            }
            var freshSimples = exePathGroup.Value.ToHashSet();
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
            _timer.Stop();
            Update(_avAppFacadeFactory.AvApps);
            _timer.Start();
        }
    }
}
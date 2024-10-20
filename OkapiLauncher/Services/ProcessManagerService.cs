using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Channels;
using System.Timers;
using System.Windows;
using System.Windows.Threading;
using OkapiLauncher.Contracts.Services;
using OkapiLauncher.Core.Models.Apps;
using OkapiLauncher.Models;
using OkapiLauncher.Models.Messages;
using CommunityToolkit.Mvvm.Messaging;

namespace OkapiLauncher.Services
{
    public class ProcessManagerService : IProcessManagerService, IRecipient<OpenAppRequest>, IRecipient<KillProcessRequest>, IRecipient<KillAllProcessesRequest>
    {
        /// <summary>
        /// Each records holds a list of all apps that share the process name.
        /// Since the process name is the key, it is quite fast to look it up
        /// ExecutablePath - simpleprocesses
        /// </summary>
        private readonly Dictionary<string, HashSet<SimpleProcess>> _dictionary = new(StringComparer.OrdinalIgnoreCase);
        private readonly IMessenger _messenger;
        private readonly IAvAppFacadeFactory _avAppFacadeFactory;
        private readonly IContentDialogService _contentDialogService;
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

        public ProcessManagerService(IMessenger messenger, IAvAppFacadeFactory avAppFacadeFactory,IContentDialogService contentDialogService)
        {
            _messenger = messenger;
            _avAppFacadeFactory = avAppFacadeFactory;
            _contentDialogService = contentDialogService;
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
            var diff = DateTime.UtcNow - _lastUpdate;
            if (diff > _recheckThreshold)
            {
                // It's gonna be updated son enough, no need to force it
                return;
            }
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
                if (!string.Equals(proc.MainModule?.FileName, app.Path,StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
                try
                {
                    var simple = new SimpleProcess(proc, _messenger, app.Path);
                    simples.Add(simple);

                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                }
                finally
                {
                    proc.Dispose();
                }
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
                if (!(processNames.Contains(process.ProcessName)
                    && process.MainModule?.FileName is string filepath
                    && stateDict.ContainsKey(filepath)))
                {
                    continue;
                }
                try
                {
                    var sp = new SimpleProcess(process, _messenger, filepath);
                    stateDict[filepath].Add(sp);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                }
            }
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

        public void Receive(KillProcessRequest message) => Kill(message.Process,message.ViewModel);
        private async void Kill(SimpleProcess process, object context)
        {
            var res = await _contentDialogService.ShowProcessKillDialog(context,process);
            if (!res)
            {
                return;
            }
            try
            {
                using var proc = Process.GetProcessById(process.Id);
                proc.Kill();
                proc.WaitForExit();
                if (_avAppFacadeFactory.TryGetAppByPath(process.Path, out var app))
                {
                    UpdateSingle(app);
                }
            }
            catch (ArgumentException)
            { }
            catch (InvalidOperationException)
            { }
        }

        public void Receive(OpenAppRequest message) => Open(message.App, message.Arguments);
        private void Open(IAvApp App, IEnumerable<string> arguments)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = App.Path,
                UseShellExecute = true,  // Use the shell to start the process
                CreateNoWindow = true, // Do not create a window
            };
            foreach (var arg in arguments)
            {
                startInfo.ArgumentList.Add(arg);
            }
            try
            {
                using var p = Process.Start(startInfo);
            }
            catch (System.ComponentModel.Win32Exception)
            {
            }
            UpdateSingle(App);
        }
        public void Receive(KillAllProcessesRequest message) => KillAll(message.AvApp,message.ViewModel);
        private async void KillAll(AvAppFacade avApp,object viewModel)
        {
            var res = await _contentDialogService.ShowAllProcessesKillDialog(viewModel, avApp);
            if (!res)
            {
                return;
            }
            //var res = MessageBox.Show($"Are you sure you want to kill all processes of {avApp.ProcessName} ({avApp.ActiveProcesses.Count} processes)?",
            //                        "Confirm process ending",
            //                        MessageBoxButton.YesNo,
            //                        MessageBoxImage.Warning);
            //if (res == MessageBoxResult.No)
            //{
            //    return;
            //}
            List<Process> procs = [];
            foreach (var active in avApp.ActiveProcesses)
            {
                var proc = Process.GetProcessById(active.Id);
                procs.Add(proc);
                proc.Kill();
            }
            foreach (var proc in procs)
            {
                proc.WaitForExit();
                proc.Dispose();
            }
            UpdateSingle(avApp);
        }
    }
}
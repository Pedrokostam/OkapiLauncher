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
using System.Collections.Immutable;
using OkapiLauncher.Services.Processes;
using System.Collections;
using CommunityToolkit.Mvvm.ComponentModel;

namespace OkapiLauncher.Services
{
    public partial class ProcessManagerService : ObservableObject, IProcessManagerService, IRecipient<OpenAppRequest>, IRecipient<KillProcessRequest>, IRecipient<KillAllProcessesRequest>
    {
        /// <summary>
        /// Each records holds a list of all apps that share the process name.
        /// Since the process name is the key, it is quite fast to look it up
        /// ExecutablePath - simpleprocesses
        /// </summary>
        private readonly IMessenger _messenger;
        private readonly IAvAppFacadeFactory _avAppFacadeFactory;
        private readonly IContentDialogService _contentDialogService;
        private readonly System.Timers.Timer _timer;
        /// <summary>
        /// How much time at minimum there must to the next scheduled updated to force it.
        /// </summary>
        /// <summary>
        /// Above this threshold updated should not be forced.
        /// </summary>
        private IProcessQuerer? Querer
        {
            get => _querer;
            set
            {
                _timer?.Stop();
                _querer = value;
                QuererType = value?.GetType();
                if (value is not null)
                {
                    if (_timer is not null)
                    {
                        _timer.Interval = value.TimerPeriod.TotalMilliseconds;
                        //Update(_avAppFacadeFactory.AvApps);
                        _timer.Start();
                    }
                }
                else
                {
                    ProcessState = InvalidAppProcesses.Instance;
                    _messenger.Send<IAppProcessInformationPacket>(ProcessState);
                }
            }
        }
        [ObservableProperty]
        private Type? _quererType = null;
        [ObservableProperty]
        public IAppProcessInformationPacket _processState = InvalidAppProcesses.Instance;

        public ProcessManagerService(IMessenger messenger, IAvAppFacadeFactory avAppFacadeFactory, IContentDialogService contentDialogService)
        {
            _messenger = messenger;
            _avAppFacadeFactory = avAppFacadeFactory;
            _contentDialogService = contentDialogService;
            _timer = new() { AutoReset = true };
            _timer.Elapsed += Timer_Elapsed;
            Querer = new DiagnosticQuerer(_messenger);
            _messenger.RegisterAll(this);
        }

        private readonly object _lock = new();
        private IProcessQuerer? _querer;

        private void Timer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            Update(_avAppFacadeFactory.AvApps);
        }



        private void Update(IReadOnlyCollection<IAvApp> apps)
        {
            var q = Stopwatch.StartNew();
            if (Querer is null || Querer.IsScheduledUpdateNear())
            {
                // A timer-based update will soon happen
                return;
            }
            try
            {
                if (Querer?.GetProcesses(apps) is AppProcessInformation fap)
                {

                    ProcessState = fap;
                    _messenger.Send<IAppProcessInformationPacket>(ProcessState);
                }
            }
            catch (ProcessException)
            {
                ReplaceQuerer();
            }
            Debug.WriteLine("Process Monitor time: {0:f2} ms",q.Elapsed.TotalMilliseconds);
        }


        private void UpdateSingle(IAvApp app)
        {
            if (Querer is null)
            {
                return;
            }
            if (Querer is not null && Querer.IsScheduledUpdateNear())
            {
                return;
            }
            try
            {
                if (Querer?.UpdateSingleApp(app) is AppProcessInformation fap)
                {

                    ProcessState = fap;
                    _messenger.Send<IAppProcessInformationPacket>(ProcessState);
                }
            }
            catch (ProcessException)
            {
                ReplaceQuerer();
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void ReplaceQuerer()
        {
            Querer = null;
            //if (Querer is DiagnosticQuerer)
            //{
            //    Querer = new WmiQuerer(_messenger);
            //}
            //else
            //{
            //    Querer = null;
            //}
        }

        public void Receive(KillProcessRequest message) => Kill(message.Process, message.ViewModel);
        private async void Kill(SimpleProcess process, object context)
        {
            var res = await _contentDialogService.ShowProcessKillDialog(context, process);
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
        public void Receive(KillAllProcessesRequest message) => KillAll(message.AvApp, message.ViewModel);
        private async void KillAll(AvAppFacade avApp, object? viewModel)
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
                try
                {
                    var proc = Process.GetProcessById(active.Id);
                    procs.Add(proc);
                    proc.Kill();

                }
                catch (ArgumentException)
                {
                }
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
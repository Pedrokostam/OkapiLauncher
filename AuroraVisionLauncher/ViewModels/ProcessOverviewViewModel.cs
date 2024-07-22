using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using AuroraVisionLauncher.Contracts;
using AuroraVisionLauncher.Contracts.ViewModels;
using AuroraVisionLauncher.Helpers;
using AuroraVisionLauncher.Models;
using AuroraVisionLauncher.Models.Messages;
using AuroraVisionLauncher.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;

namespace AuroraVisionLauncher.ViewModels;
public partial class ProcessOverviewViewModel : ObservableRecipient, INavigationAware, ITransientWindow, IRecipient<AppProcessChangedMessage>
{
    private readonly IProcessManagerService _processManagerService;
    [ObservableProperty]
    private AvAppFacade _avApp = default!;
    private readonly DispatcherTimer _timer;
    /// <summary>
    /// Timer used to delay update request from <see cref="AppProcessChangedMessage"/>
    /// </summary>
    //private readonly DispatcherTimer _auxTimer;

    public ObservableCollection<SimpleProcess> Processes { get; } = [];
    public ProcessOverviewViewModel(IProcessManagerService processManagerService, IMessenger messenger) : base(messenger)
    {
        _processManagerService = processManagerService;
        _timer = TimerHelper.GetTimer();
        _timer.Tick += UpdateEvent;
        //_auxTimer = TimerHelper.GetTimer(500);
        //_auxTimer.Tick += DelayUpdate;

    }
    //private void DelayUpdate(object? sender, EventArgs e)
    //{
    //    Update();
    //    _auxTimer.Stop();
    //    _timer.Start();
    //}

    private void UpdateEvent(object? sender, EventArgs e)
    {
        Update();
    }

    private void Update()
    {
        if (AvApp is null)
        {
            return;
        }
        var active = _processManagerService.GetActiveProcesses(AvApp);
        foreach (var p in active)
        {
            var first = Processes.FirstOrDefault(x => x == p);
            if (first is null)
            {
                Processes.Add(p);
            }
            else
            {
                first.MainWindowTitle = p.MainWindowTitle;
            }
        }
        foreach (var item in Processes.ToList())
        {
            if (!active.Contains(item))
            {
                Processes.Remove(item);
            }
        }
    }

    public void OnNavigatedFrom()
    {
        _timer.Stop();
        _timer.Tick -= UpdateEvent;
        IsActive = false;
    }

    public void OnNavigatedTo(object? parameter)
    {
        IsActive = true;
        AvApp = parameter as AvAppFacade ?? throw new InvalidOperationException("Expected an AvApp");
        Update();
        _timer.Start();
    }

    public void Receive(AppProcessChangedMessage message)
    {
        if (message.AffectedAppPresent(AvApp))
        {
            _timer.Stop();
            Update();
            _timer.Start();
        }
    }

}

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
using AuroraVisionLauncher.Services;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AuroraVisionLauncher.ViewModels;
public partial class ProcessOverviewViewModel : ObservableObject, INavigationAware, ITransientWindow
{
    private readonly IProcessManagerService _processManagerService;
    [ObservableProperty]
    private AvAppFacade _avApp = default!;
    private readonly DispatcherTimer _timer;
    public ObservableCollection<SimpleProcess> Processes { get; } = [];
    public ProcessOverviewViewModel(IProcessManagerService processManagerService)
    {
        _processManagerService = processManagerService;
        _timer = TimerHelper.GetTimer();
        _timer.Tick += UpdateEvent;

    }
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
            if (!Processes.Contains(p))
            {
                Processes.Add(p);
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
    }

    public void OnNavigatedTo(object? parameter)
    {
        AvApp = parameter as AvAppFacade ?? throw new InvalidOperationException("Expected an AvApp");
        Update();
        _timer.Start();
    }

}

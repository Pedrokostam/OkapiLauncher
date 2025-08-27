using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Accessibility;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Options;
using OkapiLauncher.Contracts;
using OkapiLauncher.Contracts.Services;
using OkapiLauncher.Contracts.ViewModels;
using OkapiLauncher.Controls.Utilities;
using OkapiLauncher.Helpers;
using OkapiLauncher.Models;
using OkapiLauncher.Models.Messages;
using OkapiLauncher.Services;

namespace OkapiLauncher.ViewModels;
public partial class ProcessOverviewViewModel : ObservableRecipient, INavigationAware, ITransientWindow, IRecipient<FreshAppProcesses>
{
    private readonly IProcessManagerService _processManagerService;
    private readonly IGeneralSettingsService _generalSettingsService;
    [ObservableProperty]
    private AvAppFacade _avApp = default!;
    /// <summary>
    /// Timer used to delay update request from <see cref="AppProcessChangedMessage"/>
    /// </summary>
    //private readonly DispatcherTimer _auxTimer;

    public ButtonSettings OverviewButtons { get; }
    public ProcessOverviewViewModel(IProcessManagerService processManagerService, IMessenger messenger, IGeneralSettingsService generalSettingsService) : base(messenger)
    {
        _processManagerService = processManagerService;
        _generalSettingsService = generalSettingsService;
        OverviewButtons = _generalSettingsService.ButtonSettings with { VisibleButtons = VisibleButtons.All, ShowDisabledButtons = true };
    }

    private void Update()
    {
        if (AvApp is null)
        {
            return;
        }
        _processManagerService.GetCurrentState.UpdateState(AvApp);
    }

    public void OnNavigatedFrom()
    {
        IsActive = false;
    }

    public void OnNavigatedTo(object? parameter)
    {
        IsActive = true;
        AvApp = parameter as AvAppFacade ?? throw new InvalidOperationException("Expected an AvApp");
        Update();
    }

    public void Receive(FreshAppProcesses message)
    {
        if (!IsActive)
        {
            return;
        }
        Application.Current?.Dispatcher.Invoke(() => message.UpdateState(AvApp));
    }
    [RelayCommand]
    private void KillAllProcesses()
    {

        Messenger.Send<KillAllProcessesRequest>(new(AvApp, this));
    }
    [RelayCommand]
    private void KillProcess(object parameter)
    {
        if (parameter is SimpleProcess proc)
        {
            Messenger.Send<KillProcessRequest>(new(proc, this));
        }
    }
}

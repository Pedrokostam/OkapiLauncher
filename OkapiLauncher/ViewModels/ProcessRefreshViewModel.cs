﻿using System.Windows.Threading;
using OkapiLauncher.Contracts.Services;
using OkapiLauncher.Contracts.ViewModels;
using OkapiLauncher.Models;
using OkapiLauncher.Services;
using OkapiLauncher.Helpers;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using OkapiLauncher.Models.Messages;
using System.Windows;

namespace OkapiLauncher.ViewModels;

public abstract class ProcessRefreshViewModel : ObservableRecipient, INavigationAware, IRecipient<FreshAppProcesses>
{
    abstract protected IList<AvAppFacade> RawApps { get; }
    protected readonly IProcessManagerService _processManagerService;
    protected readonly IAvAppFacadeFactory _appFactory;

    protected ProcessRefreshViewModel(IProcessManagerService processManagerService,
                                      IAvAppFacadeFactory appFactory,
                                      IMessenger messenger) : base(messenger)
    {
        _processManagerService = processManagerService;
        _appFactory = appFactory;
    }

    public virtual void OnNavigatedTo(object parameter)
    {
        _processManagerService.GetCurrentState.UpdateStates(RawApps);
        IsActive = true;
    }

    public void OnNavigatedFrom()
    {
        IsActive = false;
    }

    public void Receive(FreshAppProcesses message)
    {
        Application.Current?.Dispatcher.Invoke(() => message.UpdateStates(RawApps));
    }
}

using System.Windows;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Options;
using OkapiLauncher.Contracts.Services;
using OkapiLauncher.Contracts.ViewModels;
using OkapiLauncher.Controls.Utilities;
using OkapiLauncher.Helpers;
using OkapiLauncher.Models;
using OkapiLauncher.Models.Messages;
using OkapiLauncher.Services;

namespace OkapiLauncher.ViewModels;

public abstract class ProcessRefreshViewModel : ObservableRecipient, INavigationAware, IRecipient<FreshAppProcesses>
{
    abstract protected IList<AvAppFacade> RawApps { get; }
    protected readonly IProcessManagerService _processManagerService;
    protected readonly IAvAppFacadeFactory _appFactory;
    private readonly AppConfig _appConfig;
    public ButtonSettings ButtonSettings => _appConfig.ButtonSettings;
    protected ProcessRefreshViewModel(IProcessManagerService processManagerService,
                                      IAvAppFacadeFactory appFactory,
                                      IMessenger messenger,
                                      IOptions<AppConfig> appConfig) : base(messenger)
    {
        _processManagerService = processManagerService;
        _appFactory = appFactory;
        _appConfig = appConfig.Value;
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

using System.Windows.Threading;
using AuroraVisionLauncher.Contracts.Services;
using AuroraVisionLauncher.Contracts.ViewModels;
using AuroraVisionLauncher.Models;
using AuroraVisionLauncher.Services;
using AuroraVisionLauncher.Helpers;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using AuroraVisionLauncher.Models.Messages;
using System.Windows;

namespace AuroraVisionLauncher.ViewModels;

public abstract class ProcessRefreshViewModel : ObservableRecipient, INavigationAware, IRecipient<FreshAppProcesses>
{
    abstract protected IList<AvAppFacade> _rawApps { get; }
    private readonly DispatcherTimer _timer;
    protected readonly IProcessManagerService _processManagerService;
    protected readonly IAvAppFacadeFactory _appFactory;

    protected ProcessRefreshViewModel(
                                   IProcessManagerService processManagerService,
                                   IAvAppFacadeFactory appFactory, IMessenger messenger
        ) : base(messenger)
    {
        _timer = TimerHelper.GetTimer();
        _processManagerService = processManagerService;
        _appFactory = appFactory;
    }

    public virtual void OnNavigatedTo(object parameter)
    {
        _processManagerService.GetCurrentState.UpdateStates(_rawApps);
        IsActive = true;
    }

    public void OnNavigatedFrom()
    {
        _timer.Stop();
        IsActive = false;
    }

    public void Receive(FreshAppProcesses message)
    {
        Application.Current?.Dispatcher.Invoke(() => message.UpdateStates(_rawApps));
    }
}

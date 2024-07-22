using System.Collections.ObjectModel;
using System.Windows.Threading;
using AuroraVisionLauncher.Contracts.Services;
using AuroraVisionLauncher.Contracts.ViewModels;
using AuroraVisionLauncher.Core.Models.Apps;
using AuroraVisionLauncher.Models;
using AuroraVisionLauncher.Services;
using AuroraVisionLauncher.Helpers;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.ComponentModel;
using System.Windows.Data;
using AuroraVisionLauncher.Converters;
using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.Messaging;
using AuroraVisionLauncher.Models.Messages;
using Microsoft.VisualBasic;

namespace AuroraVisionLauncher.ViewModels;

public abstract class ProcessRefreshViewModel : ObservableRecipient, INavigationAware, IRecipient<AppProcessChangedMessage>
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
        _timer.Tick += Update;
        _processManagerService = processManagerService;
        _appFactory = appFactory;
    }

    public void OnNavigatedTo(object parameter)
    {
        _processManagerService.UpdateProcessActive(_rawApps);
        _timer.Start();
        IsActive = true;
    }

    public void OnNavigatedFrom()
    {
        _timer.Stop();
        _timer.Tick -= Update;
        IsActive = false;
    }

    public void Receive(AppProcessChangedMessage message)
    {
        if (message.AffectedAppPresent(_rawApps, out _))
        {
            _timer.Stop();
            _processManagerService.UpdateProcessActive(_rawApps);
            _timer.Start();
        }
    }
    private void Update(object? sender, EventArgs e)
    {
        _processManagerService.UpdateProcessActive(_rawApps);
    }
}
public sealed partial class InstalledAppsViewModel : ProcessRefreshViewModel
{
    [ObservableProperty]
    private AppSortProperty _sortProperty = AppSortProperty.Name;
    protected override IList<AvAppFacade> _rawApps { get; }
    public InstalledAppsViewModel(IAvAppFacadeFactory appFactory,
                                  IProcessManagerService processManagerService,
                                  IMessenger messenger) : base(processManagerService, appFactory, messenger)
    {
        _rawApps = new List<AvAppFacade>(_appFactory.CreateAllFacades());
        _apps = CollectionViewSource.GetDefaultView(_rawApps);
        Regroup();
    }

    partial void OnSortPropertyChanged(AppSortProperty value)
    {
        Regroup();
    }

    private void Regroup()
    {
        Apps.GroupDescriptions.Clear();
        var gd = SortProperty switch
        {
            AppSortProperty.Type => new PropertyGroupDescription(nameof(AvAppFacade.Type), AppTypeToStringConverter.Instance),
            AppSortProperty.Brand => new PropertyGroupDescription(nameof(AvAppFacade.Brand)),
            AppSortProperty.Version => new PropertyGroupDescription(nameof(AvAppFacade.Version), InterfaceVersionConverter.Instance),
            AppSortProperty.Name => new PropertyGroupDescription(nameof(AvAppFacade.Name)),
            _ => throw new NotSupportedException()
        };
        Apps.GroupDescriptions.Add(gd);
    }


    [ObservableProperty]
    private ICollectionView _apps;

}

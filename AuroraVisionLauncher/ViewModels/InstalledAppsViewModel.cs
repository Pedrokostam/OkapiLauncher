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

namespace AuroraVisionLauncher.ViewModels;

public partial class InstalledAppsViewModel : ObservableObject, INavigationAware
{
    private readonly IAvAppFacadeFactory _appFactory;
    private readonly IProcessManagerService _processManagerService;
    private readonly DispatcherTimer _timer;
    [ObservableProperty]
    private AppSortProperty _sortProperty = AppSortProperty.Type;
    private readonly List<AvAppFacade> _rawApps;
    public InstalledAppsViewModel(IAvAppFacadeFactory appFactory, IProcessManagerService processManagerService)
    {
        _appFactory = appFactory;
        _processManagerService = processManagerService;
        _rawApps = new(_appFactory.CreateAllFacades());
        _apps = CollectionViewSource.GetDefaultView(_rawApps);
        Regroup();

        //_appFactory.Populate(AvApps);
        _timer = TimerHelper.GetTimer();
        _timer.Tick += Update;
        _processManagerService.UpdateProcessActive(_rawApps);
        _timer.Start();
    }

    private void Update(object? sender, EventArgs e)
    {
        _processManagerService.UpdateProcessActive(_rawApps);
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
            _ => throw new NotSupportedException()
        };
        Apps.GroupDescriptions.Add(gd);
    }

    public void OnNavigatedTo(object parameter)
    {
    }

    public void OnNavigatedFrom()
    {
        _timer.Stop();
        _timer.Tick -= Update;
    }


    [ObservableProperty]
    private ICollectionView _apps;
}

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Options;
using Microsoft.VisualBasic;
using OkapiLauncher.Contracts.Services;
using OkapiLauncher.Converters;
using OkapiLauncher.Core.Models.Apps;
using OkapiLauncher.Models;
using OkapiLauncher.Services;
using OkapiLauncher.Views;

namespace OkapiLauncher.ViewModels;
public sealed partial class InstalledAppsViewModel : ProcessRefreshViewModel
{
    [ObservableProperty]
    private AppSortProperty _sortProperty = AppSortProperty.Name;
    protected override IList<AvAppFacade> RawApps { get; }
    public InstalledAppsViewModel(IAvAppFacadeFactory appFactory,
                                  IProcessManagerService processManagerService,
                                  IWindowManagerService windowManagerService,
                                  IContentDialogService contentDialogService,
                                  IMessenger messenger,
                                  IGeneralSettingsService generalSettingsService) : base(processManagerService, appFactory, messenger, generalSettingsService)
    {
        RawApps = new List<AvAppFacade>(_appFactory.CreateAllFacades());
        _apps = CollectionViewSource.GetDefaultView(RawApps);
        Regroup();
        _windowManagerService = windowManagerService;
        _contentDialogService = contentDialogService;
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
            _ => throw new NotSupportedException(),
        };
        Apps.GroupDescriptions.Add(gd);
    }



    [RelayCommand]
    private async Task ShowDialog()
    {
        await _contentDialogService.ShowError("| || || |_", "Is this loss?");
    }

    [ObservableProperty]
    private ICollectionView _apps;
    private readonly IWindowManagerService _windowManagerService;
    private readonly IContentDialogService _contentDialogService;
}

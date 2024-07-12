using System.Collections.ObjectModel;
using System.Windows.Threading;
using AuroraVisionLauncher.Contracts.Services;
using AuroraVisionLauncher.Contracts.ViewModels;
using AuroraVisionLauncher.Core.Models.Apps;
using AuroraVisionLauncher.Models;
using AuroraVisionLauncher.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AuroraVisionLauncher.ViewModels;

public class InstalledAppsViewModel : ObservableObject, INavigationAware
{
    private readonly IInstalledAppsProviderService _appProvider;
    private readonly IProcessManagerService _processManagerService;
    private readonly DispatcherTimer _timer;
    public InstalledAppsViewModel(IInstalledAppsProviderService appsProviderService, IProcessManagerService processManagerService)
    {
        _appProvider = appsProviderService;
        _processManagerService = processManagerService;
        foreach (var exe in _appProvider.AvApps)
        {
            AvApps.Add(new(exe));
        }
        _timer = _processManagerService.CreateTimer(AvApps);
    }
   
    public void OnNavigatedTo(object parameter)
    {
    }

    public void OnNavigatedFrom()
    {
        _timer.Stop();
    }

    public ObservableCollection<AvAppFacade> AvApps { get; } = new();
}

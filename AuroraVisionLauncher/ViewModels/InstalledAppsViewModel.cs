using System.Collections.ObjectModel;
using System.Windows.Threading;
using AuroraVisionLauncher.Contracts.Services;
using AuroraVisionLauncher.Core.Models.Apps;
using AuroraVisionLauncher.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AuroraVisionLauncher.ViewModels;

public class InstalledAppsViewModel : ObservableObject
{
    private readonly IInstalledAppsProviderService _appProvider;
    private readonly DispatcherTimer _timer;
    public InstalledAppsViewModel(IInstalledAppsProviderService appsProviderService)
    {
        _appProvider = appsProviderService;
        foreach (var exe in _appProvider.AvApps)
        {
            AvApps.Add(new(exe));
        }
        _timer = new DispatcherTimer();
        UpdateRunningStatus();
        _timer.Tick += (o,e) => UpdateRunningStatus();
        _timer.Interval = TimeSpan.FromSeconds(5);
        _timer.Start();
    }

    private void UpdateRunningStatus()
    {
        foreach (var exe in AvApps)
        {
            exe.IsLaunched = exe.CheckIfProcessIsRunning();
        }
    }

    public ObservableCollection<AvAppFacade> AvApps { get; } = new();
}

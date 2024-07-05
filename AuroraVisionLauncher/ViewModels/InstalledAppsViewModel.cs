using System.Collections.ObjectModel;
using System.Windows.Threading;
using AuroraVisionLauncher.Contracts.Services;
using AuroraVisionLauncher.Core.Models.Apps;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AuroraVisionLauncher.ViewModels;

public class InstalledAppsViewModel : ObservableObject
{
    private readonly IInstalledAppsProviderService _appProvider;
    private DispatcherTimer _timer;
    public InstalledAppsViewModel(IInstalledAppsProviderService appsProviderService)
    {
        _appProvider = appsProviderService;
        foreach (var exe in _appProvider.Executables)
        {
            Executables.Add(exe);
        }
        _timer= new DispatcherTimer();
        _timer.Tick += Timer_Tick;
        _timer.Interval = TimeSpan.FromSeconds(2);
        _timer.Start();
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
        foreach (var exe in Executables)
        {
            exe.CheckIfProcessIsRunning();
        }
    }

    public ObservableCollection<Executable> Executables { get; } = new();
}

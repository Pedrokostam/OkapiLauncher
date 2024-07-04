using System.Collections.ObjectModel;
using AuroraVisionLauncher.Contracts.Services;
using AuroraVisionLauncher.Models.Apps;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AuroraVisionLauncher.ViewModels;

public class InstalledAppsViewModel : ObservableObject
{
    private readonly IInstalledAppsProviderService _appProvider;
    public InstalledAppsViewModel(IInstalledAppsProviderService appsProviderService)
    {
        _appProvider = appsProviderService;
        foreach (var exe in _appProvider.Executables)
        {
            Executables.Add(exe);
        }
    }
    public ObservableCollection<Executable> Executables { get; } = new();
}

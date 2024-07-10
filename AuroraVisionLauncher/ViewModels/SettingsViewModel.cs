using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Input;

using AuroraVisionLauncher.Contracts.Services;
using AuroraVisionLauncher.Contracts.ViewModels;
using AuroraVisionLauncher.Helpers;
using AuroraVisionLauncher.Models;
using AuroraVisionLauncher.Services;
using AuroraVisionLauncher.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Microsoft.Extensions.Options;

namespace AuroraVisionLauncher.ViewModels;

// TODO: Change the URL for your privacy policy in the appsettings.json file, currently set to https://YourPrivacyUrlGoesHere
public partial class SettingsViewModel : ObservableObject, INavigationAware
{
    private readonly AppConfig _appConfig;
    private readonly IThemeSelectorService _themeSelectorService;
    private readonly ISystemService _systemService;
    private readonly IApplicationInfoService _applicationInfoService;
    private readonly IFileAssociationService _fileAssociationService;

    public SettingsViewModel(IOptions<AppConfig> appConfig,
                             IThemeSelectorService themeSelectorService,
                             ISystemService systemService,
                             IApplicationInfoService applicationInfoService,
                             IFileAssociationService fileAssociationService
                             )
    {
        _appConfig = appConfig.Value;
        _themeSelectorService = themeSelectorService;
        _systemService = systemService;
        _applicationInfoService = applicationInfoService;
        _fileAssociationService = fileAssociationService;
    }

    [ObservableProperty]
    private AppTheme _theme;
    [ObservableProperty]
    private string _versionDescription = string.Empty;

    [RelayCommand]
    public void OnNavigatedTo(object parameter)
    {
        VersionDescription = $"{Properties.Resources.AppDisplayName} - {_applicationInfoService.GetVersion()}";
        Theme = _themeSelectorService.GetCurrentTheme();
    }

    [RelayCommand]
    public void OnNavigatedFrom()
    {
    }
    [RelayCommand]
    private void OnSetTheme(string themeName)
    {
        var theme = (AppTheme)Enum.Parse(typeof(AppTheme), themeName);
        _themeSelectorService.SetTheme(theme);
    }
    [RelayCommand]
    private void OnPrivacyStatement()
        => _systemService.OpenInWebBrowser(_appConfig.PrivacyStatement);
    [RelayCommand]
    private void AssociateAppWithExtensions()
    {
        _fileAssociationService.SetAssociationsToApp();
    }
    [RelayCommand]
    private static void OpenInstallationFolder()
    {
        var exePath = Environment.ProcessPath;
        if (exePath is null)
        {
            return;
        }
        ExplorerHelper.OpenExplorer(exePath);
    }
}

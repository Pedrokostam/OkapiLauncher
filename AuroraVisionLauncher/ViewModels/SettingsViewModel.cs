using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using AuroraVisionLauncher.Contracts.Services;
using AuroraVisionLauncher.Contracts.ViewModels;
using AuroraVisionLauncher.Helpers;
using AuroraVisionLauncher.Models;
using AuroraVisionLauncher.Services;
using AuroraVisionLauncher.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ControlzEx.Theming;
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

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ResetColorCommand))]
    private System.Windows.Media.Color? _currentAccent;

    [RelayCommand]
    public void OnNavigatedTo(object parameter)
    {
        VersionDescription = $"{Properties.Resources.AppDisplayName} - {_applicationInfoService.GetVersion()}";
        Theme = _themeSelectorService.GetCurrentTheme();
        CurrentAccent = _themeSelectorService.GetCurrentAccent();
    }

    [RelayCommand]
    public void OnNavigatedFrom()
    {
    }
    [RelayCommand]
    private void OnSetTheme(string themeName)
    {
        var theme = (AppTheme)Enum.Parse(typeof(AppTheme), themeName);
        _themeSelectorService.SetTheme(theme, CurrentAccent);
        CurrentAccent = _themeSelectorService.GetCurrentAccent();
        Theme = _themeSelectorService.GetCurrentTheme();
    }

    partial void OnCurrentAccentChanged(System.Windows.Media.Color? value) => OnSetTheme(Theme.ToString());

    private bool CanResetColor() => CurrentAccent != null;

    [RelayCommand(CanExecute =nameof(CanResetColor))]
    private void ResetColor()=>CurrentAccent = null;

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
        ExplorerHelper.OpenExplorerAndSelect(exePath);
    }
}

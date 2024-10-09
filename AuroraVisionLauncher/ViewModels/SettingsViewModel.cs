using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using AuroraVisionLauncher.Contracts.Services;
using AuroraVisionLauncher.Contracts.ViewModels;
using AuroraVisionLauncher.Core.Models;
using AuroraVisionLauncher.Helpers;
using AuroraVisionLauncher.Models;
using AuroraVisionLauncher.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ControlzEx.Theming;
using MahApps.Metro.Controls;
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
    private readonly IUpdateCheckService _updateCheckService;
    private readonly ICustomAppSourceService _customAppSourceService;
    private readonly IContentDialogService _contentDialogService;
    private readonly IAvAppFacadeFactory _avAppFacadeFactory;

    public SettingsViewModel(IOptions<AppConfig> appConfig,
                             IThemeSelectorService themeSelectorService,
                             ISystemService systemService,
                             IApplicationInfoService applicationInfoService,
                             IFileAssociationService fileAssociationService,
                             IUpdateCheckService updateCheckService,
                             ICustomAppSourceService customAppSourceService,
                             IContentDialogService contentDialogService,
                             IAvAppFacadeFactory avAppFacadeFactory
                             )
    {
        _appConfig = appConfig.Value;
        Link = _appConfig.GithubLink;
        _themeSelectorService = themeSelectorService;
        _systemService = systemService;
        _applicationInfoService = applicationInfoService;
        _fileAssociationService = fileAssociationService;
        _updateCheckService = updateCheckService;
        _customAppSourceService = customAppSourceService;
        _contentDialogService = contentDialogService;
        _avAppFacadeFactory = avAppFacadeFactory;
        _autoCheckForUpdates = _updateCheckService.AutoCheckForUpdatesEnabled;
    }

    [ObservableProperty]
    private string _link;

    [ObservableProperty]
    private AppTheme _theme;
    [ObservableProperty]
    private string _versionDescription = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ResetColorCommand))]
    private System.Windows.Media.Color? _currentAccent;

    [ObservableProperty]
    bool _autoCheckForUpdates;

    [RelayCommand]
    public void OnNavigatedTo(object parameter)
    {
        VersionDescription = $"{Properties.Resources.AppDisplayName} - {_applicationInfoService.GetVersion()}";
        Theme = _themeSelectorService.GetCurrentTheme();
        CurrentAccent = _themeSelectorService.GetCurrentAccent();
        UpdateStatus();

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

    public ObservableCollection<FileAssociationStatus> FileAssociationStatus { get; } = new();
    public ObservableCollection<CustomAppSource> CustomSources => _customAppSourceService.CustomSources;

    partial void OnCurrentAccentChanged(System.Windows.Media.Color? value) => OnSetTheme(Theme.ToString());

    private bool CanResetColor() => CurrentAccent != null;

    [RelayCommand(CanExecute = nameof(CanResetColor))]
    private void ResetColor() => CurrentAccent = null;

    [RelayCommand]
    private void OnPrivacyStatement()
        => _systemService.OpenInWebBrowser(Link);

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AssociateAppWithExtensionsCommand))]
    private bool _associationInProgress;
    public bool CanExecuteAssociateAppWithExtensions() => !AssociationInProgress && FileAssociationStatus.Any(x=>!x.Associated);
    [RelayCommand(CanExecute = nameof(CanExecuteAssociateAppWithExtensions))]
    private async Task AssociateAppWithExtensions()
    {
        AssociationInProgress = true;
        await Task.Run(() => _fileAssociationService.SetAssociationsToApp());
        AssociationInProgress = false;
        UpdateStatus();
    }
    private void UpdateStatus()
    {
        FileAssociationStatus.Clear();
        foreach (var fs in _fileAssociationService.CheckCurrentAssociations())
        {
            FileAssociationStatus.Add(fs);
        }

    }
    partial void OnAutoCheckForUpdatesChanged(bool value)
    {
        _updateCheckService.AutoCheckForUpdatesEnabled = value;
    }
    [RelayCommand]
    private async Task AddNewSource()
    {
        var source = new CustomAppSource();
        await _contentDialogService.ShowSourceEditor(source);
        if (source.IsDefault())
        {
            return;
        }
        CustomSources.Add(source);
        _avAppFacadeFactory.RediscoverApps();
    }
    [RelayCommand]
    private void RemoveSource(object toDelete)
    {
        if (toDelete is CustomAppSource source)
        {
            CustomSources.Remove(source);
            _avAppFacadeFactory.RediscoverApps();
        }
    }
    [RelayCommand]
    private async Task EditSource(object toEdit)
    {
        if (toEdit is CustomAppSource source)
        {
            await _contentDialogService.ShowSourceEditor(source);
            _avAppFacadeFactory.RediscoverApps();
        }
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

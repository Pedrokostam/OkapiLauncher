using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using ControlzEx.Theming;
using MahApps.Metro.Controls;
using Microsoft.Extensions.Options;
using OkapiLauncher.Contracts.Services;
using OkapiLauncher.Contracts.ViewModels;
using OkapiLauncher.Controls.Utilities;
using OkapiLauncher.Core.Models;
using OkapiLauncher.Core.Models.Apps;
using OkapiLauncher.Helpers;
using OkapiLauncher.Models;
using OkapiLauncher.Services;
using OkapiLauncher.Services.Processes;
using OkapiLauncher.Views;

namespace OkapiLauncher.ViewModels;

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
    private readonly IMessenger _messenger;
    private readonly IGeneralSettingsService _generalSettingsService;
    private readonly IProcessManagerService _processManagerService;

    public SettingsViewModel(
        IOptions<AppConfig> appConfig,
        IThemeSelectorService themeSelectorService,
        ISystemService systemService,
        IApplicationInfoService applicationInfoService,
        IFileAssociationService fileAssociationService,
        IUpdateCheckService updateCheckService,
        ICustomAppSourceService customAppSourceService,
        IContentDialogService contentDialogService,
        IAvAppFacadeFactory avAppFacadeFactory,
        IMessenger messenger,
        IGeneralSettingsService generalSettingsService,
        IProcessManagerService processManagerService)
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
        _messenger = messenger;
        _generalSettingsService = generalSettingsService;
        _processManagerService = processManagerService;
        _autoCheckForUpdates = _updateCheckService.AutoCheckForUpdatesEnabled;
        var app = AvApp.Dummy("AdoptedVisionStudio.exe",
                              new(2, 0, 0, 8),
                              new(2, 0, 2, 3),
                              "Adopted Vision Studio",
                              AvType.Professional,
                              AvBrand.Adaptive,
                              "Fake app",
                              "",
                              "",
                              "");
        _avAppFacadeDummy = new(app,
                                windowManagerService: null,
                               _messenger);
        //ButtonSettings = _appConfig.ButtonSettings with { VisibleButtons = VisibleButtons.All };
        ButtonSettingsVm = new ButtonSettingsViewModel(_generalSettingsService);
        //ButtonSettings = ButtonSettingsVm.Settings;
        //ButtonSettings = new ButtonSettings() { ListOrder = [], ShowDisabledButtons = true, VisibleButtons = VisibleButtons.All };
        OnPropertyChanged(nameof(ButtonSettingsVm));
        ProcessQuererType = _processManagerService.QuererType;
        ButtonSettingsVm.PropertyChanged += ButtonSettingsVm_SettingsChanges;
    }

    private void _processManagerService_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (string.Equals(e.PropertyName, nameof(IProcessManagerService.QuererType), StringComparison.Ordinal))
        {
            ProcessQuererType = _processManagerService.QuererType;
        }
    }

    //[ObservableProperty]
    //private ButtonSettings _buttonSettings;
    public ButtonSettings ButtonSettings => ButtonSettingsVm.Settings;
    private void ButtonSettingsVm_SettingsChanges(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (!string.Equals(e.PropertyName, nameof(ButtonSettingsViewModel.Settings), StringComparison.Ordinal))
        {
            return;
        }
        OnPropertyChanged(nameof(this.ButtonSettings));
    }

    [ObservableProperty]
    private AvAppFacade _avAppFacadeDummy;

    [ObservableProperty]
    private string _link;

    [ObservableProperty]
    private AppTheme _theme;

    [ObservableProperty]
    private Type? _processQuererType;

    [ObservableProperty]
    private string _versionDescription = string.Empty;

    //[ObservableProperty]
    //private ButtonSettings _buttonSettings;

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
        _processManagerService.PropertyChanged += _processManagerService_PropertyChanged;
        UpdateStatus();
    }

    [RelayCommand]
    public void OnNavigatedFrom()
    {
        _processManagerService.PropertyChanged -= _processManagerService_PropertyChanged;
    }
    [RelayCommand]
    private void OnSetTheme(string themeName)
    {
        var theme = (AppTheme)Enum.Parse(typeof(AppTheme), themeName);
        _themeSelectorService.SetTheme(theme, CurrentAccent);
        CurrentAccent = _themeSelectorService.GetCurrentAccent();
        Theme = _themeSelectorService.GetCurrentTheme();
    }

    //partial void OnButtonSettingsChanged(ButtonSettings value)
    //{
    //    _appConfig.ButtonSettings = value;
    //}

    public ObservableCollection<FileAssociationStatus> FileAssociationStatus { get; } = [];
    public ObservableCollection<CustomAppSource> CustomSources => _customAppSourceService.CustomSources;
    [ObservableProperty]
    private ButtonSettingsViewModel _buttonSettingsVm;
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
    public bool CanExecuteAssociateAppWithExtensions() => !AssociationInProgress && FileAssociationStatus.Any(x => !x.Associated);
    [RelayCommand(CanExecute = nameof(CanExecuteAssociateAppWithExtensions))]
    private async Task AssociateAppWithExtensions()
    {
        AssociationInProgress = true;
        await _fileAssociationService.SetAssociationsToApp();
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
    [RelayCommand]
    private async Task CheckForUpdates()
    {
        await _updateCheckService.ManualPrompUpdate();
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

using System.IO;
using System.Windows;
using OkapiLauncher.Contracts.Services;
using OkapiLauncher.Models.Messages;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using OkapiLauncher.Core.Models.Apps;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Diagnostics;
using Windows.Storage;
using OkapiLauncher.Models;
using OkapiLauncher.Core.Models.Projects;
using System.Windows.Threading;
using OkapiLauncher.Services;
using OkapiLauncher.Contracts.ViewModels;
using OkapiLauncher.Helpers;
using Microsoft.VisualBasic;
using OkapiLauncher.Core.Models;
using OkapiLauncher.Properties;
using OkapiLauncher.Views;

namespace OkapiLauncher.ViewModels;

public sealed partial class LauncherViewModel : ProcessRefreshViewModel
{

    private readonly INavigationService _navigationService;
    private readonly IRecentlyOpenedFilesService _lastOpenedFilesService;
    private readonly IContentDialogService _contentDialogService;

    public LauncherViewModel(IAvAppFacadeFactory appProvider,
                             INavigationService navigationService,
                             IRecentlyOpenedFilesService lastOpenedFilesService,
                             IProcessManagerService processManagerService, IContentDialogService contentDialogService,
                             IMessenger messenger) : base(processManagerService, appProvider, messenger)
    {
        _lastOpenedFilesService = lastOpenedFilesService;
        _contentDialogService = contentDialogService;
        _navigationService = navigationService;
    }
    public bool ShouldCloseAfterLaunching
    {
        get => ((App)App.Current).ShouldCloseAfterLaunching;
        set => ((App)App.Current).ShouldCloseAfterLaunching = value;
    }


    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CopyArgumentStringCommand))]
    private LaunchOptions? _launchOptions;
    public ObservableCollection<AvAppFacade> Apps { get; } = [];
    protected override IList<AvAppFacade> RawApps => Apps;

    private bool CanLaunch()
    {
        return SelectedApp is not null && (VisionProject?.Exists ?? false);
    }
    //[RelayCommand(CanExecute = nameof(CanLaunch))]
    [RelayCommand(CanExecute = nameof(CanLaunch))]
    private void Launch(object? app)
    {
        IAvApp? appToLaunch = app as IAvApp;
        appToLaunch ??= SelectedApp;
        if (appToLaunch is null || !(VisionProject?.Exists ?? false))
        {
            return;
        }

        var args = LaunchOptions.Get(appToLaunch,VisionProject.Path)?.GetCommandLineArgs();
        if(args is null)
        {
            return;
        }
        Messenger.Send(new OpenAppRequest(appToLaunch, args));
        if (ShouldCloseAfterLaunching)
        {
            Application.Current.Shutdown();
        }

    }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LaunchCommand))]
    private VisionProjectFacade? _visionProject = null;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LaunchCommand))]
    private AvAppFacade? _selectedApp = null;

    partial void OnSelectedAppChanged(AvAppFacade? value)
    {
        LaunchOptions = LaunchOptions.Get(value, VisionProject?.Path);
    }

    private bool CanCopyArgumentString() => !string.IsNullOrWhiteSpace(LaunchOptions?.ArgumentString);
    [RelayCommand(CanExecute = nameof(CanCopyArgumentString))]
    private void CopyArgumentString()
    {
        var t = Thread.CurrentThread;
        if (LaunchOptions?.ArgumentString is not null)
        {
            Clipboard.SetText(LaunchOptions.ArgumentString);
        }
    }

    public async Task<bool> OpenProject(string filepath)
    {

        try
        {
            var project = ProjectReader.OpenProject(filepath);
            VisionProject = new VisionProjectFacade(project);

            var matchingApps = _appFactory.AvApps
                .Where(x => x.CanOpen(VisionProject))
                .OrderByDescending(x => x.Version);
            SelectedApp = null;
            _appFactory.Populate(matchingApps,
                Apps,
                perItemAction: UpdateCompatibility);
            var closestVersion = AvApp.GetClosestApp(Apps, VisionProject);
            if (closestVersion >= 0)
            {
                SelectedApp = Apps[closestVersion];
            }
            else
            {
                SelectedApp = null;
            }
            _lastOpenedFilesService.AddLastFile(filepath);
            _navigationService.NavigateTo(GetType().FullName!);
            _processManagerService.GetCurrentState.UpdateStates(Apps);
            return true;
        }
        catch (FileNotFoundException)
        {
            await _contentDialogService.ShowError(Resources.ErrorFileDoesNotExist);
            return false;
        }
        catch (InvalidDataException)
        {
            await _contentDialogService.ShowError(Resources.ErrorUnrecognizedFile);
            return false;
        }
        catch (UriFormatException)
        {
            await _contentDialogService.ShowError(Resources.ErrorInvalidXml);
            return false;
        }
        catch (UnknownProjectTypeException)
        {
            await _contentDialogService.ShowError(Resources.ErrorUnknownFileType);
            return false;
        }

    }
    private void UpdateCompatibility(AvAppFacade avApp)
    {
        avApp.Compatibility = JudgeCompatibility(avApp, VisionProject!);
    }
    private static Compatibility JudgeCompatibility(IAvApp app, IVisionProject program)
    {
        if (!app.Brand.SupportsBrand(program.Brand))
        {
            return Compatibility.Incompatible;
        }
        if (!app.CanOpen(program))
        {
            return Compatibility.Incompatible;
        }
        if (program.Version.IsUnknown)
        {
            return Compatibility.Unknown;
        }
        if (program.Type == ProductType.Runtime)
        {
            if (app.Version.Major == program.Version.Major && app.Version.Minor == program.Version.Minor && app.Version.Build == program.Version.Build)
            {
                return Compatibility.Compatible;
            }
            return Compatibility.Incompatible;
        }
        if (app.Type == ProductType.Runtime)
        {
            return app.Version.InterfaceVersion == program.Version.InterfaceVersion ? Compatibility.Compatible : Compatibility.Incompatible;
        }
        if (app.Version.CompareTo(program.Version) >= 0)
        {
            return Compatibility.Compatible;
        }
        return Compatibility.Outdated;
    }
    public override async void OnNavigatedTo(object parameter)
    {
        var t = Thread.CurrentThread;
        base.OnNavigatedTo(parameter);

        var lastFile = _lastOpenedFilesService.LastOpenedFile;
        if (parameter is string path)
        {
            bool loadGood = await OpenProject(path);
            if (!loadGood)
            {
                if (lastFile is string s)
                {
                    await OpenProject(s);
                }
                else
                {
                    _navigationService.NavigateTo(typeof(InstalledAppsViewModel).FullName!);
                }
            }
        }
        else if (lastFile is string last)
        {
            await OpenProject(last);
        }
    }
}

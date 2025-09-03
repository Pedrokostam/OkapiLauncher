using System.Collections.ObjectModel;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using OkapiLauncher.Contracts.Services;
using OkapiLauncher.Core.Models;
using OkapiLauncher.Core.Models.Apps;
using OkapiLauncher.Core.Models.Projects;
using OkapiLauncher.Models;
using OkapiLauncher.Models.Messages;
using OkapiLauncher.Properties;
using OkapiLauncher.Services;

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
                             IMessenger messenger,
                             IGeneralSettingsService generalSettingsService) : base(processManagerService, appProvider, messenger, generalSettingsService)
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
        bool isAppNull = app is null;
        IAvApp? appToLaunch = app as IAvApp;
        appToLaunch ??= SelectedApp;
        if (appToLaunch is null || !(VisionProject?.Exists ?? false))
        {
            return;
        }

        var args = LaunchOptions.Get(appToLaunch, VisionProject.Path)?.GetCommandLineArgs();
        if (args is null)
        {
            return;
        }
        Messenger.Send(new OpenAppRequest(appToLaunch, args));
        if (ShouldCloseAfterLaunching && isAppNull)
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
        if (LaunchOptions?.ArgumentString is not null)
        {
            Clipboard.SetText(LaunchOptions.ArgumentString);
        }
    }
    static readonly Regex FileDetector = new(@"(?<NORMAL>\.(avproj|avexe|fiproj|fiexe))|(?<DL>pluginconfig.xml)", RegexOptions.Compiled | RegexOptions.IgnoreCase|RegexOptions.ExplicitCapture, TimeSpan.FromMilliseconds(500));

    /// <summary>
    /// Either returns <paramref name="path"/> as is if it is a directory, or attempts to find one of applicable files.
    /// </summary>
    /// <param name="path">Path to a project/exe/pluginconfig file.</param>
    /// <returns></returns>
    /// <exception cref="Exception">If the path was a folder and no applicable file was found.</exception>
    private static string HandleDirectories(string path)
    {
        if (Directory.Exists(path))
        {
            var files = Directory.EnumerateFiles(path);
            foreach (var file in files)
            {
                if (FileDetector.IsMatch(Path.GetFileName(file)))
                {
                    return file;
                }
            }
            throw new NoApplicableFileException(path);
        }
        return path;
    }

    public async Task<bool> OpenProject(string filepath)
    {

        try
        {
            filepath = HandleDirectories(filepath);
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
            _lastOpenedFilesService.AddLastFile(project.Path);
            _navigationService.NavigateTo(GetType().FullName!);
            _processManagerService.ProcessState.UpdateStates(Apps);
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
        catch (NoApplicableFileException)
        {
            await _contentDialogService.ShowError(Resources.ErrorNoApplicableFileInFolder);
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

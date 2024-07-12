using System.IO;
using System.Windows;
using AuroraVisionLauncher.Contracts.Services;
using AuroraVisionLauncher.Models.Messages;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using AuroraVisionLauncher.Core.Models.Apps;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Diagnostics;
using Windows.Storage;
using AuroraVisionLauncher.Models;
using AuroraVisionLauncher.Core.Models.Programs;
using System.Windows.Threading;
using AuroraVisionLauncher.Services;
using AuroraVisionLauncher.Contracts.ViewModels;
using AuroraVisionLauncher.Helpers;
using Microsoft.VisualBasic;

namespace AuroraVisionLauncher.ViewModels;

public sealed partial class LauncherViewModel : ObservableObject, INavigationAware
{
    private static int counter = 0;
    public int ID { get; } = counter++;

    private readonly IAvAppFacadeFactory _appFactory;
    private readonly INavigationService _navigationService;
    private readonly IRecentlyOpenedFilesService _lastOpenedFilesService;
    private readonly IProcessManagerService _processManagerService;
    private readonly DispatcherTimer _timer;

    public LauncherViewModel(IAvAppFacadeFactory appProvider,
                             INavigationService navigationService,
                             IRecentlyOpenedFilesService lastOpenedFilesService,
                             IProcessManagerService processManagerService)
    {
        _lastOpenedFilesService = lastOpenedFilesService;
        _processManagerService = processManagerService;
        _appFactory = appProvider;
        _navigationService = navigationService;
        _timer = TimerHelper.GetTimer();
        _timer.Tick += Update;
        _processManagerService.UpdateProcessActive(Apps);
        _timer.Start();
    }

    private void Update(object? sender, EventArgs e)
    {
        _processManagerService.UpdateProcessActive(Apps);
    }


    [ObservableProperty]
    private LaunchOptions? _launchOptions;
    public ObservableCollection<AvAppFacade> Apps { get; } = [];

    private bool CanLaunch()
    {
        return SelectedApp is not null && (VisionProgram?.Exists ?? false);
    }
    [RelayCommand(CanExecute = nameof(CanLaunch))]
    private void Launch()
    {
        if (SelectedApp is null || !(VisionProgram?.Exists ?? false))
        {
            return;
        }
        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = SelectedApp.ExePath,
            UseShellExecute = true,  // Use the shell to start the process
            CreateNoWindow = true, // Do not create a window
        };
        var args = LaunchOptions!.GetCommandLineArgs();
        foreach (var arg in args)
        {
            startInfo.ArgumentList.Add(arg);
        }
        try
        {
            Process.Start(startInfo);
        }
        catch (System.ComponentModel.Win32Exception)
        {
        }

    }
    [RelayCommand(CanExecute = nameof(CanLaunch))]
    private void LaunchAndClose()
    {
        Launch();
        Application.Current.Shutdown();
    }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LaunchCommand))]
    private VisionProgramFacade? _visionProgram = null;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LaunchCommand))]
    private AvAppFacade? _selectedApp = null;

    partial void OnSelectedAppChanged(AvAppFacade? value)
    {
        LaunchOptions = LaunchOptions.Get(value, VisionProgram?.Path);
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

    private void OpenProject(string filepath)
    {
        if (!File.Exists(filepath))
        {
            MessageBox.Show("File does not exist");
        }
        try
        {
            var info = ProgramReader.GetInformation(filepath);
            VisionProgram = new VisionProgramFacade(
                Path.GetFileNameWithoutExtension(filepath),
                info.Version,
                filepath,
                info.ProgramType
                );

            var matchingApps = _appFactory.AvApps
                .Where(x => x.CanOpen(info.ProgramType))
                .OrderByDescending(x => x.Version);
            SelectedApp = null;
            _appFactory.Populate(matchingApps,
                Apps,
                perItemAction: app => app.UpdateCompatibility(VisionProgram));
            var closestVersion = AvApp.GetClosestApp(Apps, VisionProgram);
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
            _processManagerService.UpdateProcessActive(Apps);
        }
        catch (InvalidDataException)
        {
            MessageBox.Show("File is neither a projects file nor a runtime executable.");
        }

    }


    public void OnNavigatedTo(object parameter)
    {
        if (parameter is string s)
        {
            OpenProject(s);
        }
    }

    public void OnNavigatedFrom()
    {
        _timer.Stop();
    }
}

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

namespace AuroraVisionLauncher.ViewModels;

public partial class LauncherViewModel : ObservableRecipient, IRecipient<FileRequestedMessage>
{
    private readonly IInstalledAppsProviderService _appProvider;
    private readonly INavigationService _navigationService;
    private readonly IRecentlyOpenedFilesService _lastOpenedFilesService;
    private readonly DispatcherTimer _timer;

    public LauncherViewModel(IMessenger messenger, IInstalledAppsProviderService appProvider, INavigationService navigationService,IRecentlyOpenedFilesService lastOpenedFilesService) : base(messenger)
    {
        _lastOpenedFilesService = lastOpenedFilesService;
        _appProvider = appProvider;
        _navigationService = navigationService;
        OnActivated();
        _timer = new DispatcherTimer();
        UpdateRunningStatus();
        _timer.Tick += (o, e) => UpdateRunningStatus();
        _timer.Interval = TimeSpan.FromSeconds(2);
        _timer.Start();
    }

    private void UpdateRunningStatus()
    {
        foreach (var exe in Apps)
        {
            exe.IsLaunched = exe.CheckIfProcessIsRunning();
        }
    }

    [ObservableProperty]
    private LaunchOptions? _launchOptions;
    public ObservableCollection<AvAppFacade> Apps { get; } = new();


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
            CreateNoWindow = true    // Do not create a window
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
    [RelayCommand(CanExecute =nameof(CanCopyArgumentString))]
    private void CopyArgumentString()
    {
        if (LaunchOptions?.ArgumentString is not null)
        {
            Clipboard.SetText(LaunchOptions.ArgumentString);
        }
    }

    public void Receive(FileRequestedMessage message) => OpenProject(message.Value);
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

            var matchingApps = _appProvider.AvApps
                .Where(x => x.SupportsProgram(info))
                .Select(x => new AvAppFacade(x))
                .OrderByDescending(x => x.Compatibility);
            Apps.Clear();
            foreach (var app in matchingApps)
            {
                Apps.Add(app);
            }
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
            UpdateRunningStatus();
        }
        catch (InvalidDataException)
        {
            MessageBox.Show("File is neither a projects file nor a runtime executable.");
        }

    }

}

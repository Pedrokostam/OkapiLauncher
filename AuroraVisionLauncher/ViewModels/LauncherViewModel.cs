﻿using System.IO;
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

namespace AuroraVisionLauncher.ViewModels;

public partial class LauncherViewModel : ObservableRecipient, IRecipient<FileRequestedMessage>
{
    private readonly IInstalledAppsProviderService _appProvider;
    private readonly INavigationService _navigationService;

    public LauncherViewModel(IMessenger messenger, IInstalledAppsProviderService appProvider, INavigationService navigationService) : base(messenger)
    {
        _appProvider = appProvider;
        _navigationService = navigationService;
        OnActivated();
    }



    public ObservableCollection<ExecutableFacade> Apps { get; } = new();


    private bool CanLaunch()
    {
        return SelectedExecutable is not null && (VisionProgram?.Exists ?? false);
    }
    [RelayCommand(CanExecute = nameof(CanLaunch))]
    private void Launch()
    {
        if (SelectedExecutable is null || !(VisionProgram?.Exists ?? false))
        {
            return;
        }
        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = SelectedExecutable.ExePath,
            UseShellExecute = true,  // Use the shell to start the process
            CreateNoWindow = true    // Do not create a window
        };
        startInfo.ArgumentList.Add(VisionProgram!.Path);
        try
        {
            Process.Start(startInfo);
        }
        catch (System.ComponentModel.Win32Exception)
        {
        }

    }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LaunchCommand))]
    private VisionProgram? _visionProgram = null;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LaunchCommand))]
    private ExecutableFacade? _selectedExecutable = null;
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
            VisionProgram = new VisionProgram(
                Path.GetFileNameWithoutExtension(filepath),
                info.Version,
                filepath,
                info.ProgramType
                );

            var matchingExecutables = _appProvider.Executables
                .Where(x => x.SupportsProgram(info))
                .Select(x => new ExecutableFacade(x))
                .OrderByDescending(x => x.Compatibility);
            Apps.Clear();
            foreach (var executable in matchingExecutables)
            {
                Apps.Add(executable);
            }
            var closestVersion = Executable.GetClosestApp(Apps, VisionProgram);
            if (closestVersion >= 0)
            {
                SelectedExecutable = Apps[closestVersion];
            }
            else
            {
                SelectedExecutable = null;
            }
            _navigationService.NavigateTo(GetType().FullName!);
        }
        catch (InvalidDataException e)
        {
            MessageBox.Show("File is neither a projects file nor a runtime executable.");
        }

    }

}
﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ABI.Windows.Foundation;
using AuroraVisionLauncher.Core.Models.Apps;
using AuroraVisionLauncher.Core.Models.Programs;
using AuroraVisionLauncher.Helpers;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Windows.ApplicationModel.VoiceCommands;

namespace AuroraVisionLauncher.Models;
public partial class AvAppFacade : ObservableObject, IAvApp
{
    readonly private AvApp _avApp;

    public string Name => _avApp.Name;
    public string ExePath => _avApp.ExePath;
    public Version Version => _avApp.Version;
    public string NameWithVersion => $"{Name} {Version}";
    public bool IsDevelopmentBuild => _avApp.IsDevelopmentBuild;

    public AvAppType AppType => _avApp.AppType;

    public Version? SecondaryVersion => _avApp.SecondaryVersion;

    public CommandLineInterface Interface => _avApp.Interface;

    [ObservableProperty]
    private Compatibility? _compatibility = null;

    public void UpdateCompatibility(IVisionProgram program)
    {
        Compatibility = Compatibility.Get(this, program);
    }

    [ObservableProperty]
    private bool _isLaunched = false;

    public AvAppFacade(AvApp avApp)
    {
        _avApp = avApp;
    }

    public bool CheckIfProcessIsRunning() => _avApp.CheckIfProcessIsRunning();

    public bool CanOpen(ProgramType type) => _avApp.CanOpen(type);
    public bool IsNativeApp(ProgramType type) => _avApp.IsNativeApp(type);

    [RelayCommand]
    private void OpenContainingFolder() => ExplorerHelper.OpenExplorer(ExePath);
    [RelayCommand]
    private void LaunchWithoutProgram()
    {
        Process.Start(new ProcessStartInfo()
        {
            FileName = _avApp.ExePath,
        });
    }
    [RelayCommand]
    private void CopyExecutablePath()
    {
        Clipboard.SetText(ExePath);
    }
}

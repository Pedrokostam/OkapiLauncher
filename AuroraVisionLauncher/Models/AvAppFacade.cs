using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ABI.Windows.Foundation;
using AuroraVisionLauncher.Core.Models;
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
    public AvVersionFacade Version { get; }
    public string NameWithVersion => $"{Name} {Version}";
    public bool IsDevelopmentVersion => Version.IsDevelopmentVersion;

    public AvAppType AppType => _avApp.AppType;

    public AvVersionFacade? SecondaryVersion { get; }

    public CommandLineInterface Interface => _avApp.Interface;
    IAvVersion IAvApp.Version => Version;
    IAvVersion? IAvApp.SecondaryVersion => SecondaryVersion;

    public string InternalName => _avApp.InternalName;

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
        Version = new AvVersionFacade(avApp.Version);
        SecondaryVersion = avApp.SecondaryVersion is not null ? new AvVersionFacade(avApp.SecondaryVersion) : null;
    }
    public ObservableCollection<SimpleProcess> ActiveProcesses { get; } = new();

    /// <summary>
    /// Checks if any process associated with the executable is running.
    /// </summary>
    /// <returns><see langword="true"/> if the process is running; <see langword="false"/> if it is not running, or it could not be checked.</returns>
    public bool CheckIfProcessIsRunning()
    {
        try
        {
            var processes = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(InternalName));
            List<int> keptIds = [];
            List<int> indicesToRemove = [];
            foreach (var process in processes)
            {
                if(!string.Equals(process.MainModule?.FileName, ExePath, StringComparison.OrdinalIgnoreCase))
                {
                    process.Dispose();
                    continue;
                }
                var existing = ActiveProcesses.FirstOrDefault(x => x.Id == process.Id);
                keptIds.Add(process.Id);
                if (existing is null)
                {
                    ActiveProcesses.Add(new(process));
                }
                else
                {
                    existing.MainWindowTitle = process.MainWindowTitle;
                    process.Dispose();
                }
            }

            foreach (var (sp, i) in ActiveProcesses.Select((x, i) => (x, i)))
            {
                if (!keptIds.Contains(sp.Id))
                {
                    indicesToRemove.Add(i);
                }
            }

            indicesToRemove.Reverse();
            foreach (var index in indicesToRemove)
            {
                ActiveProcesses.RemoveAt(index);
            }

            return ActiveProcesses.Any();
        }
        catch (InvalidOperationException)
        {
            return false;
        }
    }

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

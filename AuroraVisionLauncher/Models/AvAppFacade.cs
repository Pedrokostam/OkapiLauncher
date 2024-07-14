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
using AuroraVisionLauncher.Contracts.Services;
using AuroraVisionLauncher.Core.Models;
using AuroraVisionLauncher.Core.Models.Apps;
using AuroraVisionLauncher.Core.Models.Projects;
using AuroraVisionLauncher.Helpers;
using AuroraVisionLauncher.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Windows.ApplicationModel.VoiceCommands;

namespace AuroraVisionLauncher.Models;
public partial class AvAppFacade : ObservableObject, IAvApp, IComparable<AvAppFacade>, IEquatable<AvAppFacade>
{
    readonly private AvApp _avApp;
    private readonly Lazy<IWindowManagerService> _windowManagerService;

    public string Name => _avApp.Name;
    public string Path => _avApp.Path;
    public string RootPath => _avApp.RootPath;
    public AvVersionFacade Version { get; }
    public string NameWithVersion => $"{Name} {Version}";
    public bool IsDevelopmentVersion => Version.IsDevelopmentVersion;
    public bool IsExecutable=>_avApp.IsExecutable;



    public AvVersionFacade? SecondaryVersion { get; }

    public CommandLineInterface Interface => _avApp.Interface;
    IAvVersion IProduct.Version => Version;
    IAvVersion? IAvApp.SecondaryVersion => SecondaryVersion;

    public string ProcessName => _avApp.ProcessName;

    [ObservableProperty]
    private Compatibility? _compatibility = null;

    public void UpdateCompatibility(IVisionProject program)
    {
        Compatibility = Compatibility.Get(this, program);
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsLaunched))]
    private int _activeProcessesNumber;

    public bool IsLaunched => ActiveProcessesNumber > 0;

    public AvAppFacade(AvApp avApp, IWindowManagerService windowManagerService)
    {
        _avApp = avApp;
        _windowManagerService = new(windowManagerService);
        Version = new AvVersionFacade(avApp.Version);
        SecondaryVersion = avApp.SecondaryVersion is not null ? new AvVersionFacade(avApp.SecondaryVersion) : null;
    }
    public ObservableCollection<SimpleProcess> ActiveProcesses { get; } = new();

    public ProductBrand Brand => _avApp.Brand;

    public ProductType Type => _avApp.Type;

    /// <summary>
    /// Checks if any process associated with the executable is running.
    /// </summary>
    /// <returns><see langword="true"/> if the process is running; <see langword="false"/> if it is not running, or it could not be checked.</returns>
    public bool CheckIfProcessIsRunning()
    {
        try
        {
            var processes = Process.GetProcessesByName(System.IO.Path.GetFileNameWithoutExtension(ProcessName));
            List<int> keptIds = [];
            List<int> indicesToRemove = [];
            foreach (var process in processes)
            {
                if (!string.Equals(process.MainModule?.FileName, Path, StringComparison.OrdinalIgnoreCase))
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

    public bool CanOpen(IVisionProject type) => _avApp.CanOpen(type);
    public bool IsNativeApp(IVisionProject type) => _avApp.IsNativeApp(type);

    [RelayCommand]
    private void OpenContainingFolder() => ExplorerHelper.OpenExplorer(RootPath);
    private bool CanLaunchExecutable() => IsExecutable;
    [RelayCommand(CanExecute =nameof(CanLaunchExecutable))]
    private void LaunchWithoutProgram()
    {
        Process.Start(new ProcessStartInfo()
        {
            FileName = _avApp.Path,
        });
    }
    [RelayCommand]
    private void CopyExecutablePath()
    {
        Clipboard.SetText(Path);
    }
    [RelayCommand]
    private void KillAllProcesses()
    {
        return;
    }

    [RelayCommand]
    private void ShowProcessOverview()
    {
        _windowManagerService.Value.OpenInNewWindow(typeof(ProcessOverviewViewModel).FullName!, this);
    }

    public int CompareTo(AvAppFacade? other) => CompareTo(other as IAvApp);
    public bool Equals(AvAppFacade? other) => _avApp == other?._avApp;
    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (ReferenceEquals(obj, null))
        {
            return false;
        }

        return Equals(obj as AvAppFacade);
    }

    public override int GetHashCode()=>_avApp.GetHashCode();

    public int CompareTo(IAvApp? other) => _avApp.CompareTo(other);

    public static bool operator ==(AvAppFacade left, AvAppFacade right)
    {
        if (ReferenceEquals(left, null))
        {
            return ReferenceEquals(right, null);
        }

        return left.Equals(right);
    }

    public static bool operator !=(AvAppFacade left, AvAppFacade right)
    {
        return !(left == right);
    }

    public static bool operator <(AvAppFacade left, AvAppFacade right)
    {
        return ReferenceEquals(left, null) ? !ReferenceEquals(right, null) : left.CompareTo(right) < 0;
    }

    public static bool operator <=(AvAppFacade left, AvAppFacade right)
    {
        return ReferenceEquals(left, null) || left.CompareTo(right) <= 0;
    }

    public static bool operator >(AvAppFacade left, AvAppFacade right)
    {
        return !ReferenceEquals(left, null) && left.CompareTo(right) > 0;
    }

    public static bool operator >=(AvAppFacade left, AvAppFacade right)
    {
        return ReferenceEquals(left, null) ? ReferenceEquals(right, null) : left.CompareTo(right) >= 0;
    }
}

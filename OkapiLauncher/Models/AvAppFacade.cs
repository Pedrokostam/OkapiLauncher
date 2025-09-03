using System.IO;
using System.Windows;
using OkapiLauncher.Contracts.Services;
using OkapiLauncher.Core.Models;
using OkapiLauncher.Core.Models.Apps;
using OkapiLauncher.Core.Models.Projects;
using OkapiLauncher.Helpers;
using OkapiLauncher.Models.Messages;
using OkapiLauncher.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using OkapiLauncher.Controls.Utilities;

namespace OkapiLauncher.Models;
public partial class AvAppFacade : ObservableObject, IAvApp, IComparable<AvAppFacade>, IEquatable<AvAppFacade>
{
    readonly private AvApp _avApp;
    private readonly Lazy<IWindowManagerService?> _windowManagerService;

    public string Name => _avApp.Name;
    public string Path => _avApp.Path;
    public string RootPath => _avApp.RootPath;
    public AvVersionFacade Version { get; }
    public string NameWithVersion => $"{Name} {Version}";
    public bool IsDevelopmentVersion => Version.IsDevelopmentVersion;
    public bool IsExecutable => _avApp.IsExecutable;
    public string? LogFolderPath => _avApp.LogFolderPath;

    public string? Description => _avApp.Description ?? Name;
    public bool IsCustom => _avApp.IsCustom;

    public ButtonSettings Susu { get; }
    public bool WarnAboutNewProcess => Type == ProductType.Professional && IsLaunched;
    public AvVersionFacade? SecondaryVersion { get; }

    public CommandLineInterface Interface => _avApp.Interface;
    IAvVersion IProduct.Version => Version;
    IAvVersion? IAvApp.SecondaryVersion => SecondaryVersion;

    public string ProcessName => _avApp.ProcessName;

    [ObservableProperty]
    private Compatibility? _compatibility = null;
    [ObservableProperty]
    private bool _processInfoAvailable=false;
    public bool ShowProcessInfo => IsExecutable && ProcessInfoAvailable;
    private readonly IMessenger _messenger;

    public DatedObservableCollection<SimpleProcess> ActiveProcesses { get; } = [];

    public bool IsLaunched => ActiveProcesses.Count > 0;

    public AvAppFacade(AvApp avApp, IWindowManagerService? windowManagerService, IMessenger messenger)
    {
        _avApp = avApp;
        _windowManagerService = new(windowManagerService);
        Version = new AvVersionFacade(avApp.Version);
        SecondaryVersion = avApp.SecondaryVersion is not null ? new AvVersionFacade(avApp.SecondaryVersion) : null;
        _messenger = messenger;
        ActiveProcesses.CollectionChanged += ActiveProcesses_CollectionChanged;
    }

    private void ActiveProcesses_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        OnPropertyChanged(nameof(IsLaunched));
        OnPropertyChanged(nameof(WarnAboutNewProcess));
    }
    partial void OnProcessInfoAvailableChanged(bool value)
    {
        OnPropertyChanged(nameof(ShowProcessInfo));
        KillAllProcessesCommand.NotifyCanExecuteChanged();
        ShowProcessOverviewCommand.NotifyCanExecuteChanged();
    }

    public ProductBrand Brand => _avApp.Brand;

    public ProductType Type => _avApp.Type;

    public bool CanOpen(IVisionProject type) => _avApp.CanOpen(type);
    public bool IsNativeApp(IVisionProject type) => _avApp.IsNativeApp(type);

    [RelayCommand]
    private void OpenContainingFolder() => ExplorerHelper.OpenExplorer(RootPath);
    private bool CanLaunchExecutable() => IsExecutable;
    [RelayCommand(CanExecute = nameof(CanLaunchExecutable))]
    public void LaunchWithoutProgram()
    {
        _messenger.Send(new OpenAppRequest(this));
    }
    [RelayCommand]
    private void CopyExecutablePath()
    {
        Clipboard.SetText(Path);
    }

    [RelayCommand(CanExecute = nameof(CanOpenLicenseFolder))]
    private void OpenLicenseFolder()
    {
        ExplorerHelper.OpenExplorer(Brand.GetLicenseKeyFolderPath());
    }

    [RelayCommand(CanExecute = nameof(CanOpenLogFolder))]
    private void OpenLogFolder()
    {
        if (LogFolderPath is not null)
            ExplorerHelper.OpenExplorer(LogFolderPath);

    }
    [RelayCommand(CanExecute =nameof(ShowProcessInfo))]
    private void KillAllProcesses()
    {
        _messenger.Send(new KillAllProcessesRequest(this,ViewModel:null));
    }
    

    public bool CanOpenLicenseFolder => Directory.Exists(Brand.GetLicenseKeyFolderPath());
    public bool CanOpenLogFolder => Directory.Exists(LogFolderPath);

    [RelayCommand(CanExecute =nameof(ShowProcessInfo))]
    private void ShowProcessOverview()
    {
        if (_windowManagerService.Value is null)
        {
            return;
        }
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

        if (obj is null)
        {
            return false;
        }

        return Equals(obj as AvAppFacade);
    }

    public override int GetHashCode() => _avApp.GetHashCode();

    public int CompareTo(IAvApp? other) => _avApp.CompareTo(other);

    public static bool operator ==(AvAppFacade left, AvAppFacade right)
    {
        if (left is null)
        {
            return right is null;
        }

        return left.Equals(right);
    }

    public static bool operator !=(AvAppFacade left, AvAppFacade right)
    {
        return !(left == right);
    }

    public override string ToString() => _avApp.ToString();

    public static bool operator <(AvAppFacade left, AvAppFacade right)
    {
        return left is null ? right is not null : left.CompareTo(right) < 0;
    }

    public static bool operator <=(AvAppFacade left, AvAppFacade right)
    {
        return left is null || left.CompareTo(right) <= 0;
    }

    public static bool operator >(AvAppFacade left, AvAppFacade right)
    {
        return left is not null && left.CompareTo(right) > 0;
    }

    public static bool operator >=(AvAppFacade left, AvAppFacade right)
    {
        return left is null ? right is null : left.CompareTo(right) >= 0;
    }
}

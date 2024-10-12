using System.Diagnostics;
using AuroraVisionLauncher.Core.Models;
using AuroraVisionLauncher.Core.Models.Apps;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Windows.System.RemoteDesktop;

namespace AuroraVisionLauncher.Models;

public abstract partial class LaunchOptions : ObservableObject
{
    private static readonly StudioLaunchOptions _studioOptions = new();
    private static readonly NoLaunchOptions _noOptions = new();
    private static readonly ExecutorLaunchOptions _executorOptions = new();

    public abstract IEnumerable<string> GetCommandLineArgs();

    /// <summary>
    /// Selects one of the static instances of launch options based on the commandline interface of the given app.
    /// Static instances allow us the setting sto persist even after changing the select app.
    /// </summary>
    /// <param name="avapp"></param>
    /// <returns></returns>
    public static LaunchOptions Get(ProductType? productType)
    {
        if(productType == ProductType.Professional)
        {
            return _studioOptions;  
        }
        if (productType == ProductType.Runtime)
        {
            return _executorOptions;
        }
        return _noOptions;
    }
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ArgumentString))]
    private string? _programPath;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ArgumentString))]
    private string? _applicationPath;
    public string ArgumentString
    {
        get
        {
            var args = GetCommandLineArgs()
                .Prepend(ApplicationPath!)
                .Select(x => (x ?? "").Contains(' ',StringComparison.Ordinal) ? $"\"{x}\"" : x);
            return string.Join(' ', args);
        }
    }
    /// <summary>
    /// Selects one of the static instances of launch options based on the commandline interface of the given app.
    /// Static instances allow us the setting sto persist even after changing the select app.
    /// </summary>
    /// <param name="avapp"></param>
    /// <returns></returns>
    /// <param name="programPath"></param>
    public static LaunchOptions Get(IAvApp? avapp, string? programPath)
    {
        var inst= Get(avapp?.Type);
        inst.ApplicationPath=avapp?.Path ?? string.Empty;
        inst.ProgramPath = programPath;
        return inst;
    }
    [RelayCommand]
    public abstract void Reset();
}
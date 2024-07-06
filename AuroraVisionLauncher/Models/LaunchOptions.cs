using System.Diagnostics;
using AuroraVisionLauncher.Core.Models.Apps;
using CommunityToolkit.Mvvm.ComponentModel;

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
    public static LaunchOptions Get(CommandLineInterface? międzymordzie)
    {
        return (międzymordzie ?? CommandLineInterface.None) switch
        {
            CommandLineInterface.None => _noOptions,
            CommandLineInterface.Studio => _studioOptions,
            CommandLineInterface.Executor => _executorOptions,
            _ => throw new NotImplementedException()
        };
    }
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ArgumentString))]
    private string? _programPath;

    [ObservableProperty]
    private string? _applicationPath;
    public string ArgumentString
    {
        get
        {
            var args = GetCommandLineArgs().Prepend(ApplicationPath!).Select(x => (x ?? "").Contains(' ') ? $"\"{x}\"" : x);
            return string.Join(' ', args);
        }
    }
    /// <summary>
    /// Selects one of the static instances of launch options based on the commandline interface of the given app.
    /// Static instances allow us the setting sto persist even after changing the select app.
    /// </summary>
    /// <param name="avapp"></param>
    /// <returns></returns>
    public static LaunchOptions Get(IAvApp? avapp)
    {
        return Get(avapp?.Interface);
    }
}
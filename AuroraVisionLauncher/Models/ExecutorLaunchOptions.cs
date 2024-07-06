using System.Text.RegularExpressions;
using AuroraVisionLauncher.Core.Models.Apps;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AuroraVisionLauncher.Models;

public partial class ExecutorLaunchOptions : LaunchOptions
{
    private static readonly Regex InvalidArgumentChecker = new Regex(@"[\s'`""]");
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ArgumentString))]
    private bool _autoClose;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ArgumentString))]
    private string? _logPipe;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ArgumentString))]
    private LogLevel _logLevel = LogLevel.Pass;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ArgumentString))]
    private bool _console;
    public override IEnumerable<string> GetCommandLineArgs()
    {
        var args = new List<string>()
        {
            "--program",
            ProgramPath!
        };
        if (Console)
        {
            args.Add("--console");
        }
        if (AutoClose && Console)
        {
            args.Add("--auto-close");
        }
        if (LogLevel != LogLevel.Pass)
        {
            args.Add("--log-level");
            args.Add(LogLevel.ToString().ToLowerInvariant());
        }
        if (!string.IsNullOrWhiteSpace(LogPipe) && !InvalidArgumentChecker.IsMatch(LogPipe.Trim()))
        {
            args.Add("--log-pipe");
            args.Add(LogPipe.Trim());
        }
        return args;
    }
}

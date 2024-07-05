using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Timers;
using AuroraVisionLauncher.Core.Models.Programs;

namespace AuroraVisionLauncher.Core.Models.Apps;
internal record Configuration
{
    public Configuration(ExecutableType executableType, IEnumerable<ProgramType> supportedPrograms)
    {
        ExecutableType = executableType;
        _supportedPrograms = new List<ProgramType>(supportedPrograms);
    }

    public ExecutableType ExecutableType { get; }
    private readonly List<ProgramType> _supportedPrograms;
    public IReadOnlyCollection<ProgramType> SupportedPrograms => _supportedPrograms.AsReadOnly();
}
public record Executable : IExecutable
{
    private static readonly Dictionary<string, Configuration> ExecutableConfigurations = new(StringComparer.OrdinalIgnoreCase)
    {
        {"aurora vision studio",    new Configuration(ExecutableType.Professional,[ProgramType.AdaptiveVisionProject,ProgramType.AuroraVisionProject])},
        {"adaptive vision studio",  new Configuration(ExecutableType.Professional,[ProgramType.AdaptiveVisionProject])},
        {"fabimage studio",         new Configuration(ExecutableType.Professional,[ProgramType.FabImageProject])},
        {"aurora vision executor",  new Configuration(ExecutableType.Runtime,[ProgramType.AdaptiveVisionProject,ProgramType.AuroraVisionProject,ProgramType.AuroraVisionRuntime])},
        {"adaptive vision executor",new Configuration(ExecutableType.Runtime,[ProgramType.AdaptiveVisionProject])},
        {"fabimage runtime",        new Configuration(ExecutableType.Runtime,[ProgramType.FabImageRuntime,ProgramType.FabImageProject])},
    };

    public string ExePath { get; }
    public Version Version { get; }
    public string Name { get; }
    public ExecutableType ExecutableType { get; }
    public bool IsDevelopmentBuild => Version.Build >= 1000;
    readonly private FileVersionInfo _originalInfo;
    private readonly List<ProgramType> _supportedPrograms;
    protected IReadOnlyCollection<ProgramType> SupportedAppTypes => _supportedPrograms.AsReadOnly();
    internal Executable(FileVersionInfo fvinfo, Configuration configuration)
    {
        ExePath = fvinfo.FileName;
        Version = ParseVersion(fvinfo);
        Name = fvinfo.ProductName ?? "N/A";
        _originalInfo = fvinfo;

        _supportedPrograms = new List<ProgramType>(configuration.SupportedPrograms);
        ExecutableType = configuration.ExecutableType;
    }
    /// <summary>
    /// Checks if any process associated with the executable is running.
    /// </summary>
    /// <returns><see langword="true"/> if the process is running; <see langword="false"/> if it is not running, or it could not be checked.</returns>
    public bool CheckIfProcessIsRunning()
    {
        try
        {
            var p = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(_originalInfo.InternalName));

            return p.Any(x => string.Equals(x.MainModule?.FileName, ExePath, StringComparison.OrdinalIgnoreCase));
        }
        catch (InvalidOperationException)
        {
            return false;
        }
    }

    protected static Version ParseVersion(FileVersionInfo fvinfo)
    {
        string? productVersion = fvinfo.ProductVersion;
        if (string.IsNullOrWhiteSpace(productVersion))
        {
            throw new ArgumentException("Empty product version field.");
        }
        if (productVersion.Contains(' '))
        {
            productVersion = productVersion.Split(' ', StringSplitOptions.TrimEntries)[0];
        }
        var ver = Version.Parse(productVersion);
        return ver;
    }
    
    static FileVersionInfo? FindInfo(string folder)

    {
        if (string.IsNullOrWhiteSpace(folder))
        {
            return null;
        }
        var dir = new DirectoryInfo(folder);
        // environment variables for studio proffesional points to AVL libraries, which are in the SDK subfolder
        if (dir.Name.Contains("sdk", StringComparison.OrdinalIgnoreCase))
        {
            dir = dir.Parent!;
        }
        var exes = dir.GetFiles("*.exe");
        FileInfo? theExe = null;
        if (folder.Contains("professional", StringComparison.OrdinalIgnoreCase))
        {
            theExe = exes.FirstOrDefault(x => x.Name.Contains("studio", StringComparison.OrdinalIgnoreCase));
        }
        else if (folder.Contains("runtime", StringComparison.OrdinalIgnoreCase))
        {
            theExe = exes.FirstOrDefault(x => x.Name.Contains("executor", StringComparison.OrdinalIgnoreCase));
        }
        if (theExe is null)
        {
            return null;
        }
        return FileVersionInfo.GetVersionInfo(theExe.FullName);
    }
    public static Executable Create(FileVersionInfo fvinfo)
    {
        if (ExecutableConfigurations.TryGetValue(fvinfo.ProductName ?? "", out Configuration? configuration))
        {
            return new Executable(fvinfo, configuration);

        }
        throw new InvalidOperationException($"Unsupported product type: {fvinfo.ProductName}");
    }

    public static bool TryCreate(string folder, [NotNullWhen(true)] out Executable? app)
    {
        if (FindInfo(folder) is not FileVersionInfo fileVersionInfo)
        {
            app = null;
            return false;
        }
        app = Create(fileVersionInfo);
        return true;
    }

    public bool SupportsProgram(ProgramInformation information)
    {
        return SupportedAppTypes.Contains(information.ProgramType);
    }
    public static int GetClosestApp(IEnumerable<IExecutable> executables, VisionProgram info)
    {
        var weights = new List<double>();
        foreach (IExecutable executable in executables)
        {
            double weight = 0;
            bool isProgramRuntime = info.Type == ProgramType.AuroraVisionRuntime || info.Type == ProgramType.FabImageRuntime;
            if (isProgramRuntime && executable.ExecutableType!=ExecutableType.Runtime || !isProgramRuntime && executable.ExecutableType == ExecutableType.Runtime)
            {
                weight = -1e21;
            }
            double programVersionTransformed = info.Version.Major * 1e12 + info.Version.Minor * 1e9 + info.Version.Build * 1e6 + info.Version.Revision;
            double executableVersionTransformed = executable.Version.Major * 1e12 + executable.Version.Minor * 1e9 + executable.Version.Build * 1e6 + executable.Version.Revision;
            weight += executableVersionTransformed - programVersionTransformed;
            weights.Add(Math.Abs(weight));
        }
        if (weights.Count == 0)
        {
            return -1;
        }
        var min = weights.Min();
        return weights.IndexOf(min);
    }
    protected string ShortForm() => $"{Name} {Version}";
    public override string ToString() => ShortForm();
}
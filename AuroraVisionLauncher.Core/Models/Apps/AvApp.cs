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
    public Configuration(AvAppType appType, IEnumerable<ProgramType> supportedPrograms)
    {
        AppType = appType;
        _supportedPrograms = new List<ProgramType>(supportedPrograms);
    }

    public AvAppType AppType { get; }
    private readonly List<ProgramType> _supportedPrograms;
    public IReadOnlyCollection<ProgramType> SupportedPrograms => _supportedPrograms.AsReadOnly();
}
internal record MultiVersion(FileVersionInfo Primary, FileVersionInfo? Secondary)
{
    public static MultiVersion? Create(FileInfo? primary, FileInfo? secondary = null)
    {
        if (primary is null)
        {
            return null;
        }
        var ver1 = FileVersionInfo.GetVersionInfo(primary.FullName);
        var ver2 = secondary is null ? null : FileVersionInfo.GetVersionInfo(secondary.FullName);
        return new MultiVersion(ver1, ver2);
    }
}
public record AvApp : IAvApp
{
    private static readonly Dictionary<string, Configuration> AvAppConfigurations = new(StringComparer.OrdinalIgnoreCase)
    {
        {"Aurora Vision Studio",    new Configuration(AvAppType.Professional,[ProgramType.AdaptiveVisionProject,ProgramType.AuroraVisionProject])},
        {"Adaptive Vision Studio",  new Configuration(AvAppType.Professional,[ProgramType.AdaptiveVisionProject])},
        {"FabImage Studio",         new Configuration(AvAppType.Professional,[ProgramType.FabImageProject])},
        {"Aurora Vision Executor",  new Configuration(AvAppType.Runtime,[ProgramType.AdaptiveVisionProject,ProgramType.AuroraVisionProject,ProgramType.AuroraVisionRuntime])},
        {"Adaptive Vision Executor",new Configuration(AvAppType.Runtime,[ProgramType.AdaptiveVisionProject])},
        {"FabImage Runtime",        new Configuration(AvAppType.Runtime,[ProgramType.FabImageRuntime,ProgramType.FabImageProject])},
        {"Deep Learning Editor",    new Configuration(AvAppType.DeepLearning,[])},
    };

    public string ExePath { get; }
    public Version Version { get; }
    public Version? SecondaryVersion { get; }
    public string Name { get; }
    public AvAppType AppType { get; }
    public bool IsDevelopmentBuild => Version.Build >= 1000;
    readonly private FileVersionInfo _originalInfo;
    private readonly List<ProgramType> _supportedPrograms;
    protected IReadOnlyCollection<ProgramType> SupportedProgramTypes => _supportedPrograms.AsReadOnly();
    internal AvApp(MultiVersion mvinfo, Configuration configuration)
    {
        ExePath = mvinfo.Primary.FileName;
        Version = ParseVersion(mvinfo.Primary);
        SecondaryVersion = mvinfo.Secondary is not null ? ParseVersion(mvinfo.Secondary) : null;
        Name = mvinfo.Primary.ProductName ?? "N/A";
        _originalInfo = mvinfo.Primary;

        _supportedPrograms = new List<ProgramType>(configuration.SupportedPrograms);
        AppType = configuration.AppType;
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

    static MultiVersion? FindInfo(string folder)

    {
        if (string.IsNullOrWhiteSpace(folder))
        {
            return null;
        }
        var dir = new DirectoryInfo(folder);

        if (dir.Name.Contains("sdk", StringComparison.OrdinalIgnoreCase))
        {
            // environment variables for studio proffesional points to AVL libraries, which are in the SDK subfolder
            dir = dir.Parent!;
        }
        else if (dir.Parent.Name.Contains("Deep Learning", StringComparison.OrdinalIgnoreCase))
        {
            // deep learning also points to library, so 1 step back and the go for deeplearning editor
            dir = new DirectoryInfo(Path.Join(dir.Parent!.FullName, "Tools", "DeepLearningEditor"));
        }

        var exes = dir.GetFiles("*.exe");

        if (folder.Contains("Professional", StringComparison.OrdinalIgnoreCase))
        {
            var primary = exes.FirstOrDefault(x => x.Name.Contains("Studio", StringComparison.OrdinalIgnoreCase));
            return MultiVersion.Create(primary);
        }
        if (folder.Contains("Runtime", StringComparison.OrdinalIgnoreCase))
        {
            var primary = exes.FirstOrDefault(x => x.Name.Contains("Executor", StringComparison.OrdinalIgnoreCase));
            return MultiVersion.Create(primary);
        }
        if (folder.Contains("Deep Learning", StringComparison.OrdinalIgnoreCase))
        {
            // deep learning editor
            var primary = exes.FirstOrDefault(x => x.Name.Contains("Editor", StringComparison.OrdinalIgnoreCase));
            // uninstaller - it has the version of DL listed on the site (what hack?)
            var secondary = dir.Parent!.Parent!.GetFiles("*.exe").FirstOrDefault(x => x.Name.Contains("unins", StringComparison.OrdinalIgnoreCase));
            return MultiVersion.Create(primary, secondary);
        }
        return null;
    }
    private static AvApp Create(MultiVersion mvinfo)
    {
        if (AvAppConfigurations.TryGetValue(mvinfo.Primary.ProductName ?? "", out Configuration? configuration))
        {
            return new AvApp(mvinfo, configuration);

        }
        throw new InvalidOperationException($"Unsupported product type: {mvinfo.Primary.ProductName}");
    }

    public static bool TryCreate(string folder, [NotNullWhen(true)] out AvApp? app)
    {
        if (FindInfo(folder) is not MultiVersion fileVersionInfo)
        {
            app = null;
            return false;
        }
        app = Create(fileVersionInfo);
        return true;
    }

    public bool SupportsProgram(ProgramInformation information)
    {
        return SupportedProgramTypes.Contains(information.ProgramType);
    }
    public static int GetClosestApp(IEnumerable<IAvApp> apps, VisionProgram info)
    {
        var weights = new List<double>();
        foreach (IAvApp app in apps)
        {
            double weight = 0;
            bool isProgramRuntime = info.Type == ProgramType.AuroraVisionRuntime || info.Type == ProgramType.FabImageRuntime;
            if (isProgramRuntime && app.AppType != AvAppType.Runtime || !isProgramRuntime && app.AppType == AvAppType.Runtime)
            {
                weight = -1e21;
            }
            double programVersionTransformed = info.Version.Major * 1e12 + info.Version.Minor * 1e9 + info.Version.Build * 1e6 + info.Version.Revision;
            double executableVersionTransformed = app.Version.Major * 1e12 + app.Version.Minor * 1e9 + app.Version.Build * 1e6 + app.Version.Revision;
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
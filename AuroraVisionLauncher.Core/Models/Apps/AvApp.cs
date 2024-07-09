using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Timers;
using AuroraVisionLauncher.Core.Models.Programs;

namespace AuroraVisionLauncher.Core.Models.Apps;
public record AvApp : IAvApp
{
    private static readonly Dictionary<string, Configuration> AvAppConfigurations = new(StringComparer.OrdinalIgnoreCase)
    {
        {"Aurora Vision Studio",    new Configuration(AvAppType.Professional,CommandLineInterface.Studio, [ProgramType.AdaptiveVisionProject,ProgramType.AuroraVisionProject])},
        {"Adaptive Vision Studio",  new Configuration(AvAppType.Professional,CommandLineInterface.Studio, [ProgramType.AdaptiveVisionProject])},
        {"FabImage Studio",         new Configuration(AvAppType.Professional,CommandLineInterface.Studio, [ProgramType.FabImageProject])},
        {"Aurora Vision Executor",  new Configuration(AvAppType.Runtime,CommandLineInterface.Executor, [ProgramType.AdaptiveVisionProject,ProgramType.AuroraVisionProject,ProgramType.AuroraVisionRuntime])},
        {"Adaptive Vision Executor",new Configuration(AvAppType.Runtime,CommandLineInterface.Executor, [ProgramType.AdaptiveVisionProject])},
        {"FabImage Runtime",        new Configuration(AvAppType.Runtime,CommandLineInterface.Executor, [ProgramType.FabImageRuntime,ProgramType.FabImageProject])},
        {"Deep Learning Editor",    new Configuration(AvAppType.DeepLearning,CommandLineInterface.None, [])},
    };

    public string ExePath { get; }
    public Version Version { get; }
    public Version? SecondaryVersion { get; }
    public string Name { get; }
    public AvAppType AppType { get; }
    public bool IsDevelopmentBuild => Version.Build >= 1000;
    readonly private FileVersionInfo _originalInfo;
    private readonly List<ProgramType> _supportedPrograms;
    public CommandLineInterface Interface { get; }
    protected IReadOnlyCollection<ProgramType> SupportedProgramTypes => _supportedPrograms.AsReadOnly();
    internal AvApp(MultiVersion mvinfo, Configuration configuration)
    {
        ExePath = mvinfo.Primary.FileName;
        Version = ParseVersion(mvinfo.Primary) ?? throw new VersionNotFoundException("The ProductVersion field is empty");
        SecondaryVersion = mvinfo.Secondary is not null ? ParseVersion(mvinfo.Secondary) : null;
        Name = mvinfo.Primary.ProductName ?? "N/A";
        _originalInfo = mvinfo.Primary;

        _supportedPrograms = new List<ProgramType>(configuration.SupportedPrograms);
        AppType = configuration.AppType;
        Interface = configuration.Interface;
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

    protected static Version? ParseVersion(FileVersionInfo fvinfo)
    {
        string? productVersion = fvinfo.ProductVersion;
        if (string.IsNullOrWhiteSpace(productVersion))
        {
            return null;
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
        else if (dir.Parent!.Name.Contains("Deep Learning", StringComparison.OrdinalIgnoreCase))
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

    public bool CanOpen(ProgramType type)
    {
        return SupportedProgramTypes.Contains(type);
    }
    private static double VersionToDouble(Version version, Version baseVersion)
    {
        var balance = 1.0d;
        // revision doesnt really matter, started with so it will never move it below 0
        // in fact the score should never be exactly zero
        balance += (version.Revision - baseVersion.Revision) / 100_000;
        // different builds denotes changes in the API, so runtime cannot be lauched for example
        balance += (version.Build - baseVersion.Build) * 10;
        // minor indicate some larger changes, but noting too ground breaking
        balance += (version.Minor - baseVersion.Minor) * 10_000;
        // hoo boy
        balance += (version.Major - baseVersion.Major) * 1_000_000;
        /*
        the goal is to find a version that gets the value closes to 0
        negative values mean the app is outdated, but may still be able to load the program
        positive values mena the app is from the future
        if we have 5.4.5.10000 and 5.3.1.01247 installed and the program is 5.2.7.23347 then we want to select the app that has the least changes,
        so is the closest to zero.

        In case we have outdated versions only, we still want to select the one closest to 0
        */

        return balance;
    }
    public static int GetClosestApp(IEnumerable<IAvApp> apps, IVisionProgram info)
    {
        var weights = new List<double>();
        bool hasPositive = false;
        bool hasNegative = false;
        foreach (IAvApp app in apps)
        {
            var score = VersionToDouble(app.Version, info.Version);
            if (!app.CanOpen(info.Type))
            {
                score = double.MinValue;
            }
            hasPositive |= score > 0;
            hasNegative |= score < 0;
            weights.Add(score);
        }
        if (weights.Count == 0 || !(hasPositive || hasNegative))
        {
            return -1;
        }
        if (hasPositive)
        {
            // negative values are set to the highest possible value, so they will not be considered
            var min = weights.Min(x => x < 0 ? double.MaxValue : x);
            return weights.IndexOf(min);
        }
        // only negatives 
        var max = weights.Max();
        if (max == double.MinValue)
        {
            return -1;
        }
        return weights.IndexOf(max);
    }
    protected string ShortForm() => $"{Name} {Version}";
    public override string ToString() => ShortForm();
}
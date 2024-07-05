using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Timers;
using AuroraVisionLauncher.Core.Models.Apps;

namespace AuroraVisionLauncher.Core.Models.Apps;

public abstract record Executable : IExecutable
{
    public string ExePath { get; }
    public Version Version { get; }
    public string Name { get; }
    public bool IsDevelopmentBuild => Version.Build >= 1000;
    readonly private FileVersionInfo _originalInfo;
    protected abstract ReadOnlyCollection<ProgramType> SupportedAppTypes { get; }
    protected Executable(FileVersionInfo fvinfo)
    {
        ExePath = fvinfo.FileName;
        Version = ParseVersion(fvinfo);
        Name = fvinfo.ProductName ?? "N/A";
        _originalInfo = fvinfo;

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

            return p.Any(x => string.Equals(x.MainModule.FileName, ExePath, StringComparison.OrdinalIgnoreCase));
        }
        catch (InvalidOperationException)
        {
            return false;
        }
    }

    protected static Version ParseVersion(FileVersionInfo fvinfo)
    {
        string productVersion = fvinfo.ProductVersion;
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
    protected Executable()
    {
        ExePath = "";
        Version = new Version();
        Name = " N/A";
    }
    static FileVersionInfo FindInfo(string folder)

    {
        if (string.IsNullOrWhiteSpace(folder))
        {
            return null;
        }
        var dir = new DirectoryInfo(folder);
        if (dir.Name.Contains("sdk", StringComparison.OrdinalIgnoreCase))
        {
            dir = dir.Parent!;
        }
        var exes = dir.GetFiles("*.exe");
        FileInfo theExe = null;
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
        return fvinfo.ProductName!.ToLowerInvariant() switch
        {
            "aurora vision studio" => new AuroraStudioExecutable(fvinfo),
            "adaptive vision studio" => new AdaptiveStudioExecutable(fvinfo),
            "fabimage studio" => new FabStudioExecutable(fvinfo),
            "aurora vision executor" => new AuroraStudioExecutable(fvinfo),
            "adaptive vision executor" => new AdaptiveRuntimeExecutable(fvinfo),
            "fabimage runtime" => new FabRuntimeExecutable(fvinfo),
            _ => throw new InvalidOperationException($"Unsupported product type: {fvinfo.ProductName}")
        };
    }

    public static bool TryCreate(string folder, [NotNullWhen(true)] out Executable app)
    {
        if (FindInfo(folder) is not FileVersionInfo fileVersionInfo)
        {
            app = null;
            return false;
        }
        app = Create(fileVersionInfo);
        return true;
    }

    public bool SupportsAvFile(AvFileInformation information)
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
            if (isProgramRuntime && executable is not RuntimeExecutable || !isProgramRuntime && executable is RuntimeExecutable)
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
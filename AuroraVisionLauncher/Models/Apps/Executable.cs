using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

namespace AuroraVisionLauncher.Models.Apps;

public enum Compatibility
{
    Unknown,
    Outdated,
    Supported,
}
public abstract record Executable
{
    public string ExePath { get; }
    public Version Version { get; }
    public string Name { get; }
    public Compatibility Compatibility { get; private set; } = Compatibility.Unknown;
    protected abstract ReadOnlyCollection<ProgramType> SupportedAppTypes { get; }
    protected Executable(FileVersionInfo fvinfo)
    {
        ExePath = fvinfo.FileName;
        Version = new Version(fvinfo.ProductVersion ?? "0.0.0.0");
        Name = fvinfo.ProductName ?? "N/A";
    }
    protected Executable()
    {
        ExePath = "";
        Version = new Version();
        Name = " N/A";
    }
    static FileVersionInfo? FindInfo(string folder)

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

    public Executable WithCompatibility(AvFileInformation avinfo)
    {
        Compatibility comp;
        if (avinfo.ProgramType == ProgramType.FabImageRuntime || avinfo.ProgramType == ProgramType.AuroraVisionRuntime)
        {
            comp = Compatibility.Unknown;
        }
        else
        {
            if (avinfo.Version > Version)
            {
                comp = Compatibility.Outdated;
            }
            else
            {
                comp = Compatibility.Supported;
            }
        }
        return this with { Compatibility = comp };
    }
    public bool SupportsAvFile(AvFileInformation information)
    {
        return SupportedAppTypes.Contains(information.ProgramType);
    }
    public static int GetClosestApp(IList<Executable> executables, VisionProgram info)
    {
        if (executables.Count == 0)
        {
            return -1;
        }
        List<double> weights= new List<double>(executables.Count);
        foreach (Executable executable in executables)
        {
            double weight =0;
            bool isProgramRuntime =info.Type==ProgramType.AuroraVisionRuntime|| info.Type==ProgramType.FabImageRuntime;
            if ((isProgramRuntime && executable is not RuntimeExecutable) || (!isProgramRuntime && executable is RuntimeExecutable))
            {
                weight = -1e21;
            }
            double programVersionTransformed = info.Version.Major*1e12 + info.Version.Minor*1e9 + info.Version.Build*1e6 +info.Version.Revision;
            double executableVersionTransformed = executable.Version.Major*1e12 + executable.Version.Minor*1e9 + executable.Version.Build*1e6 +executable.Version.Revision;
            weight += executableVersionTransformed - programVersionTransformed;
            weights.Add(Math.Abs(weight));
        }
        var min = weights.Min();
        return weights.IndexOf(min);
    }
    protected string ShortForm() => $"{Name} {Version}";
    public override string ToString() => ShortForm();
}
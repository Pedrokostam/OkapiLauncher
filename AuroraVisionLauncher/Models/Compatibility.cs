using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AuroraVisionLauncher.Core.Models;
using AuroraVisionLauncher.Core.Models.Apps;
using AuroraVisionLauncher.Core.Models.Projects;

namespace AuroraVisionLauncher.Models;
public class Compatibility
{
    private static readonly Compatibility Incompatible = new("Incompatible", "These applications cannot open the program.");
    private static readonly Compatibility Compatible = new("Supported", "These applications can open the program.");
    private static readonly Compatibility Unknown = new("Unknown", "Cannot determine compatibility due to unknown program version.");
    private static readonly Compatibility Outdated = new("Outdated", "These applications may be able to open the program, but there may be some issues.");
    private Compatibility(string category, string description)
    {
        this.Category = category;
        this.Description = description;
    }

    public string Category { get; }
    public string Description { get; }

    public static Compatibility Get(IAvApp app, IVisionProject program)
    {
        if (!app.CanOpen(program))
        {
            return Incompatible;
        }
        if (program.Version.IsUnknown)
        {
            return Unknown;
        }
        if (program.Type == ProductType.Runtime)
        {
            if (app.Version.Major == program.Version.Major && app.Version.Minor == program.Version.Minor && app.Version.Build == program.Version.Build)
            {
                return Compatible;
            }
            return Incompatible;
        }
        if (app.Version.CompareTo(program.Version) >= 0)
        {
            return Compatible;
        }
        return Outdated;
    }
}

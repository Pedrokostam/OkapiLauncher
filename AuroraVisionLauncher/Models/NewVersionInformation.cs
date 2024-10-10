using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuroraVisionLauncher.Models;
public record NewVersionInformation(DateTime ReleaseDate, string Version, string Title, bool IsAutomaticCheck)
{
    public enum Decision
    {
        Cancel,
        SkipVersion,
        OpenPage,
        LaunchUpdater
    }
    public bool DisableAutomaticUpdates { get; set; }
    public Decision UserDecision { get; set; }
}

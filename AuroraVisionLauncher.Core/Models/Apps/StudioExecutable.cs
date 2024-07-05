using System.Diagnostics;

namespace AuroraVisionLauncher.Core.Models.Apps;

public abstract record StudioExecutable : Executable
{
    protected StudioExecutable(FileVersionInfo fvinfo) : base(fvinfo)
    {

    }
}

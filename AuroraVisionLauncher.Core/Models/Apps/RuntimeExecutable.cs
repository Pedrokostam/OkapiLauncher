using System.Diagnostics;

namespace AuroraVisionLauncher.Core.Models.Apps;

public abstract record RuntimeExecutable : Executable
{
    protected RuntimeExecutable(FileVersionInfo fvinfo) : base(fvinfo)
    {

    }
}

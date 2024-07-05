using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace AuroraVisionLauncher.Core.Models.Apps;

public abstract record RuntimeExecutable : Executable
{
    protected RuntimeExecutable(FileVersionInfo fvinfo) : base(fvinfo)
    {

    }
}
public record AuroraRuntimeExecutable : RuntimeExecutable
{
    private static readonly List<ProgramType> _programTypes = new List<ProgramType>
    {
      ProgramType.AdaptiveVisionProject,
      ProgramType.AuroraVisionProject,
     ProgramType.AuroraVisionRuntime,
    };
    public AuroraRuntimeExecutable(FileVersionInfo fvinfo) : base(fvinfo)
    {
    }
    public override string ToString() => ShortForm();
    protected override ReadOnlyCollection<ProgramType> SupportedAppTypes => _programTypes.AsReadOnly();
}

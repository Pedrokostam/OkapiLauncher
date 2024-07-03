using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace AuroraVisionLauncher.Models.Apps;

public record AdaptiveRuntimeExecutable : Executable
{
    private static readonly List<ProgramType> _programTypes = new List<ProgramType>
    {
       ProgramType.AdaptiveVisionProject,
       ProgramType.AuroraVisionRuntime
    };
    public AdaptiveRuntimeExecutable(FileVersionInfo fvinfo) : base(fvinfo)
    {
    }
    public override string ToString() => ShortForm();
    protected override ReadOnlyCollection<ProgramType> SupportedAppTypes => _programTypes.AsReadOnly();
}

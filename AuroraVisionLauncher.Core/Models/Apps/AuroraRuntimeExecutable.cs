using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace AuroraVisionLauncher.Core.Models.Apps;
public record AuroraRuntimeExecutable : RuntimeExecutable
{
    private static readonly List<ProgramType> _programTypes =
    [
        ProgramType.AdaptiveVisionProject,
        ProgramType.AuroraVisionProject,
        ProgramType.AuroraVisionRuntime,
    ];
    public AuroraRuntimeExecutable(FileVersionInfo fvinfo) : base(fvinfo)
    {
    }
    public override string ToString() => ShortForm();
    protected override ReadOnlyCollection<ProgramType> SupportedAppTypes => _programTypes.AsReadOnly();
}

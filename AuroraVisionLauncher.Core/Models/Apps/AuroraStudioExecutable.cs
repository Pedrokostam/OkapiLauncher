using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace AuroraVisionLauncher.Core.Models.Apps;
public record AuroraStudioExecutable : StudioExecutable
{
    private static readonly List<ProgramType> _programTypes =
    [
       ProgramType.AuroraVisionProject,
       ProgramType.AdaptiveVisionProject,
    ];
    public AuroraStudioExecutable(FileVersionInfo fvinfo) : base(fvinfo)
    {
    }

    public override string ToString() => ShortForm();
    protected override ReadOnlyCollection<ProgramType> SupportedAppTypes => _programTypes.AsReadOnly();
}

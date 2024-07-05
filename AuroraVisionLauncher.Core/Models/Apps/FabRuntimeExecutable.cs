using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace AuroraVisionLauncher.Core.Models.Apps;

public record FabRuntimeExecutable : RuntimeExecutable
{
    private static readonly List<ProgramType> _programTypes =
    [
        ProgramType.FabImageProject,
        ProgramType.FabImageRuntime,
    ];
    public FabRuntimeExecutable(FileVersionInfo fvinfo) : base(fvinfo)
    {
    }
    public override string ToString() => ShortForm();
    protected override ReadOnlyCollection<ProgramType> SupportedAppTypes => _programTypes.AsReadOnly();
}
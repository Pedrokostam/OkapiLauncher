﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace AuroraVisionLauncher.Models.Apps;

public record AdaptiveStudioExecutable : StudioExecutable
{
    private static readonly List<ProgramType> _programTypes = new List<ProgramType>
    {
       ProgramType.AdaptiveVisionProject
    };
    public AdaptiveStudioExecutable(FileVersionInfo fvinfo) : base(fvinfo)
    {
    }

    public override string ToString() => ShortForm();
    protected override ReadOnlyCollection<ProgramType> SupportedAppTypes => _programTypes.AsReadOnly();
}

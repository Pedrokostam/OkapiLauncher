using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace AuroraVisionLauncher.Core.Models.Programs;
public static class ProgramExtensions
{
    private static readonly ProgramType[] StudioTypes = [ProgramType.AuroraVisionProject, ProgramType.AdaptiveVisionProject, ProgramType.FabImageProject];
    private static readonly ProgramType[] RuntimeTypes = [ProgramType.AuroraVisionRuntime, ProgramType.FabImageRuntime];
    public static bool IsStudio(this ProgramType programType)
    {
        return StudioTypes.Contains(programType);
    }
    public static bool IsStudio(this IVisionProgram program) => program.Type.IsStudio();
    public static bool IsRuntime(this ProgramType programType)
    {
        return RuntimeTypes.Contains(programType);
    }
    public static bool IsRuntime(this IVisionProgram program) => program.Type.IsRuntime();
}

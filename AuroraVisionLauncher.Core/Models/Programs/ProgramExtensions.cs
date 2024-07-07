using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace AuroraVisionLauncher.Core.Models.Programs;
public static class ProgramExtensions
{
    public static bool IsStudio(this ProgramType programType)
    {
        return programType == ProgramType.AuroraVisionProject
            || programType == ProgramType.AdaptiveVisionProject
            || programType == ProgramType.FabImageProject;
    }
    public static bool IsStudio(this IVisionProgram program)=>program.Type.IsStudio();
    public static bool IsRuntime(this ProgramType programType)
    {
        return programType == ProgramType.AuroraVisionRuntime
            || programType == ProgramType.FabImageRuntime;
    }
    public static bool IsRuntime(this IVisionProgram program)=>program.Type.IsRuntime();
}

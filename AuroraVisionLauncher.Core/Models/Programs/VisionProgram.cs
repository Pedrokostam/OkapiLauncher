using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuroraVisionLauncher.Core.Models.Programs
{
    public record VisionProgram(string Name, Version Version, string Path, ProgramType Type) : IVisionProgram
    {
        public VisionProgram() : this("No program selected", new Version(), "No program selected", ProgramType.None)
        {

        }
        public bool Exists => File.Exists(Path);
    }
}

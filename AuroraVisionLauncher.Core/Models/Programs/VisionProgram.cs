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
        public static readonly Version MissingVersion = new Version(0, 0, 0, 0);
        public VisionProgram() : this("No program selected", MissingVersion, "No program selected", ProgramType.None)
        {

        }
        public bool Exists => File.Exists(Path);
    }
}

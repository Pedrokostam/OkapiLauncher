using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuroraVisionLauncher.Core.Models.Programs
{
    public record VisionProgram(string Name, AvVersion Version, string Path, ProgramType Type) : IVisionProgram
    {
        public VisionProgram() : this("No program selected", AvVersion.MissingVersion, "No program selected", ProgramType.None)
        {

        }
        public bool Exists => File.Exists(Path);

        IAvVersion IVisionProgram.Version => Version;
    }
}

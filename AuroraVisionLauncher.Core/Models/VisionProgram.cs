using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AuroraVisionLauncher.Core.Models.Apps;

namespace AuroraVisionLauncher.Core.Models
{
    public record VisionProgram(string Name, Version Version, string Path, ProgramType Type)
    {
        public VisionProgram() : this("No program selected", new Version(), "No program selected", ProgramType.None)
        {

        }
        public bool Exists=>File.Exists(Path);
    }
}

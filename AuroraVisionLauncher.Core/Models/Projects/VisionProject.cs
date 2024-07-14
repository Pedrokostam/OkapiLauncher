using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuroraVisionLauncher.Core.Models.Projects
{
    public record VisionProject : IVisionProject
    {
        public string Name { get; }
        public AvVersion Version { get; }
        public ProductBrand Brand { get; }
        public string Path { get; }
        public ProductType Type { get; }
        internal VisionProject(string name, AvVersion version, ProductBrand brand, string path, ProductType type)
        {
            Name = name;
            Version = version;
            Brand = brand;
            Path = path;
            Type = type;
        }
        public bool Exists => File.Exists(Path);

        IAvVersion IProduct.Version =>Version;

      
    }
}

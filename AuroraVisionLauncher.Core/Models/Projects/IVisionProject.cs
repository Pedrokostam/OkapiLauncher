
namespace AuroraVisionLauncher.Core.Models.Projects;

public interface IVisionProject : IProduct
{
    bool Exists { get; }
    string Name { get; }
    string Path { get; }
}
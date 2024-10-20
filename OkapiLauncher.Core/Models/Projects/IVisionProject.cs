

namespace OkapiLauncher.Core.Models.Projects;

public interface IVisionProject : IProduct
{
    bool Exists { get; }
    string Name { get; }
    string Path { get; }
    DateTime DateModified { get; }
}
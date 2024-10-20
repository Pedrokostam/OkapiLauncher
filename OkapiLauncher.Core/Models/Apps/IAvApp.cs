using AuroraVisionLauncher.Core.Models.Projects;

namespace AuroraVisionLauncher.Core.Models.Apps;

public interface IAvApp : IProduct, IComparable<IAvApp>
{
    string Path { get; }
    string RootPath { get; }
    bool IsDevelopmentVersion { get; }
    string Name { get; }
    IAvVersion? SecondaryVersion { get; }
    CommandLineInterface Interface { get; }
    string ProcessName { get; }
    bool CanOpen(IVisionProject type);
    bool IsNativeApp(IVisionProject type);
    public bool IsExecutable { get; }
    string? Description { get; }
    bool IsCustom { get; }
}
using AuroraVisionLauncher.Core.Models.Programs;

namespace AuroraVisionLauncher.Core.Models.Apps;

public interface IAvApp
{
    string ExePath { get; }
    bool IsDevelopmentVersion { get; }
    string Name { get; }
    IAvVersion Version { get; }
    AvAppType AppType { get; }
    IAvVersion? SecondaryVersion { get; }
    CommandLineInterface Interface { get; }
    string InternalName { get; }
    bool CanOpen(ProgramType type);
    bool IsNativeApp(ProgramType type);
}
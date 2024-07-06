using AuroraVisionLauncher.Core.Models.Programs;

namespace AuroraVisionLauncher.Core.Models.Apps;

public interface IAvApp
{
    string ExePath { get; }
    bool IsDevelopmentBuild { get; }
    string Name { get; }
    Version Version { get; }
    AvAppType AppType { get; }
    Version? SecondaryVersion { get; }
    CommandLineInterface Interface { get; }

    bool CheckIfProcessIsRunning();
    bool SupportsProgram(ProgramInformation information);
}
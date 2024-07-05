using AuroraVisionLauncher.Core.Models.Programs;

namespace AuroraVisionLauncher.Core.Models.Apps;

public interface IExecutable
{
    string ExePath { get; }
    bool IsDevelopmentBuild { get; }
    string Name { get; }
    Version Version { get; }
    ExecutableType ExecutableType { get; }

    bool CheckIfProcessIsRunning();
    bool SupportsProgram(ProgramInformation information);
}
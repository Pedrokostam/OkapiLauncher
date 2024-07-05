
namespace AuroraVisionLauncher.Core.Models.Apps;

public interface IExecutable
{
    string ExePath { get; }
    bool IsDevelopmentBuild { get; }
    string Name { get; }
    Version Version { get; }

    bool CheckIfProcessIsRunning();
    bool SupportsAvFile(AvFileInformation information);
}
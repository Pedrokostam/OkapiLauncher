
namespace AuroraVisionLauncher.Core.Models.Programs;

public interface IVisionProgram
{
    bool Exists { get; }
    string Name { get;}
    string Path { get; }
    ProgramType Type { get; }
    Version Version { get; }
}
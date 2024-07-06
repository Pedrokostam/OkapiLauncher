using AuroraVisionLauncher.Core.Models.Programs;

namespace AuroraVisionLauncher.Core.Models.Apps;

internal record Configuration
{
    public Configuration(AvAppType appType, CommandLineInterface międzymordzie, IEnumerable<ProgramType> supportedPrograms)
    {
        AppType = appType;
        Interface = międzymordzie;
        _supportedPrograms = new List<ProgramType>(supportedPrograms);
    }

    public AvAppType AppType { get; }
    public CommandLineInterface Interface { get; }

    private readonly List<ProgramType> _supportedPrograms;
    public IReadOnlyCollection<ProgramType> SupportedPrograms => _supportedPrograms.AsReadOnly();
}

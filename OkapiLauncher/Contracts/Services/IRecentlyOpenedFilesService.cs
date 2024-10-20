using AuroraVisionLauncher.Models;

namespace AuroraVisionLauncher.Contracts.Services;
public interface IRecentlyOpenedFilesService
{
    string? LastOpenedFile { get; }

    void AddLastFile(string file);
    IEnumerable<RecentlyOpenedFileFacade> GetLastOpenedFiles();
}
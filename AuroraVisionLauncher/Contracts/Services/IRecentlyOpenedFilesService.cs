using AuroraVisionLauncher.Models;

namespace AuroraVisionLauncher.Contracts.Services;
public interface IRecentlyOpenedFilesService
{
    void AddLastFile(string file);
    IEnumerable<RecentlyOpenedFileFacade> GetLastOpenedFiles();
}
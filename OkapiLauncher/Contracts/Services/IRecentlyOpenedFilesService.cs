using OkapiLauncher.Models;

namespace OkapiLauncher.Contracts.Services;
public interface IRecentlyOpenedFilesService
{
    string? LastOpenedFile { get; }

    void AddLastFile(string file);
    IEnumerable<RecentlyOpenedFileFacade> GetLastOpenedFiles();
}
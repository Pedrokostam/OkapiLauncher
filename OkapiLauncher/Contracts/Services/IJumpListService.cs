using OkapiLauncher.Core.Models.Apps;

namespace OkapiLauncher.Contracts.Services;
public interface IJumpListService
{
    void AddFrequentItem(string itemPath);
    void AddRecentItem(string itemPath);
    void SetTasks(IEnumerable<AvApp> tasks);
}
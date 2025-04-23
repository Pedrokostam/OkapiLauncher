using OkapiLauncher.Core.Models.Apps;
using OkapiLauncher.Models;

namespace OkapiLauncher.Contracts.Services;
public interface IJumpListService
{
    void SetTasks(IEnumerable<AvApp> tasks);
}
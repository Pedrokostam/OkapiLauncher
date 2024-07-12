using System.Windows.Threading;
using AuroraVisionLauncher.Models;

namespace AuroraVisionLauncher.Services;
public interface IProcessManagerService
{

    IList<SimpleProcess> GetActiveProcesses(AvAppFacade app);
    void UpdateProcessActive(IList<AvAppFacade> apps);
}
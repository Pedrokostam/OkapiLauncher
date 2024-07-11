using System.Windows.Threading;
using AuroraVisionLauncher.Models;

namespace AuroraVisionLauncher.Services;
public interface IProcessManagerService
{
    /// <summary>
    /// Create a timer which will call <see cref="UpdateProcessActive(IList{AvAppFacade})">UpdateProcessActive</see> periodically, calls <see cref="UpdateProcessActive(IList{AvAppFacade})">UpdateProcessActive</see> once, starts the timer, returns it.
    /// </summary>
    /// <param name="apps">Reference to collection that should be checked</param>
    /// <returns>Started timer</returns>
    DispatcherTimer CreateTimer(IList<AvAppFacade> apps);
    IList<SimpleProcess> GetActiveProcesses(AvAppFacade app);
    void UpdateProcessActive(IList<AvAppFacade> apps);
}
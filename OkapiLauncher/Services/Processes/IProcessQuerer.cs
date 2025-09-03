using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OkapiLauncher.Core.Models.Apps;
using OkapiLauncher.Models.Messages;

namespace OkapiLauncher.Services.Processes;
public interface IProcessQuerer
{
    TimeSpan TimerPeriod { get; }

    /// <summary>
    /// Checks running processes for all the given apps.
    /// </summary>
    /// <param name="apps"></param>
    /// <exception cref="ProcessException"/>
    /// <returns><see cref="FreshAppProcesses"/> if the processes where queried, <see langword="null"/> if they weren't or there were no chang</returns>
    FreshAppProcesses? GetProcesses(IEnumerable<IAvApp> apps);
    /// <summary>
    /// Checks running processes for a single app.
    /// </summary>
    /// <param name="app"></param>
    /// <exception cref="ProcessException"/>
    /// <returns><see cref="FreshAppProcesses"/> if the processes where queried, <see langword="null"/> if they weren't or there were no chang</returns>
    FreshAppProcesses? UpdateSingleApp(IAvApp app);

    bool IsScheduledUpdateNear();
}

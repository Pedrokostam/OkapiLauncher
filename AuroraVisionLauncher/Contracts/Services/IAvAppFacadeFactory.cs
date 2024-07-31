using System.Collections.ObjectModel;
using AuroraVisionLauncher.Core.Models.Apps;
using AuroraVisionLauncher.Models;

namespace AuroraVisionLauncher.Contracts.Services;
public interface IAvAppFacadeFactory
{
    ReadOnlyCollection<AvApp> AvApps { get; }
    AvAppFacade Create(AvApp app);
    /// <summary>
    /// Populates the given <paramref name="appFacades"/> with facades of the items in the <paramref name="apps"/>.
    /// </summary>
    /// <param name="apps"></param>
    /// <param name="appFacades"></param>
    /// <param name="clear">Whether to clear <paramref name="appFacades"/> before adding new items.</param>
    void Populate(IEnumerable<AvApp> apps, IList<AvAppFacade> appFacades, bool clear = true,Action<AvAppFacade>? perItemAction=null);
    /// <summary>
    /// Populates the given <paramref name="appFacades"/> with facades of every detected installed app.
    /// </summary>
    /// <param name="apps"></param>
    /// <param name="appFacades"></param>
    /// <param name="clear">Whether to clear <paramref name="appFacades"/> before adding new items.</param>
    void Populate(IList<AvAppFacade> appFacades, bool clear = true, Action<AvAppFacade>? perItemAction = null);
    /// <summary>
    /// Create facades for every detected application and returns it as an enumerable.
    /// </summary>
    IEnumerable<AvAppFacade> CreateAllFacades();
    void RediscoverApps();
}
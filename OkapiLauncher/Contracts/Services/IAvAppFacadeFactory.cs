﻿using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using OkapiLauncher.Core.Models.Apps;
using OkapiLauncher.Models;

namespace OkapiLauncher.Contracts.Services;
public interface IAvAppFacadeFactory
{
    IReadOnlyList<AvApp> AvApps { get; }
    AvAppFacade? Create(IAvApp app);
    /// <summary>
    /// Populates the given <paramref name="appFacades"/> with facades of the items in the <paramref name="apps"/>.
    /// </summary>
    /// <param name="apps"></param>
    /// <param name="appFacades"></param>
    /// <param name="clear">Whether to clear <paramref name="appFacades"/> before adding new items.</param>
    void Populate(IEnumerable<IAvApp> apps, IList<AvAppFacade> appFacades, bool clear = true,Action<AvAppFacade>? perItemAction=null);
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
    public bool TryGetAppByPath(string path,[NotNullWhen(true)] out AvAppFacade? appFacade);
}


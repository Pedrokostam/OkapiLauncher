using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AuroraVisionLauncher.Contracts.Services;
using AuroraVisionLauncher.Core.Models.Apps;
using AuroraVisionLauncher.Models;
using CommunityToolkit.Mvvm.Messaging;

namespace AuroraVisionLauncher.Services;
public class AvAppFacadeFactory : IAvAppFacadeFactory
{
    private const string AdditionalFoldersKey = "AdditionalExecutableFolderPaths";
    readonly List<AvApp> _avApps;
    private readonly IWindowManagerService _windowManagerService;
    private readonly IMessenger _messenger;

    public ReadOnlyCollection<AvApp> AvApps => _avApps.AsReadOnly();
    public AvAppFacadeFactory(IWindowManagerService windowManagerService,IMessenger messenger)
    {
        var variables = Environment.GetEnvironmentVariables();
        _avApps = AppReader.GetInstalledAvApps().ToList();
        _windowManagerService = windowManagerService;
        _messenger = messenger;
    }

    public AvAppFacade Create(AvApp app)
    {
        return new(app, _windowManagerService,_messenger);
    }

    public void Populate(IList<AvAppFacade> appFacades, bool clear = true, Action<AvAppFacade>? perItemAction = null) => Populate(_avApps, appFacades, clear, perItemAction);

    public void Populate(IEnumerable<AvApp> apps, IList<AvAppFacade> appFacades, bool clear = true, Action<AvAppFacade>? perItemAction = null)
    {
        if (clear)
        {
            appFacades.Clear();
        }
        foreach (var app in apps)
        {
            var facade = Create(app);
            appFacades.Add(facade);
            perItemAction?.Invoke(facade);
        }
    }

    public IEnumerable<AvAppFacade> CreateAllFacades()=>AvApps.Select(Create);

}

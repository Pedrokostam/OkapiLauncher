using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using OkapiLauncher.Contracts.Services;
using OkapiLauncher.Core.Models.Apps;
using OkapiLauncher.Models;
using CommunityToolkit.Mvvm.Messaging;
using System.Windows.Shell;
using Microsoft.WindowsAPICodePack.Taskbar;

namespace OkapiLauncher.Services;
public class AvAppFacadeFactory : IAvAppFacadeFactory
{

    private const string AdditionalFoldersKey = "AdditionalExecutableFolderPaths";
    readonly List<AvApp> _avApps = new();
    private readonly IWindowManagerService _windowManagerService;
    private readonly IMessenger _messenger;
    private readonly ICustomAppSourceService _customAppSourceService;
    private readonly IJumpListService _jumpListService;

    public IReadOnlyList<AvApp> AvApps => _avApps.AsReadOnly();
    public AvAppFacadeFactory(IWindowManagerService windowManagerService, IMessenger messenger, ICustomAppSourceService customAppSourceService, IJumpListService jumpListService)
    {
        _customAppSourceService = customAppSourceService;
        _jumpListService = jumpListService;
        _windowManagerService = windowManagerService;
        _messenger = messenger;
        RediscoverApps();
    }

    public AvAppFacade? Create(IAvApp app)
    {
        if (app is null)
        {
            return null;
        }
        if (app is not AvApp concrete)
        {
            concrete = _avApps.Find(x => x.Path.Equals(app.Path, StringComparison.OrdinalIgnoreCase) && x.NameWithVersion.Equals(app.NameWithVersion, StringComparison.OrdinalIgnoreCase))!;
        }
        if (concrete is null)
        {
            return null;
        }
        return new(concrete, _windowManagerService, _messenger);
    }

    public void Populate(IList<AvAppFacade> appFacades, bool clear = true, Action<AvAppFacade>? perItemAction = null) => Populate(_avApps, appFacades, clear, perItemAction);

    public void Populate(IEnumerable<IAvApp> apps, IList<AvAppFacade> appFacades, bool clear = true, Action<AvAppFacade>? perItemAction = null)
    {
        if (clear)
        {
            appFacades.Clear();
        }
        foreach (var app in apps)
        {
            var facade = Create(app);
            if (facade is null)
            {
                continue;
            }
            appFacades.Add(facade);
            perItemAction?.Invoke(facade);
        }
    }

    public void RediscoverApps()
    {
        _avApps.Clear();
        var detected = AppReader.GetInstalledAvApps(_customAppSourceService.CustomSources);
        _avApps.AddRange(detected);
        _jumpListService.SetTasks(_avApps);
    }

    public IEnumerable<AvAppFacade> CreateAllFacades() => AvApps.Select(Create).OfType<AvAppFacade>();

    public bool TryGetAppByPath(string path, [NotNullWhen(true)] out AvAppFacade? appFacade)
    {
        var found = _avApps.Find(a => string.Equals(a.Path, path, StringComparison.OrdinalIgnoreCase));
        if (found is not null)
        {
            appFacade = Create(found);
            return appFacade is not null;
        }
        appFacade = null;
        return false;
    }
}

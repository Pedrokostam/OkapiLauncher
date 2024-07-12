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

namespace AuroraVisionLauncher.Services;
public class AvAppFacadeFactory : IAvAppFacadeFactory
{
    private const string AdditionalFoldersKey = "AdditionalExecutableFolderPaths";
    readonly List<AvApp> _avApps;
    private readonly IWindowManagerService _windowManagerService;

    public ReadOnlyCollection<AvApp> AvApps => _avApps.AsReadOnly();
    public AvAppFacadeFactory(IWindowManagerService windowManagerService)
    {
        var variables = Environment.GetEnvironmentVariables();
        _avApps = [];
        foreach (DictionaryEntry entry in variables)
        {
            var key_string = entry.Key?.ToString();
            var value_string = entry.Value?.ToString();
            if (key_string is null || value_string is null)
            {
                continue;
            }
            if (Regex.IsMatch(key_string,
                "^(AVS|FIS|AVLDL|FILDL)_",
                RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture,
                TimeSpan.FromMilliseconds(100)))
            {
                if (AvApp.TryCreate(value_string, out AvApp? app))
                {
                    _avApps.Add(app);
                }
            }
        }
        if (App.Current.Properties.Contains(AdditionalFoldersKey))
        {
            var paths = App.Current.Properties[AdditionalFoldersKey] as IEnumerable<string>;
            if (paths is not null)
            {
                foreach (var folder in paths)
                {
                    if (AvApp.TryCreate(folder, out AvApp? app))
                    {
                        _avApps.Add(app);
                    }
                }
            }
        }
        _windowManagerService = windowManagerService;
    }

    public AvAppFacade Create(AvApp app)
    {
        return new(app, _windowManagerService);
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
}

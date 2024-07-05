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

namespace AuroraVisionLauncher.Services;
public class InstalledAppsProviderService : IInstalledAppsProviderService
{
    private const string AdditionalFoldersKey = "AdditionalExecutableFolderPaths";
    readonly List<AvApp> _avApps;
    public ReadOnlyCollection<AvApp> AvApps => _avApps.AsReadOnly();
    public InstalledAppsProviderService()
    {
        var variables = Environment.GetEnvironmentVariables();
        _avApps = new List<AvApp>();
        foreach (DictionaryEntry entry in variables)
        {
            var key_string = entry.Key?.ToString();
            var value_string = entry.Value?.ToString();
            if (key_string is null || value_string is null)
            {
                continue;
            }
            if (Regex.IsMatch(key_string, "^(AVS|FIS|AVLDL|FILDL)_", RegexOptions.IgnoreCase) && value_string is not null)
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
    }
}

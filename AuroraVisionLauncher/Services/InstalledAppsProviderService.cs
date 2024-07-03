using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AuroraVisionLauncher.Contracts.Services;
using AuroraVisionLauncher.Models.Apps;

namespace AuroraVisionLauncher.Services;
public class InstalledAppsProviderService : IInstalledAppsProviderService
{
    readonly List<Executable> _executables;
    public ReadOnlyCollection<Executable> Executables => _executables.AsReadOnly();
    public InstalledAppsProviderService()
    {
        var variables= Environment.GetEnvironmentVariables();
        _executables = new List<Executable>();
        foreach (DictionaryEntry entry in variables)
        {
            var key_string = entry.Key?.ToString();
            var value_string = entry.Value?.ToString();
            if (key_string is null || value_string is null)
            {
                continue;
            }
            if (Regex.IsMatch(key_string, "^(AVS|FIS)_", RegexOptions.IgnoreCase) && value_string is not null)
            {
                if (Executable.TryCreate(value_string, out Executable? app))
                {
                    _executables.Add(app);
                }
            }
        }
    }
}

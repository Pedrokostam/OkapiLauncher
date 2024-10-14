using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using AuroraVisionLauncher.Contracts.Services;
using AuroraVisionLauncher.Helpers;
using AuroraVisionLauncher.Models;

namespace AuroraVisionLauncher.Services;
public class CustomAppSourceService : ICustomAppSourceService
{
    private static string _customSourcesKey = "CustomSources";
    /// <summary>
    /// Is a dependency to ensure its instantiated before.
    /// </summary>
    private readonly IPersistAndRestoreService _persistAndRestoreService;
    public CustomAppSourceService(IPersistAndRestoreService persistAndRestoreService)
    {
        GetCustomSources();
        _persistAndRestoreService = persistAndRestoreService;
    }

    public ObservableCollection<CustomAppSource> CustomSources { get; } = new();
    private void GetCustomSources()
    {
        var elems = App.Current.Properties.ReadElementList<CustomAppSource>(_customSourcesKey);
        foreach (var elem in elems)
        {
            if(elem is null)
            {
                continue;
            }
            CustomSources.Add(elem);
        }
        //App.Current.Properties[Key] = new List<RecentlyOpenedFile>();
        //if (App.Current.Properties.Contains(_customSourcesKey))
        //{
        //    System.Text.Json.JsonElement prop = (System.Text.Json.JsonElement)App.Current.Properties[_customSourcesKey]!;
        //    foreach (var item in prop.EnumerateArray())
        //    {
        //        try
        //        {
        //            var customSource = item.Deserialize<CustomAppSource>();
        //            if (customSource is not null)
        //            {
        //                CustomSources.Add(customSource);
        //            }
        //        }
        //        catch (ArgumentException)
        //        {
        //            // dont care about invalid data - its beta after all.
        //        }
        //    }
        //}
        App.Current.Properties[_customSourcesKey] = CustomSources;
    }
}

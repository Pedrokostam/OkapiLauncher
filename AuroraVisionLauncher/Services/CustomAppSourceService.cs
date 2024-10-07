using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AuroraVisionLauncher.Contracts.Services;
using AuroraVisionLauncher.Models;

namespace AuroraVisionLauncher.Services;
public class CustomAppSourceService : ICustomAppSourceService
{
    private static string _customSourcesKey = "CustomSources";

    public CustomAppSourceService()
    {
        GetCustomSources();
    }

    public ObservableCollection<CustomAppSource> CustomSources { get; } = new();
    private void GetCustomSources()
    {
        //App.Current.Properties[Key] = new List<RecentlyOpenedFile>();
        if (App.Current.Properties.Contains(_customSourcesKey))
        {
            Newtonsoft.Json.Linq.JArray prop = (Newtonsoft.Json.Linq.JArray)App.Current.Properties[_customSourcesKey]!;
            foreach (var item in prop)
            {
                try
                {
                    var customSource = item.ToObject<CustomAppSource>();
                    if (customSource is not null)
                    {
                        CustomSources.Add(customSource);
                    }
                }
                catch (ArgumentException)
                {
                    // dont care about invalid data - its beta after all.
                }
            }
        }
        App.Current.Properties[_customSourcesKey] = CustomSources;
    }
}

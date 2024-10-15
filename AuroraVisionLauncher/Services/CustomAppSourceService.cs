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
        _persistAndRestoreService = persistAndRestoreService;
        if (_persistAndRestoreService.IsDataRestored)
        {
            // if already restored, get
            InitializeData();
        }
        else
        {
            // otherwise wait for restore
            _persistAndRestoreService.DataRestored += _persistAndRestoreService_DataRestored;
        }

    }

    private void _persistAndRestoreService_DataRestored(object? sender, EventArgs e)
    {
        InitializeData();
    }

    public ObservableCollection<CustomAppSource> CustomSources { get; } = new();
    private void InitializeData()
    {
        var elems = App.Current.Properties.ReadElementList<CustomAppSource>(_customSourcesKey);
        foreach (var elem in elems)
        {
            if (elem is null)
            {
                continue;
            }
            CustomSources.Add(elem);
        }
        App.Current.Properties[_customSourcesKey] = CustomSources;
    }
}

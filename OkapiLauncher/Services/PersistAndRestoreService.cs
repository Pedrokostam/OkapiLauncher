﻿using System.Collections;
using System.IO;
using System.Text.Json;
using OkapiLauncher.Contracts.Services;
using OkapiLauncher.Core.Contracts.Services;
using OkapiLauncher.Models;

using Microsoft.Extensions.Options;

namespace OkapiLauncher.Services;

public class PersistAndRestoreService : IPersistAndRestoreService
{
    private readonly IFileService _fileService;
    private readonly AppConfig _appConfig;
    private readonly string _localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

    public PersistAndRestoreService(IFileService fileService, IOptions<AppConfig> appConfig)
    {
        _fileService = fileService;
        _appConfig = appConfig.Value;
    }

    public bool IsDataRestored { get; private set; }

    public event EventHandler? DataRestored;

    public void PersistData()
    {
        if (App.Current.Properties != null)
        {
            var folderPath = Path.Combine(_localAppData, _appConfig.ConfigurationsFolder);
            var fileName = _appConfig.AppPropertiesFileName;
            _fileService.Save(folderPath, fileName, App.Current.Properties);
        }
    }

    public void RestoreData()
    {
        var folderPath = Path.Combine(_localAppData, _appConfig.ConfigurationsFolder);
        var fileName = _appConfig.AppPropertiesFileName;
        var properties = _fileService.Read<IDictionary<string, JsonElement>>(folderPath, fileName);
        if (properties != null)
        {
            foreach (KeyValuePair<string, JsonElement> property in properties)
            {
                App.Current.Properties.Add(property.Key, property.Value);
            }
        }
        IsDataRestored = true;
        DataRestored?.Invoke(this, EventArgs.Empty);
    }
}

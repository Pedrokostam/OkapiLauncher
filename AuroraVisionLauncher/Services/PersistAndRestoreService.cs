using System.Collections;
using System.IO;
using System.Text.Json;
using AuroraVisionLauncher.Contracts.Services;
using AuroraVisionLauncher.Core.Contracts.Services;
using AuroraVisionLauncher.Models;

using Microsoft.Extensions.Options;

namespace AuroraVisionLauncher.Services;

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
                object? value = property.Value.ValueKind switch
                {
                    JsonValueKind.String => property.Value.GetString(),
                    JsonValueKind.Number => property.Value.GetDouble(),
                    JsonValueKind.True => true,
                    JsonValueKind.False => false,
                    JsonValueKind.Null => null,
                    _ => property,
                    //JsonValueKind.Undefined => throw new NotImplementedException(),
                    //JsonValueKind.Object => throw new NotImplementedException(),
                    //JsonValueKind.Array => throw new NotImplementedException(),
                };
                App.Current.Properties.Add(property.Key, value);
            }
        }
    }
}

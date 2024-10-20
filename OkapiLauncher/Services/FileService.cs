using System.IO;
using System.Text;

using OkapiLauncher.Core.Contracts.Services;

using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows.Media;
using OkapiLauncher.Helpers;

namespace OkapiLauncher.Core.Services;

public class FileService : IFileService
{
    
    public T? Read<T>(string folderPath, string fileName)
    {
        var t = JsonSerializerOptions.Default.IsReadOnly;
        var path = Path.Combine(folderPath, fileName);
        if (File.Exists(path))
        {
            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<T>(json, JsonHelper.Options);
        }
        return default;
    }

    public void Save<T>(string folderPath, string fileName, T content)
    {
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        var fileContent = JsonSerializer.Serialize(content, JsonHelper.Options);
        File.WriteAllText(Path.Combine(folderPath, fileName), fileContent, Encoding.UTF8);
    }

    public void Delete(string folderPath, string fileName)
    {
        if (fileName != null && File.Exists(Path.Combine(folderPath, fileName)))
        {
            File.Delete(Path.Combine(folderPath, fileName));
        }
    }
}

using AuroraVisionLauncher.Core.Models.Apps;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuroraVisionLauncher.Models;
public partial class CustomAppSource : ObservableObject, IAppSource
{
    [ObservableProperty]
    private string? _description;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SourcePath))]
    private string _path = "";

    public string SourcePath => ExpandPath(Path);

    public CustomAppSource()
    {
    }

    public static string ExpandPath(string path)
    {
        var temp = path.Replace("~", "%USERPROFILE%", StringComparison.Ordinal);
        return Environment.ExpandEnvironmentVariables(temp);
    }

    internal bool IsDefault()
    {
        return string.IsNullOrWhiteSpace(Description) && string.IsNullOrWhiteSpace(SourcePath);
    }
}

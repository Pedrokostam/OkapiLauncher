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
    private string _path = "";

    public string SourcePath
    {
        get
        {
            var temp = Path.Replace("~", "%USERPOFILE%", StringComparison.Ordinal);
            return Environment.ExpandEnvironmentVariables(temp);
        }
    }

    public CustomAppSource()
    {
    }
}

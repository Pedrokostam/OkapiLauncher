using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using AuroraVisionLauncher.Core.Contracts.Services;

namespace AuroraVisionLauncher.Core.Services;
public class IconService : IIconService
{

    public string? ExtractIconToFile(string? iconSourcePrimary, string destinationPath, Icon fallbackIcon)
    {
        Icon? icon = null;
        if (!string.IsNullOrWhiteSpace(iconSourcePrimary) && File.Exists(iconSourcePrimary))
        {
            icon = Icon.ExtractAssociatedIcon(iconSourcePrimary!);
        }
        icon ??= fallbackIcon;
        var destDir = Path.GetDirectoryName(destinationPath);
        if (!Directory.Exists(destDir))
        {
            Directory.CreateDirectory(destDir);
        }
        destinationPath = Path.ChangeExtension(destinationPath, ".ico");
        icon.ToBitmap().Save(destinationPath, ImageFormat.Icon);
        return destinationPath;
    }
}

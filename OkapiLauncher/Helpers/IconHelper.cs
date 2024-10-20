using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Drawing.Imaging;
using System.IO;

namespace OkapiLauncher.Helpers
{
    public static class IconHelper
    {
        public static BitmapSource ToBitmapSource(this Icon icon, int? height = null)
        {
            var opt = height.HasValue ? BitmapSizeOptions.FromHeight(height.Value) : BitmapSizeOptions.FromEmptyOptions();
            return Imaging.CreateBitmapSourceFromHIcon(icon.Handle, Int32Rect.Empty, opt);
        }
        public static string? ExtractIconToFile(string? iconSourcePrimary, string destinationPath, Icon fallbackIcon)
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
                Directory.CreateDirectory(destDir!);
            }
            destinationPath = Path.ChangeExtension(destinationPath, ".ico");
            icon.ToBitmap().Save(destinationPath, ImageFormat.Icon);
            return destinationPath;
        }
    }
}

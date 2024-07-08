using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;
using System.Windows;
using System.Windows.Media.Imaging;

namespace AuroraVisionLauncher.Helpers
{
    public static class IconHelper
    {
        public static BitmapSource ToBitmapSource(this Icon icon, int? height = null)
        {
            var opt = height.HasValue ? BitmapSizeOptions.FromHeight(height.Value) : BitmapSizeOptions.FromEmptyOptions();
            return Imaging.CreateBitmapSourceFromHIcon(icon.Handle, Int32Rect.Empty, opt);
        }
    }
}

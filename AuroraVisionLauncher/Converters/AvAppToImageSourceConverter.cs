using System;
using System.Collections.Generic;
using System.DirectoryServices.ActiveDirectory;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Shapes;
using AuroraVisionLauncher.Core.Models;
using AuroraVisionLauncher.Core.Models.Apps;
using AuroraVisionLauncher.Helpers;

namespace AuroraVisionLauncher.Converters;
public class AvAppToImageSourceConverter : IValueConverter
{

    public object? Convert(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not IAvApp app)
        {
            return value;
        }
        var brand = app.Brand.Name.Replace(" ", "");
        var type = app.Type.Name;
        var iconName = $"{brand}{type}.png";
        return "pack://application:,,,/Resources/Symbols/" + iconName;
        
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

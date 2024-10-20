using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using AuroraVisionLauncher.Models;
using AuroraVisionLauncher.Properties;

namespace AuroraVisionLauncher.Converters;
public class CompatibilityToDescriptionConverter:IValueConverter
{
    public static readonly CompatibilityToDescriptionConverter Instance = new CompatibilityToDescriptionConverter();
    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not Compatibility comp)
        {
            return value;
        }
        string key = $"Compatibility{comp}Description";
        return Resources.ResourceManager.GetString(key, culture: null) ?? value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace AuroraVisionLauncher.Converters;
public class UtcDatetimeToLocalConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not DateTime dt)
        {
            return value;
        }
        return dt.ToLocalTime();
    }

    public object? ConvertBack(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not DateTime dt)
        {
            return value;
        }
        return dt.ToUniversalTime();
    }
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace AuroraVisionLauncher.Converters;
public class BoolSelectorConverter : IValueConverter
{
    public object? TrueValue { get; set; }
    public object? FalseValue { get; set; }

    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool booleanValue)
        {
            return booleanValue ? TrueValue : FalseValue;
        }
        return FalseValue;
    }

    public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value != null)
        {
            if (value.Equals(TrueValue))
            {
                return true;
            }
            if (value.Equals(FalseValue))
            {
                return false;
            }
        }
        return false;
    }
}

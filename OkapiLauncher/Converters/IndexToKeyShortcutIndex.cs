using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace OkapiLauncher.Converters;
public class IndexToKeyShortcutIndex : IValueConverter
{
    public object? Convert(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        if(value is not int i)
        {
            return value; }
        return i switch
        {
            >= 0 and <= 9 => i + 1,
            10 => 0,
            _ => null,
        };
    }

    public object? ConvertBack(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

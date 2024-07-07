using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace AuroraVisionLauncher.Converters;
public class PathToFilenameConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not string path)
        {
            return value;
        }
        int maxLength = int.MaxValue;
        if (parameter is int integer)
        {
            maxLength = integer;
        }
        else if (parameter is string s && int.TryParse(s, out integer))
        {
            maxLength = integer;
        }
        if (maxLength < 2)
        {
            maxLength = int.MaxValue;
        }
        var name = Path.GetFileName(path);
        if (name?.Length > maxLength)
        {
            return name.Substring(0, maxLength - 1)+ "…";
        }
        return name;
    }

    public object? ConvertBack(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

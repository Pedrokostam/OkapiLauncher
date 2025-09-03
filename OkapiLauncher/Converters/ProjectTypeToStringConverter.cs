using System.Globalization;
using System.Windows.Data;
using OkapiLauncher.Core.Models;
using OkapiLauncher.Properties;

namespace OkapiLauncher.Converters;
public class ProjectTypeToStringConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not ProductType productType)
        {
            return value;
        }
        string key = $"ProjectProductType{productType.Name}Label";
        return Resources.ResourceManager.GetString(key, culture: null) ?? "UNKNOWN TYPE";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

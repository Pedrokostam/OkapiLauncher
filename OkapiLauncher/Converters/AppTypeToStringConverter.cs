using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using OkapiLauncher.Core.Models;
using OkapiLauncher.Properties;
using Windows.ApplicationModel.Resources.Core;

namespace OkapiLauncher.Converters;
public class AppTypeToStringConverter : IValueConverter
{
    public static readonly AppTypeToStringConverter Instance = new AppTypeToStringConverter();
    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not ProductType productType)
        {
            return value;
        }
        string key = $"AppProductType{productType.Name}Label";
        return Resources.ResourceManager.GetString(key, culture: null) ?? "UNKNOWN TYPE";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using AuroraVisionLauncher.Core.Models;
using AuroraVisionLauncher.Properties;
using Windows.ApplicationModel.Resources.Core;

namespace AuroraVisionLauncher.Converters;
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

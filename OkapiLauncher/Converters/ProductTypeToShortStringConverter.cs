using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using OkapiLauncher.Core.Models;

namespace OkapiLauncher.Converters;
public class ProductTypeToShortStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not ProductType type)
        {
            return value;
        }
        return type.Type switch
        {
            AvType.Professional => "Pro",
            AvType.Runtime => "Runtime",
            AvType.DeepLearningGPU => "DL",
            AvType.Library => "Lib",
            _ => throw new NotSupportedException()
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

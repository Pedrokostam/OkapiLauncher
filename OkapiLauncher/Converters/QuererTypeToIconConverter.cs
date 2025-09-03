using System.Globalization;
using System.Windows.Data;
using Material.Icons;
using OkapiLauncher.Services.Processes;

namespace OkapiLauncher.Converters;

internal class QuererTypeToIconConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Type t)
        {
            if (t == typeof(DiagnosticQuerer))
            {
                return MaterialIconKind.Tick;
            }
            else
            {
                return MaterialIconKind.Warning;
            }
        }
        return MaterialIconKind.Error;
    }
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
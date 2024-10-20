using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using OkapiLauncher.Core.Models.Apps;

namespace OkapiLauncher.Converters;

public class AvAppToImageConverter : IValueConverter
{

    public object? Convert(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not IAvApp app)
        {
            return value;
        }
        var brand = app.Brand.Name.Replace(" ", "");
        var type = app.Type.Name;
        var iconName = $"{brand}{type}.png";
        var path =  "pack://application:,,,/Resources/Symbols/" + iconName;
        var bmp = new BitmapImage();
        bmp.BeginInit();
        bmp.DecodePixelWidth = 100;
        bmp.CacheOption = BitmapCacheOption.OnLoad;
        bmp.UriSource = new Uri(path, UriKind.RelativeOrAbsolute);
        bmp.EndInit();
        return bmp;

    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
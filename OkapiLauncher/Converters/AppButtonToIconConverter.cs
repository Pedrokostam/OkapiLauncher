using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Material.Icons;
using OkapiLauncher.Controls.Utilities;

namespace OkapiLauncher.Converters
{
    class AppButtonToIconConverter : IValueConverter
    {
        public static readonly AppButtonToIconConverter Instance = new AppButtonToIconConverter();
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not VisibleButtons button)
            {
                return value;
            }
            var key = button switch
            {
                VisibleButtons.Launch =>"IconLaunch",
                VisibleButtons.Copy => "IconCopy",
                VisibleButtons.Open => "IconOpenFolder",
                VisibleButtons.License => "IconLicenseFolder",
                VisibleButtons.Log => "IconLogFolder",
                VisibleButtons.Overview => "IconProcess",
                VisibleButtons.KillAll => "IconKillAll",
                _ =>null,
            };
            if(key is null)
            {
                return MaterialIconKind.Error;
            }
            return (MaterialIconKind)Application.Current.FindResource(key);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}

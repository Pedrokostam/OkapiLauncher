using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using OkapiLauncher.Controls.Utilities;
using OkapiLauncher.Properties;

namespace OkapiLauncher.Converters
{
    class AppButtonToTextConverter : IValueConverter
    {
        public static readonly AppButtonToTextConverter Instance = new AppButtonToTextConverter();
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not VisibleButtons button)
            {
                return value;
            }
            string key = $"AppButton{button}Name";
            return Resources.ResourceManager.GetString(key, culture: null) ?? "Oops, this button should no exist";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}

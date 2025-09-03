﻿using OkapiLauncher.Helpers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace OkapiLauncher.Converters
{
    class FileToIconConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not string path)
            {
                return null;
            }
            try
            {

                var icon =Icon.ExtractAssociatedIcon(path);
                if (icon is null)
                {
                    return null;
                }
                return icon.ToBitmapSource();
            }
            catch (ArgumentException)
            {
                return null;
            }

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

﻿using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;
using OkapiLauncher.Core.Models;
using OkapiLauncher.Models;

namespace OkapiLauncher.Converters;
internal class InterfaceVersionConverter : IValueConverter
{
    public static readonly InterfaceVersionConverter Instance = new();
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not AvVersionFacade iav)
        {
            return value;
        }

        return iav.InterfaceVersion;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

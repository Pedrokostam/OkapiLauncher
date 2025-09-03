using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using OkapiLauncher.Properties;
using OkapiLauncher.Services.Processes;

namespace OkapiLauncher.Converters;
internal class QuererTypeToDescriptionConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Type t)
        {
            if (t == typeof(DiagnosticQuerer))
            {
                return Resources.ResourceManager.GetString("SettingsPageProcessMonitorDiagnostics", culture: null) ?? "Error";
            }
            else
            {
                return Resources.ResourceManager.GetString("SettingsPageProcessMonitorWMI", culture: null) ?? "Error";
            }
        }
        return Resources.ResourceManager.GetString("SettingsPageProcessMonitorNone", culture: null) ?? "Error";
    }
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using OkapiLauncher.Core.Models.Apps;
using OkapiLauncher.Models;

namespace OkapiLauncher.Helpers;
public class CommandLineOptionsTemplateSelector : DataTemplateSelector
{
    public override DataTemplate? SelectTemplate(object? item, DependencyObject container)
    {
        if (item is not LaunchOptions options)
        {
            return null;
        }
        var element = (FrameworkElement)container;
        return options switch
        {
            NoLaunchOptions => element.FindResource("NoCommandLineOptionsTemplate") as DataTemplate,
            StudioLaunchOptions =>element.FindResource("StudioCommandLineOptionsTemplate") as DataTemplate,
            ExecutorLaunchOptions => element.FindResource("ExecutorCommandLineOptionsTemplate") as DataTemplate,
            _ => null
        };
    }
}

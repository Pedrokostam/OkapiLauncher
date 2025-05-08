using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Material.Icons.WPF;
using Material.Icons;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows;
using OkapiLauncher.Models;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using OkapiLauncher.Helpers;

namespace OkapiLauncher.Controls.Utilities;
internal static class AppContextMenu
{
    private static MaterialIconKind IconOpenFolder;
    private static MaterialIconKind IconCopy;
    private static MaterialIconKind IconLaunch;
    private static MaterialIconKind IconProcess;
    private static MaterialIconKind IconLicenseFolder;
    private static MaterialIconKind IconLogFolder;
    private static bool IconInitialized = false;
    private static ICommand GetPropertyCommand(this object source, string propertyName)
    {
        var prop = source.GetType().GetProperty(propertyName);
        return prop?.GetValue(source) as ICommand ?? throw new InvalidOperationException($"Property '{propertyName}' not found or not ICommand.");
    }
    private static Binding Bind(this AvAppFacade appFacade, string path, IValueConverter? converter = null) => new Binding(path)
    {
        Source = appFacade,
        Converter = converter,
    };
    private static object ResolveResource_Throw(this FrameworkElement element, string key)
    {
        return element.TryFindResource(key) ?? throw new ArgumentNullException(nameof(key), $"Resource {key} could not be found in {element.GetType().FullName}");
    }
    private static IValueConverter ResolveConverter(this FrameworkElement element, string key)
    {
        return (IValueConverter)element.TryFindResource(key) ?? throw new ArgumentNullException(nameof(key), $"Converter {key} could not be found in {element.GetType().FullName}");
    }

    private static MenuItem ButtonOpenInstallation(this FrameworkElement element, AvAppFacade appFacade)
    {
        return new MenuItem
        {
            Header = Properties.Resources.AvAppOpenInstallationFolder,
            Command = GetPropertyCommand(appFacade, "OpenContainingFolderCommand"),
            Icon = new MaterialIcon { Kind = IconOpenFolder },
        };
    }

    private static MenuItem ButtonCopyExecutablePath(this FrameworkElement element, AvAppFacade appFacade)
    {
        return new MenuItem
        {
            Header = Properties.Resources.AvAppCopyPath,
            Command = GetPropertyCommand(appFacade, "CopyExecutablePathCommand"),
            Icon = new MaterialIcon { Kind = IconCopy },
        };
    }

    private static MenuItem ButtonLaunch(this FrameworkElement element, AvAppFacade appFacade, ICommand? customLaunchCommand)
    {
        var launchItem = new MenuItem
        {
            Header = Properties.Resources.AvAppLaunchWithNoProgram,
            Icon = new MaterialIcon { Kind = IconLaunch },
            Tag = true,
            Command = customLaunchCommand ?? appFacade.LaunchWithoutProgramCommand,
        };
        //launchItem.SetBinding(UIElement.VisibilityProperty, isExecutableBinding);
        return launchItem;
    }

    private static MenuItem ButtonOpenProcessOverview(this FrameworkElement element, AvAppFacade appFacade)
    {
        var processItem = new MenuItem
        {
            Header = "Open process overview window",
            Command = GetPropertyCommand(appFacade, "ShowProcessOverviewCommand"),
            Icon = new MaterialIcon { Kind = IconProcess },
        };
        //processItem.SetBinding(UIElement.VisibilityProperty, isExecutableBinding);
        return processItem;
    }

    private static MenuItem ButtonOpenLicenseFolder(this FrameworkElement element, AvAppFacade appFacade)
    {
        var licenseItem = new MenuItem
        {
            Header = Properties.Resources.AvAppOpenLicenseFolder,
            Command = GetPropertyCommand(appFacade, "OpenLicenseFolderCommand"),
            Icon = new MaterialIcon { Kind = IconLicenseFolder },
        };
        licenseItem.SetBinding(MenuItem.IsEnabledProperty, appFacade.Bind("CanOpenLicenseFolder"));
        return licenseItem;
    }

    private static MenuItem ButtonOpenLogFolder(this FrameworkElement element, AvAppFacade appFacade)
    {
        var logItem = new MenuItem
        {
            Header = Properties.Resources.AvAppOpenLogFolder,
            Command = GetPropertyCommand(appFacade, "OpenLogFolderCommand"),
            Icon = new MaterialIcon { Kind = IconLogFolder },
        };
        return logItem;
    }

    /// <summary>
    /// Create a context menu for <paramref name="appFacade"/> and applies it to <paramref name="target"/>.
    /// </summary>
    /// <param name="target"></param>
    /// <param name="appFacade"></param>
    /// <param name="customLaunchCommand"></param>
    /// <returns>Created <see cref="ContextMenu"/> which has already been added to <paramref name="target"/>.</returns>
    public static ContextMenu CreateAppContextMenu(
        FrameworkElement target,
        AvAppFacade appFacade,
        ICommand? customLaunchCommand)
    {
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(appFacade);
        if (!IconInitialized)
        {
            IconOpenFolder = (MaterialIconKind)target.ResolveResource_Throw("IconOpenFolder");
            IconCopy = (MaterialIconKind)target.ResolveResource_Throw("IconCopy");
            IconLaunch = (MaterialIconKind)target.ResolveResource_Throw("IconLaunch");
            IconProcess = (MaterialIconKind)target.ResolveResource_Throw("IconProcess");
            IconLicenseFolder = (MaterialIconKind)target.ResolveResource_Throw("IconLicenseFolder");
            IconLogFolder = (MaterialIconKind)target.ResolveResource_Throw("IconLogFolder");
            IconInitialized = true;
        }
        var contextMenu = new ContextMenu();

        //
        contextMenu.Items.Add(target.ButtonOpenInstallation(appFacade));
        contextMenu.Items.Add(target.ButtonCopyExecutablePath(appFacade));
        //
        contextMenu.Items.Add(new Separator());
        contextMenu.Items.Add(target.ButtonLaunch(appFacade, customLaunchCommand));
        contextMenu.Items.Add(target.ButtonOpenProcessOverview(appFacade));
        //
        contextMenu.Items.Add(new Separator());
        contextMenu.Items.Add(target.ButtonOpenLicenseFolder(appFacade));
        contextMenu.Items.Add(target.ButtonOpenLogFolder(appFacade));

        target.ContextMenu = contextMenu;
        return contextMenu;
    }


}

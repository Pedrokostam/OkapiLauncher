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
    private static MaterialIconKind IconKillAll;
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

    private static MenuItem ButtonOpenInstallation(AvAppFacade appFacade)
    {
        return new MenuItem
        {
            Header = Properties.Resources.AvAppOpenInstallationFolder,
            Command = GetPropertyCommand(appFacade, nameof(AvAppFacade.OpenContainingFolderCommand)),
            Icon = new MaterialIcon { Kind = IconOpenFolder },
        };
    }

    private static MenuItem ButtonCopyExecutablePath(AvAppFacade appFacade)
    {
        return new MenuItem
        {
            Header = Properties.Resources.AvAppCopyPath,
            Command = GetPropertyCommand(appFacade, nameof(AvAppFacade.CopyExecutablePathCommand)),
            Icon = new MaterialIcon { Kind = IconCopy },
        };
    }

    private static MenuItem ButtonLaunch(AvAppFacade appFacade)
    {
        return new MenuItem
        {
            Header = Properties.Resources.AvAppLaunchWithNoProgram,
            Icon = new MaterialIcon { Kind = IconLaunch },
            Tag = true,
            Command = appFacade.LaunchWithoutProgramCommand,
        };
    }

    private static MenuItem ButtonLaunch(AvAppFacade appFacade, ICommand customLaunchCommand)
    {
        return new MenuItem
        {
            Header = Properties.Resources.AvAppLaunchWithProgram,
            Icon = new MaterialIcon { Kind = MaterialIconKind.Powershell },
            Tag = true,
            Command = customLaunchCommand,
            CommandParameter = appFacade,
        };
    }

    private static MenuItem ButtonOpenProcessOverview(AvAppFacade appFacade)
    {
        return new MenuItem
        {
            Header = Properties.Resources.AvAppOpenOverview,
            Command = GetPropertyCommand(appFacade, nameof(AvAppFacade.ShowProcessOverviewCommand)),
            Icon = new MaterialIcon { Kind = IconProcess },
        };
    }

    private static MenuItem ButtonOpenLicenseFolder(AvAppFacade appFacade)
    {
        var killItem = new MenuItem
        {
            Header = Properties.Resources.AvAppOpenLicenseFolder,
            Command = GetPropertyCommand(appFacade, nameof(AvAppFacade.OpenLicenseFolderCommand)),
            Icon = new MaterialIcon { Kind = IconLicenseFolder },
        };
        killItem.SetBinding(MenuItem.IsEnabledProperty, appFacade.Bind(nameof(AvAppFacade.CanOpenLicenseFolder)));
        return killItem;
    }

    private static MenuItem ButtonOpenLogFolder(AvAppFacade appFacade)
    {
        return new MenuItem
        {
            Header = Properties.Resources.AvAppOpenLogFolder,
            Command = GetPropertyCommand(appFacade, nameof(AvAppFacade.OpenLogFolderCommand)),
            Icon = new MaterialIcon { Kind = IconLogFolder },
        };
    }

    private static MenuItem ButtonKillAll(AvAppFacade appFacade)
    {
        var licenseItem = new MenuItem
        {
            Header = Properties.Resources.AvAppKillAllProcesses,
            Command = GetPropertyCommand(appFacade, nameof(AvAppFacade.KillAllProcessesCommand)),
            Icon = new MaterialIcon { Kind = IconKillAll },
        };
        licenseItem.SetBinding(MenuItem.IsEnabledProperty, appFacade.Bind(nameof(AvAppFacade.IsExecutable)));
        return licenseItem;
    }


    /// <summary>
    /// Create a context menu for <paramref name="appFacade"/> and applies it to <paramref name="target"/>.
    /// </summary>
    /// <param name="target"></param>
    /// <param name="appFacade"></param>
    /// <param name="customLaunchCommand">Custom command to launch an application, which should receive <paramref name="appFacade"/> as the parameter.</param>
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
            IconKillAll = (MaterialIconKind)target.ResolveResource_Throw("IconKillAll");
            IconInitialized = true;
        }
        var contextMenu = new ContextMenu();

        contextMenu.Items.Add(ButtonOpenInstallation(appFacade));
        contextMenu.Items.Add(ButtonCopyExecutablePath(appFacade));

        contextMenu.Items.Add(new Separator());
        contextMenu.Items.Add(ButtonLaunch(appFacade));
        if (customLaunchCommand is not null)
        {
            contextMenu.Items.Add(new Separator());
            contextMenu.Items.Add(ButtonLaunch(appFacade, customLaunchCommand));
        }

        contextMenu.Items.Add(new Separator());
        contextMenu.Items.Add(ButtonOpenProcessOverview(appFacade));

        contextMenu.Items.Add(new Separator());
        contextMenu.Items.Add(ButtonOpenLicenseFolder(appFacade));
        contextMenu.Items.Add(ButtonOpenLogFolder(appFacade));
        contextMenu.Items.Add(new Separator());
        contextMenu.Items.Add(ButtonKillAll(appFacade));
        target.ContextMenu = contextMenu;
        return contextMenu;
    }


}

﻿using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;

using AuroraVisionLauncher.Contracts.Services;
using AuroraVisionLauncher.Contracts.Views;
using AuroraVisionLauncher.Core.Contracts.Services;
using AuroraVisionLauncher.Core.Services;
using AuroraVisionLauncher.Models;
using AuroraVisionLauncher.Models.Messages;
using AuroraVisionLauncher.Services;
using AuroraVisionLauncher.ViewModels;
using AuroraVisionLauncher.Views;

using CommunityToolkit.Mvvm.Messaging;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Windows.ApplicationModel.Resources.Core;

namespace AuroraVisionLauncher;

// For more information about application lifecycle events see https://docs.microsoft.com/dotnet/framework/wpf/app-development/application-management-overview

// WPF UI elements use language en-US by default.
// If you need to support other cultures make sure you add converters and review dates and numbers in your UI to ensure everything adapts correctly.
// Tracking issue for improving this is https://github.com/dotnet/wpf/issues/1946
public partial class App : Application
{
    private IHost? _host;

    public T GetService<T>()
        where T : class
        => (T)_host!.Services.GetService(typeof(T))!;

    public App()
    {
    }

    private async void OnStartup(object sender, StartupEventArgs e)
    {
        if (e.Args.Length > 1)
        {
            MessageBox.Show($"Launcher expects at most one argument.\nProvided arguments: {e.Args.Length}.", "Invalid startup arguments", MessageBoxButton.OK, MessageBoxImage.Error);
            throw new ArgumentException("Received too many arguments.");
        }
        var appLocation = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location);

        // For more information about .NET generic host see  https://docs.microsoft.com/aspnet/core/fundamentals/host/generic-host?view=aspnetcore-3.0
        _host = Host.CreateDefaultBuilder(e.Args)
                .ConfigureAppConfiguration(c =>
                {
                    c.SetBasePath(appLocation);
                })
                .ConfigureServices(ConfigureServices)
                .Build();
        Uri iconUri = new Uri("pack://application:,,,/Resources/Icons/AppIcon.ico");
        var i = new System.Windows.Media.Imaging.BitmapImage(iconUri);
        await _host.StartAsync();
        // initialize launcher vm, so that it can start listening to FileRequestMessages
        GetService<LauncherViewModel>();
        if (e.Args.Length == 1)
        {
            GetService<IMessenger>().Send(new FileRequestedMessage(e.Args[0]));
        }
    }

    private void ConfigureServices(HostBuilderContext context, IServiceCollection services)
    {
        // TODO: Register your services, viewmodels and pages here

        // App Host
        services.AddHostedService<ApplicationHostService>();

        // Activation Handlers

        // Core Services
        services.AddSingleton<IFileService, FileService>();

        // Services
        services.AddSingleton<IWindowManagerService, WindowManagerService>();
        services.AddSingleton<IApplicationInfoService, ApplicationInfoService>();
        services.AddSingleton<ISystemService, SystemService>();
        services.AddSingleton<IPersistAndRestoreService, PersistAndRestoreService>();
        services.AddSingleton<IThemeSelectorService, ThemeSelectorService>();
        services.AddSingleton<IPageService, PageService>();
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<IRequestedFilesService, RequestedFilesService>();
        services.AddSingleton<IMessenger, WeakReferenceMessenger>();
        services.AddSingleton<IInstalledAppsProviderService, InstalledAppsProviderService>();
        services.AddSingleton<IRecentlyOpenedFilesService, RecentlyOpenedFilesService>();
        services.AddSingleton<IFileAssociationService, FileAssociationService>();

        // Views and ViewModels
        services.AddSingleton<IShellWindow, ShellWindow>();
        services.AddSingleton<ShellViewModel>();

        services.AddSingleton<LauncherViewModel>();
        services.AddSingleton<LauncherPage>();

        services.AddTransient<SettingsViewModel>();
        services.AddTransient<SettingsPage>();

        services.AddTransient<IShellDialogWindow, ShellDialogWindow>();
        services.AddTransient<ShellDialogViewModel>();

        services.AddTransient<InstalledAppsViewModel>();
        services.AddTransient<InstalledAppsPage>();

        // Configuration
        services.Configure<AppConfig>(context.Configuration.GetSection(nameof(AppConfig)));
        services.AddSingleton<IRightPaneService, RightPaneService>();
    }

    private async void OnExit(object sender, ExitEventArgs e)
    {
        await _host!.StopAsync();
        _host?.Dispose();
        _host = null;
    }


    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        // TODO: Please log and handle the exception as appropriate to your scenario
        // For more info see https://docs.microsoft.com/dotnet/api/system.windows.application.dispatcherunhandledexception?view=netcore-3.0
    }
}

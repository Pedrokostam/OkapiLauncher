using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;

using CommunityToolkit.Mvvm.Messaging;

using MahApps.Metro.Controls.Dialogs;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using OkapiLauncher.Contracts.Services;
using OkapiLauncher.Contracts.Views;
using OkapiLauncher.Core.Contracts.Services;
using OkapiLauncher.Core.Services;
using OkapiLauncher.Models;
using OkapiLauncher.Models.Messages;
using OkapiLauncher.Services;
using OkapiLauncher.ViewModels;
using OkapiLauncher.Views;

namespace OkapiLauncher;

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
    public bool ShouldCloseAfterLaunching { get;  set; } = false;

    private async void OnStartup(object sender, StartupEventArgs startupArgs)
    {
        if (startupArgs.Args.Length > 1)
        {
            MessageBox.Show($"Launcher expects at most one argument.\nProvided arguments: {startupArgs.Args.Length}.", "Invalid startup arguments", MessageBoxButton.OK, MessageBoxImage.Error);
            throw new ArgumentException("Received too many arguments.",nameof(startupArgs));
        }
        var appLocation = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location)!;

        // For more information about .NET generic host see  https://docs.microsoft.com/aspnet/core/fundamentals/host/generic-host?view=aspnetcore-3.0
        _host = Host.CreateDefaultBuilder(startupArgs.Args)
                .ConfigureAppConfiguration(c =>
                {
                    c.SetBasePath(appLocation);
                })
                .ConfigureServices(ConfigureServices)
                .Build();
        await _host.StartAsync();
        // initialize launcher vm, so that it can start listening to FileRequestMessages
        GetService<FileOpenerBroker>();
        if (startupArgs.Args.Length == 1)
        {
            ShouldCloseAfterLaunching = true;
            GetService<IMessenger>().Send(new FileRequestedMessage(startupArgs.Args[0]));
        }
    }

    private void ConfigureServices(HostBuilderContext context, IServiceCollection services)
    {
        // App Host
        services.AddHostedService<ApplicationHostService>();

        // Activation Handlers

        // Core Services
        services.AddSingleton<IFileService, FileService>();

        // Services
        services.AddSingleton<IWindowManagerService, WindowManagerService>();
        services.AddSingleton<IDialogCoordinator, DialogCoordinator>();
        services.AddSingleton<IApplicationInfoService, ApplicationInfoService>();
        services.AddSingleton<ISystemService, SystemService>();
        services.AddSingleton<IPersistAndRestoreService, PersistAndRestoreService>();
        services.AddSingleton<IThemeSelectorService, ThemeSelectorService>();
        services.AddSingleton<IPageService, PageService>();
        services.AddSingleton<IContentDialogService, ContentDialogService>();
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<IRequestedFilesService, RequestedFilesService>();
        services.AddSingleton<IMessenger, StrongReferenceMessenger>();
        services.AddSingleton<IAvAppFacadeFactory, AvAppFacadeFactory>();
        services.AddSingleton<IRecentlyOpenedFilesService, RecentlyOpenedFilesService>();
        services.AddSingleton<IGeneralSettingsService, GeneralSettingsService>();
        services.AddSingleton<IFileAssociationService, FileAssociationService>();
        services.AddSingleton<IUpdateCheckService, UpdateCheckService>();
        services.AddSingleton<FileOpenerBroker>();
        services.AddSingleton<ICustomAppSourceService,CustomAppSourceService>();
        services.AddSingleton<IJumpListService,JumpListService>();
        services.AddSingleton<IAppNativeRecentFilesService, AppNativeRecentFilesService>();

        services.AddSingleton<IProcessManagerService, ProcessManagerService>();
        // Views and ViewModels
        services.AddSingleton<IShellWindow, ShellWindow>();
        services.AddSingleton<ShellViewModel>();

        services.AddTransient<LauncherViewModel>();
        services.AddTransient<LauncherPage>();

        services.AddTransient<ProcessOverviewPage>();
        services.AddTransient<ProcessOverviewViewModel>();

        services.AddTransient<SettingsViewModel>();
        services.AddTransient<SettingsPage>();

        services.AddTransient<IShellDialogWindow, ShellDialogWindow>();
        services.AddTransient<ShellDialogViewModel>();

        services.AddTransient<InstalledAppsViewModel>();
        services.AddTransient<InstalledAppsPage>();

        services.AddTransient<AboutViewModel>();
        services.AddTransient<AboutPage>();

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
        MessageBox.Show(e.Exception.ToString(), "Error", MessageBoxButton.OK,MessageBoxImage.Error);
    }
}

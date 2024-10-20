using System.IO;
using System.Reflection;
using System.Windows;
using OkapiLauncher.Contracts.Services;
using OkapiLauncher.Contracts.Views;
using OkapiLauncher.Core.Contracts.Services;
using OkapiLauncher.Core.Services;
using OkapiLauncher.Models;
using OkapiLauncher.Services;
using OkapiLauncher.ViewModels;
using OkapiLauncher.Views;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OkapiLauncher.Tests.MSTest;

[TestClass]
public class PagesTests
{
    private readonly IHost _host;
    [ClassInitialize]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1806:Do not ignore method results", Justification = "<Pending>")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
    public static void Init(TestContext testContext)
    {
        if (System.Windows.Application.Current == null)
        { new System.Windows.Application { ShutdownMode = ShutdownMode.OnExplicitShutdown }; }
    }

    public PagesTests()
    {
        var appLocation = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location);
        _host = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration(c => c.SetBasePath(appLocation))
            .ConfigureServices(ConfigureServices)
            .Build();
    }

    private void ConfigureServices(HostBuilderContext context, IServiceCollection services)
    {
        // Core Services
        services.AddSingleton<IFileService, FileService>();

        services.AddSingleton<IWindowManagerService, WindowManagerService>();
        services.AddSingleton<IApplicationInfoService, ApplicationInfoService>();
        services.AddSingleton<ISystemService, SystemService>();
        services.AddSingleton<IPersistAndRestoreService, PersistAndRestoreService>();
        services.AddSingleton<IThemeSelectorService, ThemeSelectorService>();
        services.AddSingleton<IPageService, PageService>();
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<IRequestedFilesService, RequestedFilesService>();
        services.AddSingleton<IMessenger, StrongReferenceMessenger>();
        services.AddSingleton<IAvAppFacadeFactory, AvAppFacadeFactory>();
        services.AddSingleton<IRecentlyOpenedFilesService, RecentlyOpenedFilesService>();
        services.AddSingleton<IFileAssociationService, FileAssociationService>();
        services.AddSingleton<FileOpenerBroker>();

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

        // Configuration
        services.Configure<AppConfig>(context.Configuration.GetSection(nameof(AppConfig)));
    }

    // TODO: Add tests for functionality you add to SettingsViewModel.
    [TestMethod]
    public void TestSettingsViewModelCreation()
    {
        var vm = _host.Services.GetService(typeof(SettingsViewModel));
        Assert.IsNotNull(vm);
    }

    [TestMethod]
    public void TestGetSettingsPageType()
    {
        if (_host.Services.GetService(typeof(IPageService)) is IPageService pageService)
        {
            var pageType = pageService.GetPageType(typeof(SettingsViewModel).FullName);
            Assert.AreEqual(typeof(SettingsPage), pageType);
        }
        else
        {
            Assert.Fail($"Can't resolve {nameof(IPageService)}");
        }
    }

    // TODO: Add tests for functionality you add to MainViewModel.
    [TestMethod]
    public void TestLauncherViewModelCreation()
    {
        var vm = _host.Services.GetService(typeof(LauncherViewModel));
        Assert.IsNotNull(vm);
    }

    // TODO: Add tests for functionality you add to MainViewModel.
    [TestMethod]
    public void TestInstalledAppsViewModelCreation()
    {
        var vm = _host.Services.GetService(typeof(InstalledAppsViewModel));
        Assert.IsNotNull(vm);
    }

    [TestMethod]
    public void TestGetLauncherPageType()
    {
        if (_host.Services.GetService(typeof(IPageService)) is IPageService pageService)
        {
            var pageType = pageService.GetPageType(typeof(LauncherViewModel).FullName);
            Assert.AreEqual(typeof(LauncherPage), pageType);
        }
        else
        {
            Assert.Fail($"Can't resolve {nameof(IPageService)}");
        }
    }

    
}

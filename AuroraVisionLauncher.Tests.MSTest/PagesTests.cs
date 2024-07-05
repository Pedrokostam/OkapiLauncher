using System.IO;
using System.Reflection;

using AuroraVisionLauncher.Contracts.Services;
using AuroraVisionLauncher.Core.Contracts.Services;
using AuroraVisionLauncher.Core.Services;
using AuroraVisionLauncher.Models;
using AuroraVisionLauncher.Services;
using AuroraVisionLauncher.ViewModels;
using AuroraVisionLauncher.Views;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AuroraVisionLauncher.Tests.MSTest;

[TestClass]
public class PagesTests
{
    private readonly IHost _host;

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

        // Services
        services.AddSingleton<IThemeSelectorService, ThemeSelectorService>();
        services.AddSingleton<ISystemService, SystemService>();
        services.AddSingleton<IPersistAndRestoreService, PersistAndRestoreService>();
        services.AddSingleton<IApplicationInfoService, ApplicationInfoService>();
        services.AddSingleton<IPageService, PageService>();
        services.AddSingleton<INavigationService, NavigationService>();

        // ViewModels
        services.AddTransient<SettingsViewModel>();
        services.AddSingleton<LauncherViewModel>();

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

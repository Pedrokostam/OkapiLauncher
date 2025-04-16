using System.Windows.Controls;

using CommunityToolkit.Mvvm.ComponentModel;

using OkapiLauncher.Contracts.Services;
using OkapiLauncher.Exceptions;
using OkapiLauncher.ViewModels;
using OkapiLauncher.Views;

namespace OkapiLauncher.Services;

public class PageService : IPageService
{
    private readonly Dictionary<string, Type> _pages = new(StringComparer.Ordinal);
    private readonly IServiceProvider _serviceProvider;

    public PageService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        Configure<LauncherViewModel, LauncherPage>();
        Configure<SettingsViewModel, SettingsPage>();
        Configure<InstalledAppsViewModel, InstalledAppsPage>();
        Configure<ProcessOverviewViewModel, ProcessOverviewPage>();
        Configure<AboutViewModel, AboutPage>();
    }

    public Type GetPageType(string key)
    {
        Type? pageType;
        lock (_pages)
        {
            if (!_pages.TryGetValue(key, out pageType))
            {
                throw new PageNotFoundException(key);
            }
        }

        return pageType;
    }

    public Page? GetPage(string key)
    {
        var pageType = GetPageType(key);
        return _serviceProvider.GetService(pageType) as Page;
    }

    private void Configure<VM, V>()
        where VM : ObservableObject
        where V : Page
    {
        lock (_pages)
        {
            var key = typeof(VM).FullName!;
            PageException.ThrowOnDuplicateKey<VM>(_pages);

            var type = typeof(V);
            PageException.ThrowOnDuplicateView<V>(_pages);

            _pages.Add(key, type);
        }
    }
}

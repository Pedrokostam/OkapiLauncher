using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using AuroraVisionLauncher.Contracts;
using AuroraVisionLauncher.Contracts.Services;
using AuroraVisionLauncher.Contracts.ViewModels;
using AuroraVisionLauncher.Contracts.Views;
using AuroraVisionLauncher.Core.Models.Apps;
using AuroraVisionLauncher.Models;
using AuroraVisionLauncher.ViewModels;
using AuroraVisionLauncher.Views;
using ControlzEx.Theming;
using MahApps.Metro.Controls;
using Windows.ApplicationModel.VoiceCommands;

namespace AuroraVisionLauncher.Services;

public class WindowManagerService : IWindowManagerService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IPageService _pageService;

    public Window MainWindow
        => Application.Current.MainWindow;

    public WindowManagerService(IServiceProvider serviceProvider, IPageService pageService)
    {
        _serviceProvider = serviceProvider;
        _pageService = pageService;
    }
    private string GetWindowTitle(string key, object? parameter)
    {
        if(string.Equals(key,typeof(ProcessOverviewViewModel).FullName, StringComparison.Ordinal))
        {
            if (parameter is AvAppFacade app)
            {
                return $"{app.NameWithVersion} - Process Overview";
            }
            return $"Process Overview";
        }
        return (string)(Application.Current.FindResource("AppDisplayName"));
    }

    public void OpenInNewWindow(string key, object? parameter = null)
    {
        var window = GetWindow(key);
        if (window != null )
        {
            window.Activate();
        }
        else
        {
            window = new MetroWindow()
            {
                Title = GetWindowTitle(key, parameter),
                Style = Application.Current.FindResource("CustomMetroWindow") as Style,
            };
            var frame = new Frame()
            {
                Focusable = false,
                NavigationUIVisibility = NavigationUIVisibility.Hidden,
            };

            window.Content = frame;
            var page = _pageService.GetPage(key);
            window.Closed += OnWindowClosed;
            window.Show();
            frame.Navigated += OnNavigated;
            var navigated = frame.Navigate(page, parameter);
            // Rather ugly, but apparently the new window does not the theme applied
            // It only applies after it has changed
            // Changing the theme to the current one wont dot, because the themes are exactly the same
            // So we momentarily change to the inverse theme.
            var currentTheme = ThemeManager.Current.DetectTheme(Application.Current)!;
            var inverse = ThemeManager.Current.GetInverseTheme(currentTheme);
            ThemeManager.Current.ChangeTheme(window, inverse!);
            ThemeManager.Current.ChangeTheme(window, currentTheme);
        }
    }

    public bool? OpenInDialog(string key, object? parameter = null)
    {
        var shellWindow = (Window)_serviceProvider.GetService(typeof(IShellDialogWindow))!;
        var frame = ((IShellDialogWindow)shellWindow).GetDialogFrame();
        frame.Navigated += OnNavigated;
        shellWindow.Closed += OnWindowClosed;
        var page = _pageService.GetPage(key);
        var navigated = frame.Navigate(page, parameter);
        return shellWindow.ShowDialog();
    }

    public Window? GetWindow(string key)
    {
        foreach (Window window in Application.Current.Windows)
        {
            var dataContext = window.GetDataContext();
            if (string.Equals(dataContext?.GetType().FullName, key, StringComparison.Ordinal) && dataContext is not ITransientWindow)
            {
                return window;
            }
        }

        return null;
    }

    private void OnNavigated(object sender, NavigationEventArgs e)
    {
        if (sender is Frame frame)
        {
            var dataContext = frame.GetDataContext();
            if (dataContext is INavigationAware navigationAware)
            {
                navigationAware.OnNavigatedTo(e.ExtraData);
            }
        }
    }

    private void OnWindowClosed(object? sender, EventArgs e)
    {
        if (sender is Window window)
        {
            if (window.Content is Frame frame)
            {
                frame.Navigated -= OnNavigated;
            }

            window.Closed -= OnWindowClosed;
        }
    }

    public void CloseChildWindows()
    {
        foreach (Window window in Application.Current.Windows)
        {
            if (window is not ShellWindow)
            {
                window.Close();
            }
        }
    }
}

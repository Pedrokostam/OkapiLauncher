using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using OkapiLauncher.Contracts;
using OkapiLauncher.Contracts.Services;
using OkapiLauncher.Contracts.ViewModels;
using OkapiLauncher.Contracts.Views;
using OkapiLauncher.Models;
using OkapiLauncher.ViewModels;
using OkapiLauncher.Views;
using ControlzEx.Theming;
using MahApps.Metro.Controls;
using OkapiLauncher.Helpers;

namespace OkapiLauncher.Services;

public class WindowManagerService : IWindowManagerService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IPageService _pageService;

    public Window MainWindow
        => Application.Current.MainWindow;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0290:Use primary constructor", Justification = "The fields should be readonly")]
    public WindowManagerService(IServiceProvider serviceProvider, IPageService pageService)
    {
        _serviceProvider = serviceProvider;
        _pageService = pageService;
    }
    private static string GetWindowTitle(string key, object? parameter)
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
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
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

        var currentTheme = ThemeManager.Current.DetectTheme(Application.Current)!;
        var inverse = ThemeManager.Current.GetInverseTheme(currentTheme);
        ThemeManager.Current.ChangeTheme(shellWindow, inverse!);
        ThemeManager.Current.ChangeTheme(shellWindow, currentTheme);

        return shellWindow.ShowDialog();
    }

    //public bool? OpenSourceEditingWindows(CustomAppSource source)
    //{

    //    //var shellWindow = (Window)_serviceProvider.GetService(typeof(CustomSourceEditingWindow))!;
    //    //shellWindow.DataContext= source;
    //    ////var frame = ((IShellDialogWindow)shellWindow).GetDialogFrame();
    //    ////frame.Navigated += OnNavigated;
    //    ////shellWindow.Closed += OnWindowClosed;
    //    ////var page = _pageService.GetPage(key);
    //    ////var navigated = frame.Navigate(page, parameter);
    //    //shellWindow.Owner = (Window)_serviceProvider.GetService<IShellWindow>()!;
    //    //return shellWindow.ShowDialog();
    //}

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
                if(frame.GetDataContext() is INavigationAware navigationAware)
                {
                    navigationAware.OnNavigatedFrom();
                }
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

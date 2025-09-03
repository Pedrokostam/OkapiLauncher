using System.Windows.Controls;
using System.Windows.Navigation;

using OkapiLauncher.Contracts.Services;
using OkapiLauncher.Contracts.ViewModels;

using MahApps.Metro.Controls;
using OkapiLauncher.Contracts.EventArgs;

namespace OkapiLauncher.Services;

public class RightPaneService : IRightPaneService
{
    private readonly IPageService _pageService;
    private Frame _frame =default!;
    private object? _lastParameterUsed;
    private SplitView _splitView = default!;

    public event EventHandler PaneOpened = default!;

    public event EventHandler PaneClosed = default!;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0290:Use primary constructor", Justification = "The fields should be readonly")]
    public RightPaneService(IPageService pageService)
    {
        _pageService = pageService;
    }

    public void Initialize(Frame rightPaneFrame, SplitView splitView)
    {
        _frame = rightPaneFrame;
        _splitView = splitView;
        _frame.Navigated += OnNavigated;
        _splitView.PaneClosed += OnPaneClosed;
    }

    public void CleanUp()
    {
        _frame.Navigated -= OnNavigated;
        _splitView.PaneClosed -= OnPaneClosed;
    }

    public void OpenInRightPane(string pageKey, object? parameter = null)
    {
        var pageType = _pageService.GetPageType(pageKey);
        if (_frame.Content?.GetType() != pageType || (parameter != null && !parameter.Equals(_lastParameterUsed)))
        {
            var page = _pageService.GetPage(pageKey);
            var navigated = _frame.Navigate(page, parameter);
            if (navigated)
            {
                _lastParameterUsed = parameter;
                var dataContext = _frame.GetDataContext();
                if (dataContext is INavigationAware navigationAware)
                {
                    navigationAware.OnNavigatedFrom();
                }
            }
        }

        _splitView.IsPaneOpen = true;
        PaneOpened?.Invoke(this, EventArgs.Empty);
    }

    private void OnNavigated(object? sender, NavigationEventArgs e)
    {
        if (sender is Frame frame)
        {
            frame.CleanNavigation();
            var dataContext = frame.GetDataContext();
            if (dataContext is INavigationAware navigationAware)
            {
                navigationAware.OnNavigatedTo(e.ExtraData);
            }
        }
    }

    private void OnPaneClosed(object? sender, EventArgs e)
        => PaneClosed?.Invoke(this, e);

    public void OpenInRightPane<T>(object? parameter = null)=>OpenInRightPane(typeof(T).FullName!, parameter);
   
}

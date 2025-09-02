using System.Windows.Controls;
using System.Windows.Navigation;
using OkapiLauncher.Contracts.EventArgs;
using OkapiLauncher.Contracts.Services;
using OkapiLauncher.Contracts.ViewModels;

namespace OkapiLauncher.Services;

public class NavigationService : INavigationService
{
    private readonly IPageService _pageService;
    private Frame _frame=default!;
    private object? _lastParameterUsed;

    public event EventHandler<NavigatedToEventArgs>? Navigated;

    public bool CanGoBack => _frame.CanGoBack;

    public object? CurrentDataContext => _frame.GetDataContext();

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0290:Use primary constructor", Justification = "The fields should be readonly")]
    public NavigationService(IPageService pageService)
    {
        _pageService = pageService;
    }

    public void Initialize(Frame shellFrame)
    {
        if (_frame == null)
        {
            _frame = shellFrame;
            _frame.Navigated += OnNavigated;
        }
    }

    public void UnsubscribeNavigation()
    {
        _frame.Navigated -= OnNavigated;
        _frame = null!;
    }

    public void GoBack()
    {
        if (_frame.CanGoBack)
        {
            var vmBeforeNavigation = _frame.GetDataContext();
            _frame.GoBack();
            if (vmBeforeNavigation is INavigationAware navigationAware)
            {
                navigationAware.OnNavigatedFrom();
            }
        }
    }

    public bool NavigateTo<ViewModelType>(object? parameter = null)=>NavigateTo(typeof(ViewModelType).FullName!, parameter);
    public bool NavigateTo(string pageKey, object? parameter = null)
    {
        var pageType = _pageService.GetPageType(pageKey);

        if (_frame.Content?.GetType() != pageType || (parameter != null && !parameter.Equals(_lastParameterUsed)))
        {
            //_frame.Tag = clearNavigation;
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

            return navigated;
        }

        return false;
    }

    public void CleanNavigation()
        => _frame.CleanNavigation();

    private void OnNavigated(object sender, NavigationEventArgs e)
    {
        if (sender is Frame frame)
        {
            //bool clearNavigation = (bool)frame.Tag;
            //if (clearNavigation)
            //{
            //    frame.CleanNavigation();
            //}
            frame?.CleanNavigation();

            var dataContext = frame?.GetDataContext();
            if (dataContext is INavigationAware navigationAware)
            {
                navigationAware.OnNavigatedTo(e.ExtraData);
            }
            Navigated?.Invoke(this, NavigatedToEventArgs.FromDataContext(dataContext));
        }
    }

   
}

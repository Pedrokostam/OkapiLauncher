using System.Windows.Controls;

namespace AuroraVisionLauncher.Contracts.Services;

public interface INavigationService
{
    event EventHandler<string> Navigated;

    bool CanGoBack { get; }
    object? CurrentDataContext { get; }

    void Initialize(Frame shellFrame);

    /// <summary>
    /// Attempts to navigate to the appropriate page.
    /// </summary>
    /// <param name="viewModelTypeName"></param>
    /// <param name="parameter"></param>
    /// <returns><see langword="true"/> if successful navigation; <see langword="false"/> if not</returns>
    bool NavigateTo(string viewModelTypeName, object? parameter = null);

    void GoBack();

    void UnsubscribeNavigation();

    void CleanNavigation();
    /// <summary>
    /// Attempts to navigate to the appropriate page.
    /// </summary>
    /// <param name="parameter"></param>
    /// <returns><see langword="true"/> if successful navigation; <see langword="false"/> if not</returns>
    bool NavigateTo<ViewModelType>(object? parameter = null);
}

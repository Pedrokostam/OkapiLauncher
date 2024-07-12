using System.Windows.Controls;

namespace AuroraVisionLauncher.Contracts.Services;

public interface INavigationService
{
    event EventHandler<string> Navigated;

    bool CanGoBack { get; }

    void Initialize(Frame shellFrame);

    bool NavigateTo(string pageKey, object? parameter = null);

    void GoBack();

    void UnsubscribeNavigation();

    void CleanNavigation();
}

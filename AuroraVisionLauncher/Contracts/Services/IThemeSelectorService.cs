using AuroraVisionLauncher.Models;

namespace AuroraVisionLauncher.Contracts.Services;

public interface IThemeSelectorService
{
    void InitializeTheme();

    void SetTheme(AppTheme theme);

    AppTheme GetCurrentTheme();
}

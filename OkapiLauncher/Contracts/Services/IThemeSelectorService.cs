using System.Windows.Media;
using AuroraVisionLauncher.Models;

namespace AuroraVisionLauncher.Contracts.Services;

public interface IThemeSelectorService
{
    void InitializeTheme();

    void SetTheme(AppTheme theme,Color? customColor);

    AppTheme GetCurrentTheme();
    Color? GetCurrentAccent();
}

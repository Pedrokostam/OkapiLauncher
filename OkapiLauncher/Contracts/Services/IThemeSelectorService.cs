using System.Windows.Media;
using OkapiLauncher.Models;

namespace OkapiLauncher.Contracts.Services;

public interface IThemeSelectorService
{
    void InitializeTheme();

    void SetTheme(AppTheme theme,Color? customColor);

    AppTheme GetCurrentTheme();
    Color? GetCurrentAccent();
}

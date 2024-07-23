using System.Windows;
using System.Windows.Media;
using AuroraVisionLauncher.Contracts.Services;
using AuroraVisionLauncher.Models;

using ControlzEx.Theming;

using MahApps.Metro.Theming;
using Newtonsoft.Json.Linq;

namespace AuroraVisionLauncher.Services;

public class ThemeSelectorService : IThemeSelectorService
{
    private const string _hcDarkTheme = "pack://application:,,,/Styles/Themes/HC.Dark.Blue.xaml";
    private const string _hcLightTheme = "pack://application:,,,/Styles/Themes/HC.Light.Blue.xaml";
    private const string DarkTheme = "pack://application:,,,/Styles/Themes/Dark.xaml";
    private const string LightTheme = "pack://application:,,,/Styles/Themes/Light.xaml";
    private const string _customThemeColorKey = "CustomThemeColor";
    private const string _themeKey = "Theme";

    public ThemeSelectorService()
    {
    }

    public void InitializeTheme()
    {
        // TODO: Mahapps.Metro supports syncronization with high contrast but you have to provide custom high contrast themes
        // We've added basic high contrast dictionaries for Dark and Light themes
        // Please complete these themes following the docs on https://mahapps.com/docs/themes/thememanager#creating-custom-themes
        ThemeManager.Current.AddLibraryTheme(new LibraryTheme(new Uri(_hcDarkTheme), MahAppsLibraryThemeProvider.DefaultInstance));
        ThemeManager.Current.AddLibraryTheme(new LibraryTheme(new Uri(_hcLightTheme), MahAppsLibraryThemeProvider.DefaultInstance));
        ThemeManager.Current.AddLibraryTheme(new LibraryTheme(new Uri(DarkTheme), MahAppsLibraryThemeProvider.DefaultInstance));
        ThemeManager.Current.AddLibraryTheme(new LibraryTheme(new Uri(LightTheme), MahAppsLibraryThemeProvider.DefaultInstance));

        var theme = GetCurrentTheme();
        var color = GetCurrentAccent();
        SetTheme(theme, color);
    }

    public void SetTheme(AppTheme themeEnum, Color? customColor)
    {
        SyncTheme(themeEnum);
        if (themeEnum != AppTheme.System)
        {
            if (customColor is null)
            {
                ThemeManager.Current.ChangeTheme(Application.Current, $"{themeEnum}", SystemParameters.HighContrast);
            }
            else
            {
                Theme newTheme = new Theme(name: "CustomTheme",
                                   displayName: "CustomTheme",
                                   baseColorScheme: themeEnum.ToString(),
                                   colorScheme: "CustomAccent",
                                   primaryAccentColor: customColor.Value,
                                   showcaseBrush: new SolidColorBrush(customColor.Value),
                                   isRuntimeGenerated: true,
                                   isHighContrast: false);
                var baseTheme = ThemeManager.Current.GetTheme($"{themeEnum}", SystemParameters.HighContrast)!;
                foreach (var key in baseTheme.Resources.Keys)
                {
                    if (key is not string customKey || !customKey.StartsWith("Custom.", StringComparison.Ordinal))
                    {
                        continue;
                    }
                    newTheme.Resources[customKey] = baseTheme.Resources[customKey];
                }
                ThemeManager.Current.ChangeTheme(Application.Current, newTheme);
            }
        }

        App.Current.Properties[_themeKey] = themeEnum.ToString();
        App.Current.Properties[_customThemeColorKey] = customColor;
    }

    private static void SyncTheme(AppTheme themeEnum)
    {
        if (themeEnum == AppTheme.System)
        {
            ThemeManager.Current.ThemeSyncMode = ThemeSyncMode.SyncAll;
            ThemeManager.Current.SyncTheme();
        }
        else
        {
            ThemeManager.Current.ThemeSyncMode = ThemeSyncMode.SyncWithHighContrast;
            ThemeManager.Current.SyncTheme();
        }
    }

    public AppTheme GetCurrentTheme()
    {
        if (App.Current.Properties.Contains(_themeKey))
        {
            var themeName = App.Current.Properties[_themeKey]!.ToString();
            if (Enum.TryParse(themeName, out AppTheme theme))
            {
                return theme;
            }
            return AppTheme.System;
        }

        return AppTheme.System;
    }

    public Color? GetCurrentAccent()
    {
        if (App.Current.Properties.Contains(_customThemeColorKey))
        {
            var currcol = App.Current.Properties[_customThemeColorKey];
            if (currcol is string s)
            {
                return (Color?)ColorConverter.ConvertFromString(s);
            }
            else if (currcol is Color c)
            {
                return c;
            }
            return null;
        }
        return null;
    }
}

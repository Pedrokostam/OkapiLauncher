using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using AuroraVisionLauncher.Contracts.Services;
using AuroraVisionLauncher.Helpers;
using AuroraVisionLauncher.Models;

using ControlzEx.Theming;

using MahApps.Metro.Theming;

namespace AuroraVisionLauncher.Services;
/*
Taupe - #FF87794E
Cyan - #FF1BA1E2
Mauve - #FF76608A
Green - #FF60A917
Sienna - #FFA0522D
Olive - #FF6D8764
Green - #FF60A917
Emerald - #FF008A00
Violet - #FFAA00FF
Steel - #FF647687
Teal - #FF00ABA9
Blue - #FF0078D7
Yellow - #FFFEDE06
Red - #FFE51400
Purple - #FF6459DF
Magenta - #FFD80073
Orange - #FFFA6800
Indigo - #FF6A00FF
Emerald - #FF008A00
Pink - #FFF472D0
Teal - #FF00ABA9
Sienna - #FFA0522D
Indigo - #FF6A00FF
Cyan - #FF1BA1E2
Lime - #FFA4C400
Magenta - #FFD80073
Red - #FFE51400
Taupe - #FF87794E
Violet - #FFAA00FF
Yellow - #FFFEDE06
Steel - #FF647687
Brown - #FF825A2C
Olive - #FF6D8764
Brown - #FF825A2C
Cobalt - #FF0050EF
Orange - #FFFA6800
Crimson - #FFA20025
Purple - #FF6459DF
Amber - #FFF0A30A
Lime - #FFA4C400
Cobalt - #FF0050EF
Amber - #FFF0A30A
Crimson - #FFA20025
Blue - #FF0078D7
Pink - #FFF472D0
Mauve - #FF76608A
 */

public class ThemeSelectorService : IThemeSelectorService
{
    //private const string _hcDarkTheme = "pack://application:,,,/Styles/Themes/HC.Dark.Blue.xaml";
    //private const string _hcLightTheme = "pack://application:,,,/Styles/Themes/HC.Light.Blue.xaml";
    private const string DarkTheme = "pack://application:,,,/Styles/Themes/Dark.xaml";
    private const string LightTheme = "pack://application:,,,/Styles/Themes/Light.xaml";
    private const string CustomThemeColorKey = "CustomThemeColor";
    private const string ThemeKey = "Theme";

    private static readonly ResourceDictionary OtherDark = new ResourceDictionary() { Source = new Uri("pack://application:,,,/Styles/Themes/Dark.Other.xaml") };
    private static readonly ResourceDictionary OtherLight = new ResourceDictionary() { Source = new Uri("pack://application:,,,/Styles/Themes/Light.Other.xaml") };
    /// <summary>
    /// Is a dependency to ensure its instantiated before.
    /// </summary>
    private readonly IPersistAndRestoreService _persistAndRestoreService;
    private bool _initialized;

    public ThemeSelectorService(IPersistAndRestoreService persistAndRestoreService)
    {
        _persistAndRestoreService = persistAndRestoreService;
        if (_persistAndRestoreService.IsDataRestored)
        {
            InitializeData();
        }
        _persistAndRestoreService.DataRestored += _persistAndRestoreService_DataRestored;
    }

    private void _persistAndRestoreService_DataRestored(object? sender, EventArgs e)
    {
        InitializeData();
    }

    private void InitializeData()
    {
        if (!_initialized && _persistAndRestoreService.IsDataRestored)
        {
            App.Current.Properties.InitializeDictKey<Color?>(CustomThemeColorKey);
            App.Current.Properties.InitializeDictKey<AppTheme>(ThemeKey);
            _initialized = true;
        }
    }

    public void InitializeTheme()
    {
        //foreach (var t in ThemeManager.Current.Themes)
        //{
        //    Debug.WriteLine($"{t.ColorScheme} - {t.PrimaryAccentColor}");
        //}
        // TODO: Mahapps.Metro supports syncronization with high contrast but you have to provide custom high contrast themes
        // We've added basic high contrast dictionaries for Dark and Light themes
        // Please complete these themes following the docs on https://mahapps.com/docs/themes/thememanager#creating-custom-themes
        //ThemeManager.Current.AddLibraryTheme(new LibraryTheme(new Uri(DarkTheme), libraryThemeProvider: null));
        //ThemeManager.Current.AddLibraryTheme(new LibraryTheme(new Uri(LightTheme), libraryThemeProvider: null));
        //ThemeManager.Current.AddLibraryTheme(new LibraryTheme(new Uri(_hcDarkTheme), MahAppsLibraryThemeProvider.DefaultInstance));
        //ThemeManager.Current.AddLibraryTheme(new LibraryTheme(new Uri(_hcLightTheme), MahAppsLibraryThemeProvider.DefaultInstance));
        SetTheme(SelectedTheme, SelectedCustomColorAccent);
    }

    public void SetTheme(AppTheme themeEnum, Color? customColor)
    {
        SyncTheme(themeEnum);
        Theme activeTheme;
        if (themeEnum != AppTheme.System)
        {
            string baseColorName = themeEnum switch { AppTheme.Light => "Red", _ => "Yellow" };
            string themeName = $"{themeEnum}.{baseColorName}";
            if (customColor is null)
            {
                activeTheme = ThemeManager.Current.ChangeTheme(Application.Current, themeName, false)!;
            }
            else
            {
                activeTheme = RuntimeThemeGenerator.Current.GenerateRuntimeTheme(themeEnum.ToString(), customColor.Value)!;
            }
        }
        else
        {
            activeTheme = ThemeManager.Current.DetectTheme()!;
        }

        var baseColor = themeEnum switch
        {
            AppTheme.System => Enum.Parse<AppTheme>(WindowsThemeHelper.GetWindowsBaseColor()),
            _ => themeEnum,
        };
        var other = baseColor == AppTheme.Light ? OtherLight : OtherDark;
        foreach (var key in other.Keys)
        {
            activeTheme.Resources[key] = other[key];
        }
        ThemeManager.Current.ChangeTheme(Application.Current, activeTheme);
        SelectedTheme = themeEnum;
        SelectedCustomColorAccent = customColor;
    }
    /// <summary>
    /// Sync with general theme (dark/light) and accent color.
    /// </summary>
    private const ThemeSyncMode SystemThemeSyncMode = ThemeSyncMode.SyncWithAppMode | ThemeSyncMode.SyncWithAccent;
    /// <summary>
    /// Do not sync at all.
    /// </summary>
    private const ThemeSyncMode ManualThemeSyncMode = ThemeSyncMode.DoNotSync;
    private static void SyncTheme(AppTheme themeEnum)
    {
        ThemeManager.Current.ThemeSyncMode = themeEnum == AppTheme.System ? SystemThemeSyncMode : ManualThemeSyncMode;
        ThemeManager.Current.SyncTheme();
    }

    AppTheme IThemeSelectorService.GetCurrentTheme() => SelectedTheme;

    Color? IThemeSelectorService.GetCurrentAccent() => SelectedCustomColorAccent;
    public AppTheme SelectedTheme
    {
        get
        {
            return (AppTheme)App.Current.Properties[ThemeKey]!;
        }
        set => App.Current.Properties[ThemeKey] = value;
    }

    public Color? SelectedCustomColorAccent
    {
        get
        {
            return App.Current.Properties[CustomThemeColorKey] as Color?;
        }
        set => App.Current.Properties[CustomThemeColorKey] = value;
    }
}

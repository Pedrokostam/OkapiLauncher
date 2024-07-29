using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using AuroraVisionLauncher.Contracts.Services;
using AuroraVisionLauncher.Models;

using ControlzEx.Theming;

using MahApps.Metro.Theming;
using Newtonsoft.Json.Linq;

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

public class SuperLibraryThemeProvider : LibraryThemeProvider
{
    /// <inheritdoc/>
    public static new readonly SuperLibraryThemeProvider DefaultInstance = new SuperLibraryThemeProvider();

    public SuperLibraryThemeProvider():base(true)
    {
            
    }

    public override LibraryTheme? GetLibraryTheme(DictionaryEntry dictionaryEntry)
    {
        return base.GetLibraryTheme(dictionaryEntry);
    }

    public override IEnumerable<LibraryTheme> GetLibraryThemes()
    {
        return base.GetLibraryThemes();
    }

    protected override bool IsPotentialThemeResourceDictionary(DictionaryEntry dictionaryEntry)
    {
        return base.IsPotentialThemeResourceDictionary(dictionaryEntry);
    }

    public override void FillColorSchemeValues(Dictionary<string, string> values, RuntimeThemeColorValues colorValues)
    {
        throw new ArgumentException();
        // Check if all needed parameters are not null
        if (values is null)
            throw new ArgumentNullException(nameof(values));
        if (colorValues is null)
            throw new ArgumentNullException(nameof(colorValues));

        // Add the values you like to override
        values.Add("MahApps.Colors.AccentBase", "[AccentBaseColor]");
        values.Add("MahApps.Colors.Accent", "[AccentColor]");
        values.Add("MahApps.Colors.Accent2", "[AccentColor2]");
        values.Add("MahApps.Colors.Accent3", "[AccentColor3]");
        values.Add("MahApps.Colors.Accent4", "[AccentColor4]");

        values.Add("MahApps.Colors.Highlight", "[HighlightColor]");
        values.Add("MahApps.Colors.IdealForeground", colorValues.IdealForegroundColor.ToString(CultureInfo.InvariantCulture));
    }

}

public class ThemeSelectorService : IThemeSelectorService
{
    //private const string _hcDarkTheme = "pack://application:,,,/Styles/Themes/HC.Dark.Blue.xaml";
    //private const string _hcLightTheme = "pack://application:,,,/Styles/Themes/HC.Light.Blue.xaml";
    private const string _darkTheme = "pack://application:,,,/Styles/Themes/Dark.xaml";
    private const string _lightTheme = "pack://application:,,,/Styles/Themes/Light.xaml";
    private const string _customThemeColorKey = "CustomThemeColor";
    private const string _themeKey = "Theme";

    private static readonly ResourceDictionary _otherDark = new ResourceDictionary() { Source = new Uri("pack://application:,,,/Styles/Themes/Dark.Other.xaml") };
    private static readonly ResourceDictionary _otherLight = new ResourceDictionary() { Source = new Uri("pack://application:,,,/Styles/Themes/Light.Other.xaml") };


    public ThemeSelectorService()
    {
     
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
        ThemeManager.Current.RegisterLibraryThemeProvider(SuperLibraryThemeProvider.DefaultInstance);
        ThemeManager.Current.AddLibraryTheme(new LibraryTheme(new Uri(_darkTheme), SuperLibraryThemeProvider.DefaultInstance));
        ThemeManager.Current.AddLibraryTheme(new LibraryTheme(new Uri(_lightTheme), SuperLibraryThemeProvider.DefaultInstance));
        //ThemeManager.Current.AddLibraryTheme(new LibraryTheme(new Uri(_hcDarkTheme), MahAppsLibraryThemeProvider.DefaultInstance));
        //ThemeManager.Current.AddLibraryTheme(new LibraryTheme(new Uri(_hcLightTheme), MahAppsLibraryThemeProvider.DefaultInstance));
        var theme = GetCurrentTheme();
        var color = GetCurrentAccent();
        SetTheme(theme, color);
    }

    public void SetTheme(AppTheme themeEnum, Color? customColor)
    {
        SyncTheme(themeEnum);
        Theme activeTheme;
        if (themeEnum != AppTheme.System)
        {
            if (customColor is null)
            {
                activeTheme = ThemeManager.Current.ChangeTheme(Application.Current, themeEnum.ToString(), false)!;
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
        var other = baseColor == AppTheme.Light ? _otherLight : _otherDark;
        foreach (var key in other.Keys)
        {
            activeTheme.Resources[key] = other[key];
        }
        ThemeManager.Current.ChangeTheme(Application.Current, activeTheme);
        App.Current.Properties[_themeKey] = themeEnum.ToString();
        App.Current.Properties[_customThemeColorKey] = customColor;
    }

    private static void SyncTheme(AppTheme themeEnum)
    {
            ThemeManager.Current.ThemeSyncMode = ThemeSyncMode.SyncAll;
        if (themeEnum == AppTheme.System)
        {
        }
            ThemeManager.Current.SyncTheme();
        //else
        //{
        //    ThemeManager.Current.ThemeSyncMode = ThemeSyncMode.SyncWithHighContrast;
        //    ThemeManager.Current.SyncTheme();
        //}
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

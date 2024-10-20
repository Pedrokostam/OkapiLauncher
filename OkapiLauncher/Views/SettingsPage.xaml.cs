using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using OkapiLauncher.ViewModels;
using ControlzEx.Theming;

namespace OkapiLauncher.Views;

public partial class SettingsPage : Page
{
    public SettingsPage(SettingsViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    private void ColorPicker_SelectedColorChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<System.Windows.Media.Color?> e)
    {
        //if (e.NewValue.HasValue)
        //{
        //    Theme newTheme = new Theme(name: "CustomTheme",
        //                               displayName: "CustomTheme",
        //                               baseColorScheme: "Light",
        //                               colorScheme: "CustomAccent",
        //                               primaryAccentColor: e.NewValue.Value,
        //                               showcaseBrush: new SolidColorBrush(e.NewValue.Value),
        //                               isRuntimeGenerated: true,
        //                               isHighContrast: false);

        //    ThemeManager.Current.ChangeTheme(Application.Current, newTheme);
        //}
    }

}

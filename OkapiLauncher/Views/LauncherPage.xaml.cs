using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using OkapiLauncher.ViewModels;

namespace OkapiLauncher.Views;

public partial class LauncherPage : Page
{
    public LauncherPage(LauncherViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    private void ThisLauncherPage_Loaded(object sender, System.Windows.RoutedEventArgs e)
    {
        LaunchButton.Focus();
    }

    private void LaunchButton_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (Keyboard.Modifiers != ModifierKeys.Control)
        {
            return;
        }
        if (e.Key == Key.Enter)
        {
            if (LaunchButton.IsEnabled)
            {
                LaunchButton.Command.Execute(LaunchButton.CommandParameter);
            }
            e.Handled = true;
            return;
        }
    }

    private void AppList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        AppList.ScrollIntoView(AppList.SelectedItem);
    }
}
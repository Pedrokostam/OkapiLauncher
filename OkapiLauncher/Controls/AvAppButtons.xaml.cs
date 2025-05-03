using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Material.Icons;
using OkapiLauncher.Controls.Utilities;
using OkapiLauncher.Models;

namespace OkapiLauncher.Controls;
/// <summary>
/// Interaction logic for AvAppButtons.xaml
/// </summary>
public partial class AvAppButtons : UserControl
{
    public ICommand LaunchCommand
    {
        get { return (ICommand)GetValue(LaunchCommandProperty); }
        set { SetValue(LaunchCommandProperty, value); }
    }

    // Using a DependencyProperty as the backing store for LaunchCommand.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty LaunchCommandProperty =
        DependencyProperty.Register(nameof(LaunchCommand), typeof(ICommand), typeof(AvAppButtons), new PropertyMetadata(defaultValue: null, OnLaunchCommandChanged));




    public Settings ButtonSettings
    {
        get { return (Settings)GetValue(ButtonSettingsProperty); }
        set { SetValue(ButtonSettingsProperty, value); }
    }

    // Using a DependencyProperty as the backing store for ButtonSettings.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty ButtonSettingsProperty =
        DependencyProperty.Register("ButtonSettings", typeof(Settings), typeof(AvAppButtons), new PropertyMetadata(new Settings(), OnButtonSettingsChanged));



    private static void OnLaunchCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {

        if (d is AvAppFacadeListItem userControl)
        {
            var kind = userControl.LaunchCommand is null ? MaterialIconKind.Launch : MaterialIconKind.Powershell;
            userControl.LaunchButton.SetIconKind(kind);
            var tooltip = userControl.LaunchCommand is null ? Properties.Resources.AvAppLaunchWithNoProgram : Properties.Resources.AvAppLaunchWithProgram;
            userControl.LaunchButton.ToolTip = tooltip;
            foreach (MenuItem menuItem in userControl.RootGrid.ContextMenu.Items.OfType<MenuItem>())
            {
                if (menuItem.Tag is bool tagged && tagged)
                {
                    menuItem.Header = tooltip;
                    return;
                }
            }
            // When the UserControlDataContext changes, update the DataContext of the root element
        }

    }

    public static readonly DependencyProperty AppFacadeProperty =
        DependencyProperty.Register(
            nameof(AppFacade),
            typeof(AvAppFacade),
            typeof(AvAppButtons),
            new PropertyMetadata(defaultValue: null, OnUserControlAppFacadeChanged));

    public AvAppFacade? AppFacade
    {
        get => GetValue(AppFacadeProperty) as AvAppFacade;
        set => SetValue(AppFacadeProperty, value);
    }

    private static void OnUserControlAppFacadeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is AvAppButtons userControl && !Equals(userControl.AppFacade, e.NewValue as AvAppFacade))
        {
            // When the UserControlDataContext changes, update the DataContext of the root element
            userControl.AppFacade = e.NewValue as AvAppFacade;
        }
    }

    private static void OnButtonSettingsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is AvAppButtons userControl && e.NewValue is Settings s)
        {
            userControl.ButtonSettings = s;
            foreach (var but in userControl.ButtonGrid.Children.OfType<Button>())
            {
                var tag = (ButtonVisibility)but.Tag;
                Grid.SetColumn(but, s.GetPosition(tag));
                bool isVisible = s.VisibleButtons.HasFlag(tag) && (but.IsEnabled || s.ShowDisabledButtons);
                but.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
            }
        }
    }
    public AvAppButtons()
    {
        InitializeComponent();
    }
}

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


    public ButtonSettings ButtonSettings
    {
        get { return (ButtonSettings)GetValue(ButtonSettingsProperty); }
        set { SetValue(ButtonSettingsProperty, value); }
    }

    // Using a DependencyProperty as the backing store for ButtonSettings.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty ButtonSettingsProperty =
        DependencyProperty.Register("ButtonSettings", typeof(ButtonSettings), typeof(AvAppButtons), new PropertyMetadata(new ButtonSettings(), OnButtonSettingsChanged));



    private static void OnLaunchCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is AvAppButtons userControl)
        {
            UpdateLaunchButton(userControl);
            //{
            //    if (menuItem.Tag is bool tagged && tagged)
            //    {
            //        menuItem.Header = tooltip;
            //        return;
            //    }
            //}
            // When the UserControlDataContext changes, update the DataContext of the root element
        }

    }

    private static void UpdateLaunchButton(AvAppButtons userControl)
    {
        var kind = userControl.LaunchCommand is null ? MaterialIconKind.Launch : MaterialIconKind.Powershell;
        userControl.LaunchButton.SetIconKind(kind);
        var tooltip = userControl.LaunchCommand is null ? Properties.Resources.AvAppLaunchWithNoProgram : Properties.Resources.AvAppLaunchWithProgram;
        userControl.LaunchButton.ToolTip = tooltip;
        //foreach (MenuItem menuItem in userControl.RootGrid.ContextMenu.Items.OfType<MenuItem>())
        userControl.LaunchButton.IsEnabled = userControl.AppFacade?.IsExecutable ?? false;
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
            UpdateControl(userControl);
        }
    }

    private static void OnButtonSettingsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is AvAppButtons userControl && e.NewValue is ButtonSettings s)
        {
            userControl.ButtonSettings = s;
            UpdateControl(userControl);
        }
    }

    private static void UpdateButtons(AvAppButtons userControl)
    {
        var ordered = userControl.ButtonGrid.Children.OfType<Button>().Select(x => ((VisibleButtons)x.Tag, userControl.ButtonSettings.GetPosition((VisibleButtons)x.Tag)));
        foreach (var but in userControl.ButtonGrid.Children.OfType<Button>())
        {
            var tag = (VisibleButtons)but.Tag;
            Grid.SetColumn(but, userControl.ButtonSettings.GetPosition(tag));
            bool isVisible = userControl.ButtonSettings.VisibleButtons.HasFlag(tag) && (but.IsEnabled || userControl.ButtonSettings.ShowDisabledButtons);
            but.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    private void LaunchButton_Click(object sender, RoutedEventArgs e)
    {
        Launch();
    }
    private void Launch()
    {
        if (LaunchCommand is null)
        {
            AppFacade?.LaunchWithoutProgram();
        }
        else
        {
            LaunchCommand.Execute(AppFacade);
        }
    }
    private static void UpdateControl(AvAppButtons userControl)
    {
        UpdateLaunchButton(userControl);
        UpdateButtons(userControl);
    }
    public AvAppButtons()
    {
        InitializeComponent();
    }

    private void UC_Root_Loaded(object sender, RoutedEventArgs e)
    {
        UpdateControl(this);
    }
}

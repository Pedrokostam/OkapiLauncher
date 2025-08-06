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
using Material.Icons.WPF;
using Material.Icons;
using OkapiLauncher.Models;
using System.Diagnostics;
using OkapiLauncher.Controls.Utilities;
using System.Windows.Media.Media3D;


namespace OkapiLauncher.Controls
{
    /// <summary>
    /// Interaction logic for AvAppFacadeListItem.xaml
    /// </summary>
    public partial class AvAppFacadeListItem : UserControl
    {


        public ICommand LaunchCommand
        {
            get { return (ICommand)GetValue(LaunchCommandProperty); }
            set { SetValue(LaunchCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LaunchCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LaunchCommandProperty =
            DependencyProperty.Register(nameof(LaunchCommand), typeof(ICommand), typeof(AvAppFacadeListItem), new PropertyMetadata(defaultValue: null));


        //private static void OnLaunchCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        //{
        //    if (d is AvAppFacadeListItem userControl)
        //    {
        //        var kind = userControl.LaunchCommand is null ? MaterialIconKind.Launch : MaterialIconKind.Powershell;
        //        userControl.LaunchButton.SetIconKind(kind);
        //        var tooltip = userControl.LaunchCommand is null ? Properties.Resources.AvAppLaunchWithNoProgram : Properties.Resources.AvAppLaunchWithProgram;
        //        userControl.LaunchButton.ToolTip = tooltip;
        //        foreach (MenuItem menuItem in userControl.RootGrid.ContextMenu.Items.OfType<MenuItem>())
        //        {
        //            if(menuItem.Tag is bool tagged && tagged)
        //            {
        //                menuItem.Header = tooltip;
        //                return;
        //            }
        //        }
        //        // When the UserControlDataContext changes, update the DataContext of the root element
        //    }

        //}

        public static readonly DependencyProperty AppFacadeProperty =
            DependencyProperty.Register(
                nameof(AppFacade),
                typeof(AvAppFacade),
                typeof(AvAppFacadeListItem),
                new PropertyMetadata(defaultValue: null, OnUserControlAppFacadeChanged));

        public AvAppFacade? AppFacade
        {
            get => GetValue(AppFacadeProperty) as AvAppFacade;
            set => SetValue(AppFacadeProperty, value);
        }

        private static void OnUserControlAppFacadeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is AvAppFacadeListItem userControl && !Equals(userControl.AppFacade, e.NewValue as AvAppFacade))
            {
                var aaf = e.NewValue as AvAppFacade;
                // When the UserControlDataContext changes, update the DataContext of the root element
                userControl.AppFacade = aaf;
                if (aaf is not null)
                {
                    AppContextMenu.CreateAppContextMenu(userControl.RootGrid, aaf, userControl.LaunchCommand);
                }
                else
                {
                    userControl.RootGrid.ContextMenu = null;
                }
            }
        }

        public AvAppFacadeListItem()
        {
            InitializeComponent();
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

        private void UserControl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Launch();
        }

        private void RootGrid_Loaded(object sender, RoutedEventArgs e)
        {
            if (AppFacade is not null)
            {
                AppContextMenu.CreateAppContextMenu((sender as Grid)!, AppFacade, LaunchCommand);
            }
        }
    }
}

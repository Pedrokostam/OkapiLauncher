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
            DependencyProperty.Register(nameof(LaunchCommand), typeof(ICommand), typeof(AvAppFacadeListItem), new PropertyMetadata(null, OnLaunchCommandChanged));


        private static void OnLaunchCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is AvAppFacadeListItem userControl)
            {
                var kind = userControl.LaunchCommand is null ? MaterialIconKind.Launch : MaterialIconKind.Powershell;
                userControl.LaunchButton.SetIconKind(kind);
                var tooltip = userControl.LaunchCommand is null ? Properties.Resources.AvAppLaunchWithNoProgram : Properties.Resources.AvAppLaunchWithProgram;
                userControl.LaunchButton.ToolTip = tooltip;
                foreach (MenuItem menuItem in userControl.RootGrid.ContextMenu.Items)
                {
                    if(menuItem.Tag is bool tagged && tagged)
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
                typeof(AvAppFacadeListItem),
                new PropertyMetadata(null, OnUserControlDataContextChanged));

        public AvAppFacade? AppFacade
        {
            get => GetValue(AppFacadeProperty) as AvAppFacade;
            set => SetValue(AppFacadeProperty, value);
        }
        private static void OnUserControlDataContextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is AvAppFacadeListItem userControl)
            {
                // When the UserControlDataContext changes, update the DataContext of the root element
                userControl.DataContext = e.NewValue;
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
    }
}

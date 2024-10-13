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

namespace AuroraVisionLauncher.Controls
{
    /// <summary>
    /// Interaction logic for AvAppFacadeListItem.xaml
    /// </summary>
    public partial class AvAppFacadeListItem : UserControl
    {
        public static readonly DependencyProperty AppFacadeProperty =
            DependencyProperty.Register(
                nameof(AppFacade),
                typeof(object),
                typeof(AvAppFacadeListItem),
                new PropertyMetadata(null, OnUserControlDataContextChanged));

        public object AppFacade
        {
            get => GetValue(AppFacadeProperty);
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
    }
}

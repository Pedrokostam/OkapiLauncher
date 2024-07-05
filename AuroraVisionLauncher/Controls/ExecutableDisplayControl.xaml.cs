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
using AuroraVisionLauncher.Models;

namespace AuroraVisionLauncher.Controls;
/// <summary>
/// Interaction logic for ExecutableDisplayControl.xaml
/// </summary>
public partial class ExecutableDisplayControl : UserControl
{
    public ExecutableDisplayControl()
    {
        InitializeComponent();
        Root.DataContext = this;
    }

    public ExecutableFacade Item
    {
        get { return (ExecutableFacade)GetValue(ItemProperty); }
        set { SetValue(ItemProperty, value); }
    }

    // Using a DependencyProperty as the backing store for Item.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty ItemProperty =
    DependencyProperty.Register("Item", typeof(ExecutableFacade), typeof(ExecutableDisplayControl), new PropertyMetadata(null));




}

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

   
}

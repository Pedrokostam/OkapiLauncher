using System.Windows.Controls;
using System.Windows.Input;
using AuroraVisionLauncher.ViewModels;

namespace AuroraVisionLauncher.Views;

public partial class LauncherPage : Page
{
    public LauncherPage(LauncherViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

   
}

using System.Windows.Controls;

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

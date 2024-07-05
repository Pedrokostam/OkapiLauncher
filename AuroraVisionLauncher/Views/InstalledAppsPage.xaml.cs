using System.Windows.Controls;

using AuroraVisionLauncher.ViewModels;

namespace AuroraVisionLauncher.Views;

public partial class InstalledAppsPage : Page
{
    public InstalledAppsPage(InstalledAppsViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}

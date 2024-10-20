using System.Windows.Controls;

using OkapiLauncher.ViewModels;

namespace OkapiLauncher.Views;

public partial class InstalledAppsPage : Page
{
    public InstalledAppsPage(InstalledAppsViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        
    }

}

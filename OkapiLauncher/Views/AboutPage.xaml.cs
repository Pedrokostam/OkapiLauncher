using System.Windows.Controls;

using OkapiLauncher.ViewModels;

namespace OkapiLauncher.Views;

public partial class AboutPage : Page
{
    public AboutPage(AboutViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}

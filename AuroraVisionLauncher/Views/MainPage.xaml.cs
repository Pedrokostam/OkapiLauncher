using System.Windows.Controls;

using AuroraVisionLauncher.ViewModels;

namespace AuroraVisionLauncher.Views;

public partial class MainPage : Page
{
    public MainPage(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}

using System.Windows.Controls;

using AuroraVisionLauncher.ViewModels;

namespace AuroraVisionLauncher.Views;

public partial class BlankPage : Page
{
    public BlankPage(BlankViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}

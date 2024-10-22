using System.Windows.Controls;

using OkapiLauncher.ViewModels;

namespace OkapiLauncher.Views;

public partial class HelpPage : Page
{
    public HelpPage(HelpViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}

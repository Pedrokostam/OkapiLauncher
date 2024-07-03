using System.Windows.Controls;

using AuroraVisionLauncher.Contracts.Views;
using AuroraVisionLauncher.ViewModels;

using MahApps.Metro.Controls;

namespace AuroraVisionLauncher.Views;

public partial class ShellWindow : MetroWindow, IShellWindow
{
    public ShellWindow(ShellViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    public Frame GetNavigationFrame()
        => shellFrame;

    public Frame GetRightPaneFrame()
        => rightPaneFrame;

    public void ShowWindow()
        => Show();

    public void CloseWindow()
        => Close();

    public SplitView GetSplitView()
        => splitView;
}

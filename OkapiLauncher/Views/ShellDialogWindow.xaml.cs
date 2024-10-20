using System.Windows.Controls;

using AuroraVisionLauncher.Contracts.Views;
using AuroraVisionLauncher.ViewModels;

using MahApps.Metro.Controls;

namespace AuroraVisionLauncher.Views;

public partial class ShellDialogWindow : MetroWindow, IShellDialogWindow
{
    public ShellDialogWindow(ShellDialogViewModel viewModel)
    {
        InitializeComponent();
        viewModel.SetResult = OnSetResult;
        DataContext = viewModel;
    }

    public Frame GetDialogFrame()
        => dialogFrame;

    private void OnSetResult(bool? result)
    {
        DialogResult = result;
        Close();
    }
}

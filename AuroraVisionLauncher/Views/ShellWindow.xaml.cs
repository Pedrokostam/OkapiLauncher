using System.Windows;
using System.Windows.Controls;

using AuroraVisionLauncher.Contracts.Views;
using AuroraVisionLauncher.Models.Messages;
using AuroraVisionLauncher.ViewModels;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using MahApps.Metro.Controls;

namespace AuroraVisionLauncher.Views;

public partial class ShellWindow : MetroWindow, IShellWindow
{
    public ShellWindow(ShellViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
#if DEBUG
        var mi = new MenuItem
        {
            Header = "Force GC.Collect",
            Command = new RelayCommand(Collect),
        };
        var sep = new Separator();
        FileMenu.Items.Insert(FileMenu.Items.Count - 2, sep);
        FileMenu.Items.Insert(FileMenu.Items.Count - 2, mi);
#endif
    }
#if DEBUG
    private void Collect()
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
    }
#endif



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

    private void MetroWindow_PreviewDrop(object sender, System.Windows.DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files.Length == 1)
            {
                ((App)App.Current).GetService<IMessenger>().Send(new FileRequestedMessage(files[0]));
            }
        }
    }

    private void MetroWindow_PreviewDragOver(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files != null && files.Length == 1)
            {
                e.Effects = DragDropEffects.Copy;
                e.Handled = true;
            }
            else
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
            }
        }
        else
        {
            e.Effects = DragDropEffects.None;
            e.Handled = true;
        }
    }

}

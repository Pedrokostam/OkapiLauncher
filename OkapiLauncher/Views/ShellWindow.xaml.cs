using System.Windows;
using System.Windows.Controls;

using OkapiLauncher.Contracts.Views;
using OkapiLauncher.Models.Messages;
using OkapiLauncher.ViewModels;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

namespace OkapiLauncher.Views;

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
        FileMenu.Items.Add(sep);
        FileMenu.Items.Add(mi);
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
                ((ShellViewModel)DataContext).MenuViewsLauncherCommand.NotifyCanExecuteChanged();
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

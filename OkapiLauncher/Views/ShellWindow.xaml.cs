using System.Windows;
using System.Windows.Controls;
using OkapiLauncher.Contracts.Views;
using OkapiLauncher.Models.Messages;
using OkapiLauncher.ViewModels;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System.IO;
using System.Text;

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
                string filepath = files[0];
                if (Path.GetExtension(filepath).Equals(".lnk", StringComparison.OrdinalIgnoreCase))
                {
                    string? dereferencedPath = GetLnkTargetPath(filepath);
                    if (dereferencedPath is null)
                    {
                        return;
                    }
                    filepath = dereferencedPath;
                }
                ((ShellViewModel)DataContext).MenuViewsLauncherCommand.NotifyCanExecuteChanged();
                ((App)App.Current).GetService<IMessenger>().Send(new FileRequestedMessage(filepath));
            }
        }
    }

    public static string GetLnkTargetPath(string filepath)
    {
        using var br = new BinaryReader(System.IO.File.OpenRead(filepath));
        // skip the first 20 bytes (HeaderSize and LinkCLSID)
        br.ReadBytes(0x14);
        // read the LinkFlags structure (4 bytes)
        uint lflags = br.ReadUInt32();
        // if the HasLinkTargetIDList bit is set then skip the stored IDList
        // structure and header
        if ((lflags & 0x01) == 1)
        {
            br.ReadBytes(0x34);
            var skip = br.ReadUInt16(); // this counts of how far we need to skip ahead
            br.ReadBytes(skip);
        }
        // get the number of bytes the path contains
        var length = br.ReadUInt32();
        // skip 12 bytes (LinkInfoHeaderSize, LinkInfoFlgas, and VolumeIDOffset)
        br.ReadBytes(0x0C);
        // Find the location of the LocalBasePath position
        var lbpos = br.ReadUInt32();
        // Skip to the path position
        // (subtract the length of the read (4 bytes), the length of the skip (12 bytes), and
        // the length of the lbpos read (4 bytes) from the lbpos)
        br.ReadBytes((int)lbpos - 0x14);
        var size = length - lbpos - 0x02;
        var bytePath = br.ReadBytes((int)size);
        var path = Encoding.UTF8.GetString(bytePath, 0, bytePath.Length);
        return path;
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

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using OkapiLauncher.Contracts.Services;
using OkapiLauncher.Core.Models;
using OkapiLauncher.Helpers;
using OkapiLauncher.Models;
using OkapiLauncher.Models.Messages;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;

namespace OkapiLauncher.Services;
public class RecentlyOpenedFilesService : ObservableRecipient, IRecentlyOpenedFilesService
{
    private readonly string Key = "LastOpenedFiles";
    const int FileCountLimit = 30;
    private List<RecentlyOpenedFile> LastOpenedPaths => (List<RecentlyOpenedFile>)App.Current.Properties[Key]!;
    /// <summary>
    /// Is a dependency to ensure its instantiated before.
    /// </summary>
    private readonly IPersistAndRestoreService _persistAndRestoreService;
    public string? LastOpenedFile { get; private set; }

    public RecentlyOpenedFilesService(IMessenger messenger, IPersistAndRestoreService persistAndRestoreService) : base(messenger)
    {
        _persistAndRestoreService = persistAndRestoreService;
        if (_persistAndRestoreService.IsDataRestored)
        {
            // if already restored, get
            InitializeData();
        }
        else
        {
            // otherwise wait for restore
            _persistAndRestoreService.DataRestored += _persistAndRestoreService_DataRestored;
        }


        IsActive = true;
    }

    private void _persistAndRestoreService_DataRestored(object? sender, EventArgs e)
    {
        InitializeData();
    }

    private void InitializeData()
    {
        var rofs = App.Current.Properties.ReadElementList<RecentlyOpenedFile>(Key);
        App.Current.Properties[Key] = rofs.ToList();
    }

    public IEnumerable<RecentlyOpenedFileFacade> GetLastOpenedFiles() => GetFacades();
    public void AddLastFile(string file)
    {
        var rof = new RecentlyOpenedFile(file);
        LastOpenedPaths.Remove(rof);
        LastOpenedPaths.Insert(0, rof);
        while (LastOpenedPaths.Count > FileCountLimit)
        {
            LastOpenedPaths.RemoveAt(FileCountLimit);
        }
        IEnumerable<RecentlyOpenedFileFacade> enumerable = GetFacades();
        LastOpenedFile = file;
        Messenger.Send(new RecentFilesChangedMessage(enumerable));
    }

    private IEnumerable<RecentlyOpenedFileFacade> GetFacades()
    {
        return LastOpenedPaths.Select(
                    (x, i) => new RecentlyOpenedFileFacade(x, i)
                    );
    }
}

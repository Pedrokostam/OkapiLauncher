using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using AuroraVisionLauncher.Contracts.Services;
using AuroraVisionLauncher.Core.Models;
using AuroraVisionLauncher.Helpers;
using AuroraVisionLauncher.Models;
using AuroraVisionLauncher.Models.Messages;
using CommunityToolkit.Mvvm.Messaging;

namespace AuroraVisionLauncher.Services;
public class RecentlyOpenedFilesService : IRecentlyOpenedFilesService
{
    private readonly string Key = "LastOpenedFiles";
    private readonly IMessenger _messenger;
    const int FileCountLimit = 30;
    private List<RecentlyOpenedFile> LastOpenedPaths => (List<RecentlyOpenedFile>)App.Current.Properties[Key]!;
    public RecentlyOpenedFilesService(IMessenger messenger)
    {
        //App.Current.Properties[Key] = new List<RecentlyOpenedFile>();
        if (!App.Current.Properties.Contains("LastOpenedFiles"))
        {
        }
        else
        {
            Newtonsoft.Json.Linq.JArray prop = (Newtonsoft.Json.Linq.JArray)App.Current.Properties[Key]!;
            var list = new List<RecentlyOpenedFile>();
            foreach (var item in prop)
            {
                try
                {
                    var rof = item.ToObject<RecentlyOpenedFile>();
                    list.Add(rof);
                }
                catch (ArgumentException)
                {
                    // dont care about invalid data - its beta after all.
                }
            }
            App.Current.Properties[Key] = list;
        }

        this._messenger = messenger;
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
        _messenger.Send(new RecentFilesChangedMessage(enumerable));
    }

    private IEnumerable<RecentlyOpenedFileFacade> GetFacades()
    {
        return LastOpenedPaths.Select(
                    (x, i) => new RecentlyOpenedFileFacade(x, i)
                    );
    }
}

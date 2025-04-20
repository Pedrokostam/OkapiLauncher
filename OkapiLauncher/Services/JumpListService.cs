using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shell;
using System.Xml.Linq;
using OkapiLauncher.Contracts.Services;
using OkapiLauncher.Converters;
using OkapiLauncher.Core.Models;
using OkapiLauncher.Core.Models.Apps;
using OkapiLauncher.Helpers;
using OkapiLauncher.Models;

namespace OkapiLauncher.Services;
public class JumpListService : IJumpListService
{
    private static readonly AppTypeToStringConverter _appTypeToStringConverter = new();
    internal class RecentJumpItem : JumpTask
    {
        public RecentJumpItem(RecentlyOpenedFileFacade roff) : base()
        {
            Title = Path.GetFileName(roff.FilePath);
            Arguments = roff.FilePath;
            Description = roff.FilePath;
            ApplicationPath = Environment.ProcessPath;
            CustomCategory = "Recent";
        }
    }
    internal class AppJumpItem : JumpTask
    {
        public AppJumpItem(AvApp app, IFileAssociationService fileAssociationService)
        {
            var name = $"{app.Name} {app.Version}";
            Title = name;
            Arguments = null;
            Description = string.Format(ResourceHelper.GetTextResource("JumplistLaunchAppDescription")!, name);
            ApplicationPath = app.Path;
            IconResourcePath = fileAssociationService.GetLocalIconPath(app.Brand, app.Type);
            CustomCategory = _appTypeToStringConverter.Convert(app.Type,typeof(string), parameter: null!,CultureInfo.InvariantCulture) as string;
        }
    }
    private bool _iconsRestored = false;
    private readonly IFileAssociationService _fileAssociationService;

    public const int RecentItemsLimit = 10;

    private static JumpList AppJumpList => JumpList.GetJumpList(App.Current);

    public void SetTasks(IEnumerable<AvApp> tasks)
    {
        if (!_iconsRestored)
        {
            _fileAssociationService.RestoreIconFiles();
            _iconsRestored = true;
        }
        AppJumpList.JumpItems.RemoveAll(x => x is AppJumpItem);
        foreach (var app in tasks.Where(x => x.IsExecutable).OrderBy(x=>x))
        {
            AppJumpList.JumpItems.Insert(0, new AppJumpItem(app, _fileAssociationService));
        }
        AppJumpList.Apply();
    }
    public JumpListService(IFileAssociationService fileAssociationService)
    {
        _fileAssociationService = fileAssociationService;
        if (JumpList.GetJumpList(App.Current) is null)
        {
            JumpList.SetJumpList(App.Current, new JumpList());
        }
        AppJumpList.ShowFrequentCategory = false;
        AppJumpList.ShowRecentCategory = false;
    }
}

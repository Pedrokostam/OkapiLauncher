using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shell;
using OkapiLauncher.Contracts.Services;
using OkapiLauncher.Core.Models.Apps;

namespace OkapiLauncher.Services;
public class JumpListService : IJumpListService
{
    private readonly IFileAssociationService _fileAssociationService;

    private JumpList AppJumpList => JumpList.GetJumpList(App.Current);

    public void AddRecentItem(string itemPath)
    {
        JumpList.AddToRecentCategory(itemPath);
    }

    public void AddFrequentItem(string itemPath)
    {
    }

    public void SetTasks(IEnumerable<AvApp> tasks)
    {
        _fileAssociationService.RestoreIconFiles();
        foreach (var app in tasks.Where(x=>x.IsExecutable))
        {
            var name = $"{app.Name} {app.Version}";
            var jumpListItem = new JumpTask()
            {
                Title = name,
                Arguments = app.Path,
                Description = $"Launch {name}",
                ApplicationPath = Environment.ProcessPath,
                IconResourcePath = _fileAssociationService.GetLocalIconPath(app.Brand,app.Type),
                //CustomCategory = "Applications",
            };
            AppJumpList.JumpItems.Add(jumpListItem);
        }
        AppJumpList.Apply();
    }
    public JumpListService(IFileAssociationService fileAssociationService)
    {
        if (JumpList.GetJumpList(App.Current) is null)
        {
            JumpList.SetJumpList(App.Current, new JumpList());
        }
        AppJumpList.ShowFrequentCategory = true;
        AppJumpList.ShowRecentCategory = true;
        _fileAssociationService = fileAssociationService;
        //var jlist = JumpList.GetJumpList(App.Current) ?? new();
        //JumpList.SetJumpList(App.Current, jlist);
    }
}

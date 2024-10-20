using OkapiLauncher.Core.Models.Apps;
using OkapiLauncher.Models;
using OkapiLauncher.Models.Updates;

namespace OkapiLauncher.Contracts.Services;

public interface IContentDialogService
{
    Task<bool> ShowAllProcessesKillDialog(object context, IAvApp app);
    Task ShowError(string message, string? title=null);
    Task ShowMessage(string message, string? title=null);
    Task ShowMessage(object context, string message, string? title = null);
    Task<bool> ShowProcessKillDialog(object context, SimpleProcess process);
    Task ShowSourceEditor(CustomAppSource source);
    Task<UpdatePromptResult> ShowVersionDecisionDialog(HtmlVersionResponse newVersionInformation);
}
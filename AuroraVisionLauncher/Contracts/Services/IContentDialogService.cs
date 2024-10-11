using AuroraVisionLauncher.Models;
using AuroraVisionLauncher.Models.Updates;

namespace AuroraVisionLauncher.Contracts.Services;

public interface IContentDialogService
{
    Task ShowError(string message, string? title=null);
    Task ShowMessage(string message, string? title=null);
    Task ShowSourceEditor(CustomAppSource source);
    Task<UpdatePromptResult> ShowVersionDecisionDialog(HtmlVersionResponse newVersionInformation);
}
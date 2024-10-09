using AuroraVisionLauncher.Models;

namespace AuroraVisionLauncher.Contracts.Services;

public interface IContentDialogService
{
    Task ShowError(string message, string? title=null);
    Task ShowSourceEditor(CustomAppSource source);
}
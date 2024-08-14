using AuroraVisionLauncher.Models;

namespace AuroraVisionLauncher.Contracts.Services;

public interface IContentDialogService
{
    Task ShowError(string title, string message);
    Task ShowSourceEditor(CustomAppSource source);
}
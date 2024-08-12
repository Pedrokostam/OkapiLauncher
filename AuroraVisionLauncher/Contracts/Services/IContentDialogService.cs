namespace AuroraVisionLauncher.Contracts.Services;

public interface IContentDialogService
{
    Task ShowError(string title, string message);
}
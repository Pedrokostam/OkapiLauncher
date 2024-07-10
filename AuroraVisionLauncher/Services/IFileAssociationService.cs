namespace AuroraVisionLauncher.Services;

public interface IFileAssociationService
{
    void SetAssociationsToApp(string? mainAppExecutablePath = null);
}

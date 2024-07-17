namespace AuroraVisionLauncher.Contracts.Services;

public interface IFileAssociationService
{
    void SetAssociationsToApp(string? mainAppExecutablePath = null);
}

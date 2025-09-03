using OkapiLauncher.Models;

namespace OkapiLauncher.Contracts.Services;

public interface IFileAssociationService
{
    Task SetAssociationsToApp(string? mainAppExecutablePath = null);
    IEnumerable<FileAssociationStatus> CheckCurrentAssociations(string? mainAppExecutablePath = null);
}

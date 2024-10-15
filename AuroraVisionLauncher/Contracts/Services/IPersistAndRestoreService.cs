namespace AuroraVisionLauncher.Contracts.Services;

public interface IPersistAndRestoreService
{
    event EventHandler? DataRestored;
    void RestoreData();
    bool IsDataRestored { get; }
    void PersistData();
}

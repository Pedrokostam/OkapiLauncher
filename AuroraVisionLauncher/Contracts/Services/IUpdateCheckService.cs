namespace AuroraVisionLauncher.Contracts.Services;

public interface IUpdateCheckService
{
    bool AutoCheckForUpdatesEnabled { get; set; }

    Task AutoCheckForUpdates();
    Task CheckForUpdates();
}
namespace OkapiLauncher.Contracts.Services;

public interface IUpdateCheckService
{
    bool AutoCheckForUpdatesEnabled { get; set; }
    /// <summary>
    /// Check for updates and prompts the user what to do. Will ignore ignored versions.
    /// </summary>
    /// <returns>Newest available version, different from the current one</returns>
    Task<string?> AutoPromptUpdate();
    /// <summary>
    /// Check for updates and prompts the user what to do. Will not ignore any versions.
    /// </summary>
    /// <returns>Newest available version, different from the current one</returns>
    Task<string?> ManualPrompUpdate();

}
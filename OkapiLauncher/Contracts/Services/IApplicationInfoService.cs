namespace OkapiLauncher.Contracts.Services;

public interface IApplicationInfoService
{
    public enum InstallationScope
    {
        /// <summary>
        /// The current app is not registered. It is in portable mode.
        /// </summary>
        Portable,
        /// <summary>
        /// The current app is installed system-wide, for all users.
        /// </summary>
        LocalMachine,
        /// <summary>
        /// The current app is installed only for the current user.
        /// </summary>
        CurrentUser,
        /// <summary>
        /// An app with matching GUID is installed on the system, but it is not this app.
        /// </summary>
        Conflict,
    }
    /// <summary>
    /// Gets the <see cref="DateTime"/> on which this application assembly was built.
    /// </summary>
    /// <returns>Date of built of this application assembly.</returns>
    DateTime GetBuildDatetime();

    /// <summary>
    /// Gets the fullpath to the folder with the application's executable.
    /// </summary>
    /// <returns>Fullpath to the folder with the executable.</returns>
    string GetFolder();

    /// <summary>
    /// Gets the name of the application's executable.
    /// </summary>
    /// <returns>Name of the executable.</returns>
    string GetExeName();

    /// <summary>
    /// Gets the unique <see cref="Guid"/> assigned to this application.
    /// </summary>
    /// <returns>Unique identifier of this app.</returns>
    Guid GetGuid();
    /// <summary>
    /// Gets the <see cref="Version"/> of this application.
    /// </summary>
    /// <returns>Version of this app.</returns>
    Version GetVersion();
    /// <summary>
    /// Checks whether the app is registered as installed app (properly installed apps have associated key in Microsoft\Windows\CurrentVersion\Uninstall in Registry and show up in Add or Remove program menu).
    /// </summary>
    /// <returns><see cref="InstallationScope"/> of the installation. </returns>
    InstallationScope IsRegisteredAsInstalledApp();
}

namespace OkapiLauncher.Models;

public class AppConfig
{
    public string ConfigurationsFolder { get; set; } = default!;
    public string IconsFolder { get; set; } = default!;
    public string AppPropertiesFileName { get; set; } = default!;
    public string GithubLink { get; set; } = default!;
    /// <summary>
    /// Link to the latest release on GitHub (only full releases, no pre-releases, no drafts)
    /// </summary>
    public string UpdateLink { get; set; } = default!;
}

namespace AuroraVisionLauncher.Contracts.Services;

public interface ISystemService
{
    void OpenInWebBrowser(string url);

    void LaunchInstaller(string? installerPath);
}

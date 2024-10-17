using System.Diagnostics;
using System.IO;
using AuroraVisionLauncher.Contracts.Services;
using AuroraVisionLauncher.Helpers;

namespace AuroraVisionLauncher.Services;

public class SystemService : ISystemService
{
    private readonly IApplicationInfoService _applicationInfoService;

    public SystemService(IApplicationInfoService applicationInfoService)
    {
        _applicationInfoService = applicationInfoService;
    }

    public void LaunchInstaller(string? installerPath)
    {
        if (!File.Exists(installerPath))
        {
            return;
        }
        var options = new ProcessStartInfo(installerPath);
        if (PrivilegeHelper.IsUserAdmin())
        {
            options.ArgumentList.Add("/ALLUSERS");
        }
        else
        {
            options.ArgumentList.Add("/CURRENTUSER");
        }
        if (!_applicationInfoService.IsRegisteredAsInstalledApp())
        {
            // if it is registered, installer will detect
            // if not we have to provide out own path
            var path = _applicationInfoService.GetFolder().Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            options.ArgumentList.Add($"/DIR={path}");
        }
        Process.Start(options);
    }

    public void OpenInWebBrowser(string url)
    {
        // For more info see https://github.com/dotnet/corefx/issues/10361
        var psi = new ProcessStartInfo
        {
            FileName = url,
            UseShellExecute = true
        };
        Process.Start(psi);
    }
}

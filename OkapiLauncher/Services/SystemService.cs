using System.Diagnostics;

using OkapiLauncher.Contracts.Services;

namespace OkapiLauncher.Services;

public class SystemService : ISystemService
{
    private readonly IApplicationInfoService _applicationInfoService;

    public SystemService(IApplicationInfoService applicationInfoService)
    {
        _applicationInfoService = applicationInfoService;
    }

    public void LaunchInstaller(string? installerPath)
    {
        installerPath = @"C:\Users\Pedro\source\repos\AuroraVisionLauncher\Release\AuroraVisionLauncher 1.1.1.0 installer.exe";
        if (!File.Exists(installerPath))
        {
            return;
        }
        var scope = _applicationInfoService.IsRegisteredAsInstalledApp();
        if (scope == IApplicationInfoService.InstallationScope.Conflict)
        {
            return;
        }
        var requiresElevationForAppFolder = PrivilegeHelper.CheckFolderRequiresElevation(_applicationInfoService.GetFolder());
        var options = new ProcessStartInfo(installerPath);
        if (requiresElevationForAppFolder==PrivilegeHelper.RequiredElevation.Elevated)
        {
            options.ArgumentList.Add("/ALLUSERS");
            // this will force the app to run as admin
        }
        // if app is not in an elevated folder, let the user choose.
        //else
        //{
        //    options.ArgumentList.Add("/CURRENTUSER");
        //    // this will run the app without verb, as current user
        //}
        // if the app is already installed, installer will detect the installation folder and update
        // if not we want to set the current folder as the install location
        var path = _applicationInfoService.GetFolder().Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
        // specifying dir will have no effect if we are updating, so we can just set it
        options.ArgumentList.Add($"/DIR={path}");
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

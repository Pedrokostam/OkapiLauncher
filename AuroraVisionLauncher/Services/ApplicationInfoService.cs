using System.Diagnostics;
using System.Reflection;

using AuroraVisionLauncher.Contracts.Services;

namespace AuroraVisionLauncher.Services;

public class ApplicationInfoService : IApplicationInfoService
{
    public ApplicationInfoService()
    {
    }

    public Version GetVersion()
    {
        // Set the app version in AuroraVisionLauncher > Properties > Package > PackageVersion
        string assemblyLocation = Assembly.GetExecutingAssembly().Location;
        var version = FileVersionInfo.GetVersionInfo(assemblyLocation).FileVersion;
        return new Version(version);
    }
}

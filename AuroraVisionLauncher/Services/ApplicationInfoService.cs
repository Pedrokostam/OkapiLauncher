using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using AuroraVisionLauncher.Contracts.Services;
using Microsoft.Win32;

namespace AuroraVisionLauncher.Services;

public class ApplicationInfoService : IApplicationInfoService
{
    private readonly Assembly _appAssembly;
    private Version? _appVersion;
    private DateTime? _appBuildDate;
    private Guid? _appGuid;
    public ApplicationInfoService()
    {
        _appAssembly = Assembly.GetCallingAssembly();
    }

    public string GetFolder() => _appAssembly.Location;

    /// <inheritdoc cref="IApplicationInfoService.GetVersion()"/>
    /// <remarks>Uses the version defined in the project file before build. If it is not present, returns version 0.0.0.0</remarks>
    public Version GetVersion()
    {
        if (_appVersion is null)
        {
            // Set the app version in AuroraVisionLauncher > Properties > Package > PackageVersion
            string assemblyLocation = _appAssembly.Location;
            var version = FileVersionInfo.GetVersionInfo(assemblyLocation).FileVersion;
            if (Version.TryParse(version, out var parsed))
            {
                _appVersion = parsed;
            }
            else
            {
                _appVersion = new Version(0, 0, 0, 0);
            }
        }
        return _appVersion;
    }

    /// <inheritdoc cref="IApplicationInfoService.GetGuid()"/>
    /// <remarks>Uses assembly's <see cref="GuidAttribute"/>. If it is not present, generates new <see cref="GUID"/></remarks>
    public Guid GetGuid()
    {
        if (_appGuid is null)
        {
            var guidAttribute = _appAssembly.GetCustomAttribute<GuidAttribute>();
            // TODO test for guid
            _appGuid = guidAttribute switch
            {
                not null => new Guid(guidAttribute.Value),
                _ => Guid.NewGuid(),
            };
        }
        return _appGuid.Value;
    }

    /// <inheritdoc cref="IApplicationInfoService.GetBuildDatetime()"/>
    /// <remarks>Uses assembly's <see cref="BuildDateAttribute"/>. If it is not present, returns modification date of the app's executable.</remarks>
    public DateTime GetBuildDatetime()
    {
        if (_appBuildDate is null)
        {
            var buildAttribute = _appAssembly.GetCustomAttribute<BuildDateAttribute>();
            _appBuildDate = buildAttribute switch
            {
                not null => buildAttribute.DateTime,
                _ => File.GetLastWriteTime(_appAssembly.Location),
            };
        }
        return _appBuildDate.Value;
    }

    /// <inheritdoc cref="IApplicationInfoService.GetBuildDatetime()"/>
    /// <remarks>Checks Microsoft\Windows\CurrentVersion\Uninstall in both LocalMachine and CurrentUser, looking for the key with the matching <see cref="GetGuid">GUID</see>.</remarks>
    public bool IsRegisteredAsInstalledApp()
    {
        return CheckUninstallKeys(Registry.CurrentUser) || CheckUninstallKeys(Registry.LocalMachine);
    }

    private bool CheckUninstallKeys(RegistryKey rootKey)
    {
        string assemblyLocation = _appAssembly.Location;
        using var uninstall = rootKey.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall");
        if (uninstall is null)
        {
            return false;
        }
        foreach (var subkeyName in uninstall.GetSubKeyNames())
        {
            using var subkey = uninstall.OpenSubKey(subkeyName);
            if (subkey is null)
            {
                continue;
            }
            var installLocation = subkey.GetValue("InstallLocation") as string;
            if (assemblyLocation.Equals(installLocation, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

        }
        return false;
    }

    public string GetExeName() => Path.GetFileName(_appAssembly.FullName) ?? string.Empty;
}

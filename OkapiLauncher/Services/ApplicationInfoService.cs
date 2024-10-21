using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using OkapiLauncher.Contracts.Services;
using static OkapiLauncher.Contracts.Services.IApplicationInfoService;

namespace OkapiLauncher.Services;

public class ApplicationInfoService : IApplicationInfoService
{
    private readonly Assembly _appAssembly;
    private Version? _appVersion;
    private DateTime? _appBuildDate;
    private Guid? _appGuid;
    public ApplicationInfoService()
    {
        _appAssembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
    }

    public string GetFolder() => Path.GetDirectoryName(_appAssembly.Location)!;

    /// <inheritdoc cref="IApplicationInfoService.GetVersion()"/>
    /// <remarks>Uses the version defined in the project file before build. If it is not present, returns version 0.0.0.0</remarks>
    public Version GetVersion()
    {
        if (_appVersion is null)
        {
            // Set the app version in AuroraVisionLauncher > Properties > Package > PackageVersion
            string assemblyLocation = _appAssembly.Location;
            var version = FileVersionInfo.GetVersionInfo(assemblyLocation).FileVersion;
            // TODO test for version
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
                _ => File.GetLastWriteTime(GetFolder()),
            };
        }
        return _appBuildDate.Value;
    }

    /// <inheritdoc cref="IApplicationInfoService.IsRegisteredAsInstalledApp()"/>
    /// <remarks>Checks Microsoft\Windows\CurrentVersion\Uninstall in both LocalMachine and CurrentUser, looking for the key with the matching <see cref="GetGuid">GUID</see>.</remarks>
    public InstallationScope IsRegisteredAsInstalledApp()
    {
        var scope = CheckUninstallKeys(Registry.CurrentUser);
        // if it return CurrentUser or Conflict, we short-return
        if (scope == InstallationScope.Portable)
        {
            // HKCU did not have this app, try local machine
            scope = CheckUninstallKeys(Registry.LocalMachine);
        }
        // LocalMachine, CurrentUser or Portable.
        return scope;
    }

    private InstallationScope CheckUninstallKeys(RegistryKey rootKey)
    {
        using var uninstall = rootKey.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall") ?? throw new InvalidOperationException("Cuold not access registry");
        string guidString = GetGuid().ToString();
        var matchingKeyName = uninstall.GetSubKeyNames().FirstOrDefault(x => x.Contains(guidString, StringComparison.OrdinalIgnoreCase));
        if (matchingKeyName is null)
        {
            return InstallationScope.Portable;
        }
        using var matchingKey = uninstall.OpenSubKey(matchingKeyName);
        if (string.Equals(matchingKey?.GetValue("InstallLocation") as string, GetFolder(), StringComparison.OrdinalIgnoreCase))
        {
            return rootKey == Registry.LocalMachine ? InstallationScope.LocalMachine : InstallationScope.CurrentUser;
        }
        return InstallationScope.Conflict;

    }

    public string GetExeName() => Path.ChangeExtension(Path.GetFileName(_appAssembly.FullName), "exe") ?? string.Empty;
}

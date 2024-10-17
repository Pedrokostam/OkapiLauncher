using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using AuroraVisionLauncher.Contracts.Services;

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
}

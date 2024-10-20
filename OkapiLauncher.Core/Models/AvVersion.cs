using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OkapiLauncher.Core.Models.Apps;

namespace OkapiLauncher.Core.Models;
public class AvVersion : IAvVersion
{
    public static readonly Version MissingVersionBase = new(0, 0, 0, 0);
    public static readonly AvVersion MissingVersion = new(MissingVersionBase);
    public AvVersion(Version version)
        : this(version.Major, version.Minor, version.Build, version.Revision) { }
    public AvVersion(IAvVersion version)
        : this(version.Major, version.Minor, version.Build, version.Revision) { }
    public AvVersion(int major, int minor, int build, int revision)
    {
        Major = major;
        Minor = minor;
        Build = build;
        Revision = revision;
    }
    public int Major { get; }
    public int Minor { get; }
    public int Build { get; }
    public int Revision { get; }
    public bool IsDevelopmentVersion => CheckIfDevelopmentVersion(this);

    public bool IsUnknown => IsMissingVersion(this);

    public Version InterfaceVersion => new Version(Major, Minor, Build);

    public static bool TryParse(string? versionString, [NotNullWhen(true)] out AvVersion? version)
    {
        if (string.IsNullOrWhiteSpace(versionString))
        {
            version = null;
            return false;
        }
        var spaceIndex = versionString.IndexOf(' ');
        if (spaceIndex >= 0)
        {
            versionString = versionString[..spaceIndex];
        }
        version = new(Version.Parse(versionString));
        return true;
    }
    public static AvVersion? Parse(string? versionString)
    {
        if (TryParse(versionString, out AvVersion? version))
        {
            return version;
        }
        return null;
    }
    public static bool TryParse(FileVersionInfo? finfo, [NotNullWhen(true)] out AvVersion? version)
    {
        return TryParse(finfo?.ProductVersion, out version);
    }
    public static AvVersion? Parse(FileVersionInfo? finfo)
    {
        if (TryParse(finfo, out AvVersion? version))
        {
            return version;
        }
        return null;
    }
    public Version ToVersion() => ToVersion(this);
    public override int GetHashCode() => GetHashCode(this);
    public override string ToString() => ToString(this);
    public bool Equals([NotNullWhen(true)] IAvVersion? other) => Equals(this, other);
    public int CompareTo(IAvVersion? other) => CompareTo(this, other);
    public bool Supports(IAvVersion versionToLoad) => Supports(this, versionToLoad);
    public bool IsSupportedBy(IAvVersion loaderVersion) => IsSupportedBy(loaderVersion, this);
    public bool IsRuntimeCompatibleWith(IAvVersion other) => IsRuntimeCompatibleWith(this, other);
    // -----------------------------------------------------------------------------------------------------------------------
    #region Statics

    public static bool CheckIfDevelopmentVersion(IAvVersion version) => version.Build >= 1000;
    public static int GetHashCode(IAvVersion version)
    {
        return HashCode.Combine(version.Major, version.Minor, version.Build, version.Revision);
    }
    public static string ToString(IAvVersion version)
    {
        return $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
    }
    public static bool IsRuntimeCompatibleWith(IAvVersion a, IAvVersion b)
    {
        ArgumentNullException.ThrowIfNull(a);
        ArgumentNullException.ThrowIfNull(b);
        return a.Major == b.Major
            && a.Minor == b.Minor
            && a.Build == b.Build;
    }
    public static bool Supports(IAvVersion supporter, IAvVersion supportee)
    {
        ArgumentNullException.ThrowIfNull(supporter);
        ArgumentNullException.ThrowIfNull(supportee);
        return supporter.Major >= supportee.Major
            || supporter.Minor >= supportee.Minor
            || supporter.Build >= supportee.Build;
    }
    public static bool IsSupportedBy(IAvVersion supportee, IAvVersion supporter) => Supports(supporter, supportee);

    public static bool Equals(IAvVersion? a, IAvVersion? b)
    {
        if (a is null && b is null)
        {
            return true;
        }
        if (a is null || b is null)
        {
            return false;
        }
        if (ReferenceEquals(a, b))
        {
            return true;
        }
        return a.Major == b.Major
            && a.Minor == b.Minor
            && a.Build == b.Build
            && a.Revision == b.Revision;
    }

    public static bool IsMissingVersion(IAvVersion version)
    {
        return version.Major == 0
            && version.Minor == 0
            && version.Build == 0
            && version.Revision == 0;
    }

    public static Version ToVersion(IAvVersion version) => new(version.Major, version.Minor, version.Build, version.Revision);

    public static int CompareTo(IAvVersion? a, IAvVersion? b)
    {
        if (a is null && b is null)
        {
            return 0;
        }
        if (b is null)
        {
            return 1;
        }
        if (a is null)
        {
            return -1;
        }
        var majorComp = a.Major.CompareTo(b.Major);
        if (majorComp != 0)
        {
            return majorComp;
        }
        var minorComp = a.Minor.CompareTo(b.Minor);
        if (minorComp != 0)
        {
            return minorComp;
        }
        var buildComp = a.Build.CompareTo(b.Build);
        if (buildComp != 0)
        {
            return buildComp;
        }
        return a.Revision.CompareTo(b.Revision);
    }

    public static AvVersion? FromFile(FileInfo fileInfo) => FromFile(fileInfo.FullName);
    public static AvVersion? FromFile(string filePath)
    {
        var ver = FileVersionInfo.GetVersionInfo(filePath);
        return AvVersion.Parse(ver.ProductVersion);
    }

    #endregion
}

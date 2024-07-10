using System;

namespace AuroraVisionLauncher.Core.Models;

public interface IAvVersion : IComparable<IAvVersion>, IEquatable<IAvVersion>
{
    int Major { get; }
    int Minor { get; }
    int Build { get; }
    int Revision { get; }
    bool IsDevelopmentVersion { get; }
    bool IsUnknown { get; }
    Version ToVersion();
    bool IsRuntimeCompatibleWith(IAvVersion otherVersion);
    bool IsSupportedBy(IAvVersion loaderVersion);
    bool Supports(IAvVersion versionToLoad);
}
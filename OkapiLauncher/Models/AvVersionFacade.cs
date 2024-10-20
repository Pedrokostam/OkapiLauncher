using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OkapiLauncher.Core.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using Windows.Security.Cryptography.Core;

namespace OkapiLauncher.Models
{
    public sealed partial class AvVersionFacade : ObservableObject, IAvVersion, IComparable<AvVersionFacade>, IComparable
    {
        public readonly static AvVersionFacade MissingVersion = new AvVersionFacade(AvVersion.MissingVersionBase);

        public AvVersionFacade(Version version)
            : this(version.Major, version.Minor, version.Build, version.Revision) { }
        public AvVersionFacade(IAvVersion version)
            : this(version.Major, version.Minor, version.Build, version.Revision) { }
        public AvVersionFacade(int major, int minor, int build, int revision)
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
        public bool IsDevelopmentVersion => AvVersion.CheckIfDevelopmentVersion(this);
        public bool IsUnknown => AvVersion.IsMissingVersion(this);
        public override int GetHashCode() => AvVersion.GetHashCode(this);
        public override string ToString() => AvVersion.ToString(this);
        public bool Equals([NotNullWhen(true)] IAvVersion? other) => Equals(this, other);
        public int CompareTo(IAvVersion? other) => AvVersion.CompareTo(this, other);
        public bool Supports(IAvVersion versionToLoad) => AvVersion.Supports(this, versionToLoad);
        public bool IsSupportedBy(IAvVersion loaderVersion) => AvVersion.IsSupportedBy(loaderVersion, this);
        public bool IsRuntimeCompatibleWith(IAvVersion other) => AvVersion.IsRuntimeCompatibleWith(this, other);


        public Version ToVersion() => AvVersion.ToVersion(this);

        public int CompareTo(AvVersionFacade? other) => AvVersion.CompareTo(this, other);

        public int CompareTo(object? obj) => AvVersion.CompareTo(this, obj as IAvVersion);

        public Version InterfaceVersion => new Version(Major, Minor, Build);
    }
}

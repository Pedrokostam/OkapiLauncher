using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Timers;
using OkapiLauncher.Core.Helpers;
using OkapiLauncher.Core.Models.Projects;

namespace OkapiLauncher.Core.Models.Apps;
public record AvApp : IAvApp
{
    public string Path { get; }
    public string RootPath { get; }
    public string? LogFolderPath { get; }
    public string AppDataPath { get; }
    public AvVersion Version { get; }
    public AvVersion? SecondaryVersion { get; }
    public string Name { get; }
    public string NameWithVersion => $"{Name} {Version}";
    public string? Description { get; }
    public bool IsCustom => Description is not null;
    public string ProcessName { get; }
    public bool IsExecutable => Type.IsExecutable;
    public bool IsDevelopmentVersion => Version.Build >= 1000;
    public CommandLineInterface Interface { get; }
    protected IReadOnlyCollection<ProductType> SupportedProgramTypes => Type.SupportedAvTypes;

    IAvVersion IProduct.Version => Version;

    IAvVersion? IAvApp.SecondaryVersion => SecondaryVersion;

    public ProductBrand Brand { get; }
    public ProductType Type { get; }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="mainInfo"></param>
    /// <param name="secondaryVersion"></param>
    /// <param name="type"></param>
    /// <param name="brand"></param>
    /// <param name="rootInstallationPath"></param>
    /// <param name="description"></param>
    /// <exception cref="VersionNotFoundException"></exception>
    internal AvApp(FileVersionInfo mainInfo, AvVersion? secondaryVersion, ProductType type, ProductBrand brand, string rootInstallationPath, string? description = null)
    {
        Path = mainInfo.FileName;
        Version = AvVersion.Parse(mainInfo) ?? throw new VersionNotFoundException("The ProductVersion field is empty");
        SecondaryVersion = secondaryVersion;
        Name = GetName(mainInfo, type);
        ProcessName = System.IO.Path.GetFileNameWithoutExtension(mainInfo.InternalName!);
        Type = type;
        Brand = brand;
        Description = description;
        RootPath = rootInstallationPath;
        LogFolderPath = GetLogFolderPath(this);
        AppDataPath = GetAppDataPath(this);
    }
    private static string GetName(FileVersionInfo finfo, ProductType type)
    {
        string baseName = finfo.ProductName ?? type.Name;
        return type.Type switch
        {
            AvType.DeepLearningGPU => $"{baseName} GPU",
            AvType.DeepLearningCPU => $"{baseName} CPU",
            _ => baseName,
        };
    }
    public bool CanOpen(IVisionProject project)
    {
        return SupportedProgramTypes.Contains(project.Type) && Brand.SupportsBrand(project.Brand);
    }
    private static double VersionToDouble(IAvVersion version, IAvVersion baseVersion)
    {
        var balance = 1.0d;
        // revision doesnt really matter, started with so it will never move it below 0
        // in fact the score should never be exactly zero
        balance += (version.Revision - baseVersion.Revision) / 100_000.0;
        // different builds denotes changes in the API, so runtime cannot be lauched for example
        balance += (version.Build - baseVersion.Build) * 10;
        // minor indicate some larger changes, but noting too ground breaking
        balance += (version.Minor - baseVersion.Minor) * 10_000;
        // hoo boy
        balance += (version.Major - baseVersion.Major) * 1_000_000;
        /*
        the goal is to find a version that gets the value closes to 0
        negative values mean the app is outdated, but may still be able to load the program
        positive values mena the app is from the future
        if we have 5.4.5.10000 and 5.3.1.01247 installed and the program is 5.2.7.23347 then we want to select the app that has the least changes,
        so is the closest to zero.

        In case we have outdated versions only, we still want to select the one closest to 0
        */

        return balance;
    }
    public static int GetClosestApp(IEnumerable<IAvApp> apps, IVisionProject project)
    {
        if (project.Type.Type == AvType.Runtime)
        {
            // Avexes dont have a version, so we just select the newest available runtime
            return apps.IndexOfMax(x => x.Version);
        }
        var weights = new List<double>();
        bool hasPositive = false;
        bool hasNegative = false;
        foreach (IAvApp app in apps)
        {
            var score = VersionToDouble(app.Version, project.Version);
            if (!app.CanOpen(project))
            {
                score = double.MinValue;
            }
            if (!app.IsNativeApp(project))
            {
                // lower non-native app score by 100 million
                // it is lower than any negative score can get, so they will always be chosen
                // only as the last resort.
                score -= 100_000_000;
            }
            hasPositive |= score > 0;
            hasNegative |= score < 0;
            weights.Add(score);
        }
        if (weights.Count == 0 || !(hasPositive || hasNegative))
        {
            return -1;
        }
        if (hasPositive)
        {
            // negative values are set to the highest possible value, so they will not be considered
            var min = weights.Min(x => x < 0 ? double.MaxValue : x);
            return weights.IndexOf(min);
        }
        // only negatives 
        var max = weights.Max();
        if (max == double.MinValue)
        {
            return -1;
        }
        return weights.IndexOf(max);
    }
    protected string ShortForm() => $"{Name} {Version}";
    public override string ToString() => ShortForm();
    public bool IsNativeApp(IVisionProject type) => IsNativeApp(this, type);
    public static bool IsNativeApp(IAvApp app, IVisionProject project)
    {
        return app.Type == project.Type;
    }

    public int CompareTo(IAvApp? other)
    {
        if (other == null)
            return 1;
        int type = Type.CompareTo(other.Type);
        if (type != 0)
        {
            return type;
        }
        int brand = Brand.CompareTo(other.Brand);
        if (brand != 0)
        {
            return brand;
        }
        int ver = Version.CompareTo(other.Version);
        if (ver != 0)
        {
            return -ver;
        }
        return Name.CompareTo(other.Name);
    }

    private static string? GetLogFolderPath(IAvApp app)
    {
        return app.Type.Type switch
        {
            AvType.Professional or AvType.Runtime => System.IO.Path.Join(GetAppDataPath(app), "Logs"),
            AvType.DeepLearningGPU => GetAppDataPath(app),
            _ => null,
        };
    }

    private static string GetAppDataPath(IAvApp app)
    {
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        return System.IO.Path.Join(localAppData, app.Brand.Name, System.IO.Path.GetFileName(app.RootPath));
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using OkapiLauncher.Core.Helpers;
using OkapiLauncher.Core.Exceptions;
using System.Data;
using Microsoft.Win32;

namespace OkapiLauncher.Core.Models.Apps;
public static partial class AppReader
{
    private static readonly PathStem[] _pathStems = [
            new("AdaptiveVisionStudio.exe"){FoldersToLeave=["SDK"]},
            new("AuroraVisionStudio.exe"){FoldersToLeave=["SDK"]},
            new("FabImageStudio.exe"){FoldersToLeave=["SDK"]},
            new("AdaptiveVisionExecutor.exe"),
            new("AuroraVisionExecutor.exe"),
            new("FabImageExecutor.exe"),
            new("bin","x64","AVL.dll"),
            new("bin","x64","FIL.dll"),
            new("Tools","DeepLearningEditor","DeepLearningEditor.exe"){FoldersToLeave=["Library"]},
        ];
    /// <summary>
    /// Finds and return all relevant applications, including custom locations
    /// </summary>
    /// <param name="additionalPaths"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static IEnumerable<AvApp> GetInstalledAvApps(IEnumerable<IAppSource>? additionalPaths = null)
    {
        List<AvApp> apps = [];
        var sources = GetAllRelevantPaths(additionalPaths);
        foreach (var source in sources)
        {
            apps.AddNotNull(GetAvAppFromSource(source));
        }
        apps.Sort();
        return apps;
    }
    /// <summary>
    /// Checks the given source and returns an <see cref="AvApp"/> for it or null.
    /// </summary>
    /// <param name="source"></param>
    /// <exception cref="UndeterminableBrandException"/>
    /// <exception cref="VersionNotFoundException"/>
    /// <returns>An instance of <see cref="AvApp"/> or null</returns>
    public static AvApp? GetAvAppFromSource(IAppSource source)
    {
        string? filepath = null;
        foreach (var stem in _pathStems)
        {
            filepath = stem.MatchPathInfo(source.SourcePath);
            if (filepath is not null)
            {
                break;
            }
        }
        if (filepath is null)
        {
            return null;
        }
        var type = ProductType.FromFilepath(filepath);
        var root = type.GetRootFolder(filepath);
        var brand = ProductBrand.FromFilepath(filepath, type);
        //var primaryVersion = AvVersion.FromFile(filepath) ?? throw new ArgumentException($"Specified file does not contain primary version information: {filepath}");
        var fileVersion = FileVersionInfo.GetVersionInfo(filepath);
        var secondaryVersion = GetSecondaryVersion(root, type, brand);
        AvApp app = new(
                fileVersion,
                secondaryVersion,
                type,
                brand,
                root,
                source.Description);
        return app;
    }

    private static AvVersion? GetSecondaryVersionFromRegistry(string rootFolder, ProductType type, ProductBrand brand)
    {
        try
        {
            string regPath = string.Join('\\', ["SOFTWARE", brand.Name]);
            using var brandKey = Registry.LocalMachine.OpenSubKey(regPath);
            if (brandKey is null)
            {
                return null;
            }
            var dlKeys = brandKey.GetSubKeyNames().Where(x => x.Contains("Learning", StringComparison.OrdinalIgnoreCase));
            foreach (var dlkey in dlKeys)
            {
                using var subkey = brandKey.OpenSubKey(dlkey);
                var path = subkey?.GetValue("Path") as string;
                var versionString = subkey?.GetValue("Version") as string;
                if (Version.TryParse(versionString, out var version) && string.Equals(path, rootFolder, StringComparison.OrdinalIgnoreCase))
                {
                    return new AvVersion(version);
                }
            }
        }
        catch
        {
        }
        return null;
    }
    private static AvVersion? GetSecondaryVersionFromUninstaller(string rootFolder, ProductType type, ProductBrand brand)
    {
        var uninstallers = new DirectoryInfo(rootFolder).GetFiles("unins*.exe");
        if (uninstallers.Any())
        {
            var finfo = FileVersionInfo.GetVersionInfo(uninstallers[0].FullName);
            var productVersion = finfo.ProductVersion;
            if (AvVersion.TryParse(productVersion, out var version))
            {
                return version;
            }
        }
        return null;
    }
    private static AvVersion? GetSecondaryVersion(string rootFolder, ProductType type, ProductBrand brand)
    {
        if (type.Type != AvType.DeepLearningGPU)
        {
            return null;
        }
        return GetSecondaryVersionFromRegistry(rootFolder, type, brand) ?? GetSecondaryVersionFromUninstaller(rootFolder, type, brand);
    }
    /// <summary>
    /// Gets all relevant paths (environmental and custom) and returns them as <see cref="AppSource">AppSources</see>.
    /// </summary>
    /// <param name="additionalPaths"></param>
    /// <returns>Enumerable of <see cref="AppSource">AppSources</see></returns>
    private static IEnumerable<AppSource> GetAllRelevantPaths(IEnumerable<IAppSource>? additionalPaths)
    {
        var variables = Environment.GetEnvironmentVariables();
        List<AppSource> paths = [];
        if (additionalPaths is not null)
        {
            foreach (var path in additionalPaths)
            {
                if (Directory.Exists(path.SourcePath))
                {
                    paths.Add(AppSource.FromInterface(path));
                }
            }
        }
#if DEBUG
        // convert dictionary to ordereb list
        var listvars = new List<DictionaryEntry>(variables.Count);
        foreach (DictionaryEntry e in variables)
        {
            listvars.Add(e);
        }
        listvars = [.. listvars.OrderBy(x => x.Key)];
#else
        // work on dictionary directly
        var listvars  = variables;
#endif
        foreach (DictionaryEntry entry in listvars)
        {
            var key_string = entry.Key?.ToString();
            var value_string = entry.Value?.ToString();
            if (key_string is null || value_string is null)
            {
                continue;
            }
            if (Regex.IsMatch(key_string,
                "^(AVS|FIS|AVLDL|FILDL|AVL|FIL)_",
                RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture,
                TimeSpan.FromMilliseconds(100)))
            {
                if (Directory.Exists(value_string))
                {
                    paths.Add(new(null, value_string, true));
                }
            }
        }
        return paths.DistinctBy(x => x.SourcePath, StringComparer.OrdinalIgnoreCase);
    }
}

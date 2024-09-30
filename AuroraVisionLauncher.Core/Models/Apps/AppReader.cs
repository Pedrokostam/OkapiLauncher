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

namespace AuroraVisionLauncher.Core.Models.Apps;
public static partial class AppReader
{

    private static readonly string[] _AdaptiveVisionStudioName = ["AdaptiveVisionStudio.exe"];
    private static readonly string[] _AuroraVisionStudioName = ["AuroraVisionStudio.exe"];
    private static readonly string[] _FabImageStudioName = ["FabImageStudio.exe"];
    private static readonly string[] _AdaptiveVisionExecutorName = ["AdaptiveVisionExecutor.exe"];
    private static readonly string[] _AuroraVisionExecutorName = ["AuroraVisionExecutor.exe"];
    private static readonly string[] _FabImageExecutorName = ["FabImageExecutor.exe"];
    private static readonly string[] _AuroraAVLName = ["Aurora Vision Library", "AVL.dll"];
    private static readonly string[] _AdaptiveAVLName = ["Adaptive Vision Library", "AVL.dll"];
    private static readonly string[] _FILName = ["FabImage Library", "FIL.dll"];



    private record struct PathInfo(DirectoryInfo BasePath, AvType Type)
    {
        public static implicit operator (DirectoryInfo basePath, AvType type)(PathInfo value)
        {
            return (value.BasePath, value.Type);
        }

        public static implicit operator PathInfo((DirectoryInfo basePath, AvType type) value)
        {
            return new PathInfo(value.basePath, value.type);
        }

        public string? GetRootDllExeFolderPath()
        {
            string path = BasePath.FullName;
            var folder = Type switch
            {
                AvType.Professional => path,
                AvType.Runtime => path,
                AvType.DeepLearning => Path.Join(path, "tools", "DeepLearningEditor"),
                AvType.Library => Path.Join(path, "bin", "x64"),
                _ => throw new NotSupportedException()
            };
            return folder;
        }
        private static string GetRootDllExeName(AvBrand brand, AvType type)
        {
            return (brand, type) switch
            {
                (AvBrand.Adaptive, AvType.Professional) => "AdaptiveVisionStudio.exe",
                (AvBrand.Aurora, AvType.Professional) => "AuroraVisionStudio.exe",
                (AvBrand.FabImage, AvType.Professional) => "FabImageStudio.exe",
                (AvBrand.Adaptive, AvType.Runtime) => "AdaptiveVisionExecutor.exe",
                (AvBrand.Aurora, AvType.Runtime) => "AuroraVisionExecutor.exe",
                (AvBrand.FabImage, AvType.Runtime) => "FabImageExecutor.exe",
                (AvBrand.Adaptive, AvType.Library) => "AVL.dll",
                (AvBrand.Aurora, AvType.Library) => "AVL.dll",
                (AvBrand.FabImage, AvType.Library) => "FIL.dll",
                (_, AvType.DeepLearning) => "DeepLearningEditor.exe",
                _ => throw new NotSupportedException()
            };
        }
        public string GetRootDllExePath(AvBrand brand)
        {
            return Path.Join(GetRootDllExeFolderPath(), GetRootDllExeName(brand, this.Type));
        }
        //public static PathInfo FromBaseFolder(string folderPath)
        //{
        //    var opt = new EnumerationOptions()
        //    {
        //        MaxRecursionDepth = 2,
        //        RecurseSubdirectories = true,
        //        MatchCasing = MatchCasing.CaseInsensitive,
        //        AttributesToSkip = FileAttributes.Directory | FileAttributes.Hidden,
        //    };
        //    var pattern  = // get all potential filenames names of exes/dlls
        //        // then some way to differentaite between aurora and adaptive library
        //    Directory.GetFiles(folderPath, "", opt);
        //}
    }
    /// <summary>
    /// Finds and return all relevant applications, including custom locations
    /// </summary>
    /// <param name="additionalPaths"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static IEnumerable<AvApp> GetInstalledAvApps(IEnumerable<IAppSource>? additionalPaths = null)
    {
        var sources = GetAllRelevantPaths(additionalPaths);
        List<AvApp> apps = [];
        foreach (var source in sources)
        {
            PathInfo? maybeInfo = GetPathInfo(source.SourcePath);
            if (maybeInfo is not PathInfo info)
            {
                continue;
            }
            var brand = ProductBrand.FindBrandByLicense(info.BasePath.FullName);
            var exePath = info.GetRootDllExePath(brand.Brand);
            var type = ProductType.FromAvType(info.Type);
            var primaryVersion = AvVersion.FromFile(exePath) ?? throw new ArgumentException($"Specified file does not contain primary version information: {exePath}");
            var secondaryVersion = GetSecondaryVersion(info);
            AvApp app = new(
                FileVersionInfo.GetVersionInfo(exePath),
                secondaryVersion,
                type,
                brand,
                info.BasePath.FullName,
                source.Description);
            apps.Add(app);
        }
        apps.Sort();
        return apps;
    }

    private static AvVersion? GetSecondaryVersion(PathInfo info)
    {
        if (info.Type != AvType.DeepLearning)
        {
            return null;
        }
        var uninstallers = info.BasePath.GetFiles("unins*.exe");
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
        var listvars = new List<DictionaryEntry>(variables.Count);
        foreach (DictionaryEntry e in variables)
        {
            listvars.Add(e);
        }
        listvars = listvars.OrderBy(x => x.Key).ToList();
#else
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




    private static bool PathMatch(string path, string matcher)
    {
        return path.Contains(matcher, StringComparison.OrdinalIgnoreCase);
    }
    private static PathInfo? GetPathInfo(string folderPath)
    {
        var dir = new DirectoryInfo(folderPath);
        if (PathMatch(dir.Name, "sdk"))
        {
            // environment variables for studio proffesional points to AVL libraries, which are in the SDK subfolder
            return (dir.Parent!, AvType.Professional);
        }
        if (PathMatch(dir.Parent!.Name, "Deep Learning"))
        {
            // deep learning also points to library, so 1 step back and the go for deeplearning editor
            return (dir.Parent!, AvType.DeepLearning);
        }
        if (PathMatch(dir.Name, "Runtime"))
        {
            return (dir, AvType.Runtime);
        }
        if (PathMatch(dir.Name, "Library"))
        {
            return (dir, AvType.Library);
        }
        return null;
    }
    static MultiVersion? FindInfo(string folder)
    {
        if (string.IsNullOrWhiteSpace(folder))
        {
            return null;
        }
        var dir = new DirectoryInfo(folder);

        if (dir.Name.Contains("sdk", StringComparison.OrdinalIgnoreCase))
        {
            // environment variables for studio proffesional points to AVL libraries, which are in the SDK subfolder
            dir = dir.Parent!;
        }
        else if (dir.Parent!.Name.Contains("Deep Learning", StringComparison.OrdinalIgnoreCase))
        {
            // deep learning also points to library, so 1 step back and the go for deeplearning editor
            dir = new DirectoryInfo(Path.Join(dir.Parent!.FullName, "Tools", "DeepLearningEditor"));
        }

        var exes = dir.GetFiles("*.exe");

        if (folder.Contains("Professional", StringComparison.OrdinalIgnoreCase))
        {
            var primary = exes.FirstOrDefault(x => x.Name.Contains("Studio", StringComparison.OrdinalIgnoreCase));
            return MultiVersion.Create(primary);
        }
        if (folder.Contains("Runtime", StringComparison.OrdinalIgnoreCase))
        {
            var primary = exes.FirstOrDefault(x => x.Name.Contains("Executor", StringComparison.OrdinalIgnoreCase));
            return MultiVersion.Create(primary);
        }
        if (folder.Contains("Deep Learning", StringComparison.OrdinalIgnoreCase))
        {
            // deep learning editor
            var primary = exes.FirstOrDefault(x => x.Name.Contains("Editor", StringComparison.OrdinalIgnoreCase));
            // uninstaller - it has the version of DL listed on the site (what hack?)
            var secondary = dir.Parent!.Parent!.GetFiles("*.exe").FirstOrDefault(x => x.Name.Contains("unins", StringComparison.OrdinalIgnoreCase));
            return MultiVersion.Create(primary, secondary);
        }
        return null;
    }
}

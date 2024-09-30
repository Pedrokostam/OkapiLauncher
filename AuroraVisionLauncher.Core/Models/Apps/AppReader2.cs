using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AuroraVisionLauncher.Core.Models.Apps
{
    readonly record struct PathInfo2(string RootFolder, string RelativeExePath, AvType AppType, AvBrand AppBrand)
    {
        public string ExePath => Path.Combine(RootFolder, RelativeExePath);
    }
    record RelativeLocator
    {
        public string ExeName { get; }
        private Regex? _relativeMatcher { get; }
        public int MaxDepth { get; }
        public AvType AppType { get; }
        public AvBrand? AppBrand { get; }
        public RelativeLocator(string exeRelativeToRoot, AvType appType, AvBrand? appBrand)
        {
            string normalized = exeRelativeToRoot = exeRelativeToRoot.Replace('\\', '/').Replace('/', Path.PathSeparator);
            if (string.Equals(normalized, exeRelativeToRoot, StringComparison.Ordinal))
            {
                // if strign are identical, we have no separators, regex not needed
                _relativeMatcher = null;
                MaxDepth = 0;
            }
            else
            {
                // reges needed  because wee look for a pattern in the path
                _relativeMatcher = new Regex(Regex.Escape(normalized), RegexOptions.IgnoreCase);
                MaxDepth = normalized.Count(c => c == Path.PathSeparator);
            }
            ExeName = Path.GetFileName(exeRelativeToRoot);
            AppType = appType;
            AppBrand = appBrand;

        }
        public PathInfo2? TryFind(string? path)
        {
            if (File.Exists(path))
            {
                path = Directory.GetParent(path)?.FullName;
            }
            ArgumentNullException.ThrowIfNull(path);
            var options = new EnumerationOptions()
            {
                MaxRecursionDepth = MaxDepth,
                RecurseSubdirectories = true,
                MatchCasing = MatchCasing.CaseInsensitive,
            };
            List<string> possibleFiles = [];
            var fileEnumerator = Directory.EnumerateFiles(path, ExeName, options);
            if (_relativeMatcher is null)
            {
                // only 1 match is permitted
                possibleFiles.AddRange(fileEnumerator);
            }
            else
            {
                foreach (var item in fileEnumerator)
                {
                    if (_relativeMatcher.IsMatch(item))
                    {
                        possibleFiles.Add(item);
                    }
                }
            }
            if (possibleFiles.Count > 1)
            {
                throw new InvalidOperationException("Multiple matching files found");
            }
            if (possibleFiles.Count == 0)
            {
                return null;
            }
            string filePath = Path.GetFullPath(possibleFiles[0]);
            DirectoryInfo rootFolder = Directory.GetParent(filePath)!;
            for (int i = 0; i < MaxDepth; i++)
            {
                rootFolder = rootFolder!.Parent!;
            }
            string relativePath = Path.GetRelativePath(rootFolder!.FullName, filePath);
            if (AppBrand is not null)
            {
                return new PathInfo2(rootFolder.FullName, relativePath, AppType, AppBrand.Value);
            }
            var brand = ProductBrand.FindBrandByLicense(rootFolder.FullName);
            return new PathInfo2(rootFolder.FullName, relativePath, AppType, brand.Brand);

        }
    }
    public static class AppReader2
    {
        private static readonly RelativeLocator[] _locators = [
            new("AdaptiveVisionStudio.exe",AvType.Professional,AvBrand.Adaptive),
            new("AuroraVisionStudio.exe",AvType.Professional,AvBrand.Aurora),
            new("FabImageStudio.exe",AvType.Professional,AvBrand.FabImage),
            new("AdaptiveVisionExecutor.exe",AvType.Runtime,AvBrand.Adaptive),
            new("AuroraVisionExecutor.exe",AvType.Runtime,AvBrand.Aurora),
            new("FabImageExecutor.exe",AvType.Runtime,AvBrand.FabImage),
            new("bin/x64/AVL.dll",AvType.Library,null),
            new("bin/x64/FIL.dll",AvType.Library,AvBrand.FabImage),
            new("Tools/DeepLearningEditor/DeepLearningEditor.exe",AvType.DeepLearning,null),
            ];
        private static string GetRootDllExeName(AvBrand brand, AvType type)
        {
            var q = (AvBrand.Adaptive, AvType.Professional);
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

        private static readonly Dictionary<string, (AvBrand? Brand, AvType Type)> Gups = new(StringComparer.OrdinalIgnoreCase)
        {
            {"AdaptiveVisionStudio.exe",(AvBrand.Adaptive, AvType.Professional)},
            {"AuroraVisionStudio.exe" ,(AvBrand.Aurora, AvType.Professional)},
            {"FabImageStudio.exe",(AvBrand.FabImage, AvType.Professional)},
            {"AdaptiveVisionExecutor.exe",(AvBrand.Adaptive, AvType.Runtime)},
            {"AuroraVisionExecutor.exe",(AvBrand.Aurora, AvType.Runtime)},
            {"FabImageExecutor.exe",(AvBrand.FabImage, AvType.Runtime)},
            {"AVL.dll",(null, AvType.Library)},
            {"FIL.dll",(AvBrand.FabImage, AvType.Library)},
            {"DeepLearningEditor.exe",(null, AvType.DeepLearning)},
        };
        private class RootRelativeLocator
        {
            public RootRelativeLocator(string )
            {

            }
        }

        public static void JudgePath(string path)
        {
            if (File.Exists(path))
            {

            }
            else if (Directory.Exists(path))
            {

            }




        }
    }

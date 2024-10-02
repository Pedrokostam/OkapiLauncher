using System.Text.RegularExpressions;

namespace AuroraVisionLauncher.Core.Models.Apps.Detection
{
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
        //public PathInfo2? TryFind(string? path)
        //{
        //    if (File.Exists(path))
        //    {
        //        path = Directory.GetParent(path)?.FullName;
        //    }
        //    ArgumentNullException.ThrowIfNull(path);
        //    var options = new EnumerationOptions()
        //    {
        //        MaxRecursionDepth = MaxDepth,
        //        RecurseSubdirectories = true,
        //        MatchCasing = MatchCasing.CaseInsensitive,
        //    };
        //    List<string> possibleFiles = [];
        //    var fileEnumerator = Directory.EnumerateFiles(path, ExeName, options);
        //    if (_relativeMatcher is null)
        //    {
        //        // only 1 match is permitted
        //        possibleFiles.AddRange(fileEnumerator);
        //    }
        //    else
        //    {
        //        foreach (var item in fileEnumerator)
        //        {
        //            if (_relativeMatcher.IsMatch(item))
        //            {
        //                possibleFiles.Add(item);
        //            }
        //        }
        //    }
        //    if (possibleFiles.Count > 1)
        //    {
        //        throw new InvalidOperationException("Multiple matching files found");
        //    }
        //    if (possibleFiles.Count == 0)
        //    {
        //        return null;
        //    }
        //    string filePath = Path.GetFullPath(possibleFiles[0]);
        //    DirectoryInfo rootFolder = Directory.GetParent(filePath)!;
        //    for (int i = 0; i < MaxDepth; i++)
        //    {
        //        rootFolder = rootFolder!.Parent!;
        //    }
        //    string relativePath = Path.GetRelativePath(rootFolder!.FullName, filePath);
        //    if (AppBrand is not null)
        //    {
        //        return new PathInfo2(rootFolder.FullName, relativePath, AppType, AppBrand.Value);
        //    }
        //    var brand = ProductBrand.FindBrandByLicense(rootFolder.FullName);
        //    return new PathInfo2(rootFolder.FullName, relativePath, AppType, brand.Brand);

        //}
    }
}

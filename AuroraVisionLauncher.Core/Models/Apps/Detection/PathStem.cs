using System.Diagnostics;

namespace AuroraVisionLauncher.Core.Models.Apps.Detection
{
    [DebuggerDisplay("{Filename}")]
    class PathStem
    {
        private string[] _parts;
        public ReadOnlySpan<string> Parts => (ReadOnlySpan<string>)_parts;
        public string Filename => _parts[^1];
        public int MaxDepth => _parts.Length;
        private string? _ending = null;
        /// <summary>
        /// Folders which, if the path ends in them should be exited for their parent
        /// </summary>
        public string[] FoldersToLeave { get; init; } = [];
        public string Ending
        {
            get
            {
                _ending ??= string.Join(Path.DirectorySeparatorChar, _parts).ToLowerInvariant();
                return _ending;
            }
        }
        public ReadOnlySpan<string> Preceding => new(_parts, 0, _parts.Length - 1);
        public PathStem(params string[] parts)
        {
            _parts = parts;
        }
        public string? MatchPathInfo(string path)
        {
            if (File.Exists(path))
            {
                path = Directory.GetParent(path)?.FullName!;
            }
            // Environmental variables can point to not-root folders
            // Here we go up until we are not in an avoidable folder
            if (FoldersToLeave.Length > 0)
            {
                while (FoldersToLeave.Contains(Path.GetFileName(path), StringComparer.OrdinalIgnoreCase))
                {
                    path = Path.GetDirectoryName(path)!;
                    if (path is null)
                    { throw new InvalidOperationException($"Infinite loop at avoiding folders for {path}"); }
                }
            }
            ArgumentNullException.ThrowIfNull(path);
            if (!Directory.Exists(path))
            {
                return null;
            }
            var options = new EnumerationOptions()
            {
                MaxRecursionDepth = MaxDepth,
                RecurseSubdirectories = true,
                MatchCasing = MatchCasing.CaseInsensitive,
            };
            var fileEnumerator = Directory.EnumerateFiles(path, Filename, options);
            string? exeFilepath = null;
            foreach (var file in fileEnumerator)
            {
                if (!file.EndsWith(Ending, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
                if (exeFilepath is not null)
                {
                    throw new InvalidOperationException("Multiple matching files found");
                }
                exeFilepath = file;
            }
            return exeFilepath;

        }
    }
}

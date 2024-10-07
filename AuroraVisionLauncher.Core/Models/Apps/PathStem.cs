using System.Collections.ObjectModel;
using System.Diagnostics;

namespace AuroraVisionLauncher.Core.Models.Apps
{
    /// <summary>
    /// Class which specifeid how an app path should look.
    /// </summary>
    [DebuggerDisplay("{Filename}")]
    class PathStem
    {
        private readonly string[] _parts;
        /// <summary>
        /// Steps to the executable from the root folder of the app (exclusive) to the filename (inclusive)
        /// </summary>
        public IReadOnlyList<string> Parts => new ReadOnlyCollection<string>(_parts);
        /// <summary>
        /// The filename of the executable.
        /// </summary>
        public string Filename => _parts[^1];
        public int MaxDepth => _parts.Length;
        /// <summary>
        /// Folders which, if the path ends in them should be exited for their parent. If the path to match ends at one of this, its parent will be analyzed instead. Order does not matter, unlike in <see cref="Parts"/>
        /// </summary>
        public string[] FoldersToLeave { get; init; } = [];
        /// <summary>
        /// Combined <see cref="Parts"/> using the system separator and lowercased.
        /// </summary>
        public string Ending { get; set; }

        public ReadOnlySpan<string> Preceding => new(_parts, 0, _parts.Length - 1);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="parts">Steps to the executable from the root folder of the app (exclusive) to the filename (inclusive). Must have at least 1 element (filename).</param>
        public PathStem(params string[] parts)
        {
            if (parts.Length == 0)
            {
                throw new ArgumentException("At least the filename must be specified");
            }
            _parts = parts;
            Ending = string.Join(Path.DirectorySeparatorChar, _parts).ToLowerInvariant();
        }
        /// <summary>
        /// Checks whether the given filepath matches. The filepath will match if it ends at any of the parts or the root folder of the app.
        /// </summary>
        /// <param name="path">Path in which executable should be stored.</param>
        /// <returns>Path to the executable or null</returns>
        /// <exception cref="InvalidOperationException"></exception>
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
                    {
                        throw new InvalidOperationException($"Infinite loop at avoiding folders for {path}");
                    }
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

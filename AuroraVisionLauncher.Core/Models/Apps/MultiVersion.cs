using System.Diagnostics;

namespace AuroraVisionLauncher.Core.Models.Apps;

internal record MultiVersion(FileVersionInfo Primary, FileVersionInfo? Secondary)
{
    public static MultiVersion? Create(FileInfo? primary, FileInfo? secondary = null)
    {
        if (primary is null)
        {
            return null;
        }
        var ver1 = FileVersionInfo.GetVersionInfo(primary.FullName);
        var ver2 = secondary is null ? null : FileVersionInfo.GetVersionInfo(secondary.FullName);
        return new MultiVersion(ver1, ver2);
    }
}

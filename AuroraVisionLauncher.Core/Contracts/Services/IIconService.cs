using System.Drawing;

namespace AuroraVisionLauncher.Core.Contracts.Services;

public interface IIconService
{
    /// <summary>
    /// Extracts the icon from <paramref name="iconSourcePrimary"/> and saves it to the given <paramref name="destinationPath"/> (with extension set to .ico).
    /// <para/>
    /// If <paramref name="iconSourcePrimary"/> file does not exists, <paramref name="iconSourceFallback"/> is used as the icon source instead.
    /// </summary>
    /// <param name="iconSourcePrimary"></param>
    /// <param name="destinationPath"></param>
    /// <param name="falbackIcon">Instance of icon to be used if primary source fails.</param>
    /// <returns>Path to the extracted icon, or <see langword="null"/> if the extraction failed.</returns>
    string? ExtractIconToFile(string? iconSourcePrimary, string destinationPath, Icon falbackIcon);
}
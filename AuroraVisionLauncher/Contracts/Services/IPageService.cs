using System.Windows.Controls;

namespace AuroraVisionLauncher.Contracts.Services;

public interface IPageService
{
    /// <summary>
    /// Gets page type for the given key.
    /// </summary>
    /// <param name="viewModelTypeName">View model type name</param>
    /// <returns><see cref="Type"/> of the page.</returns>
    Type? GetPageType(string viewModelTypeName);

    /// <summary>
    /// Gets page for the given key.
    /// </summary>
    /// <param name="viewModelTypeName">View model type name</param>
    /// <returns>An instance of the page.</returns>
    Page? GetPage(string viewModelTypeName);
}

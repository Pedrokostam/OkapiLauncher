using System.Windows.Controls;

namespace AuroraVisionLauncher.Contracts.Services;

public interface IPageService
{
    Type GetPageType(string key);

    Page GetPage(string key);
}

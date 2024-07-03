using System.Collections.ObjectModel;
using AuroraVisionLauncher.Models.Apps;

namespace AuroraVisionLauncher.Contracts.Services;
public interface IInstalledAppsProviderService
{
    ReadOnlyCollection<Executable> Executables { get; }
}
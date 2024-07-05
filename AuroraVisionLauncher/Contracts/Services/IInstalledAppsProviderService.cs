using System.Collections.ObjectModel;
using AuroraVisionLauncher.Core.Models.Apps;

namespace AuroraVisionLauncher.Contracts.Services;
public interface IInstalledAppsProviderService
{
    ReadOnlyCollection<AvApp> AvApps { get; }
}
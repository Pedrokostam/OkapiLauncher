using System.Collections.ObjectModel;
using AuroraVisionLauncher.Models;

namespace AuroraVisionLauncher.Contracts.Services;
public interface ICustomAppSourceService
{
    ObservableCollection<CustomAppSource> CustomSources { get; }
}
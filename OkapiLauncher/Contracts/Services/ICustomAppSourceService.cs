using System.Collections.ObjectModel;
using OkapiLauncher.Models;

namespace OkapiLauncher.Contracts.Services;
public interface ICustomAppSourceService
{
    ObservableCollection<CustomAppSource> CustomSources { get; }
}
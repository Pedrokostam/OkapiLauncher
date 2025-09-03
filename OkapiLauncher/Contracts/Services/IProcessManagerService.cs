using System.ComponentModel;
using System.Windows.Threading;
using OkapiLauncher.Models;
using OkapiLauncher.Models.Messages;

namespace OkapiLauncher.Services;
public interface IProcessManagerService: INotifyPropertyChanged
{
    FreshAppProcesses? ProcessState { get; }
    Type? QuererType { get; }
}
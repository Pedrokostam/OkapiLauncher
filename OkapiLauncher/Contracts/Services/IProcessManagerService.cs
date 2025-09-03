using System.ComponentModel;
using System.Windows.Threading;
using OkapiLauncher.Models;
using OkapiLauncher.Models.Messages;

namespace OkapiLauncher.Contracts.Services;
public interface IProcessManagerService: INotifyPropertyChanged
{
    IAppProcessInformationPacket ProcessState { get; }
    Type? QuererType { get; }
}
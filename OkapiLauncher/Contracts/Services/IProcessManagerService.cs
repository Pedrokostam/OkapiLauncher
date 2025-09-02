using System.Windows.Threading;
using OkapiLauncher.Models;
using OkapiLauncher.Models.Messages;

namespace OkapiLauncher.Services;
public interface IProcessManagerService
{
    FreshAppProcesses? ProcessState { get; }

}
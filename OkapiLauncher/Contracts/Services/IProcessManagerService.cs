using System.Windows.Threading;
using AuroraVisionLauncher.Models;
using AuroraVisionLauncher.Models.Messages;

namespace AuroraVisionLauncher.Services;
public interface IProcessManagerService
{
    FreshAppProcesses GetCurrentState { get; }

}
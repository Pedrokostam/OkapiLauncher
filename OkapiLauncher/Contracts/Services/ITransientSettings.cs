using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OkapiLauncher.Contracts.Services;
public interface ITransientSettings : INotifyPropertyChanged
{
    bool ProcessMonitoringEnabled { get; }
}

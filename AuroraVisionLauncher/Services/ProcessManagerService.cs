using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using AuroraVisionLauncher.Core.Models.Apps;
using AuroraVisionLauncher.Models;
using CommunityToolkit.Mvvm.Messaging;

namespace AuroraVisionLauncher.Services
{
    public class ProcessManagerService : IProcessManagerService
    {
        private readonly Dictionary<string, List<AvAppFacade>> _dictionary = new(StringComparer.OrdinalIgnoreCase);
        private readonly IMessenger _messenger;

        public ProcessManagerService(IMessenger messenger)
        {
            _messenger = messenger;
        }
        public void UpdateProcessActive(IEnumerable<AvAppFacade> apps)
        {
            _dictionary.Clear();
            foreach (var item in apps)
            {
                if (_dictionary.TryGetValue(item.ProcessName, out var list))
                {
                    list.Add(item);
                }
                else
                {
                    _dictionary[item.ProcessName] = [item];
                }
            }
            foreach (var app in apps)
            {
                app.ActiveProcessesNumber = 0;
            }
            var allProcesses = Process.GetProcesses();

            foreach (var process in allProcesses)
            {
                try
                {
                    if (_dictionary.TryGetValue(process.ProcessName, out var appList))
                    {
                        foreach (var innerApp in appList)
                        {
                            if (string.Equals(innerApp.Path, process.MainModule?.FileName, StringComparison.OrdinalIgnoreCase))
                            {
                                innerApp.ActiveProcessesNumber++;
                            }
                        }
                    }
                }
                catch
                {
                }
                finally
                {
                    process.Dispose();
                }
            }
        }

        public IList<SimpleProcess> GetActiveProcesses(AvAppFacade app)
        {
            var processes = Process.GetProcessesByName(app.ProcessName);
            var simples = new List<SimpleProcess>();
            foreach (var process in processes)
            {
                try
                {
                    if (string.Equals(process.MainModule?.FileName, app.Path, StringComparison.OrdinalIgnoreCase))
                    {
                        simples.Add(new(process, _messenger));
                    }
                }
                catch
                {
                    process.Dispose();
                }
            }
            return simples;
        }
    }
}

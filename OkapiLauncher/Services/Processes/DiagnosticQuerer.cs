using System.ComponentModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.Messaging;
using OkapiLauncher.Core.Models.Apps;
using OkapiLauncher.Models;
using OkapiLauncher.Models.Messages;

namespace OkapiLauncher.Services.Processes;

public sealed class DiagnosticQuerer(IMessenger messenger) : ProcessQuerer(messenger)
{
    public override FreshAppProcesses? GetProcesses(IEnumerable<IAvApp> apps)
    {
        if (!Monitor.TryEnter(_lock))
        {
            return null;
        }
        Clear();
        try
        {
            foreach (var app in apps)
            {
                Update(app);
            }
            MarkUpdate();
            return DictToFAP();
        }
        finally
        {
            Monitor.Exit(_lock);
        }
    }
    private void Update(IAvApp app)
    {
        var rawProcesses = Process.GetProcessesByName(app.ProcessName);
        List<SimpleProcess> simples = new List<SimpleProcess>(rawProcesses.Length);
        if (rawProcesses.Length == 0)
        {
            _dict[app.Path] = [];
            return;
        }
        foreach (var proc in rawProcesses)
        {
            try
            {
                var mainModule = proc.MainModule;
                var mainModulePath = mainModule?.FileName;
                if (!string.Equals(mainModulePath, app.Path, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
                var simple = new SimpleProcess(proc.Id, proc.MainWindowTitle, proc.ProcessName, proc.StartTime, _messenger, app.Path);
                simples.Add(simple);
            }
            catch (Win32Exception w32)
            {
                throw new ProcessException(w32);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
            finally
            {
                proc.Dispose();
            }
        }
        _dict[app.Path] = simples;
    }
    public override FreshAppProcesses? UpdateSingleApp(IAvApp app)
    {
        if (!Monitor.TryEnter(_lock))
        {
            return null;
        }
        try
        {

            Update(app);
            return DictToFAP();
        }
        finally
        {
            Monitor.Exit(_lock);
        }
    }
}

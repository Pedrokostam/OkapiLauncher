using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using OkapiLauncher.Core.Models.Apps;
using OkapiLauncher.Models;
using OkapiLauncher.Models.Messages;

namespace OkapiLauncher.Helpers;
public class ProcessQuerer
{
    private readonly object _lock = new object();
    private readonly IMessenger _messenger;
    private Dictionary<string, ManagementObjectSearcher> _cachedWmiQueries = new Dictionary<string, ManagementObjectSearcher>(StringComparer.OrdinalIgnoreCase);

    public bool WmiMode { get; private set; }
    private IDictionary<string, HashSet<SimpleProcess>> Processes { get; } = new Dictionary<string, HashSet<SimpleProcess>>(StringComparer.OrdinalIgnoreCase);
    public ProcessQuerer(IMessenger messenger)
    {
        _messenger = messenger;
    }
    private SimpleProcess MakeProcess(ManagementBaseObject obj)
    {
        var id = (int)(uint)obj["ProcessId"];
        using var proc = Process.GetProcessById(id);
        return new SimpleProcess(
            id,
            proc.MainWindowTitle,
            proc.ProcessName,
            proc.StartTime,
            _messenger,
            (string)obj["ExecutablePath"]
            );
    }
    private IEnumerable<SimpleProcess> MakeProcesses(ManagementObjectCollection coll)
    {
        foreach (var item in coll)
        {
            yield return MakeProcess(item);
        }
    }
    private IEnumerable<SimpleProcess> GetProcessesOfApp(IAvApp app)
    {
        try
        {
            if (WmiMode)
            {
                return GetProcesses_Wmi(app);
            }

            return GetProcesses_Diagnostics(app);
        }
        catch (Win32Exception)
        {
            WmiMode = true;
            return GetProcesses_Wmi(app);
        }
        catch (Exception)
        {
        }
        return [];
    }
    private IEnumerable<SimpleProcess> GetProcessesOfApps(IEnumerable<IAvApp> app)
    {
        try
        {
            if (WmiMode)
            {
                return GetProcesses_Wmi(app);
            }

            return GetProcesses_Diagnostics(app);
        }
        catch (Win32Exception)
        {
            WmiMode = true;
            return GetProcesses_Wmi(app);
        }
        catch (Exception)
        {
        }
        return [];
    }

    private IEnumerable<SimpleProcess> GetProcesses_Diagnostics(IAvApp app)
    {
        var rawProcesses = Process.GetProcessesByName(app.ProcessName);
        List<SimpleProcess> simples = new List<SimpleProcess>(rawProcesses.Length);

        foreach (var proc in rawProcesses)
        {
            if (!string.Equals(proc.MainModule?.FileName, app.Path, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }
            try
            {
                var simple = new SimpleProcess(proc.Id, proc.MainWindowTitle, proc.ProcessName, proc.StartTime, _messenger, app.Path);
                simples.Add(simple);

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
        return simples;
    }
    private IEnumerable<SimpleProcess> GetProcesses_Wmi(IEnumerable<IAvApp> apps)
    {
        if (!_cachedWmiQueries.TryGetValue(app.Path, out var searcher))
        {
            string query = $"SELECT ProcessID, ExecutablePath, Name, CreationDate FROM Win32_process WHERE ExecutablePath='{app.Path}'";
            searcher = new ManagementObjectSearcher(query);
            _cachedWmiQueries[app.Path] = searcher;
        }
        using var yields = searcher.Get();
        return MakeProcesses(yields);
    }
    private IEnumerable<SimpleProcess> GetProcesses_Wmi(IAvApp app)
    {
        if (!_cachedWmiQueries.TryGetValue(app.Path, out var searcher))
        {
            string query = $"SELECT ProcessID, ExecutablePath, Name, CreationDate FROM Win32_process WHERE ExecutablePath='{app.Path}'";
            searcher = new ManagementObjectSearcher(query);
            _cachedWmiQueries[app.Path] = searcher;
        }
        using var yields = searcher.Get();
        return MakeProcesses(yields);
    }
    public void RefreshAll(IEnumerable<IAvApp> apps)
    {
        if (!Monitor.TryEnter(_lock))
        {
            return;
        }
        try
        {
            Processes.Clear();
            foreach (var app in apps)
            {
                var newpp = GetProcessesOfApp(app);
                Processes[app.Path] = [.. newpp];
            }
            Send();
        }
        finally
        {
            Monitor.Exit(_lock);
        }
    }
    public void RefreshApp(IAvApp app)
    {
        if (!Monitor.TryEnter(_lock))
        {
            return;
        }
        try
        {
            var newpp = GetProcessesOfApp(app);
            Processes[app.Path] = [.. newpp];
            Send();
        }
        finally
        {
            Monitor.Exit(_lock);
        }
    }
    /// <summary>
    /// Send new fresh app processes message through messenger
    /// </summary>
    private void Send()
    {

        var fap = new FreshAppProcesses((IReadOnlyDictionary<string, HashSet<SimpleProcess>>)Processes);
        _messenger.Send<FreshAppProcesses>(fap);
    }
}

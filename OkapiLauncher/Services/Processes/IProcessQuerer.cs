using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using OkapiLauncher.Core.Models.Apps;
using OkapiLauncher.Models;
using OkapiLauncher.Models.Messages;

namespace OkapiLauncher.Services.Processes;
public sealed class ProcessException(Exception baseException) : Exception
{
    public Exception Exception { get; private set; } = baseException;
}
public interface IProcessQuerer
{
    TimeSpan TimerPeriod { get; }

    /// <summary>
    /// Checks running processes for all the given apps.
    /// </summary>
    /// <param name="apps"></param>
    /// <exception cref="ProcessException"/>
    /// <returns></returns>
    FreshAppProcesses? GetProcesses(IEnumerable<IAvApp> apps);
    /// <summary>
    /// Checks running processes for a single app.
    /// </summary>
    /// <param name="app"></param>
    /// <exception cref="ProcessException"/>
    /// <returns></returns>
    FreshAppProcesses? UpdateSingleApp(IAvApp app);

    bool ShouldUpdate();

}
public abstract class ProcessQuerer : IProcessQuerer
{
    protected readonly IMessenger _messenger;
    protected readonly IDictionary<string, IList<SimpleProcess>> _dict = new Dictionary<string, IList<SimpleProcess>>(StringComparer.OrdinalIgnoreCase);
    protected readonly object _lock = new();
    private int LashHash { get; set; }
    protected DateTime LastUpdate { get; set; } = DateTime.MinValue;
    private static readonly TimeSpan GracePeriod = TimeSpan.FromMilliseconds(300);
    virtual public TimeSpan TimerPeriod { get; } = TimeSpan.FromMilliseconds(2000);
    protected ProcessQuerer(IMessenger messenger)
    {
        _messenger = messenger;
    }

    public abstract FreshAppProcesses? GetProcesses(IEnumerable<IAvApp> apps);
    public abstract FreshAppProcesses? UpdateSingleApp(IAvApp app);
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "MA0064:Avoid locking on publicly accessible instance", Justification = "<Pending>")]
    protected FreshAppProcesses? DictToFAP()
    {
        Debug.WriteLine(_dict.Sum(x => x.Value.Count));
        var fap = new FreshAppProcesses(_dict);
        return new FreshAppProcesses(_dict);
    }

    public bool ShouldUpdate()
    {
        return DateTime.UtcNow - LastUpdate > (TimerPeriod - GracePeriod);
    }
    protected void MarkUpdate()
    {
        LastUpdate = DateTime.UtcNow;
    }
    protected void Clear()
    {
        foreach (var key in _dict.Keys)
        {
            _dict[key].Clear();
        }
    }
}
public sealed class DiagnosticQuerer : ProcessQuerer
{
    public DiagnosticQuerer(IMessenger messenger) : base(messenger)
    {
    }

    public override FreshAppProcesses? GetProcesses(IEnumerable<IAvApp> apps)
    {
        if (!Monitor.TryEnter(_lock))
        {
            return null;
        }
        Debug.WriteLine("Entering");
        Clear();
        try
        {
            foreach (var app in apps)
            {
                Update(app);
                Debug.WriteLine(_dict[app.Path].Count);
            }
            MarkUpdate();
            return DictToFAP();
        }
        finally
        {
            Debug.WriteLine("Exiting");

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
            if (!string.Equals(proc.MainModule?.FileName, app.Path, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }
            try
            {
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
        if (Monitor.TryEnter(_lock))
        {
            Update(app);
        }
        return DictToFAP();
    }
}
public sealed partial class WmiQuerer : ProcessQuerer
{
    private readonly Dictionary<string, ManagementObjectSearcher> _queries = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, ManagementObjectSearcher> _globalQueries = new(StringComparer.Ordinal);
    public WmiQuerer(IMessenger messenger) : base(messenger)
    {
    }
    static readonly Regex Slasher = new(@"[\\\/]", RegexOptions.Compiled, TimeSpan.FromMilliseconds(500));

    private static string GetQueryKey(IEnumerable<IAvApp> apps)
    {
        var en = apps.Where(x => x.IsExecutable).Select(x => $"{x.ProcessName.ToLowerInvariant()}{x.Version.ToVersion()}");
        return string.Join("", en);
    }
    private static string GetQuerizedPath(IAvApp app)
    {
        return "'" + Slasher.Replace(app.Path, @"\\") + "'";
    }
    public override FreshAppProcesses? GetProcesses(IEnumerable<IAvApp> apps)
    {
        if (!Monitor.TryEnter(_lock))
        {
            return null;
        }
        try
        {
            var key = GetQueryKey(apps);
            var paths = apps.Select(x=>"'"+x.ProcessName+".exe'").ToList();
            if (paths.Count == 0)
            {
                Clear();
                return DictToFAP();
            }
            if (!_queries.TryGetValue(key, out var query))
            {
                string condition = "Name=" + string.Join(" OR Name=", paths);
                var strQuery = $"SELECT ProcessID, ExecutablePath, Name, CreationDate FROM Win32_process WHERE {condition}";
                query = new ManagementObjectSearcher(strQuery);
                _queries[key] = query;
            }
            var res = query.Get();
            var result = MakeProcesses(query.Get());
            Clear();
            foreach (var item in result)
            {
                if (_dict.TryGetValue(item.Path, out var coll))
                {
                    coll.Add(item);
                }
                else
                {
                    coll = [item];
                    _dict[item.Path] = coll;
                }
            }
            MarkUpdate();
            return DictToFAP();
        }
        catch (Win32Exception e)
        {
            throw new ProcessException(e);
        }
        finally
        {
            Monitor.Exit(_lock);
        }
    }
    private IEnumerable<SimpleProcess> MakeProcesses(ManagementObjectCollection coll)
    {
        foreach (var item in coll)
        {
            yield return MakeProcess(item);
        }
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
    public override FreshAppProcesses? UpdateSingleApp(IAvApp app)
    {
        if (!Monitor.TryEnter(_lock))
        {
            return null;
        }
        try
        {
            if (!_queries.TryGetValue(app.Path, out var query))
            {
                query = new ManagementObjectSearcher($"SELECT ProcessID, ExecutablePath, Name, CreationDate FROM Win32_process WHERE ExecutablePath='{GetQuerizedPath(app)}'");
                _queries[app.Path] = query;
            }
            var result = query.Get();
            _dict[app.Path] = [.. MakeProcesses(result)];
            return DictToFAP();
        }
        catch (Win32Exception e)
        {

            throw new ProcessException(e);
        }
    }
}
using System.ComponentModel;
using System.Diagnostics;
using System.Management;
using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.Messaging;
using OkapiLauncher.Core.Models.Apps;
using OkapiLauncher.Models;
using OkapiLauncher.Models.Messages;

namespace OkapiLauncher.Services.Processes;

public sealed  class WmiQuerer(IMessenger messenger) : ProcessQuerer(messenger)
{
    private readonly Dictionary<string, ManagementObjectSearcher> _queries = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, ManagementObjectSearcher> _globalQueries = new(StringComparer.Ordinal);
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
            var paths = apps.Select(x => "'" + x.ProcessName + ".exe'").ToList();
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
                query = new ManagementObjectSearcher($"SELECT ProcessID, ExecutablePath, Name, CreationDate FROM Win32_process WHERE ExecutablePath={GetQuerizedPath(app)}");
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
        finally
        {
            Monitor.Exit(_lock);
        }
    }
}
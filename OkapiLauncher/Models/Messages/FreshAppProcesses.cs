using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using OkapiLauncher.Core.Models.Apps;
using Microsoft.Extensions.Logging;
using OkapiLauncher.Core.Helpers;
using System.Diagnostics;
using System.Windows.Threading;
using System.Windows;

namespace OkapiLauncher.Models.Messages;
public class FreshAppProcesses
{
    private readonly Dictionary<string, HashSet<SimpleProcess>> _dict = new Dictionary<string, HashSet<SimpleProcess>>(StringComparer.OrdinalIgnoreCase);
    public static readonly FreshAppProcesses Empty = new([]);
    public int GetHash()
    {
        var hash = 04092025;
        foreach (var kvp in _dict.Values)
        {
            foreach (var item in kvp)
            {
                hash = HashCode.Combine(hash, item.MainWindowTitle, item.Id);
            }
        }
        return hash;
    }
    public FreshAppProcesses(IEnumerable<SimpleProcess> processes)
    {
        foreach (var process in processes)
        {
            if (_dict.TryGetValue(process.ProcessName, out var list))
            {
                list.Add(process);
            }
            else
            {
                _dict[process.ProcessName] = [process];
            }
        }
        foreach (var kvp in _dict)
        {
            _dict[kvp.Key] = [.. kvp.Value];
        }
    }

    public FreshAppProcesses(IReadOnlyDictionary<string, IEnumerable<SimpleProcess>> newState)
    {
        foreach (var kvp in newState)
        {
            _dict[kvp.Key] = [.. kvp.Value];
        }
    }
    public FreshAppProcesses(IReadOnlyDictionary<string, IList<SimpleProcess>> newState)
    {
        foreach (var kvp in newState)
        {
            _dict[kvp.Key] = [.. kvp.Value];
        }
    }
    public FreshAppProcesses(IDictionary<string, IEnumerable<SimpleProcess>> newState)
    {
        foreach (var kvp in newState)
        {
            _dict[kvp.Key] = [.. kvp.Value.OrderBy(x => x.StartTime)];
        }
    }
    public FreshAppProcesses(IDictionary<string, IList<SimpleProcess>> newState)
    {
        foreach (var kvp in newState)
        {
            _dict[kvp.Key] = [.. kvp.Value.OrderBy(x => x.StartTime)];
        }
    }
   
    /// <summary>
    /// Modifies all items in <paramref name="apps"/> so that their <see cref="AvAppFacade.ActiveProcesses">ActiveProcesses</see> reflects state from this instance.
    /// </summary>
    /// <param name="apps"></param>
    public void UpdateStates(IEnumerable<AvAppFacade> apps)
    {
        foreach (var app in apps)
        {
            UpdateState(app);
        }
    }

    /// <summary>
    /// Modifies the given instance of <paramref name="app"/> so that its <see cref="AvAppFacade.ActiveProcesses">ActiveProcesses</see> reflects state from this instance.
    /// </summary>
    /// <param name="app"></param>
    public void UpdateState(AvAppFacade app)
    {
        ArgumentNullException.ThrowIfNull(app);
        //if (!app.ActiveProcesses.NeedsUpdate())
        //{
        //    Debug.WriteLine("Skipped update of facade");
        //    return;
        //}
        if (!_dict.TryGetValue(app.Path, out var newProcs))
        {
            app.ActiveProcesses.Clear();
            return;
        }
        if (newProcs.Count == 0)
        {
            app.ActiveProcesses.Clear();
            return;
        }
        var toRemove = new List<int>();
        foreach (var proc in app.ActiveProcesses.Select((value, index) => (value, index)))
        {
            if (newProcs.TryGetValue(proc.value, out var newProc))
            {
                proc.value.UpdateFrom(newProc);
            }
            else
            {
                toRemove.Add(proc.index);
            }
        }
        toRemove.Sort();
        toRemove.Reverse();
        foreach (var index in toRemove)
        {
            app.ActiveProcesses.RemoveAt(index);
        }
        var brandNew = newProcs.Except(app.ActiveProcesses);
        foreach (var proc in brandNew)
        {
            var insertionIndex = app.ActiveProcesses.FindInsertionIndex(x => x.StartTime > proc.StartTime);
            app.ActiveProcesses.Insert(insertionIndex, proc.Clone());
        }
    }

}

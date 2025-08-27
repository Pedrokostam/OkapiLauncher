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
    private readonly Dictionary<string, IList<SimpleProcess>> _dict= new Dictionary<string, IList<SimpleProcess>>(StringComparer.OrdinalIgnoreCase);

    public FreshAppProcesses(IEnumerable<SimpleProcess> processes)
    {
        foreach (var process in processes)
        { 
            if(_dict.TryGetValue(process.ProcessName,out var list))
            {
                list.Add(process);
            }
            else
            {
                _dict[process.ProcessName] = [process];
            }
        }
        foreach (var v in _dict.Values)
        { 
        v.or}
    }

    public FreshAppProcesses(IDictionary<string, IEnumerable<SimpleProcess>> newState)
    {
        foreach (var kvp in newState)
        {
            _dict[kvp.Key] = [.. kvp.Value];
        }
    }

    public FreshAppProcesses(IDictionary<string, HashSet<SimpleProcess>> newState)
    {
        foreach (var kvp in newState)
        {
            _dict[kvp.Key] = [.. kvp.Value];
        }
    }

    public void UpdateStates(IEnumerable<AvAppFacade> apps)
    {
        foreach (var app in apps)
        {
            UpdateState(app);
        }
    }

    public void UpdateState(AvAppFacade app)
    {
        ArgumentNullException.ThrowIfNull(app);
        if (!app.ActiveProcesses.NeedsUpdate())
        {
            Debug.WriteLine("Skipped update of facade");
            return;
        }
        if (!_dict.TryGetValue(app.Path, out var newProcs))
        {
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

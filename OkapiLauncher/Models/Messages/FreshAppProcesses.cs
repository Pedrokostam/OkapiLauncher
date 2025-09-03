﻿using System;
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
    private readonly Dictionary<string, HashSet<SimpleProcess>> _dict;

    public FreshAppProcesses(IDictionary<string, IEnumerable<SimpleProcess>> newState)
    {
        _dict = new Dictionary<string, HashSet<SimpleProcess>>(StringComparer.OrdinalIgnoreCase);
        foreach (var kvp in newState)
        {
            _dict[kvp.Key] = kvp.Value.ToHashSet();
        }
    }

    public FreshAppProcesses(IDictionary<string, HashSet<SimpleProcess>> newState)
    {
        _dict = new Dictionary<string, HashSet<SimpleProcess>>(StringComparer.OrdinalIgnoreCase);
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

    public IReadOnlyDictionary<string, HashSet<SimpleProcess>> State => new ReadOnlyDictionary<string, HashSet<SimpleProcess>>(_dict);

}

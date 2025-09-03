using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OkapiLauncher.Models;
public class DatedObservableCollection<T> : ObservableCollection<T>
{
    public readonly TimeSpan UpdateInterval = TimeSpan.FromSeconds(10);
    public DateTime UpdatedAt { get; private set; } = DateTime.MinValue;

    public bool NeedsUpdate()
    {
        return DateTime.UtcNow - UpdatedAt > UpdateInterval;
    }

    protected override void ClearItems()
    {
        base.ClearItems();
        UpdatedAt = DateTime.UtcNow;
    }

    protected override void InsertItem(int index, T item)
    {
        base.InsertItem(index, item);
        UpdatedAt = DateTime.UtcNow;
    }
    protected override void MoveItem(int oldIndex, int newIndex)
    {
        base.MoveItem(oldIndex, newIndex);
        UpdatedAt = DateTime.UtcNow;
    }
    protected override void RemoveItem(int index)
    {
        base.RemoveItem(index);
        UpdatedAt = DateTime.UtcNow;
    }
    protected override void SetItem(int index, T item)
    {
        base.SetItem(index, item);
        UpdatedAt = DateTime.UtcNow;
    }
}

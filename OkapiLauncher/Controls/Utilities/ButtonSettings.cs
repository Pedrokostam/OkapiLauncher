using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;
using System.Windows.Controls;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace OkapiLauncher.Controls.Utilities;
[StructLayout(LayoutKind.Sequential)]
public readonly record struct ButtonSettings
{
    public static readonly ImmutableArray<VisibleButtons> DefaultOrder;
    public VisibleButtons VisibleButtons { get; init; }
    public bool ShowDisabledButtons { get; init; }
    public int IconSize { get; init; } = 45;
    public ImmutableArray<VisibleButtons> Order { get; init; } = new();
    [JsonIgnore]
    public IEnumerable<VisibleButtons> ListOrder
    {
        get => Order;
        init
        {
            var valueList = value.ToList();
            var missing = DefaultOrder.Where(x => !valueList.Contains(x)).ToList();
            valueList.AddRange(missing);
            Order = valueList.ToImmutableArray();
        }
    }
    [JsonIgnore]
    public string StringOrder
    {
        get => new([.. Order.Cast<int>().Select(x => x.ToString("X")[0])]);
        init
        {
            var ints = value.ToLowerInvariant().ToCharArray().Select(c => Convert.ToInt32(c.ToString(), 16)).ToList().Cast<VisibleButtons>();
            ListOrder = ints;
        }
    }
    static ButtonSettings()
    {
        List<VisibleButtons> l = [
        VisibleButtons.Launch,
        VisibleButtons.Copy,
        VisibleButtons.Open,
        VisibleButtons.License,
        VisibleButtons.Log,
        VisibleButtons.Overview,
        VisibleButtons.KillAll,
        ];
        DefaultOrder = l.ToImmutableArray();
        Default = new()
        {
            Order = DefaultOrder,
            ShowDisabledButtons = false,
            VisibleButtons = VisibleButtons.Default,
            IconSize = 45,
        };
    }
    public ButtonSettings()
    {
        VisibleButtons = VisibleButtons.Default;
        ShowDisabledButtons = false;
        Order = DefaultOrder;
    }
    public static readonly ButtonSettings Default;
    public int GetPosition(VisibleButtons button) => Order.IndexOf(button);
}

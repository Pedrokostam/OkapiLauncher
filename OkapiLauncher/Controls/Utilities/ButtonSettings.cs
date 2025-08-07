using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Windows.Controls;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace OkapiLauncher.Controls.Utilities;
[StructLayout(LayoutKind.Sequential)]
public readonly record struct ButtonSettings
{
    private const string DefaultOrder = "1234567";
    private readonly string _order = DefaultOrder;
    public static readonly ImmutableArray<VisibleButtons> Defaulto;
    public VisibleButtons VisibleButtons { get; init; }
    public bool ShowDisabledButtons { get; init; }
    public ImmutableArray<VisibleButtons> Order { get; init; }
    public IEnumerable<VisibleButtons> ListOrder
    {
        get => Order;
        init
        {
            var valueList = value.ToList();
            var missing = Defaulto.Where(x => !valueList.Contains(x)).ToList();
            valueList.AddRange(missing);
            Order = valueList.ToImmutableArray();
        }
    }
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
        Defaulto = l.ToImmutableArray();
    }
    public ButtonSettings()
    {
        VisibleButtons = VisibleButtons.Default;
        ShowDisabledButtons = false;
        Order = Defaulto;
    }
    public int GetPosition(VisibleButtons button) => Order.IndexOf(button);
}

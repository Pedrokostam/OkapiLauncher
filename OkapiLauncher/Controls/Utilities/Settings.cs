using System.Runtime.InteropServices;
using System.Windows.Controls;

namespace OkapiLauncher.Controls.Utilities;
[StructLayout(LayoutKind.Sequential)]
public readonly record struct Settings
{
    public ButtonVisibility VisibleButtons { get; init; }
    public bool ShowDisabledButtons { get; init; }
    public string Order { get; init; }

    public Settings(ButtonVisibility visibleButtons = ButtonVisibility.Default, bool showDisabledButtons = false, string order = "01234567")
    {
        VisibleButtons = visibleButtons;
        ShowDisabledButtons = showDisabledButtons;
        Order = order;
    }
    public Settings()
    {
        VisibleButtons = ButtonVisibility.Default;
        ShowDisabledButtons = false;
        Order = "01234567";
    }
    public int GetPosition(ButtonVisibility button)
    {
        int exponent = (int)Math.Log2((int)button);
        return Order.IndexOf(exponent.ToString("X"), StringComparison.Ordinal);
    }
}

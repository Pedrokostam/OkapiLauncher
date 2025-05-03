namespace OkapiLauncher.Controls.Utilities;
[Flags]
public enum ButtonVisibility
{
    None = 0,
    //
    Launch = 1 << 0,
    Copy = 1 << 1,
    Open = 1 << 2,
    License = 1 << 3,
    Log = 1 << 4,
    Overview = 1 << 5,
    Kill = 1 << 6,
    KillAll = 1 << 7,
    //
    All = (KillAll << 1) - 1,
    //
    Default = Launch | Copy | Open | License | Log,
    BothKills = Kill | KillAll,
}

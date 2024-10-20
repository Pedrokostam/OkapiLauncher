using System.Runtime.InteropServices;

namespace OkapiLauncher.Models.Updates;

[StructLayout(LayoutKind.Auto)]
public readonly record struct UpdatePromptResult(UpdateDecision Decision, bool DisableAutomaticUpdates, string? UpdaterFilepath)
{

}

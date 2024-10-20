using System.Runtime.InteropServices;

namespace AuroraVisionLauncher.Models.Updates;

[StructLayout(LayoutKind.Auto)]
public readonly record struct UpdatePromptResult(UpdateDecision Decision, bool DisableAutomaticUpdates)
{

}

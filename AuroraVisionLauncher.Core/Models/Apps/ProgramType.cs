namespace AuroraVisionLauncher.Core.Models.Apps;

[Flags]
public enum ProgramType
{
    None,
    /// <summary>
    /// Aurora Vision Project - .avproj
    /// </summary>
    AuroraVisionProject,
    /// <summary>
    /// Adaptive Vision Project - .avproj
    /// </summary>
    AdaptiveVisionProject,
    /// <summary>
    /// Aurora Vision Runtime - .avexe
    /// </summary>
    AuroraVisionRuntime,
    /// <summary>
    /// FabImage Project - .fiproj
    /// </summary>
    FabImageProject,
    /// <summary>
    /// FabImage Runtime - .fiexe
    /// </summary>
    FabImageRuntime
}

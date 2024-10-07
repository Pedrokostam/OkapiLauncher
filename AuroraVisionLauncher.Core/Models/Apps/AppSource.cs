namespace AuroraVisionLauncher.Core.Models.Apps;
public static partial class AppReader
{
    internal record AppSource(string? Description, string SourcePath, bool IsFromEnvironment) : IAppSource
    {
        internal static AppSource FromInterface(IAppSource appSource) => new(appSource.Description, appSource.SourcePath, IsFromEnvironment: false);
    };
}

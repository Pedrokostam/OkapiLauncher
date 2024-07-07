namespace AuroraVisionLauncher.Models;

public class NoLaunchOptions : LaunchOptions
{
    public override  IEnumerable<string> GetCommandLineArgs()
    {
        return Enumerable.Empty<string>();
    }

    public override void Reset()
    {
    }
}

namespace AuroraVisionLauncher.Models;

public class NoLaunchOptions : LaunchOptions
{
    public override  IEnumerable<string> GetCommandLineArgs()
    {
        return [];
    }

    public override void Reset()
    {
    }
}

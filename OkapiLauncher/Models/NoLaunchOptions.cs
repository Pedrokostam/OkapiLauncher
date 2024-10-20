namespace OkapiLauncher.Models;

public class NoLaunchOptions : LaunchOptions
{
    public override bool HasAnyOptions => false;
    public override IEnumerable<string> GetCommandLineArgs()
    {
        return [];
    }

    public override void Reset()
    {
    }
}

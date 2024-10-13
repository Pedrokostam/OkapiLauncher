using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AuroraVisionLauncher.Models;

public partial class StudioLaunchOptions : LaunchOptions
{

    public override bool HasAnyOptions => false;

    public override IEnumerable<string> GetCommandLineArgs()
    {
        return new string[] { ProgramPath! };
    }

    public override void Reset()
    {
        
    }
}

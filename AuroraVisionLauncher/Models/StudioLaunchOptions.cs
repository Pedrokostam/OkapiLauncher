using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AuroraVisionLauncher.Models;

public partial class StudioLaunchOptions : LaunchOptions
{
   

    public override IEnumerable<string> GetCommandLineArgs()
    {
        return new string[] { ProgramPath! };
    }
}

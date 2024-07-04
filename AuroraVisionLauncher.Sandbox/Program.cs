using AuroraVisionLauncher.Core.Services;

namespace AuroraVisionLauncher.Sandbox;

internal class Program
{
    static void Main(string[] args)
    {
        var service = new FileAssociationService();
        service.GetCurrentAssociations();
    }
}

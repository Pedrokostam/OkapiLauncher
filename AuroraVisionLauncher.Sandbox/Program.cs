
using System.Security.AccessControl;
using System.Security.Principal;

namespace AuroraVisionLauncher.Sandbox;

internal class Program
{
    static void Main(string[] args)
    {
        FolderRequireElevation(@"C:\Program Files\Aurora Vision\Aurora Vision Studio 5.3 Professional");
        //var service = new FileAssociationService(new IconService());
        //service.GetCurrentAssociations();
    }
    public static bool FolderRequireElevation(string folderPath)
    {
        string path = Path.Join(folderPath, Guid.NewGuid().ToString());
        try
        {
            using FileStream filestream = File.Create(path, 64, FileOptions.DeleteOnClose);
            return true;
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }
    }
}

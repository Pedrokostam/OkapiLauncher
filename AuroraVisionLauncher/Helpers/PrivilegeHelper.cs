using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace AuroraVisionLauncher.Helpers;
internal class PrivilegeHelper
{
    public enum RequiredElevation
    {
        NoElevation,
        Elevated
    }
    /// <summary>
    /// Checks if elevation is required to write in the folder.
    /// </summary>
    /// <param name="folderPath">Path to the folder in which write operations will be conducted.</param>
    /// <returns><see langword="true"/> if the folder requires special permissions; <see langword="false"/> otherwise.</returns>
    public static RequiredElevation CheckFolderRequiresElevation(string folderPath)
    {
        string path = Path.Join(folderPath, Guid.NewGuid().ToString());
        try
        {
            using FileStream filestream = File.Create(path, 64, FileOptions.DeleteOnClose);
            return RequiredElevation.NoElevation;
        }
        catch (UnauthorizedAccessException)
        {
            return RequiredElevation.Elevated;
        }
    }
    /// <summary>
    /// Checks if the current user has admin rights.
    /// </summary>
    /// <returns></returns>
    public static bool IsUserAdmin()
    {
        using WindowsIdentity identity = WindowsIdentity.GetCurrent();
        WindowsPrincipal principal = new WindowsPrincipal(identity);
        return principal.IsInRole(WindowsBuiltInRole.Administrator);
    }
}

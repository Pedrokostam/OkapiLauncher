using System;
using System.Collections.Generic;
using System.Text;
using AuroraVisionLauncher.Core.Contracts.Services;
using Microsoft.Win32;

namespace AuroraVisionLauncher.Core.Services;
public interface IFileAssociationService
{
    Dictionary<string, string> GetCurrentAssociations();
    void SetAssociationsToApp(string mainAppExecutablePath, string? appName, IList<string>? extensionsToAssociate = null);
}
public class FileAssociationService : IFileAssociationService
{
    public const string RegistryAppName = "AuroraVisionLauncher";
    private static readonly string[] _extensions = [".avproj", ".fiproj", ".avexe", ".fiexe"];

    public Dictionary<string, string> GetCurrentAssociations()
    {
        var classes = Registry.CurrentUser.OpenSubKey("Software")!.OpenSubKey("Classes")!;
        Dictionary<string, string> userAssociations = [];
        foreach (var extension in _extensions)
        {
            if (classes.OpenSubKey(extension)?.GetValue(null) is string association)
            {
                userAssociations[extension] = association;
            }
        }
        return userAssociations;
    }

    public FileAssociationService()
    {
    }
    /// <summary>
    /// Creates keys in the registry that define what icon to use for each extension associated with the app and extension.
    /// </summary>
    /// <param name="mainAppPath"></param>
    /// <param name="appName"></param>
    private void SetAppShellKeys(string mainAppPath, string? appName)
    {
        foreach (var extension in _extensions)
        {
            SetAppShellKey(extension, mainAppPath, appName);
        }

    }
    private void SetAppShellKey(string extensionKey, string mainAppExecutablePath, string? appName)
    {
        var appFolder = Path.GetDirectoryName(mainAppExecutablePath);
        appName ??= Path.GetFileNameWithoutExtension(mainAppExecutablePath);
        var registryKeyName = appName + extensionKey.ToUpperInvariant();

        var classes = GetRegistryClasses();
        classes.DeleteSubKeyTree(registryKeyName);

        var appKey = classes.CreateSubKey(registryKeyName);
        appKey.SetValue(null, "Launcher association for " + extensionKey);

        var iconKey = appKey.CreateSubKey("DefaultIcon");
        // {app folder}/icons/{extensionKey without dot}.ico
        var iconPath = Path.Combine(appFolder!, "icons", extensionKey.TrimStart('.') + ".ico");
        // No need to enclose in quotes; 0 means use the first icon available
        iconKey.SetValue(null, $"{iconPath},0");

        var commandOpenShellKey = appKey.CreateSubKey("shell").CreateSubKey("open").CreateSubKey("command");
        commandOpenShellKey.SetValue(null, $"\"{mainAppExecutablePath}\" \"%1\"");


    }

    private static RegistryKey GetRegistryClasses()
    {
        return Registry.CurrentUser.OpenSubKey("Software")!.OpenSubKey("Classes")!;
    }

    public void SetAssociationsToApp(string mainAppExecutablePath, string? appName, IList<string>? extensionsToAssociate = null)
    {
        SetAppShellKeys(mainAppExecutablePath, appName);
        extensionsToAssociate ??= _extensions;

        appName ??= Path.GetFileNameWithoutExtension(mainAppExecutablePath);

        var classes = GetRegistryClasses();

        foreach (string extension in extensionsToAssociate)
        {
            var registryKeyName = appName + extension.ToUpperInvariant();
            var extKey = classes.CreateSubKey(extension);
            extKey.SetValue(null, registryKeyName);
        }

    }
}

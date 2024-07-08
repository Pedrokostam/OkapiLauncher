using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security.AccessControl;
using System.Text;
using System.Windows;
using AuroraVisionLauncher.Core.Contracts.Services;
using AuroraVisionLauncher.Helpers;
using AuroraVisionLauncher.Models;
using Microsoft.Extensions.Options;
using Microsoft.Win32;

namespace AuroraVisionLauncher.Services;
public interface IFileAssociationService
{
    void SetAssociationsToApp(string? mainAppExecutablePath = null);
}
public class FileAssociationService : IFileAssociationService
{
    private readonly record struct AssociationPackage(string IconResourcePath, string Extension)
    {
        public string IconName => Path.GetFileName(IconResourcePath);
    }

    private readonly AssociationPackage[] associations = [
        new AssociationPackage("Resources/Icons/AuroraVisionExecutor.ico",".avexe"),
        new AssociationPackage("Resources/Icons/AuroraVisionStudio.ico",".avproj"),
        new AssociationPackage("Resources/Icons/FabImageStudio.ico",".fiproj"),
        new AssociationPackage("Resources/Icons/FabImageRuntime.ico",".fiexe"),
        ];

    public const string RegistryAppName = "AuroraVisionLauncher";
    private static readonly string[] _extensions = [".avproj", ".fiproj", ".avexe", ".fiexe"];
    private readonly AppConfig _appConfig;

    //public Dictionary<string, string> GetCurrentAssociations()
    //{
    //    var classes = Registry.CurrentUser.OpenSubKey("Software")!.OpenSubKey("Classes")!;
    //    Dictionary<string, string> userAssociations = [];
    //    foreach (var extension in _extensions)
    //    {
    //        if (classes.OpenSubKey(extension)?.GetValue(null) is string association)
    //        {
    //            userAssociations[extension] = association;
    //        }
    //    }
    //    return userAssociations;
    //}

    public FileAssociationService(IOptions<AppConfig> appConfig)
    {
        this._appConfig = appConfig.Value;
    }
    /// <summary>
    /// Creates keys in the registry that define what icon to use for each extension associated with the app and extension.
    /// </summary>
    /// <param name="mainAppPath"></param>
    /// <param name="appName"></param>
    private void SetAppShellKeys(string mainAppPath)
    {
        foreach (var association in associations)
        {
            SetAppShellKey(association, mainAppPath);
        }

    }
    /// <summary>
    /// Create a key tree for an extension, all with commands, icon etc.
    /// </summary>
    /// <param name="association"></param>
    /// <param name="mainAppExecutablePath"></param>
    private void SetAppShellKey(AssociationPackage association, string mainAppExecutablePath)
    {
        // create name for registry key, like AuroraVisionLauncher.avproj
        var registryKeyName = GetExtensionRegistryName(association);

        var classes = GetRegistryClasses();
        // delete existing keys
        classes.DeleteSubKeyTree(registryKeyName,false);

        var appKey = classes.CreateSubKey(registryKeyName,true);
        //appKey.SetValue(null, "Launcher association for " + association.Extension);

        var iconKey = appKey.CreateSubKey("DefaultIcon",true);

        var iconPath = GetIconName(association);
        // No need to enclose in quotes; 0 means use the first icon available
        iconKey.SetValue(null, $"{iconPath},0");

        var commandOpenShellKey = appKey.CreateSubKey("shell",true)
            .CreateSubKey("open",true)
            .CreateSubKey("command",true);
        commandOpenShellKey.SetValue(null, $"\"{mainAppExecutablePath}\" \"%1\"");
    }

    private static string GetExtensionRegistryName(AssociationPackage association)
    {
        return RegistryAppName + association.Extension;
    }

    private static RegistryKey GetRegistryClasses()
    {
        var k = Registry.CurrentUser.OpenSubKey("Software\\Classes",true)!;
        return k;
    }


    public void RestoreIconFiles()
    {
        var appdata = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var iconFolder = Path.Combine(appdata, _appConfig.IconsFolder);
        Directory.CreateDirectory(iconFolder);
        foreach (var assoc in associations)
        {
            var iconStream = ResourceHelper.GetResourceStream(assoc.IconResourcePath);
            iconStream.Seek(0, SeekOrigin.Begin);
            string iconPath = GetIconName(assoc);
            using FileStream fs = new FileStream(iconPath, FileMode.Create);
            iconStream.CopyTo(fs);
        }
    }

    private string GetIconName(AssociationPackage assoc)
    {
        var appdata = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var iconFolder = Path.Combine(appdata, _appConfig.IconsFolder);
        return Path.Combine(iconFolder, assoc.IconName);
    }

    public void SetAssociationsToApp(string? mainAppExecutablePath = null)
    {
        try
        {
            string user = Environment.UserDomainName + "\\" + Environment.UserName;
            var sec = new RegistrySecurity();
            sec.AddAccessRule(new RegistryAccessRule(user,
                                                     RegistryRights.ReadKey | RegistryRights.SetValue | RegistryRights.CreateSubKey | RegistryRights.Delete,
                                                     InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                                                     PropagationFlags.InheritOnly,
                                                     AccessControlType.Allow));
            GetRegistryClasses().SetAccessControl(sec);
            mainAppExecutablePath ??= Process.GetCurrentProcess().MainModule!.FileName;

            RestoreIconFiles();

            SetAppShellKeys(mainAppExecutablePath);

            RemoveExplorerAssociations();

            SetAssociations();
        }
        catch (Exception ex)
        {

            MessageBox.Show(ex.Message);
        }

    }

    private void RemoveExplorerAssociations()
    {
        var fileExts = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\FileExts", true)!;
        foreach (var assoc in associations)
        {
            if(fileExts.OpenSubKey(assoc.Extension,true) is RegistryKey key)
            {
                key.DeleteSubKey("UserChoice", false);
            }
        }
    }

    private void SetAssociations()
    {
        var classes = GetRegistryClasses();

        foreach (var association in associations)
        {
            var registryKeyName = GetExtensionRegistryName(association);
            var extKey = classes.CreateSubKey(association.Extension);
            extKey.SetValue(null, registryKeyName);
        }
    }
}

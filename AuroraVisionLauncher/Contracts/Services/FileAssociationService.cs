using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Windows;
using AuroraVisionLauncher.Core.Contracts.Services;
using AuroraVisionLauncher.Helpers;
using AuroraVisionLauncher.Models;
using MahApps.Metro.Converters;
using Microsoft.Extensions.Options;
using Microsoft.Win32;

namespace AuroraVisionLauncher.Services;
public class FileAssociationService : IFileAssociationService
{

    private static RegistryKey CreateOrOpenRegistryPathWritable(params string[] steps)
    {
        return CreateOrOpenRegistrPathImpl(steps, writable: true);
    }


    /// <summary>
    /// Creates a subkey, or returns the existing one.
    /// </summary>
    /// <param name="steps"></param>
    /// <returns></returns>
    private static RegistryKey CreateOrOpenRegistrPathImpl(string[] steps, bool writable)
    {
        string path = CreateRegistryPathString(steps);
        return Registry.CurrentUser.CreateSubKey(path, writable);
    }

    private static string CreateRegistryPathString(params string[] steps)
    {
        return string.Join('\\', steps);
    }

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
            // create name for registry key, like AuroraVisionLauncher.avproj
            var registryKeyName = GetExtensionRegistryName(association);
            // delete existing keys
            string extensionSubkey = CreateRegistryPathString("Software", "Classes", registryKeyName);
            Registry.CurrentUser.DeleteSubKeyTree(extensionSubkey, throwOnMissingSubKey: false);
            //Registry.CurrentUser.Close();
            using var appKey = CreateOrOpenRegistryPathWritable("Software", "Classes", registryKeyName);

            using var iconKey = appKey.CreateSubKey("DefaultIcon", writable: true);

            var iconPath = GetIconName(association);
            // No need to enclose in quotes; 0 means use the first icon available
            iconKey.SetValue(name: null, $"{iconPath},0");

            using var commandOpenShellKey = appKey.CreateSubKey(CreateRegistryPathString("shell", "open", "command"));

            commandOpenShellKey.SetValue(name: null, $"\"{mainAppPath}\" \"%1\"");
        }

    }
    //private RegistryKey GetRegistryClasses()
    //{
    //    return CreateOrOpenRegistryPathWritable("Software", "Classes");
    //}

    private static string GetExtensionRegistryName(AssociationPackage association)
    {
        return RegistryAppName + association.Extension;
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
    private static bool IsAdministrator()
    {
        using WindowsIdentity identity = WindowsIdentity.GetCurrent();
        WindowsPrincipal principal = new WindowsPrincipal(identity);
        return principal.IsInRole(WindowsBuiltInRole.Administrator);
    }

    public void SetAssociationsToApp(string? mainAppExecutablePath = null)
    {
        if (!IsAdministrator())
        {
            var res = MessageBox.Show("""
                                      To change file association administrative privileges are required.
                                      Do you want to restart the application with those rights?
                                      """,
                                      "Administrative privileges required",
                                      MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
            if (res == MessageBoxResult.No)
            {
                return;
            }
            try
            {
                Process.Start(new ProcessStartInfo()
                {
                    FileName = Environment.ProcessPath,
                    UseShellExecute = true,
                    Verb = "runas", 
                });
                Environment.Exit(0);
            }
            catch (System.ComponentModel.Win32Exception)
            {
                MessageBox.Show("User cancelled elevation", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        try
        {
            mainAppExecutablePath ??= Environment.ProcessPath!;

            RestoreIconFiles();

            RemoveExplorerAssociations();

            SetAppShellKeys(mainAppExecutablePath);

            SetAssociations();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }

    }

    private void RemoveExplorerAssociations()
    {

        using var fileExts = CreateOrOpenRegistryPathWritable("Software", "Microsoft", "Windows", "CurrentVersion", "Explorer", "FileExts");
        foreach (var assoc in associations)
        {
            using var assocKey = fileExts.OpenSubKey(assoc.Extension, writable: true);
            assocKey?.DeleteSubKey("UserChoice", throwOnMissingSubKey: false);
        }
    }

    private void SetAssociations()
    {
        foreach (var association in associations)
        {
            var registryKeyName = GetExtensionRegistryName(association);
            using var extKey = CreateOrOpenRegistryPathWritable("Software", "Classes", association.Extension);
            extKey.SetValue(name: null, registryKeyName);
        }
    }
}

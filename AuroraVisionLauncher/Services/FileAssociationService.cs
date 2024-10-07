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
using AuroraVisionLauncher.Contracts.Services;
using AuroraVisionLauncher.Core.Contracts.Services;
using AuroraVisionLauncher.Helpers;
using AuroraVisionLauncher.Models;
using MahApps.Metro.Converters;
using Microsoft.Extensions.Options;
using Microsoft.Win32;
using Windows.Networking.NetworkOperators;
using System.Text.Json;
using System.Text.Json.Serialization;

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

    private readonly record struct AssociationPackage(string IconPath, string Extension)
    {
        public string IconName => Path.GetFileName(IconPath);
    }

    private readonly AssociationPackage[] _associations = [
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
        foreach (var association in _associations)
        {
            try
            {
                // create name for registry key, like AuroraVisionLauncher.avproj
                var registryKeyName = GetExtensionRegistryName(association);
                // delete existing keys
                string extensionSubkey = CreateRegistryPathString("Software", "Classes", registryKeyName);
                Registry.CurrentUser.DeleteSubKeyTree(extensionSubkey, throwOnMissingSubKey: false);
                //Registry.CurrentUser.Close();
                using var appKey = CreateOrOpenRegistryPathWritable("Software", "Classes", registryKeyName);

                using var iconKey = appKey.CreateSubKey("DefaultIcon", writable: true);

                var iconPath = GetFullIconPath(association);
                // No need to enclose in quotes; 0 means use the first icon available
                iconKey.SetValue(name: null, $"{iconPath},0");

                using var commandOpenShellKey = appKey.CreateSubKey(CreateRegistryPathString("shell", "open", "command"));

                commandOpenShellKey.SetValue(name: null, $"\"{mainAppPath}\" \"%1\"");
            }
            catch (Exception e)
            {
                throw new Exception($"On set shell keys and icons {association.Extension}", e);
            }
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
        foreach (var assoc in _associations)
        {
            var iconStream = ResourceHelper.GetResourceStream(assoc.IconPath);
            iconStream.Seek(0, SeekOrigin.Begin);
            string iconPath = GetFullIconPath(assoc);
            using FileStream fs = new FileStream(iconPath, FileMode.Create);
            iconStream.CopyTo(fs);
        }
    }

    private string GetFullIconPath(AssociationPackage assoc)
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
    private ProcessStartInfo GetStartInfo(string mainAppExecutablePath, bool runAsAdministrator)
    {
        var startInfo = new ProcessStartInfo()
        {
            FileName = "powershell",
            UseShellExecute = true,
        };
        if (runAsAdministrator)
        {
            startInfo.Verb = "runAsAdministrator";
        }
        startInfo.ArgumentList.Add("-ExecutionPolicy");
        startInfo.ArgumentList.Add("Bypass");
        startInfo.ArgumentList.Add("-NoLogo");
        startInfo.ArgumentList.Add("-NoProfile");
        //startInfo.ArgumentList.Add("-NoExit");
        startInfo.ArgumentList.Add("-NonInteractive");
        startInfo.ArgumentList.Add("-File");
        // TODO move file to resource 
        startInfo.ArgumentList.Add("C:\\Users\\Pedro\\source\\repos\\AuroraVisionLauncher\\AuroraVisionLauncher\\Services\\ps.ps1");
        startInfo.ArgumentList.Add(GetKeyPhrase());
        startInfo.ArgumentList.Add(RegistryAppName);
        startInfo.ArgumentList.Add(mainAppExecutablePath);
        startInfo.ArgumentList.Add(GetParameterJson());
        return startInfo;
    }
    public void SetAssociationsToApp(string? mainAppExecutablePath = null)
    {
        mainAppExecutablePath ??= Environment.ProcessPath!;

        RestoreIconFiles();
        var startInfo = GetStartInfo(mainAppExecutablePath, runAsAdministrator:true);
        try
        {
            Process.Start(startInfo);
        }
        catch (System.ComponentModel.Win32Exception)
        {
            //TODO add a dialog that tells you that all bets are off if you want to do it without admin
            startInfo = GetStartInfo(mainAppExecutablePath, runAsAdministrator:false);
            Process.Start(startInfo);
        }
        // TODO add checking whether association are correct
    }
    private void RemoveExplorerAssociations()
    {

        using var fileExts = CreateOrOpenRegistryPathWritable("Software", "Microsoft", "Windows", "CurrentVersion", "Explorer", "FileExts");
        foreach (var assoc in _associations)
        {
            try
            {
                // UserChoice is protected by default, but since its in ClassUser we can change the permissions
                using var userChoice = fileExts.OpenSubKey(CreateRegistryPathString(assoc.Extension, "UserChoice"), RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryRights.ChangePermissions);
                if (userChoice is not null)
                {
                    string username = WindowsIdentity.GetCurrent().Name;
                    RegistrySecurity security = userChoice.GetAccessControl();
                    AuthorizationRuleCollection accRules = security.GetAccessRules(true, true, typeof(NTAccount));

                    foreach (RegistryAccessRule accRule in accRules)
                    {
                        if (accRule.IdentityReference.Value.Equals(username, StringComparison.OrdinalIgnoreCase)
                            && accRule.AccessControlType == AccessControlType.Deny)
                        {
                            security.RemoveAccessRule(accRule);
                        }
                    }
                    userChoice.SetAccessControl(security);
                }
                fileExts.DeleteSubKeyTree(assoc.Extension, false);
            }
            catch (Exception e)
            {

                throw new Exception($"On delete {fileExts.Name + "\\" + assoc.Extension}", e);
            }
        }
    }

    private void SetAssociations()
    {
        foreach (var association in _associations)
        {
            var registryKeyName = GetExtensionRegistryName(association);
            try
            {
                using var extKey = CreateOrOpenRegistryPathWritable("Software", "Classes", association.Extension);
                extKey.SetValue(name: null, registryKeyName);
            }
            catch (Exception e)
            {
                throw new Exception($"On associate {registryKeyName}", e);
            }
        }
    }
    private string GetKeyPhrase()
    {
        var crypto = System.Security.Cryptography.MD5.Create();
        var bytes = System.Text.Encoding.UTF8.GetBytes(RegistryAppName);
        var hash = crypto.ComputeHash(bytes);
        return string.Concat(hash.Select(x => x.ToString("x2")));
    }
    private string GetParameterJson()
    {
        var assoc = _associations.Select(x=>x with { IconPath = GetFullIconPath(x) });
        return JsonSerializer.Serialize(assoc, new JsonSerializerOptions { WriteIndented = false });
    }
}

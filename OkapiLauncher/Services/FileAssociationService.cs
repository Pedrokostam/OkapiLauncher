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
using OkapiLauncher.Contracts.Services;
using OkapiLauncher.Helpers;
using OkapiLauncher.Models;
using Microsoft.Extensions.Options;
using Microsoft.Win32;
using Windows.Networking.NetworkOperators;
using System.Text.Json;
using CommunityToolkit.Mvvm.Messaging.Messages;
using OkapiLauncher.Core.Models.Apps;
using OkapiLauncher.Core.Models;
using System.Security.Policy;

namespace OkapiLauncher.Services;
public partial class FileAssociationService : IFileAssociationService
{
    /// <summary>
    /// Represents a temporary PowerShell script that is created in the system's temporary directory
    /// and automatically deleted when disposed. This is useful for executing transient scripts
    /// without leaving residual files on the system.
    /// </summary>
    internal partial class VanishingScript : IDisposable
    {
        public string FilePath { get; }
        public VanishingScript()
        {
            var name = Guid.NewGuid() + ".ps1";
            FilePath = Path.Join(Path.GetTempPath(), name);
            Assembly assembly = Assembly.GetExecutingAssembly();
            using Stream? stream = assembly.GetManifestResourceStream("OkapiLauncher.Services.Set-FileAssociations.ps1");
            ArgumentNullException.ThrowIfNull(stream);
            using FileStream filestream = new FileStream(FilePath, FileMode.Create, FileAccess.Write);
            stream.CopyTo(filestream);
        }

        public void Dispose()
        {
            try
            {
                if (File.Exists(FilePath))
                {
                    File.Delete(FilePath);
                }

            }
            catch (DirectoryNotFoundException) { }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }
        public static implicit operator string(VanishingScript fg) => fg.FilePath;
    }
    private static RegistryKey CreateOrOpenRegistryPathWritable(params string[] steps)
    {
        return CreateOrOpenRegistrPathImpl(steps, writable: true);
    }
    private static RegistryKey CreateOrOpenRegistryPathNonWritable(params string[] steps)
    {
        return CreateOrOpenRegistrPathImpl(steps, writable: false);
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

    private readonly record struct AssociationPackage(AvBrand Brand, AvType Type, string IconPath, string? Extension)
    {
        public string IconName => Path.GetFileName(IconPath);
    }

    /// <summary>
    /// All application associated with their icons.
    /// </summary>
    private readonly AssociationPackage[] _iconAssociations = [
        new AssociationPackage(AvBrand.Aurora,AvType.Professional,"Resources/Icons/AuroraVisionStudio.ico",".avproj"),
        new AssociationPackage(AvBrand.Aurora,AvType.Runtime,"Resources/Icons/AuroraVisionExecutor.ico",".avexe"),
        new AssociationPackage(AvBrand.FabImage,AvType.Professional,"Resources/Icons/FabImageStudio.ico",".fiproj"),
        new AssociationPackage(AvBrand.FabImage,AvType.Runtime,"Resources/Icons/FabImageExecutor.ico",".fiexe"),

        new AssociationPackage(AvBrand.Adaptive,AvType.Professional,"Resources/Icons/AdaptiveVisionStudio.ico",Extension: null),
        new AssociationPackage(AvBrand.Adaptive,AvType.Runtime,"Resources/Icons/AdaptiveVisionExecutor.ico", Extension: null),
        new AssociationPackage(AvBrand.Adaptive,AvType.DeepLearning,"Resources/Icons/AdaptiveVisionDeepLearning.ico", Extension: null),
        new AssociationPackage(AvBrand.Aurora,AvType.DeepLearning,"Resources/Icons/AuroraVisionDeepLearning.ico", Extension: null),
        new AssociationPackage(AvBrand.FabImage,AvType.DeepLearning,"Resources/Icons/FabImageDeepLearning.ico", Extension: null),
    ];
    /// <summary>
    /// All applications that have an extension associated with them.
    /// </summary>
    private IEnumerable<AssociationPackage > Associations => _iconAssociations.Where(x=>x.Extension is not null);

    public const string RegistryAppName = "OkapiLauncher";

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

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0290:Use primary constructor", Justification = "The fields should be readonly")]
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
        foreach (var association in _iconAssociations)
        {
            try
            {
                // create name for registry key, like OkapiLauncher.avproj
                var registryKeyName = GetExtensionRegistryName(association);
                // delete existing keys
                string extensionSubkey = CreateRegistryPathString("Software", "Classes", registryKeyName);
                MegaDeleteTree(Registry.CurrentUser, extensionSubkey);
                //Registry.CurrentUser.DeleteSubKeyTree(extensionSubkey, throwOnMissingSubKey: false);
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
    private void UpdateIconIfNeeded(Stream iconStream, string iconPath)
    {
        iconStream.Seek(0, SeekOrigin.Begin);
        if (!File.Exists(iconPath))
        {
            using FileStream fs = new FileStream(iconPath, FileMode.Create);
            iconStream.CopyTo(fs);
            return;
        }
        ReadOnlySpan<byte> iconSpan = stackalloc byte[(int)iconStream.Length];
        using var fileStream = File.OpenRead(iconPath);
        ReadOnlySpan<byte> fileSpan = stackalloc byte[(int)fileStream.Length];
        if (iconSpan.SequenceEqual(fileSpan))
        {
            // same stuff, dont write
            return;
        }
        using FileStream fs2 = new FileStream(iconPath, FileMode.Create);
        iconStream.CopyTo(fs2);
    }
    public void RestoreIconFiles()
    {
        var appdata = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var iconFolder = Path.Combine(appdata, _appConfig.IconsFolder);
        Directory.CreateDirectory(iconFolder);
        foreach (var assoc in _iconAssociations)
        {
            string iconPath = GetFullIconPath(assoc);
            var iconStream = ResourceHelper.GetResourceStream(assoc.IconPath);
            UpdateIconIfNeeded(iconStream, iconPath);
        }
    }

    /// <summary>
    /// Get the path of copied icon resource (in LocalAppData).
    /// </summary>
    /// <param name="assoc"></param>
    /// <returns></returns>
    private string GetFullIconPath(AssociationPackage assoc)
    {
        var appdata = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var iconFolder = Path.Combine(appdata, _appConfig.IconsFolder);
        return Path.Combine(iconFolder, assoc.IconName);
    }
    public string GetLocalIconPath(AvBrand brand, AvType type)
    {
        var assoc = _iconAssociations.FirstOrDefault(x => x.Brand == brand && x.Type == type);
        if (assoc == default)
        {
            throw new KeyNotFoundException($"No icon for {brand} {type}");
        }
        return GetFullIconPath(assoc);
    }
    private ProcessStartInfo GetStartInfo(string mainAppExecutablePath, VanishingScript scriptToRun, bool runAsAdministrator)
    {
        var startInfo = new ProcessStartInfo()
        {
            FileName = "powershell",
            UseShellExecute = true,
            Verb = runAsAdministrator ? "runas" : "",
        };
        startInfo.ArgumentList.Add("-ExecutionPolicy");
        startInfo.ArgumentList.Add("Bypass");
        startInfo.ArgumentList.Add("-NoLogo");
        startInfo.ArgumentList.Add("-NoProfile");
        //startInfo.ArgumentList.Add("-NoExit");
        //startInfo.ArgumentList.Add("-NonInteractive");
        startInfo.ArgumentList.Add("-File");
        startInfo.ArgumentList.Add(scriptToRun.FilePath);
        startInfo.ArgumentList.Add(GetKeyPhrase());
        startInfo.ArgumentList.Add(RegistryAppName);
        startInfo.ArgumentList.Add(mainAppExecutablePath);
        startInfo.ArgumentList.Add(GetParameterJson());
        return startInfo;
    }
    public async Task SetAssociationsToApp(string? mainAppExecutablePath = null)
    {
        var t = CheckCurrentAssociations(mainAppExecutablePath);
        mainAppExecutablePath ??= Environment.ProcessPath!;
        RestoreIconFiles();
        //RemoveExplorerAssociations();
        //SetAppShellKeys(mainAppExecutablePath);
        //SetAssociations();
        //return;
        using var tempScript = new VanishingScript();
        var startInfo = GetStartInfo(mainAppExecutablePath, tempScript, runAsAdministrator: false);
        var process = Process.Start(startInfo);
        if (process is null)
        {
            return;
        }
        await process.WaitForExitAsync();
        // TODO add checking whether association are correct
    }
    private void RemoveExplorerAssociations()
    {

        using var fileExts = CreateOrOpenRegistryPathWritable("Software", "Microsoft", "Windows", "CurrentVersion", "Explorer", "FileExts");
        foreach (var assoc in Associations)
        {
            if (assoc.Extension is null)
            {
                continue;
            }
            try
            {
                MegaDeleteTree(fileExts, assoc.Extension);
                //// UserChoice is protected by default, but since its in ClassUser we can change the permissions
                //using var userChoice = fileExts.OpenSubKey(CreateRegistryPathString(assoc.Extension, "UserChoice"), RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryRights.ChangePermissions);
                //if (userChoice is not null)
                //{
                //    string username = WindowsIdentity.GetCurrent().Name;
                //    RegistrySecurity security = userChoice.GetAccessControl();
                //    AuthorizationRuleCollection accRules = security.GetAccessRules(true, true, typeof(NTAccount));

                //    foreach (RegistryAccessRule accRule in accRules)
                //    {
                //        if (accRule.IdentityReference.Value.Equals(username, StringComparison.OrdinalIgnoreCase)
                //            && accRule.AccessControlType == AccessControlType.Deny)
                //        {
                //            security.RemoveAccessRule(accRule);
                //        }
                //    }
                //    userChoice.SetAccessControl(security);
                //}
                //fileExts.DeleteSubKeyTree(assoc.Extension, false);
            }
            catch (Exception e)
            {
                throw new Exception($"On delete {fileExts.Name + "\\" + assoc.Extension}", e);
            }
        }
    }
    private static void MegaDeleteTree(RegistryKey key, string treeName)
    {
        try
        {
            key.DeleteSubKeyTree(treeName, throwOnMissingSubKey: true);
            var names = key.GetSubKeyNames();
            if (names.Contains(treeName, StringComparer.OrdinalIgnoreCase))
            {
                key.DeleteSubKey(treeName);
            }
        }
        catch (ArgumentException)
        {
            var subkey = key.OpenSubKey(treeName, writable: false);
            if (subkey is null)
            {
                return;
            }
            foreach (var item in subkey.GetSubKeyNames())
            {
                MegaDeleteTree(subkey, item);
            }
            subkey.Close();
            key.DeleteSubKey(treeName, throwOnMissingSubKey: false);
        }
    }

    private void SetAssociations()
    {
        foreach (var association in Associations)
        {
            if (association.Extension is null)
            {
                continue;
            }
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
    private static string GetKeyPhrase()
    {
        var crypto = System.Security.Cryptography.MD5.Create();
        var bytes = Encoding.UTF8.GetBytes(RegistryAppName);
        var hash = crypto.ComputeHash(bytes);
        return string.Concat(hash.Select(x => x.ToString("x2")));
    }
    private string GetParameterJson()
    {
        var assoc = Associations.Select(x => x with { IconPath = GetFullIconPath(x) });
        return JsonSerializer.Serialize(assoc, new JsonSerializerOptions { WriteIndented = false });
    }

    private static T? GetValue<T>(RegistryKey? key, string? name = null) where T : class
    {
        if (key is null)
        {
            return null;
        }
        return key.GetValue(name, defaultValue: null) as T;
    }

    public IEnumerable<FileAssociationStatus> CheckCurrentAssociations(string? mainAppExecutablePath = null)
    {
        var list = new List<FileAssociationStatus>();
        mainAppExecutablePath ??= Environment.ProcessPath!;
        string mainAppName = Path.GetFileName(mainAppExecutablePath);
        foreach (var association in _iconAssociations)
        {
            if (association.Extension is null)
            {
                continue;
            }
            var registryKeyName = GetExtensionRegistryName(association);
            using var commandKey = CreateOrOpenRegistryPathNonWritable("Software", "Classes", registryKeyName, "shell", "open", "command");
            bool commandGood = GetValue<string>(commandKey)?.Contains(mainAppExecutablePath, StringComparison.OrdinalIgnoreCase) ?? false;

            using var classesKey = CreateOrOpenRegistryPathNonWritable("Software", "Classes", association.Extension);
            bool classesGood = string.Equals(GetValue<string>(classesKey), registryKeyName, StringComparison.OrdinalIgnoreCase);

            using var userchoice = CreateOrOpenRegistryPathNonWritable("Software", "Microsoft", "Windows", "CurrentVersion", "Explorer", "FileExts", association.Extension, "UserChoice");
            var userChoiceValue = GetValue<string>(userchoice, "ProgId");
            bool userChoiceGood = userChoiceValue is null || string.Equals(userChoiceValue, mainAppName, StringComparison.OrdinalIgnoreCase);

            list.Add(new(association.Extension, classesGood && commandGood && userChoiceGood));
        }
        return list;
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using AuroraVisionLauncher.Contracts.ViewModels;
using AuroraVisionLauncher.Models.Updates;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;

namespace AuroraVisionLauncher.ViewModels;
public partial class VersionDecisionDialogViewModel : ObservableValidator, INavigationAware, IDialogViewModel<UpdatePromptResult>
{
    //public Func<Task> CloseDialog { get; }
    public HtmlVersionResponse VersionInformation { get; }
    public bool IsFromInstaller { get; }
    private readonly TaskCompletionSource<UpdatePromptResult> _done = new();
    public string Message => string.Format(System.Globalization.CultureInfo.InvariantCulture, Properties.Resources.VersionCheckDialogMessageFormat, VersionInformation.VersionTag, VersionInformation.ReleaseDate.ToLocalTime());
    public VersionDecisionDialogViewModel(Func<Task> closeDialog, HtmlVersionResponse information)
    {
        //CloseDialog = closeDialog;
        VersionInformation = information;
        IsFromInstaller = TestIsInstalled();
    }
    public bool AutomaticButtonEnabled => VersionInformation.IsAutomaticCheck && !ShouldDisableAutoUpdates;
    public bool AutoUpdateEnabled => VersionInformation.InstallerDownloadLink is not null;
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(DisableAutomaticChecksCommand))]
    private bool _shouldDisableAutoUpdates;
    private void SetResult(UpdateDecision decision)
    {
        _done.SetResult(new UpdatePromptResult(decision, AutomaticButtonEnabled));
    }
    public void OnNavigatedFrom()
    {
    }
    [RelayCommand]
    private void OpenDownloadPage()
    {
        SetResult(UpdateDecision.OpenPage);
    }
    [RelayCommand]
    private void Cancel()
    {
        SetResult(UpdateDecision.Cancel);
    }
    [RelayCommand]
    private void IgnoreRelease()
    {
        SetResult(UpdateDecision.SkipVersion);
    }

    public void OnNavigatedTo(object parameter)
    {
        ValidateAllProperties();
    }
    [RelayCommand(CanExecute = nameof(AutomaticButtonEnabled))]
    private void DisableAutomaticChecks()
    {
        ShouldDisableAutoUpdates = true;
    }
    public Task WaitForExit()
    {
        return _done.Task;
    }

    [ObservableProperty]
    private DownloadProgressViewModel? _progressViewModel = null;

    [RelayCommand(CanExecute = nameof(AutoUpdateEnabled))]
    private async Task AutoUpdate()
    {
        ProgressViewModel = new(VersionInformation, IsFromInstaller);
        string destinationFilePath = Path.GetTempFileName();
        var isDownloaded = await ProgressViewModel.DownloadFileAsync(destinationFilePath);
        //ProgressViewModel = null;
        if (!isDownloaded)
        {
            return;
        }
        Process.Start(destinationFilePath);
    }
    private static bool TestIsInstalled()
    {
        return CheckUninstallKeys(Registry.CurrentUser) || CheckUninstallKeys(Registry.LocalMachine);
    }
    private static bool CheckUninstallKeys(RegistryKey rootKey)
    {
        string assemblyLocation = Assembly.GetExecutingAssembly().Location;
        using var uninstall = rootKey.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall");
        if (uninstall is null)
        {
            return false;
        }
        foreach (var subkeyName in uninstall.GetSubKeyNames())
        {
            using var subkey = uninstall.OpenSubKey(subkeyName);
            if (subkey is null)
            {
                continue;
            }
            var installLocation = subkey.GetValue("InstallLocation") as string;
            if (assemblyLocation.Equals(installLocation, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

        }
        return false;
    }
    Task<UpdatePromptResult> IDialogViewModel<UpdatePromptResult>.WaitForExit() => _done.Task;
}

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
using AuroraVisionLauncher.Helpers;
using AuroraVisionLauncher.Models.Updates;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;

namespace AuroraVisionLauncher.ViewModels;
public partial class VersionDecisionDialogViewModel : ObservableValidator, INavigationAware, IDialogViewModel<UpdatePromptResult>
{
    //public Func<Task> CloseDialog { get; }
    public UpdateDataCarier UpdateInfo { get; }
    private readonly TaskCompletionSource<UpdatePromptResult> _done = new();
    public string Message => string.Format(
        System.Globalization.CultureInfo.InvariantCulture,
        Properties.Resources.VersionCheckDialogMessageFormat,
        UpdateInfo.HtmlResponse?.VersionTag,
        UpdateInfo.HtmlResponse?.ReleaseDate.ToLocalTime());
    public VersionDecisionDialogViewModel(UpdateDataCarier information)
    {
        UpdateInfo = information;
    }
    public bool AutomaticButtonEnabled => UpdateInfo.IsAutomaticUpdateCheck && !ShouldDisableAutoUpdates;
    public bool AutoUpdateEnabled => UpdateInfo.HtmlResponse?.InstallerDownloadLink is not null;
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(DisableAutomaticChecksCommand))]
    private bool _shouldDisableAutoUpdates;
    private void SetResult(UpdateDecision decision)
    {
        _done.SetResult(new UpdatePromptResult(decision, AutomaticButtonEnabled,UpdaterFilePath));
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
    private async Task DownloadUpdater()
    {
        ProgressViewModel = new(UpdateInfo);
        string destinationFilePath = Path.GetTempFileName();
        var isDownloaded = await ProgressViewModel.DownloadFileAsync(destinationFilePath);
        //ProgressViewModel = null;
        if (!isDownloaded)
        {
            return;
        }
        UpdaterFilePath = destinationFilePath;
        SetResult(UpdateDecision.LaunchUpdater);
    }
    public string? UpdaterFilePath { get; private set; } = null;
    Task<UpdatePromptResult> IDialogViewModel<UpdatePromptResult>.WaitForExit() => _done.Task;
}

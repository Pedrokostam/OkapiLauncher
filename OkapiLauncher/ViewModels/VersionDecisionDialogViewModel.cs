using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using AuroraVisionLauncher.Contracts.ViewModels;
using AuroraVisionLauncher.Models.Updates;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AuroraVisionLauncher.ViewModels;
public partial class VersionDecisionDialogViewModel : ObservableValidator, INavigationAware, IDialogViewModel<UpdatePromptResult>
{
    //public Func<Task> CloseDialog { get; }
    public HtmlVersionResponse VersionInformation { get; }
    private readonly TaskCompletionSource<UpdatePromptResult> _done = new();
    public string Message => string.Format(System.Globalization.CultureInfo.InvariantCulture, Properties.Resources.VersionCheckDialogMessageFormat, VersionInformation.VersionTag, VersionInformation.ReleaseDate.ToLocalTime());
    public VersionDecisionDialogViewModel(Func<Task> closeDialog, HtmlVersionResponse information)
    {
        //CloseDialog = closeDialog;
        VersionInformation = information;
    }
    public bool AutomaticButtonEnabled => VersionInformation.IsAutomaticCheck && !ShouldDisableAutoUpdates;

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

    Task<UpdatePromptResult> IDialogViewModel<UpdatePromptResult>.WaitForExit()=>_done.Task;
}

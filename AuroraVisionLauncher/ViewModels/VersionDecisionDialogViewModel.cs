using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using AuroraVisionLauncher.Contracts.ViewModels;
using AuroraVisionLauncher.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AuroraVisionLauncher.ViewModels;
public partial class VersionDecisionDialogViewModel : ObservableValidator, INavigationAware, IDialogViewModel
{
    //public Func<Task> CloseDialog { get; }
    public NewVersionInformation Information { get; }
    private readonly TaskCompletionSource _done = new();
    public string Message => string.Format(System.Globalization.CultureInfo.InvariantCulture, Properties.Resources.VersionCheckDialogMessageFormat, Information.Version, Information.ReleaseDate.ToLocalTime());
    public VersionDecisionDialogViewModel(Func<Task> closeDialog, NewVersionInformation information)
    {
        //CloseDialog = closeDialog;
        Information = information;
    }
    public bool AutomaticButtonEnabled => Information.IsAutomaticCheck && !Information.DisableAutomaticUpdates;

    public void OnNavigatedFrom()
    {
    }
    [RelayCommand]
    private void OpenDownloadPage()
    {
        Information.UserDecision = NewVersionInformation.Decision.OpenPage;
        _done.SetResult();
    }
    [RelayCommand]
    private void Cancel()
    {
        Information.UserDecision = NewVersionInformation.Decision.Cancel;
        _done.SetResult();
    }
    [RelayCommand]
    private void IgnoreRelease()
    {
        Information.UserDecision = NewVersionInformation.Decision.SkipVersion;
        _done.SetResult();
    }

    public void OnNavigatedTo(object parameter)
    {
        ValidateAllProperties();
    }
    [RelayCommand(CanExecute = nameof(AutomaticButtonEnabled))]
    private void DisableAutomaticChecks()
    {
        Information.DisableAutomaticUpdates = true;
        DisableAutomaticChecksCommand.NotifyCanExecuteChanged();
    }
    public Task WaitForExit()
    {
        return _done.Task;
    }
}

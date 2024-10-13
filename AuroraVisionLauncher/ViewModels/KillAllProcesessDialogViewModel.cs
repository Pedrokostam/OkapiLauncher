using AuroraVisionLauncher.Contracts.ViewModels;
using AuroraVisionLauncher.Core.Models.Apps;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AuroraVisionLauncher.ViewModels;

public partial class KillAllProcesessDialogViewModel : ObservableValidator, INavigationAware, IDialogViewModel<bool>
{
    public IAvApp App { get; }
    private readonly TaskCompletionSource<bool> _done = new();
    public KillAllProcesessDialogViewModel(IAvApp app)
    {
        App = app;
    }

    public Task WaitForExit() => WaitForExit();
    Task<bool> IDialogViewModel<bool>.WaitForExit()
    {
        return _done.Task;
    }

    public void OnNavigatedTo(object parameter)
    {

    }

    public void OnNavigatedFrom()
    {
    }
    [RelayCommand]
    private void YesKill()
    {
        _done.SetResult(true);
    }
    [RelayCommand]
    private void NoSpare()
    {
        _done.SetResult(false);
    }
}
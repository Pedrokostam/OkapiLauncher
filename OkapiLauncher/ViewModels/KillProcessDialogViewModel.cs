using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using AuroraVisionLauncher.Contracts.ViewModels;
using AuroraVisionLauncher.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AuroraVisionLauncher.ViewModels;

public partial class KillProcessDialogViewModel : ObservableValidator, INavigationAware, IDialogViewModel<bool>
{
    public SimpleProcess Process { get; }
    public string ProcessName => $"{Process.ProcessName} - {Process.MainWindowTitle}";
    private readonly TaskCompletionSource<bool> _done = new();
    public KillProcessDialogViewModel(SimpleProcess process)
    {
        Process = process;
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

using System.Windows.Input;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace OkapiLauncher.ViewModels;

public partial class ShellDialogViewModel : ObservableObject
{
    public Action<bool?>? SetResult { get; set; }

    public ShellDialogViewModel()
    {
    }
    [RelayCommand]
    private void OnClose()
    {
        bool result = true;
        if (SetResult is null)
        {
            return;
        }
        SetResult(result);
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AuroraVisionLauncher.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AuroraVisionLauncher.ViewModels;
public partial class CustomSourceDialogEditorViewModel : ObservableObject
{
    public Func<Task> CloseDialog { get; }
    private CustomAppSource _source;
    public CustomSourceDialogEditorViewModel(CustomAppSource source, Func<Task> closeDialogAction)
    {
        CloseDialog = closeDialogAction;
        _source = source;
        _description = source.Description ?? "";
        _path = source.Path;

    }
    [ObservableProperty]
    private string _description;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SourcePath))]
    private string _path;
    public string SourcePath => CustomAppSource.ExpandPath(Path);

    [RelayCommand]
    private async Task Accept()
    {
        Debug.WriteLine("Accept");
        _source.Path = Path;
        _source.Description = Description;
        await CloseDialog.Invoke().ConfigureAwait(true);
    }

    [RelayCommand]
    private async Task Cancel()
    {
        await CloseDialog.Invoke().ConfigureAwait(true);
    }
}

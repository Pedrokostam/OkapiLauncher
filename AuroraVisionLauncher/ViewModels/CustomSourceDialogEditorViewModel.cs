using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AuroraVisionLauncher.Contracts.ViewModels;
using AuroraVisionLauncher.Core.Models.Apps;
using AuroraVisionLauncher.Models;
using AuroraVisionLauncher.Validators;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace AuroraVisionLauncher.ViewModels;


public partial class CustomSourceDialogEditorViewModel : ObservableValidator, INavigationAware, IDialogViewModel
{
    //public Func<Task> CloseDialog { get; }
    private readonly CustomAppSource _source;
    public CustomSourceDialogEditorViewModel(CustomAppSource source, Func<Task> closeDialogAction)
    {
        //CloseDialog = closeDialogAction;
        _source = source;
        _description = source.Description ?? "";
        _path = source.Path;
        UpdateMatchedApp();
    }
    [ObservableProperty]
    [MinLength(1)]
    [NotifyDataErrorInfo]
    private string _description;
    [ObservableProperty]
    //[MustBeDirectory]
    [NotifyPropertyChangedFor(nameof(SourcePath))]
    [NotifyCanExecuteChangedFor(nameof(AcceptCommand))]
    [NotifyDataErrorInfo]
    [MinLength(1)]
    private string _path;
    public string SourcePath => CustomAppSource.ExpandPath(Path);

    partial void OnPathChanged(string value)
    {
        UpdateMatchedApp();
    }

    private void UpdateMatchedApp()
    {
        MatchedApp = AppReader.GetAvAppFromSource(new CustomAppSource() { Description = Description, Path = Path });
    }

    public bool PathExists => File.Exists(Path);

    private readonly TaskCompletionSource _done = new();
    [ObservableProperty]
    private AvApp? _matchedApp = null;

    public Task WaitForExit()
    {
        return _done.Task;
    }

    private bool CanAccept()
    {
        return !this.HasErrors;
    }

    [RelayCommand(CanExecute = nameof(CanAccept))]
    private void Accept()
    {
        Debug.WriteLine("Accept");
        _source.Path = Path;
        _source.Description = Description;
        _done.SetResult();
    }

    [RelayCommand]
    private void Cancel()
    {
        _done.SetResult();
    }

    public void OnNavigatedTo(object parameter)
    {
        ValidateAllProperties();
        AcceptCommand.NotifyCanExecuteChanged();
    }

    public void OnNavigatedFrom()
    {
    }
    [RelayCommand]
    private void OpenFilePicker()
    {
        string initialDir = string.IsNullOrWhiteSpace(Path) ? Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) : Path;
        var picker = new CommonOpenFileDialog()
        {
            InitialDirectory = initialDir,
            IsFolderPicker = true,
        };
        picker.Title = "Select executable or dynamic library";
        var res = picker.ShowDialog();
        if (res == CommonFileDialogResult.Ok)
        {
            Path = picker.FileName;
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using OkapiLauncher.Contracts.ViewModels;
using OkapiLauncher.Core.Models.Apps;
using OkapiLauncher.Models;
using OkapiLauncher.Properties;
using OkapiLauncher.Validators;

namespace OkapiLauncher.ViewModels;


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
    [Required(ErrorMessageResourceName = "ValidationStringEmpty", ErrorMessageResourceType = typeof(Resources))]
    [RegularExpression(@".*\S+.*", ErrorMessageResourceName = "ValidationStringEmpty", ErrorMessageResourceType = typeof(Resources))]
    [NotifyPropertyChangedFor(nameof(SourcePath))]
    [NotifyCanExecuteChangedFor(nameof(AcceptCommand))]
    [NotifyDataErrorInfo]
    private string _description;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SourcePath))]
    [NotifyCanExecuteChangedFor(nameof(AcceptCommand))]
    [NotifyDataErrorInfo]
    [Required(ErrorMessageResourceName = "ValidationStringEmpty", ErrorMessageResourceType = typeof(Resources))]
    [RegularExpression(@".*\S+.*", ErrorMessageResourceName = "ValidationStringEmpty", ErrorMessageResourceType = typeof(Resources))]
    [MustBeDirectory]
    [CustomValidation(typeof(CustomSourceDialogEditorViewModel), nameof(ValidateMatchedApp))]
    private string _path;
    public string SourcePath => CustomAppSource.ExpandPath(Path);

    partial void OnPathChanged(string value)
    {
        UpdateMatchedApp();
        ValidateProperty(value, nameof(Path));

    }

    private bool UpdateMatchedApp()
    {
        MatchedApp = AppReader.GetAvAppFromSource(new CustomAppSource() { Description = Description, Path = Path.Trim() });
        return MatchedApp is null;
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
        return !this.HasErrors && MatchedApp is not null;
    }

    [RelayCommand(CanExecute = nameof(CanAccept))]
    private void Accept()
    {
        Debug.WriteLine("Accept");
        _source.Path = Path.Trim();
        _source.Description = Description.Trim();
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
        picker.Title = "Select the root directory of the application (not file)";
        var res = picker.ShowDialog();
        if (res == CommonFileDialogResult.Ok)
        {
            Path = picker.FileName;
        }
    }

    public static ValidationResult? ValidateMatchedApp(
     string? path,
     ValidationContext context)
    {
        if (string.IsNullOrWhiteSpace(path))
            return ValidationResult.Success;

        var vm = (CustomSourceDialogEditorViewModel)context.ObjectInstance;

        return vm.MatchedApp is null
            ? new ValidationResult(Resources.CustomSourceEditorAppNotMatched)
            : ValidationResult.Success;
    }
}
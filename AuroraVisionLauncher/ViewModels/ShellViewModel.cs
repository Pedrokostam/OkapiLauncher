using System.Windows;
using System.Windows.Input;

using AuroraVisionLauncher.Contracts.Services;
using AuroraVisionLauncher.Models.Messages;
using AuroraVisionLauncher.Properties;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace AuroraVisionLauncher.ViewModels;

// You can show pages in different ways (update main view, navigate, right pane, new windows or dialog)
// using the NavigationService, RightPaneService and WindowManagerService.
// Read more about MenuBar project type here:
// https://github.com/microsoft/TemplateStudio/blob/main/docs/WPF/projectTypes/menubar.md
public partial class ShellViewModel : ObservableObject
{
    private readonly INavigationService _navigationService;
    private readonly IRightPaneService _rightPaneService;
    private readonly IMessenger _messenger;

    public ShellViewModel(INavigationService navigationService, IRightPaneService rightPaneService, IMessenger messenger)
    {
        _navigationService = navigationService;
        _rightPaneService = rightPaneService;
        _messenger = messenger;
    }

    [RelayCommand()]
    private void OnLoaded()
    {
        _navigationService.Navigated += OnNavigated;
    }

    [RelayCommand()]
    private void OnUnloaded()
    {
        _rightPaneService.CleanUp();
        _navigationService.Navigated -= OnNavigated;
    }

    private bool CanGoBack()
        => _navigationService.CanGoBack;

    [RelayCommand(CanExecute = nameof(CanGoBack))]
    private void OnGoBack()
        => _navigationService.GoBack();

    private void OnNavigated(object? sender, string? viewModelName)
    {
        GoBackCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand()]
    private void OnMenuFileExit()
        => Application.Current.Shutdown();


    [RelayCommand()]
    private void OnMenuViewsLauncher()
        => _navigationService.NavigateTo(typeof(LauncherViewModel).FullName!, null, true);

    [RelayCommand()]
    private void OnMenuFileSettings()
        => _rightPaneService.OpenInRightPane(typeof(SettingsViewModel).FullName!);

    [RelayCommand()]
    private void OnMenuViewsInstalledApps()
        => _navigationService.NavigateTo(typeof(InstalledAppsViewModel).FullName!, null, true);
    [RelayCommand()]
    private void OnMenuFileOpenProject()
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "All Files (*.*)|*.*|Aurora Vision Files|*.avproj;*.avexe|FabImage Files|*.fiproj;*.fiexe|Project Files|*.avproj;*.fiproj|Runtime Files|*.avexe;*.fiexe",
            Multiselect = false
        };
        var result = dialog.ShowDialog();
        if (result == true)
        {
            //_navigationService.NavigateTo(typeof(LauncherViewModel).FullName!);
            OpenProject(dialog.FileName);
        }
    }

    private void OpenProject(string path)
    {
        _messenger.Send(new FileRequestedMessage(path));
    }
}

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
public class ShellViewModel : ObservableObject
{
    private readonly INavigationService _navigationService;
    private readonly IRightPaneService _rightPaneService;
    private readonly IMessenger _messenger;

    private RelayCommand _goBackCommand;
    private ICommand _menuFileSettingsOpenProjectCommand;
    private ICommand _menuViewsInstalledAppsCommand;
    private ICommand _menuFileSettingsCommand;
    private ICommand _menuViewsBlankCommand;
    private ICommand _menuViewsMainCommand;
    private ICommand _menuFileExitCommand;
    private ICommand _loadedCommand;
    private ICommand _unloadedCommand;

    public RelayCommand GoBackCommand => _goBackCommand ??= new RelayCommand(OnGoBack, CanGoBack);

    public ICommand MenuFileSettingsOpenProjectCommand => _menuFileSettingsOpenProjectCommand ?? new RelayCommand(OnMenuFileOpenProject);
    public ICommand MenuViewsInstalledAppsCommand => _menuViewsInstalledAppsCommand ??= new RelayCommand(OnMenuViewsInstalledApps);
    public ICommand MenuFileSettingsCommand => _menuFileSettingsCommand ??= new RelayCommand(OnMenuFileSettings);

    public ICommand MenuFileExitCommand => _menuFileExitCommand ??= new RelayCommand(OnMenuFileExit);

    public ICommand LoadedCommand => _loadedCommand ??= new RelayCommand(OnLoaded);

    public ICommand UnloadedCommand => _unloadedCommand ??= new RelayCommand(OnUnloaded);

    public ShellViewModel(INavigationService navigationService, IRightPaneService rightPaneService,IMessenger messenger)
    {
        _navigationService = navigationService;
        _rightPaneService = rightPaneService;
        _messenger = messenger;
    }

    private void OnLoaded()
    {
        _navigationService.Navigated += OnNavigated;
    }

    private void OnUnloaded()
    {
        _rightPaneService.CleanUp();
        _navigationService.Navigated -= OnNavigated;
    }

    private bool CanGoBack()
        => _navigationService.CanGoBack;

    private void OnGoBack()
        => _navigationService.GoBack();

    private void OnNavigated(object sender, string viewModelName)
    {
        GoBackCommand.NotifyCanExecuteChanged();
    }

    private void OnMenuFileExit()
        => Application.Current.Shutdown();

    public ICommand MenuViewsMainCommand => _menuViewsMainCommand ??= new RelayCommand(OnMenuViewsMain);

    private void OnMenuViewsMain()
        => _navigationService.NavigateTo(typeof(MainViewModel).FullName, null, true);

    private void OnMenuFileSettings()
        => _rightPaneService.OpenInRightPane(typeof(SettingsViewModel).FullName);

    private void OnMenuViewsInstalledApps()
        => _navigationService.NavigateTo(typeof(InstalledAppsViewModel).FullName, null, true);
    private void OnMenuFileOpenProject()
    {
        var dialog = new Microsoft.Win32.OpenFileDialog();
        dialog.Filter = "All Files (*.*)|*.*|Aurora Vision Files|*.avproj;*.avexe|FabImage Files|*.fiproj;*.fiexe|Project Files|*.avproj;*.fiproj|Runtime Files|*.avexe;*.fiexe";
        dialog.Multiselect = false;
        var result = dialog.ShowDialog();
        _navigationService.NavigateTo(typeof(MainViewModel).FullName!);
        if (result == true)
        {
            OpenProject(dialog.FileName);
        }
    }

    private void OpenProject(string path)
    {
        _messenger.Send(new FileRequestedMessage(path));
    }
}

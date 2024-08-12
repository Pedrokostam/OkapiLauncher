using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Effects;
using AuroraVisionLauncher.Contracts.Services;
using AuroraVisionLauncher.Contracts.Views;
using AuroraVisionLauncher.ViewModels;
using MahApps.Metro.Controls.Dialogs;

namespace AuroraVisionLauncher.Services;
public class ContentDialogService : IContentDialogService
{
    private readonly IDialogCoordinator _dialogCoordinator;
    private readonly ShellViewModel _mainWindow;

    public Task ShowError(string title, string message)
    {
        return _dialogCoordinator.ShowMessageAsync(_mainWindow, title, message);
    }

    public ContentDialogService(IDialogCoordinator dialogCoordinator, ShellViewModel mainWindow)
    {
        _dialogCoordinator = dialogCoordinator;
        _mainWindow = mainWindow;
    }

    //public Task ShowContent(string content)
    //{
    //    var dialog = App.
    //    return _dialogCoordinator.ShowMetroDialogAsync(_mainWindow,)
    //}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Effects;
using AuroraVisionLauncher.Contracts.Services;
using AuroraVisionLauncher.Contracts.ViewModels;
using AuroraVisionLauncher.Contracts.Views;
using AuroraVisionLauncher.Models;
using AuroraVisionLauncher.ViewModels;
using AuroraVisionLauncher.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MahApps.Metro.Controls.Dialogs;

namespace AuroraVisionLauncher.Services;
public class ContentDialogService : IContentDialogService
{
    private readonly IDialogCoordinator _dialogCoordinator;
    private readonly ShellViewModel _context;

    public Task ShowError(string title, string message)
    {
        return _dialogCoordinator.ShowMessageAsync(_context, title, message);
    }
    public async Task ShowSourceEditor(CustomAppSource source)
    {
        var dialog = new CustomSourceEditorDialog();
        var vm = new CustomSourceDialogEditorViewModel(source, async () => await Task.FromResult(true));
        await DoDoDa(vm, dialog);
    }

    private async Task DoDoDa(IDialogViewModel viewModel, BaseMetroDialog dialog)
    {
        dialog.DataContext = viewModel;
        await _dialogCoordinator.ShowMetroDialogAsync(_context, dialog);
        if (viewModel is INavigationAware nav)
        {
            nav.OnNavigatedTo(null!);
        }
        await viewModel.WaitForExit();
        await _dialogCoordinator.HideMetroDialogAsync(_context, dialog);
    }

    public ContentDialogService(IDialogCoordinator dialogCoordinator, ShellViewModel mainWindow)
    {
        _dialogCoordinator = dialogCoordinator;
        _context = mainWindow;
    }

    //public Task ShowContent(string content)
    //{
    //    var dialog = App.
    //    return _dialogCoordinator.ShowMetroDialogAsync(_mainWindow,)
    //}
}

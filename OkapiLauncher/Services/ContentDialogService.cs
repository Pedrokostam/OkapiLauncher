using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Effects;
using OkapiLauncher.Contracts.Services;
using OkapiLauncher.Contracts.ViewModels;
using OkapiLauncher.Contracts.Views;
using OkapiLauncher.Core.Models.Apps;
using OkapiLauncher.Models;
using OkapiLauncher.Models.Updates;
using OkapiLauncher.Properties;
using OkapiLauncher.ViewModels;
using OkapiLauncher.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MahApps.Metro.Controls.Dialogs;
using AuroraVisionLauncher.ViewModels;

namespace OkapiLauncher.Services;
public class ContentDialogService : IContentDialogService
{
    private readonly IDialogCoordinator _dialogCoordinator;
    private  Lazy<ShellViewModel> _context= new Lazy<ShellViewModel>(()=>((App)App.Current).GetService<ShellViewModel>());

    public Task ShowError(string message, string? title = null)
    {
        return _dialogCoordinator.ShowMessageAsync(_context.Value, title ?? Resources.ErrorDialogHeader, message);
    }
    public Task ShowMessage(object context, string message, string? title = null)
    {
        return _dialogCoordinator.ShowMessageAsync(context, title ?? "", message);
    }
    public Task ShowMessage(string message, string? title = null)
    {
        return _dialogCoordinator.ShowMessageAsync(_context.Value, title ?? "", message);
    }
    public async Task ShowSourceEditor(CustomAppSource source)
    {
        var dialog = new CustomSourceEditorDialog();
        var vm = new CustomSourceDialogEditorViewModel(source, async () => await Task.FromResult(true));
        await ShowMetroDialog(vm, dialog);
    }
    public async Task<bool> ShowProcessKillDialog(object context,SimpleProcess process)
    {
        var dialog = new KillProcessDialog();
        var vm = new KillProcessDialogViewModel(process);
       return await ShowMetroDialog(vm, dialog,context);
    }
    public async Task<bool> ShowAllProcessesKillDialog(object context, IAvApp app)
    {
        var dialog = new KillAllProcessDialog();
        var vm = new KillAllProcesessDialogViewModel(app);
        return await ShowMetroDialog(vm, dialog,context);
    }
    public async Task<UpdatePromptResult> ShowVersionDecisionDialog(UpdateDataCarier carrier)
    {
        var dialog = new VersionDecisionDialog();
        var vm = new VersionDecisionDialogViewModel(carrier);
        return await ShowMetroDialog(vm, dialog);
    }

    private async Task ShowMetroDialog(IDialogViewModel viewModel, BaseMetroDialog dialog, object? context = null)
    {
        dialog.DataContext = viewModel;
        var ctx = context ?? _context.Value;
        await _dialogCoordinator.ShowMetroDialogAsync(ctx, dialog);
        if (viewModel is INavigationAware nav)
        {
            nav.OnNavigatedTo(null!);
        }
        await viewModel.WaitForExit();
        await _dialogCoordinator.HideMetroDialogAsync(ctx, dialog);
    }

    private async Task<T> ShowMetroDialog<T>(IDialogViewModel<T> viewModel, BaseMetroDialog dialog, object? context=null)
    {
        dialog.DataContext = viewModel;
        var ctx = context ?? _context.Value;
        await _dialogCoordinator.ShowMetroDialogAsync(ctx, dialog);
        if (viewModel is INavigationAware nav)
        {
            nav.OnNavigatedTo(null!);
        }
        var res = await viewModel.WaitForExit();
        await _dialogCoordinator.HideMetroDialogAsync(ctx, dialog);
        return res;
    }

    public ContentDialogService(IDialogCoordinator dialogCoordinator)
    {
        _dialogCoordinator = dialogCoordinator;
    }

    //public Task ShowContent(string content)
    //{
    //    var dialog = App.
    //    return _dialogCoordinator.ShowMetroDialogAsync(_mainWindow,)
    //}
}

using System.Windows;
using OkapiLauncher.Models;

namespace OkapiLauncher.Contracts.Services;

public interface IWindowManagerService
{
    Window MainWindow { get; }

    void OpenInNewWindow(string pageKey, object? parameter = null);

    bool? OpenInDialog(string pageKey, object? parameter = null);

    Window? GetWindow(string pageKey);
    void CloseChildWindows();
    //bool? OpenSourceEditingWindows(CustomAppSource source);
}

using System.Runtime.InteropServices;
using System.Windows;
using OkapiLauncher.Contracts.Services;
using OkapiLauncher.Properties;

namespace OkapiLauncher.Helpers;
internal static class ClipboardHelper
{
    /// <summary>
    /// Copies text to the clipboard, falling back to non-persistent copy if flushing fails
    /// (e.g. another process holds the clipboard open). Shows an error dialog instead of throwing.
    /// </summary>
    public static void SetText(string text)
    {
        try
        {
            Clipboard.SetDataObject(text, copy: true);
        }
        catch (ExternalException)
        {
            try
            {
                Clipboard.SetDataObject(text, copy: false);
            }
            catch (ExternalException)
            {
                _ = ((App)App.Current).GetService<IContentDialogService>().ShowError(Resources.ErrorClipboard);
            }
        }
    }
}

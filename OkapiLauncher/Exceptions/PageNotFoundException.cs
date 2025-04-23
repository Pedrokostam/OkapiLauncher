using System.Windows.Input;

namespace OkapiLauncher.Exceptions;

public class PageNotFoundException(string pageKey) : Exception($"Page not found: {pageKey}. Did you forget to call PageService.Configure?")
{
}
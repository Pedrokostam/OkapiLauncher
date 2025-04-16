namespace OkapiLauncher.Exceptions;

public class PageDuplicateKeyException(string pageKey) : Exception($"The key {pageKey} is already configured in PageService")
{
}

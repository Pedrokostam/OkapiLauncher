using OkapiLauncher.Contracts.Services;

namespace OkapiLauncher.Services;

public abstract class ThemeSelectorServiceBase
{
    /// <summary>
    /// Is a dependency to ensure its instantiated before.
    /// </summary>
    private readonly IPersistAndRestoreService _persistAndRestoreService;
    protected ThemeSelectorServiceBase(IPersistAndRestoreService persistAndRestoreService)
    {
        _persistAndRestoreService = persistAndRestoreService;
    }

    protected abstract void InitializeData();
}
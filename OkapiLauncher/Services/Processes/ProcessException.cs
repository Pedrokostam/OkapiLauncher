namespace OkapiLauncher.Services.Processes;

public sealed class ProcessException(Exception baseException) : Exception
{
    public Exception Exception { get; private set; } = baseException;
}

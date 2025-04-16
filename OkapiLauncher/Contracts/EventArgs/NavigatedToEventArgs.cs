namespace OkapiLauncher.Contracts.EventArgs;

public class NavigatedToEventArgs(string dataContextFullName) : System.EventArgs
{
    public string DataContextFullName { get; } = dataContextFullName;
    public static NavigatedToEventArgs FromDataContext<T>(T dataContext)
    {
        ArgumentNullException.ThrowIfNull(dataContext);
        return new NavigatedToEventArgs(dataContext.GetType().FullName!);
    }
}

namespace OkapiLauncher.Models.Messages;

public class InvalidAppProcesses : IAppProcessInformationPacket
{
    public static readonly InvalidAppProcesses Instance = new();
    public void UpdateState(AvAppFacade app)
    {
        if (app.ProcessInfoAvailable)
        {
            app.ProcessInfoAvailable = false;
            app.ActiveProcesses.Clear();
        }
    }

    public void UpdateStates(IEnumerable<AvAppFacade> apps)
    {
        foreach (var item in apps)
        {
            UpdateState(item);
        }
    }
}

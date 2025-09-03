namespace OkapiLauncher.Models.Messages;

public sealed record KillAllProcessesRequest(AvAppFacade AvApp,object? ViewModel)
{
}

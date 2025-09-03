
namespace OkapiLauncher.Models.Messages;

public interface IAppProcessInformationPacket
{
    void UpdateState(AvAppFacade app);
    void UpdateStates(IEnumerable<AvAppFacade> apps);
}
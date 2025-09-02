using CommunityToolkit.Mvvm.Messaging.Messages;

namespace OkapiLauncher.Models.Messages;
public class FileRequestedMessage : ValueChangedMessage<string>
{
    public FileRequestedMessage(string filepath) : base(filepath)
    {
    }
}

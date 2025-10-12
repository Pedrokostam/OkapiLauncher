using CommunityToolkit.Mvvm.Messaging.Messages;

namespace OkapiLauncher.Models.Messages;
public class FileRequestedMessage : ValueChangedMessage<string>
{
    internal bool AutoLoad { get; set; }
    public FileRequestedMessage(string filepath) : base(filepath)
    {
    }
}
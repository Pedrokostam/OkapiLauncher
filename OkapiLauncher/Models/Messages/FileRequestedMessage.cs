using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging.Messages;
using Windows.System;

namespace OkapiLauncher.Models.Messages;
public class FileRequestedMessage : ValueChangedMessage<string>
{
    public FileRequestedMessage(string filepath) : base(filepath)
    {
    }
}

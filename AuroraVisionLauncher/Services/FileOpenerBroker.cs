using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AuroraVisionLauncher.Contracts.Services;
using AuroraVisionLauncher.Models.Messages;
using AuroraVisionLauncher.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;

namespace AuroraVisionLauncher.Services
{
    public class FileOpenerBroker :ObservableRecipient, IRecipient<FileRequestedMessage>
    {
        private readonly INavigationService _navigationService;
        public FileOpenerBroker(IMessenger messenger, INavigationService navigationService):base(messenger) 
        {
            _navigationService = navigationService;
            IsActive = true;
        }

        public void Receive(FileRequestedMessage message)
        {
            _navigationService.NavigateTo(typeof(LauncherViewModel).FullName!, message.Value);
        }
    }
}

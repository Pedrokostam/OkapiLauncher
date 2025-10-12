using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using OkapiLauncher.Contracts.Services;
using OkapiLauncher.Models.Messages;
using OkapiLauncher.ViewModels;

namespace OkapiLauncher.Services;

public class FileOpenerBroker : ObservableRecipient, IRecipient<FileRequestedMessage>
{
    private readonly INavigationService _navigationService;
    public FileOpenerBroker(IMessenger messenger, INavigationService navigationService) : base(messenger)
    {
        _navigationService = navigationService;
        IsActive = true;
    }

    public async void Receive(FileRequestedMessage message)
    {
        if (_navigationService.NavigateTo<LauncherViewModel>(message))
        {
            return;
        }
        if (_navigationService.CurrentDataContext is LauncherViewModel viewModel)
        {
            await viewModel.OpenProject(message.Value, message.AutoLoad);
        }
    }
}
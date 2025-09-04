using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Options;
using OkapiLauncher.Contracts.Services;
using OkapiLauncher.Models;
using static Microsoft.WindowsAPICodePack.Shell.PropertySystem.SystemProperties.System;

namespace OkapiLauncher.ViewModels;

public partial class AboutViewModel : ObservableObject
{
    private readonly AppConfig _appConfig;
    private readonly ISystemService _systemService;
    private readonly IApplicationInfoService _applicationInfoService;

    public AboutViewModel(IOptions<AppConfig> appConfig, ISystemService systemService, IApplicationInfoService applicationInfoService)
    {
        _appConfig = appConfig.Value;
        _systemService = systemService;
        _applicationInfoService = applicationInfoService;
        this.Link = _appConfig.GithubLink;
        BuildDate = _applicationInfoService.GetBuildDatetime().ToLocalTime();
        Version = _applicationInfoService.GetVersion();
    }
    [ObservableProperty]
    private DateTime _buildDate;
    [ObservableProperty]
    private Version _version;
    [ObservableProperty]
    private string _link;
    [RelayCommand]
    private void OpenRepoPage()
    => _systemService.OpenInWebBrowser(Link);
}

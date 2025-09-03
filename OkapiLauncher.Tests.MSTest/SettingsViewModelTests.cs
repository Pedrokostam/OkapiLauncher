﻿using OkapiLauncher.Contracts.Services;
using OkapiLauncher.Core.Contracts.Services;
using OkapiLauncher.Core.Services;
using OkapiLauncher.Models;
using OkapiLauncher.ViewModels;

using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

namespace OkapiLauncher.Tests.MSTest;

[TestClass]
public class SettingsViewModelTests
{
    public SettingsViewModelTests()
    {
    }

    [TestMethod]
    public void TestSettingsViewModel_SetCurrentTheme()
    {
        var mockThemeSelectorService = new Mock<IThemeSelectorService>();
        mockThemeSelectorService.Setup(mock => mock.GetCurrentTheme()).Returns(AppTheme.Light);
        var mockAppConfig = new Mock<IOptions<AppConfig>>();
        var mockSystemService = new Mock<ISystemService>();
        var mockFileUpdateService = new Mock<IUpdateCheckService>();
        var mockApplicationInfoService = new Mock<IApplicationInfoService>();
        var mockFileAssociationService = new Mock<IFileAssociationService>();
        var mockCustomAppSourceService = new Mock<ICustomAppSourceService>();
        var mockContentDialogService = new Mock<IContentDialogService>();

        var settingsVm = new SettingsViewModel(mockAppConfig.Object,
            mockThemeSelectorService.Object,
            mockSystemService.Object,
            mockApplicationInfoService.Object,
            mockFileAssociationService.Object,
            mockFileUpdateService.Object,
            mockCustomAppSourceService.Object,
            mockContentDialogService.Object
            );
        settingsVm.OnNavigatedTo(null);

        Assert.AreEqual(AppTheme.Light, settingsVm.Theme);
    }

    [TestMethod]
    public void TestSettingsViewModel_SetCurrentVersion()
    {
        var mockThemeSelectorService = new Mock<IThemeSelectorService>();
        var mockAppConfig = new Mock<IOptions<AppConfig>>();
        var mockSystemService = new Mock<ISystemService>();
        var mockApplicationInfoService = new Mock<IApplicationInfoService>();
        var mockFileAssociationService = new Mock<IFileAssociationService>();
        var mockFileUpdateService = new Mock<IUpdateCheckService>();
        var testVersion = new Version(1, 2, 3, 4);
        mockApplicationInfoService.Setup(mock => mock.GetVersion()).Returns(testVersion);

        var settingsVm = new SettingsViewModel(mockAppConfig.Object,
            mockThemeSelectorService.Object,
            mockSystemService.Object,
            mockApplicationInfoService.Object,
            mockFileAssociationService.Object,
            mockFileUpdateService.Object);
        settingsVm.OnNavigatedTo(null);

        Assert.AreEqual($"Aurora Vision Launcher - {testVersion}", settingsVm.VersionDescription);
    }

    [TestMethod]
    public void TestSettingsViewModel_SetThemeCommand()
    {
        var mockThemeSelectorService = new Mock<IThemeSelectorService>();
        var mockAppConfig = new Mock<IOptions<AppConfig>>();
        var mockFileUpdateService = new Mock<IUpdateCheckService>();
        var mockSystemService = new Mock<ISystemService>();
        var mockApplicationInfoService = new Mock<IApplicationInfoService>();
        var mockFileAssociationService = new Mock<IFileAssociationService>();

        var settingsVm = new SettingsViewModel(mockAppConfig.Object,
            mockThemeSelectorService.Object,
            mockSystemService.Object,
            mockApplicationInfoService.Object,
            mockFileAssociationService.Object,
            mockFileUpdateService.Object);
        settingsVm.SetThemeCommand.Execute(AppTheme.Light.ToString());

        mockThemeSelectorService.Verify(mock => mock.SetTheme(AppTheme.Light, null));
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using OkapiLauncher.Contracts.Services;
using OkapiLauncher.Helpers;
using OkapiLauncher.Models;
using OkapiLauncher.Models.Updates;
using Microsoft.Extensions.Options;

namespace OkapiLauncher.Services;
public class UpdateCheckService : IUpdateCheckService
{


    private const string AutoCheckKey = "AutoCheckForUpdatesEnabled";
    private const string LastCheckDateKey = "LastUpdateCheckDate";
    private const string IgnoredReleaseKey = "IgnoredRelease";
    private readonly AppConfig _appConfig;
    private readonly ISystemService _systemService;
    private readonly IContentDialogService _contentDialogService;
    /// <summary>
    /// Is a dependency to ensure its instantiated before.
    /// </summary>
    private readonly IPersistAndRestoreService _persistAndRestoreService;

    public bool AutoCheckForUpdatesEnabled
    {
        get => (bool)App.Current.Properties[AutoCheckKey]!;
        set => App.Current.Properties[AutoCheckKey] = value;
    }
    public DateTime LastCheckDate
    {
        get => (DateTime)App.Current.Properties[LastCheckDateKey]!;
        set => App.Current.Properties[LastCheckDateKey] = value;
    }
    public string? IgnoredVersion
    {
        get => App.Current.Properties[IgnoredReleaseKey] as string;
        set => App.Current.Properties[IgnoredReleaseKey] = value;
    }
    public async Task AutoPromptUpdate()
    {
        if (AutoCheckForUpdatesEnabled && LastCheckDate.Date != DateTime.UtcNow.Date)
        {
            await CheckForUpdates_impl(isAuto: true);
        }
    }
    public async Task ManualPrompUpdate() => await CheckForUpdates_impl(isAuto: false);

    private async Task CheckForUpdates_impl(bool isAuto)
    {
        using HttpClient client = new HttpClient();
        client.DefaultRequestHeaders.Add("User-Agent", "OkapiLauncher"); // GitHub requires a user-agent header
        try
        {
            HttpResponseMessage response = await client.GetAsync(_appConfig.UpdateLink);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            JsonDocument responseDocument = JsonDocument.Parse(responseBody);
            var versionResponse = HtmlVersionResponse.FromJsonDocument(responseDocument, isAuto);
            JsonElement releaseInfo = responseDocument.RootElement;
            var shouldPrompt = versionResponse.ShouldPromptUser(IgnoredVersion);
            if (shouldPrompt == HtmlVersionResponse.PromptAction.DontShowDialog)
            {
                return;
            }
            if (shouldPrompt == HtmlVersionResponse.PromptAction.ShowNoUpdatesMessageDialog)
            {
                await _contentDialogService.ShowMessage(Properties.Resources.VersionCheckDialogNoUpdatesMessage, Properties.Resources.VersionCheckDialogNoUpdatesHeader);
                return;
            }
            var promptResult = await _contentDialogService.ShowVersionDecisionDialog(versionResponse);
            if (promptResult.DisableAutomaticUpdates)
            {
                AutoCheckForUpdatesEnabled = false;
            }
            switch (promptResult.Decision)
            {
                case UpdateDecision.Cancel:
                    break;
                case UpdateDecision.SkipVersion:
                    IgnoredVersion = versionResponse.VersionTag;
                    break;
                case UpdateDecision.OpenPage:
                    _systemService.OpenInWebBrowser(_appConfig.GithubLink + "/releases/latest");
                    break;
                case UpdateDecision.LaunchUpdater:
                    break;
            }
        }
        catch (HttpRequestException)
        {
        }
        finally
        {
            LastCheckDate = DateTime.UtcNow;
        }
    }


    public UpdateCheckService(IOptions<AppConfig> appConfig, ISystemService systemService, IContentDialogService contentDialogService, IPersistAndRestoreService persistAndRestoreService)
    {
        _appConfig = appConfig.Value;
        _systemService = systemService;
        _contentDialogService = contentDialogService;
        _persistAndRestoreService = persistAndRestoreService;
        if (_persistAndRestoreService.IsDataRestored)
        {
            // if already restored, get
            InitializeData();
        }
        else
        {
            // otherwise wait for restore
            _persistAndRestoreService.DataRestored += _persistAndRestoreService_DataRestored;
        }
    }
    private void _persistAndRestoreService_DataRestored(object? sender, EventArgs e)
    {
        InitializeData();
    }

    private static void InitializeData()
    {
        App.Current.Properties.InitializeDictKey<bool>(AutoCheckKey, defaultValue: false);
        App.Current.Properties.InitializeDictKey<DateTime>(LastCheckDateKey, defaultValue: DateTime.UnixEpoch);
        App.Current.Properties.InitializeDictKey<string>(IgnoredReleaseKey);
    }
}

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
using AuroraVisionLauncher.Contracts.Services;
using AuroraVisionLauncher.Models;
using AuroraVisionLauncher.Models.Updates;
using Microsoft.Extensions.Options;

namespace AuroraVisionLauncher.Services;
public class UpdateCheckService : IUpdateCheckService
{


    private const string AutoCheckKey = "AutoCheckForUpdatesEnabled";
    private const string LastCheckDateKey = "LastUpdateCheckDate";
    private const string IgnoredReleaseKey = "IgnoredRelease";
    private readonly AppConfig _appConfig;
    private readonly ISystemService _systemService;
    private readonly IContentDialogService _contentDialogService;

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
        client.DefaultRequestHeaders.Add("User-Agent", "AuroraVisionLauncher"); // GitHub requires a user-agent header
        try
        {
            HttpResponseMessage response = await client.GetAsync("https://api.github.com/repos/PedroKostam/AuroraVisionLauncher/releases/latest");
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
                await _contentDialogService.ShowMessage("🎉🎉 You have the newest version! 🎉🎉", "No updates found");
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
        LastCheckDate = DateTime.UtcNow;
    }


    public UpdateCheckService(IOptions<AppConfig> appConfig, ISystemService systemService, IContentDialogService contentDialogService)
    {
        _appConfig = appConfig.Value;
        _systemService = systemService;
        _contentDialogService = contentDialogService;
        if (App.Current.Properties.Contains(AutoCheckKey))
        {
            var acfu = App.Current.Properties[AutoCheckKey];
            App.Current.Properties[AutoCheckKey] = acfu is bool b ? b : true;
        }
        else
        {
            App.Current.Properties[AutoCheckKey] = true;
        }
        if (App.Current.Properties.Contains(LastCheckDateKey))
        {
            var acfu = App.Current.Properties[LastCheckDateKey];
            App.Current.Properties[LastCheckDateKey] = acfu is DateTime d ? d : DateTime.UnixEpoch;
        }
        else
        {
            App.Current.Properties[LastCheckDateKey] = DateTime.UnixEpoch;
        }
        if (App.Current.Properties.Contains(IgnoredReleaseKey))
        {
            var tag = App.Current.Properties[IgnoredReleaseKey] as string;
            App.Current.Properties[IgnoredReleaseKey] = tag;
        }
        else
        {
            App.Current.Properties[IgnoredReleaseKey] = null;
        }
    }
}

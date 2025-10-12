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
using System.Windows.Input;
using Microsoft.Extensions.Options;
using OkapiLauncher.Contracts.Services;
using OkapiLauncher.Helpers;
using OkapiLauncher.Models;
using OkapiLauncher.Models.Updates;

namespace OkapiLauncher.Services;
public class UpdateCheckService : IUpdateCheckService
{


    private const string AutoCheckKey = "AutoCheckForUpdatesEnabled";
    private const string LastCheckDateKey = "LastUpdateCheckDate";
    private const string IgnoredReleaseKey = "IgnoredRelease";
    private readonly AppConfig _appConfig;
    private readonly ISystemService _systemService;
    private readonly IContentDialogService _contentDialogService;
    private readonly IApplicationInfoService _applicationInfoService;
    /// <summary>
    /// Is a dependency to ensure its instantiated before.
    /// </summary>
    private readonly IPersistAndRestoreService _persistAndRestoreService;

    public bool AutoCheckForUpdatesEnabled
    {
        get => (bool)App.Current.Properties[AutoCheckKey]!;
        set => App.Current.Properties[AutoCheckKey] = value;
    }
    private static DateTime LastCheckDate
    {
        get => (DateTime)App.Current.Properties[LastCheckDateKey]!;
        set => App.Current.Properties[LastCheckDateKey] = value;
    }
    private static string? IgnoredVersion
    {
        get => App.Current.Properties[IgnoredReleaseKey] as string;
        set => App.Current.Properties[IgnoredReleaseKey] = value;
    }
    public Task<string?> AutoPromptUpdate()
    {
        return CheckForUpdates_impl(isAuto: true);
    }

    private bool IsAutomaticUpdateScheduled()
    {
        return AutoCheckForUpdatesEnabled && LastCheckDate.Date != DateTime.UtcNow.Date;
    }

    public Task<string?> ManualPrompUpdate() => CheckForUpdates_impl(isAuto: false);

    private static bool DebugOverride()
    {
#if DEBUG
        var modifiers = Keyboard.Modifiers;
        return modifiers.HasFlag(ModifierKeys.Control) && modifiers.HasFlag(ModifierKeys.Shift);
#else
        return false;
#endif
    }

    private async Task<string?> CheckForUpdates_impl(bool isAuto)
    {
        using HttpClient client = new HttpClient();
        client.DefaultRequestHeaders.Add("User-Agent", "OkapiLauncher"); // GitHub requires a user-agent header
        try
        {
            HttpResponseMessage response = await client.GetAsync(_appConfig.UpdateLink);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            JsonDocument responseDocument = JsonDocument.Parse(responseBody);
            var updateCarrier = UpdateDataCarier.Create(_applicationInfoService, isAuto, responseDocument, IgnoredVersion);
            var shouldPrompt = updateCarrier.ShouldPromptUser();
            shouldPrompt = DebugOverride() ? PromptAction.ShowPrompUpdateDialog : shouldPrompt;
            string? outputVersionTag = updateCarrier.IsIgnoredVersion() ? null : updateCarrier.RemoteVersion;
            if (isAuto && !IsAutomaticUpdateScheduled())
            {
                // if the check is automatic but it was not scheduled, just return tag
                return outputVersionTag;
            }
            if (shouldPrompt == PromptAction.DontShowDialog)
            {
                return outputVersionTag;
            }
            if (shouldPrompt == PromptAction.ShowNoUpdatesMessageDialog)
            {
                await _contentDialogService.ShowMessage(Properties.Resources.VersionCheckDialogNoUpdatesMessage, Properties.Resources.VersionCheckDialogNoUpdatesHeader);
                return outputVersionTag;
            }
            var promptResult = await _contentDialogService.ShowVersionDecisionDialog(updateCarrier);
            if (promptResult.DisableAutomaticUpdates)
            {
                AutoCheckForUpdatesEnabled = false;
            }
            switch (promptResult.Decision)
            {
                case UpdateDecision.Cancel:
                    break;
                case UpdateDecision.SkipVersion:
                    IgnoredVersion = updateCarrier.HtmlResponse?.VersionTag;
                    break;
                case UpdateDecision.OpenPage:
                    _systemService.OpenInWebBrowser(_appConfig.GithubLink + "/releases/latest");
                    break;
                case UpdateDecision.LaunchUpdater:
                    _systemService.LaunchInstaller(promptResult.UpdaterFilepath);
                    break;

            }
            return outputVersionTag;
        }
        catch (HttpRequestException)
        {
        }
        finally
        {
            LastCheckDate = DateTime.UtcNow;
        }
        return null;
    }


    public UpdateCheckService(IOptions<AppConfig> appConfig, ISystemService systemService, IContentDialogService contentDialogService, IPersistAndRestoreService persistAndRestoreService, IApplicationInfoService applicationInfoService)
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
            _persistAndRestoreService.DataRestored += PersistAndRestoreService_DataRestored;
        }

        _applicationInfoService = applicationInfoService;
    }
    private void PersistAndRestoreService_DataRestored(object? sender, EventArgs e)
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
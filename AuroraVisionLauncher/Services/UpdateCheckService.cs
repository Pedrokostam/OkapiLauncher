using System;
using System.Collections.Generic;
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
using Microsoft.Extensions.Options;
//using Newtonsoft.Json.Linq;

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
    public async Task AutoCheckForUpdates()
    {
        //if (AutoCheckForUpdatesEnabled && LastCheckDate.Date != DateTime.UtcNow.Date)
        {
            await CheckForUpdates_impl(isAuto: true);
        }
    }
    public async Task CheckForUpdates() => await CheckForUpdates_impl(isAuto: false);
    private async Task CheckForUpdates_impl(bool isAuto)
    {
        using HttpClient client = new HttpClient();
        client.DefaultRequestHeaders.Add("User-Agent", "AuroraVisionLauncher"); // GitHub requires a user-agent header

        try
        {

            HttpResponseMessage response = await client.GetAsync("https://api.github.com/repos/PedroKostam/AuroraVisionLauncher/releases/latest");
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Latest release info: {responseBody}");
            JsonDocument responseDocument = JsonDocument.Parse(responseBody);
            JsonElement releaseInfo = responseDocument.RootElement;

            // Extract specific information
            string? tagName = releaseInfo.GetProperty("tag_name").GetString();
            string? releaseName = releaseInfo.GetProperty("name").GetString();
            bool dateGood = releaseInfo.GetProperty("published_at").TryGetDateTime(out DateTime publishedAt);
            if (tagName is null || releaseName is null || !dateGood)
            {
                return;
            }
            if (tagName.Equals(App.Current.Properties[IgnoredReleaseKey] as string, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
            if (!CheckLastReleaseIsNewer(publishedAt,tagName))
            {
                return;
            }
            var newinfo = new NewVersionInformation(publishedAt, tagName, isAuto);
            await _contentDialogService.ShowVersionDecisionDialog(newinfo);
            if (newinfo.DisableAutomaticUpdates)
            {
                AutoCheckForUpdatesEnabled = false;
            }
            switch (newinfo.UserDecision)
            {
                case NewVersionInformation.Decision.Cancel:
                    break;
                case NewVersionInformation.Decision.SkipVersion:
                    IgnoredVersion = tagName;
                    break;
                case NewVersionInformation.Decision.OpenPage:
                    _systemService.OpenInWebBrowser(_appConfig.GithubLink + "/releases/latest");
                    break;
                case NewVersionInformation.Decision.LaunchUpdater:
                    break;
            }
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine($"Request error: {e.Message}");
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
            App.Current.Properties[LastCheckDateKey] = acfu is DateTime d ? d : true;
        }
        else
        {
            App.Current.Properties[LastCheckDateKey] = DateTime.MinValue;
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
    private static readonly TimeSpan UploadTolerance = TimeSpan.FromHours(6);
    private bool CheckLastReleaseIsNewer(DateTime releaseDate, string tag)
    {
        var buildDate = GetBuildDate();
        var difference = releaseDate - buildDate;
        if (difference < TimeSpan.Zero)
        {
            return false;
        }
        if (difference < UploadTolerance)
        {
            var currentVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString();
            bool tagsMatch = string.Equals(currentVersion, tag, StringComparison.OrdinalIgnoreCase);
            return !tagsMatch;
        }
        return true;
    }
    private static DateTime GetBuildDate()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var buildAttrib = assembly.GetCustomAttribute<BuildDateAttribute>();
        return buildAttrib?.DateTime ?? File.GetLastWriteTime(assembly.Location);
    }
}

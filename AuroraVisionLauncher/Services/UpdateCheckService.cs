using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using AuroraVisionLauncher.Contracts.Services;
using AuroraVisionLauncher.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;

namespace AuroraVisionLauncher.Services;
public class UpdateCheckService : IUpdateCheckService
{
    private const string AutoCheckKey = "AutoCheckForUpdatesEnabled";
    private const string LastCheckDateKey = "LastUpdateCheckDate";
    private readonly AppConfig _appConfig;
    private readonly ISystemService _systemService;


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
    public async Task AutoCheckForUpdates()
    {
        //if (AutoCheckForUpdatesEnabled && LastCheckDate.Date != DateTime.UtcNow.Date)
        {
            await CheckForUpdates_impl(isAuto: true);
        }
    }
    public async Task CheckForUpdates()=> await CheckForUpdates_impl(isAuto:false);
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
            JObject releaseInfo = JObject.Parse(responseBody);

            // Extract specific information
            string? tagName = releaseInfo["tag_name"]?.ToString();
            string? releaseName = releaseInfo["name"]?.ToString();
            string? publishedAt = releaseInfo["published_at"]?.ToString();
            if (tagName is null || releaseName is null || publishedAt is null)
            {
                return;
            }
            var version = new Version(tagName);
            if (version >= Assembly.GetExecutingAssembly().GetName().Version)
            {
                var msg = $"There is a newer version available:\n{version} ({releaseName}) release on {publishedAt}\nDo you want to open the download page?";
                if (isAuto)
                {
                    msg += "\n(You can disable automatic check in settings)";
                }
                var res = MessageBox.Show(msg, "New version available", MessageBoxButton.YesNo);
                if (res == MessageBoxResult.Yes)
                {
                    _systemService.OpenInWebBrowser(_appConfig.GithubLink + "/releases/latest");
                }
            }
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine($"Request error: {e.Message}");
        }
        LastCheckDate = DateTime.UtcNow;
    }
    public UpdateCheckService(IOptions<AppConfig> appConfig, ISystemService systemService)
    {
        _appConfig = appConfig.Value;
        _systemService = systemService;
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
    }
}

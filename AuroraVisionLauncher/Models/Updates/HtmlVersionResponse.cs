using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AuroraVisionLauncher.Models.Updates;
public record HtmlVersionResponse
{
    public enum PromptAction
    {
        DontShowDialog,
        ShowNoUpdatesMessageDialog,
        ShowPrompUpdateDialog,
    }
    public HtmlVersionResponse()
    {

    }
    public HtmlVersionResponse(string versionTag, string versionTitle, DateTime releaseDate, bool isAutomaticCheck, string? installerDownloadLink)
    {
        VersionTag = versionTag;
        VersionTitle = versionTitle;
        ReleaseDate = releaseDate;
        IsAutomaticCheck = isAutomaticCheck;
        InstallerDownloadLink = installerDownloadLink;
    }

    public string VersionTag { get; } = string.Empty;
    public string VersionTitle { get; } = string.Empty;
    public DateTime ReleaseDate { get; } = DateTime.MinValue;
    public bool IsAutomaticCheck { get; } = false;
    public string? InstallerDownloadLink { get; }
    public static HtmlVersionResponse FromJsonDocument(JsonDocument document, bool isAutomaticCheck)
    {
        JsonElement releaseInfo = document.RootElement;
        // Extract specific information
        string? tagName = releaseInfo.GetProperty("tag_name").GetString();
        string? releaseName = releaseInfo.GetProperty("name").GetString();
        bool dateGood = releaseInfo.GetProperty("published_at").TryGetDateTime(out DateTime publishedAt);
        if (tagName is null || releaseName is null || !dateGood)
        {
            return new HtmlVersionResponse();
        }
        string? installerDownloadLink = null;
        foreach (var asset in releaseInfo.GetProperty("assets").EnumerateArray())
        {
            var contentType = asset.GetProperty("content_type").GetString();
            var downloadUrl = asset.GetProperty("browser_download_url").GetString();
            var name = asset.GetProperty("name").GetString() ?? "";
            if (string.Equals(contentType, "application/x-msdownload", StringComparison.OrdinalIgnoreCase)
                && name.Contains("install", StringComparison.OrdinalIgnoreCase))
            {
                installerDownloadLink = downloadUrl;
            }
        }

        return new HtmlVersionResponse(tagName, releaseName, publishedAt, isAutomaticCheck, installerDownloadLink);
    }
    public PromptAction ShouldPromptUser(string? ignoredVersion)
    {
        if (VersionTag == "" || VersionTitle == "" || ReleaseDate == DateTime.MaxValue)
        {
            return IsAutomaticCheck ? PromptAction.DontShowDialog : PromptAction.ShowNoUpdatesMessageDialog;
        }
        if (string.Equals(VersionTag, ignoredVersion, StringComparison.OrdinalIgnoreCase))
        {
            // if version is ignored, dont show dialog if it is automatic check
            // otherwise show an update prompt
            return IsAutomaticCheck ? PromptAction.DontShowDialog : PromptAction.ShowPrompUpdateDialog; // if its auto check dont infor
        }
        if (CheckLastReleaseIsNewer())
        {
            return PromptAction.ShowPrompUpdateDialog;
        }
        return IsAutomaticCheck ? PromptAction.DontShowDialog : PromptAction.ShowNoUpdatesMessageDialog;
    }
    private static readonly TimeSpan _uploadTolerance = TimeSpan.FromHours(6);
    private bool CheckLastReleaseIsNewer()
    {
        var buildDate = GetBuildDate();
        var difference = ReleaseDate - buildDate;
        if (difference < TimeSpan.Zero)
        {
            return false;
        }
        if (difference < _uploadTolerance)
        {
            var currentVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString();
            bool tagsMatch = string.Equals(currentVersion, VersionTag, StringComparison.OrdinalIgnoreCase);
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

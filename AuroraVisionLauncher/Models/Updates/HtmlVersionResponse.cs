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
    public HtmlVersionResponse(string versionTag, string versionTitle, DateTime releaseDate, bool isAutomaticCheck, string? installerDownloadLink, DateTime buildDate)
    {
        VersionTag = versionTag;
        VersionTitle = versionTitle;
        ReleaseDate = releaseDate;
        IsAutomaticCheck = isAutomaticCheck;
        InstallerDownloadLink = installerDownloadLink;
        AssemblyBuildDate = buildDate;
    }
    private static readonly DateTime DatePlaceholder = DateTime.MinValue;
    public DateTime AssemblyBuildDate { get; } = DatePlaceholder;
    public string VersionTag { get; } = string.Empty;
    public string VersionTitle { get; } = string.Empty;
    public DateTime ReleaseDate { get; } = DatePlaceholder;
    public bool IsAutomaticCheck { get; } = false;
    public string? InstallerDownloadLink { get; }
    public static HtmlVersionResponse FromJsonDocument(JsonDocument document, bool isAutomaticCheck, DateTime buildDate)
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

        var installerDownloadLink = GetDownloadLink(releaseInfo);

        return new HtmlVersionResponse(tagName,
                                       releaseName,
                                       publishedAt,
                                       isAutomaticCheck,
                                       installerDownloadLink,
                                       buildDate);
    }

    private static string? GetDownloadLink(JsonElement releaseInfo)
    {
        foreach (var asset in releaseInfo.GetProperty("assets").EnumerateArray())
        {
            var contentType = asset.GetProperty("content_type").GetString();
            var downloadUrl = asset.GetProperty("browser_download_url").GetString();
            var name = asset.GetProperty("name").GetString() ?? "";

            bool isExe = string.Equals(contentType, "application/x-msdownload", StringComparison.OrdinalIgnoreCase);
            bool hasInstallInName = name.Contains("install", StringComparison.OrdinalIgnoreCase);
            if (isExe && hasInstallInName)
            {
                return downloadUrl;
            }
        }
        return null;
    }

    private bool IsUnitialized()
    {
        return VersionTag == ""
            || VersionTitle == ""
            || ReleaseDate == DatePlaceholder
            || AssemblyBuildDate == DatePlaceholder;
    }
    public PromptAction ShouldPromptUser(string? ignoredVersion)
    {
        if (IsUnitialized())
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
    private static readonly TimeSpan UploadTolerance = TimeSpan.FromHours(6);
    private bool CheckLastReleaseIsNewer()
    {
        var difference = ReleaseDate - AssemblyBuildDate;
        if (difference < TimeSpan.Zero)
        {
            return false;
        }
        if (difference < UploadTolerance)
        {
            var currentVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString();
            bool tagsMatch = string.Equals(currentVersion, VersionTag, StringComparison.OrdinalIgnoreCase);
            return !tagsMatch;
        }
        return true;
    }
}

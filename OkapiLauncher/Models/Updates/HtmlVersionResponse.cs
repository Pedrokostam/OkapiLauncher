using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace OkapiLauncher.Models.Updates;
public record HtmlVersionResponse
{
    public HtmlVersionResponse()
    {

    }
    public HtmlVersionResponse(string versionTag,
                               string versionTitle,
                               DateTime releaseDate,
                               string? installerDownloadLink)
    {
        VersionTag = versionTag;
        VersionTitle = versionTitle;
        ReleaseDate = releaseDate;
        InstallerDownloadLink = installerDownloadLink;
    }
    private static readonly DateTime DatePlaceholder = DateTime.MinValue;
    public string VersionTag { get; } = string.Empty;
    public string VersionTitle { get; } = string.Empty;
    public DateTime ReleaseDate { get; } = DatePlaceholder;
    public string? InstallerDownloadLink { get; }
    public static HtmlVersionResponse? FromJsonDocument(JsonDocument document)
    {
        JsonElement releaseInfo = document.RootElement;
        // Extract specific information
        string? tagName = releaseInfo.GetProperty("tag_name").GetString();
        string? releaseName = releaseInfo.GetProperty("name").GetString();
        bool dateGood = releaseInfo.GetProperty("published_at").TryGetDateTime(out DateTime publishedAt);

        if (tagName is null || releaseName is null || !dateGood)
        {
            return null;
        }

        var installerDownloadLink = GetDownloadLink(releaseInfo);

        return new HtmlVersionResponse(tagName,
                                       releaseName,
                                       publishedAt,
                                       installerDownloadLink);
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
}

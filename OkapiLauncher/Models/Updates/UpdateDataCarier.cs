using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using OkapiLauncher.Contracts.Services;

namespace OkapiLauncher.Models.Updates;
public class UpdateDataCarier
{
    private UpdateDataCarier(HtmlVersionResponse? htmlResponse, bool isAutomaticUpdateCheck, DateTime appBuildDate, IApplicationInfoService.InstallationScope isAppRegistered, string[] ignoredVersions, Version appVersion)
    {
        HtmlResponse = htmlResponse;
        IsAutomaticUpdateCheck = isAutomaticUpdateCheck;
        AppBuildDate = new DateTime(2025,09,03,21,41,0);// appBuildDate;
        InstallationScope = isAppRegistered;
        IgnoredVersions = ignoredVersions;
        AppVersion = appVersion;
    }

    public HtmlVersionResponse? HtmlResponse { get; }
    public bool IsAutomaticUpdateCheck { get; }
    public DateTime AppBuildDate { get; }
    public Version AppVersion { get; }
    public bool IsConflictedInstallation => InstallationScope == IApplicationInfoService.InstallationScope.Conflict;
    public IApplicationInfoService.InstallationScope InstallationScope { get; }
    public string[] IgnoredVersions { get; }
    public bool CanDownload => HtmlResponse?.InstallerDownloadLink is not null && InstallationScope != IApplicationInfoService.InstallationScope.Conflict;
    public static UpdateDataCarier Create(IApplicationInfoService infoService, bool isAutomaticCheck, JsonDocument jsonReponse, params string?[] ignoredVersions)
    {
        bool autoCheck = isAutomaticCheck;
        var version = infoService.GetVersion();
        var buildDate = infoService.GetBuildDatetime();
        var isRegistered = infoService.IsRegisteredAsInstalledApp();
        var html = HtmlVersionResponse.FromJsonDocument(jsonReponse);
        var ignored = ignoredVersions.OfType<string>().ToArray();
        return new UpdateDataCarier(html, isAutomaticCheck, buildDate, isRegistered, ignored, version);
    }

    public PromptAction ShouldPromptUser()
    {
        if (HtmlResponse is null)
        {
            return IsAutomaticUpdateCheck ? PromptAction.DontShowDialog : PromptAction.ShowNoUpdatesMessageDialog;
        }
        if (CheckLastReleaseIsNewer())
        {
            if (IsAutomaticUpdateCheck && IgnoredVersions.Contains(HtmlResponse.VersionTag, StringComparer.OrdinalIgnoreCase))
            {
                // if version is ignored, dont show dialog if it is automatic check
                // otherwise show an update prompt
                return PromptAction.DontShowDialog; // if its auto check dont inform
            }
            return PromptAction.ShowPrompUpdateDialog;
        }
        return IsAutomaticUpdateCheck ? PromptAction.DontShowDialog : PromptAction.ShowNoUpdatesMessageDialog;
    }
    private static readonly TimeSpan UploadTolerance = TimeSpan.FromHours(6);
    private bool CheckLastReleaseIsNewer()
    {
        if (HtmlResponse is null)
        {
            return false;
        }
        var difference = HtmlResponse.ReleaseDate - AppBuildDate;
        Trace.WriteLine($"UPDATE CHECK App build on: {AppBuildDate}; new version from: {HtmlResponse.ReleaseDate}. Difference: {difference}");
        if (difference < TimeSpan.Zero)
        {
            return false;
        }
        if (difference < UploadTolerance)
        {
            bool tagsMatch = AppVersion.ToString().Contains(HtmlResponse.VersionTag, StringComparison.OrdinalIgnoreCase);
            Trace.WriteLine($"UPDATE CHECK App version is {AppVersion}, potential version is {HtmlResponse.VersionTag}. Tags match? {tagsMatch}");
            return !tagsMatch;
        }
        return true;
    }
}

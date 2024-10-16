using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using AuroraVisionLauncher.Models.Updates;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AuroraVisionLauncher.ViewModels;
public partial class DownloadProgressViewModel : ObservableObject
{
    public DownloadProgressViewModel(HtmlVersionResponse response, bool isFromInstaller)
    {
        DownloadLink = response.InstallerDownloadLink ?? "";
        Filename = DownloadLink.Split("/")[^1];
        IsManualInstallation = !isFromInstaller;
    }
    public string DownloadLink { get; }
    public string Filename { get; }
    public bool IsManualInstallation { get; }

    [ObservableProperty]
    private double _progress = -1;
    [ObservableProperty]
    private long _totalBytes = -1;
    [ObservableProperty]
    private long _currentBytes;

    [RelayCommand]
    private void Cancel()
    {
        _cancelDownloadTokenSource?.Cancel();
    }
    private CancellationTokenSource? _cancelDownloadTokenSource = null;
    public async Task<bool> DownloadFileAsync(string destinationFilePath)
    {
        try
        {
            _cancelDownloadTokenSource = new();
            using HttpClient client = new HttpClient();
            using HttpResponseMessage response = await client.GetAsync(DownloadLink, HttpCompletionOption.ResponseHeadersRead, _cancelDownloadTokenSource.Token);
            response.EnsureSuccessStatusCode();

            TotalBytes = response.Content.Headers.ContentLength ?? -1L;
            var canReportProgress = TotalBytes != -1;
            await using Stream contentStream = await response.Content.ReadAsStreamAsync(_cancelDownloadTokenSource.Token);
            await using Stream fileStream = new FileStream(destinationFilePath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, useAsync: true);
            CurrentBytes = 0L;
            var buffer = new byte[8192];
            var isMoreToRead = true;
            do
            {
                var read = await contentStream.ReadAsync(buffer, _cancelDownloadTokenSource.Token);
                if (read == 0)
                {
                    isMoreToRead = false;
                    Progress = 100;
                    continue;
                }
                await fileStream.WriteAsync(buffer.AsMemory(0, read), _cancelDownloadTokenSource.Token);
                CurrentBytes += read;
                if (canReportProgress)
                {
                    var percentComplete = (CurrentBytes * 1d) / (TotalBytes * 1d) * 100;
                    Progress = percentComplete;
                }
            }
            while (isMoreToRead);
            return true;
        }
        catch (TaskCanceledException)
        {
            try
            {
                File.Delete(destinationFilePath);
            }
            catch
            {
            }
            return false;
        }
        finally
        {
            _cancelDownloadTokenSource?.Dispose();
            _cancelDownloadTokenSource = null;
        }
    }
}

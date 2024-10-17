namespace AuroraVisionLauncher.Contracts.Services;

public interface IApplicationInfoService
{
    DateTime GetBuildDatetime();
    Guid GetGuid();
    Version GetVersion();
}

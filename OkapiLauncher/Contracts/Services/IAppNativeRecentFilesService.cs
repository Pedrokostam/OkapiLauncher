using OkapiLauncher.Core.Models.Apps;
namespace OkapiLauncher.Contracts.Services
{
    public interface IAppNativeRecentFilesService
    {
        RecentAppFiles? GetAppFiles(AvApp app);
        IEnumerable<RecentAppFiles> GetAllAppsFiles();

        record RecentAppFiles:IEquatable<RecentAppFiles>
        {
            public RecentAppFiles(AvApp app, IEnumerable<string> filepaths)
            {
                App = app;
                _filepaths = filepaths.ToList();
                int listHash = 1374;
                foreach (var item in filepaths)
                {
                    listHash=HashCode.Combine(listHash, item);
                }
                Uniquifier = HashCode.Combine(app.Brand, app.Version, listHash);
            }

            public AvApp App { get; }
            private List<string> _filepaths;
            public IReadOnlyList<string> Filepaths => _filepaths.AsReadOnly();
            private int Uniquifier { get; }
            public override int GetHashCode()
            {
                return Uniquifier;
            }
        }
    }
}

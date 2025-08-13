using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using OkapiLauncher.Contracts.Services;
using OkapiLauncher.Core.Models;
using OkapiLauncher.Core.Models.Apps;

namespace OkapiLauncher.Services
{
    public class AppNativeRecentFilesService : IAppNativeRecentFilesService
    {
        private class DateList
        {
            public DateTime ModificationDate { get; set; } = DateTime.MinValue;
            public List<string> FilePaths { get; set; } = [];
        }
        public AppNativeRecentFilesService(IAvAppFacadeFactory avAppFacadeFactory)
        {
            _avAppFacadeFactory = avAppFacadeFactory;
        }
        private readonly Dictionary<string, DateList> _modificationDates = new(StringComparer.OrdinalIgnoreCase);
        private readonly IAvAppFacadeFactory _avAppFacadeFactory;

        private IEnumerable<string> ReadAppRecentFiles(AvApp app)
        {
            if (app.Type != ProductType.Professional)
            {
                return [];
            }
            string settingsFilePath = Path.Join(app.AppDataPath, "settings.xml");
            if (!_modificationDates.ContainsKey(settingsFilePath))
            {
                _modificationDates[settingsFilePath] = new();
            }
            var dateList = _modificationDates[settingsFilePath];
            // skip checking if we checked no too long ago
            if (DateTime.UtcNow - dateList.ModificationDate < TimeSpan.FromSeconds(20))
            {
                return dateList.FilePaths;
            }
            var modifDate = File.GetLastWriteTimeUtc(settingsFilePath);
            if (dateList.ModificationDate < modifDate)
            {
                dateList.FilePaths.Clear();
                using var reader = XmlReader.Create(settingsFilePath);
                reader.ReadToFollowing("RecentFiles");
                while (reader.ReadToFollowing("RecentFile"))
                {
                    reader.Read();
                    dateList.FilePaths.Add(reader.ReadContentAsString());
                }
                dateList.ModificationDate = modifDate;
            }
            return dateList.FilePaths;
        }
        public IAppNativeRecentFilesService.RecentAppFiles GetAppFiles(AvApp app)
        {
            return new IAppNativeRecentFilesService.RecentAppFiles(app, ReadAppRecentFiles(app));
        }

        public IEnumerable<IAppNativeRecentFilesService.RecentAppFiles> GetAllAppsFiles()
        {
            var proffs = _avAppFacadeFactory.AvApps.Where(x => !x.IsCustom && x.Type.HasFlag(AvType.Professional)).ToList();
            var usedPaths = new HashSet<string>(proffs.Select(x => x.AppDataPath), StringComparer.OrdinalIgnoreCase);
            var customProffs = _avAppFacadeFactory.AvApps.Where(x => x.IsCustom && x.Type.HasFlag(AvType.Professional) && !usedPaths.Contains(x.AppDataPath)).ToList();
            proffs.AddRange(customProffs);
            return proffs.OrderByDescending(x=>x.Version).Select(x => GetAppFiles(x));
        }
    }
}

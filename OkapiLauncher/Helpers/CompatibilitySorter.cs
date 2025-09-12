using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OkapiLauncher.Contracts.Services;
using OkapiLauncher.Core.Models;
using OkapiLauncher.Core.Models.Apps;
using OkapiLauncher.Core.Models.Projects;
using OkapiLauncher.Models;

namespace OkapiLauncher.Helpers;
internal sealed class CompatibilitySorter(IVisionProject project, IAvAppFacadeFactory factory, IList<AvAppFacade> appList) : IComparer<AvApp>
{
    public IVisionProject Project { get; } = project;
    public IAvAppFacadeFactory Factory { get; } = factory;
    private IList<AvAppFacade> _collection = appList;
    public int Getto(IEnumerable<AvApp> apps)
    {
        Factory.Populate(Filter(apps), _collection, perItemAction: UpdateCompatibility);
        // _collection is sorted from oldest version to newest
        var t = (int)(Project.Type.Type & AvType.NonVersionableTypes)!=0;
        if ((int)(Project.Type.Type & AvType.NonVersionableTypes) != 0)
        {
            // Project has no version - takes the newest version
            return _collection.Count - 1;
        }

        int? exactVersionCustom = null;
        int? largerCustom = null;
        for (int i = 0; i < _collection.Count; i++)
        {
            var app = _collection[i];
            if (Equals(app.Version, Project.Version))
            {
                if (app.IsCustom && exactVersionCustom is null)
                {
                    // found custom version - might find non-custom still
                    exactVersionCustom = i;
                    continue;
                }

                // found exact version
                return i;
            }
            if (app.Version.CompareTo(Project.Version) > 0)
            {
                if (app.IsCustom && largerCustom is null)
                {
                    // found custom version - might find non-custom still
                    largerCustom = i;
                    continue;
                }

                // found exact version
                return i;
            }
        }
        if(exactVersionCustom is int custom)
        {
            return custom;
        }

        return -1;
    }
    private void UpdateCompatibility(AvAppFacade avApp)
    {
        avApp.Compatibility = JudgeCompatibility(avApp, Project);
        avApp.MostCompatible = IAvVersion.Equals(avApp.Version, Project.Version);
    }
    private static Compatibility JudgeCompatibility(IAvApp app, IVisionProject program)
    {
        if (!app.Brand.SupportsBrand(program.Brand))
        {
            return Compatibility.Incompatible;
        }
        if (!app.CanOpen(program))
        {
            return Compatibility.Incompatible;
        }
        if (program.Version.IsUnknown)
        {
            return Compatibility.Unknown;
        }
        if (program.Type == ProductType.Runtime)
        {
            // ignore revision
            if (app.Version.Major == program.Version.Major && app.Version.Minor == program.Version.Minor && app.Version.Build == program.Version.Build)
            {
                return Compatibility.Compatible;
            }
            return Compatibility.Incompatible;
        }
        if (app.Version.CompareTo(program.Version) >= 0)
        {
            return Compatibility.Compatible;
        }
        return Compatibility.Outdated;
    }
    private IEnumerable<AvAppFacade> Filter(IEnumerable<AvApp> apps)
    {
        return apps
            .Where(x => x.CanOpen(Project))
            .Select(Factory.Create)
            .OfType<AvAppFacade>()
            .OrderBy(x => x.Version);
    }

    public int Compare(AvApp? x, AvApp? y)
    {
        throw new NotImplementedException();
    }
}

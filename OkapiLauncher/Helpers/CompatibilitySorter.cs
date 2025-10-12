using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using OkapiLauncher.Contracts.Services;
using OkapiLauncher.Core.Models;
using OkapiLauncher.Core.Models.Apps;
using OkapiLauncher.Core.Models.Projects;
using OkapiLauncher.Models;

namespace OkapiLauncher.Helpers;
internal sealed class CompatibilitySorter(IVisionProject project, IAvAppFacadeFactory factory, IList<AvAppFacade> appList)
{
    private readonly record struct Entry(IAvApp App, int Index)
    {
        public IAvVersion Version => App.Version;
        public bool Custom => App.IsCustom;
        public int CompareTo(Entry other)
        {
            var v = Version.CompareTo(other.Version);
            if (v != 0)
            {
                return v;
            }
            // if they have the same version
            return (Custom, other.Custom) switch
            {
                //non-custom version have priority, i.e. they are lower
                (true, false) => 1, // this instance is custom so it goes after
                (false, true) => -1,
                _ => 0, // they are equal
            };
        }
    }

    public IVisionProject Project { get; } = project;
    public IAvAppFacadeFactory Factory { get; } = factory;
    private IList<AvAppFacade> _collection = appList;
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "MA0051:Method is too long", Justification = "I don't feel like refactoring :|")]
    public int GetClosestVersion(IEnumerable<AvApp> apps)
    {
        Factory.Populate(Filter(apps), _collection, perItemAction: UpdateCompatibility);
        // _collection is sorted from newest version to oldest
        if ((int)(Project.Type.Type & AvType.NonVersionableTypes) != 0)
        {
            // Project has no version - takes the newest version
            return 0;
        }
        Entry? exactCustomVersion = null;
        Entry? largeVersion = null;
        Entry? smallVersion = null;
        // going from the newest (largest) version to the lowest
        for (int i = 0; i < _collection.Count; i++)
        {
            var app = _collection[i];
            if (Project.Type == ProductType.Professional && app.Type == ProductType.Runtime)
            {
                // do not suggest runtimes for studio projects
                continue;
            }
            int versionComparison = app.Version.CompareTo(Project.Version);
            if (versionComparison > 0)
            {
                if (!largeVersion.HasValue)
                {
                    largeVersion = new Entry(app, i);
                    continue;
                }
                int comparison = largeVersion.Value.Version.CompareTo(app.Version);
                if (comparison > 0)
                {
                    largeVersion = new Entry(app, i);
                }
                if (comparison == 0 && !app.IsCustom)
                {
                    largeVersion = new Entry(app, i);
                }
                continue;
            }
            // if we got here, all later versions are already gone, only exact and older remain
            if (versionComparison == 0)
            {
                if (app.IsCustom)
                {
                    exactCustomVersion = new(app, i);
                    // found custom version - might find non-custom still
                    continue;
                }
                // found the exact version
                return i;
            }
            if (versionComparison < 0)
            {
                if (!smallVersion.HasValue)
                {
                    smallVersion = new Entry(app, i);
                    continue;
                }
                int comparison = smallVersion.Value.Version.CompareTo(app.Version);
                if (comparison > 0)
                {
                    smallVersion = new Entry(app, i);
                }
                if (comparison == 0 && !app.IsCustom)
                {
                    smallVersion = new Entry(app, i);
                }
                continue;
            }

        }
        var entry = exactCustomVersion ?? largeVersion ?? smallVersion;

        return entry?.Index ?? -1;
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
            .OrderByDescending(x => x.Version);
    }
}

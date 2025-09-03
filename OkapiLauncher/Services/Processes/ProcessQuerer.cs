using System.Diagnostics;
using CommunityToolkit.Mvvm.Messaging;
using OkapiLauncher.Core.Models.Apps;
using OkapiLauncher.Models;
using OkapiLauncher.Models.Messages;

namespace OkapiLauncher.Services.Processes;

public abstract class ProcessQuerer(IMessenger messenger) : IProcessQuerer
{
    protected readonly IMessenger _messenger = messenger;
    protected readonly IDictionary<string, IList<SimpleProcess>> _dict = new Dictionary<string, IList<SimpleProcess>>(StringComparer.OrdinalIgnoreCase);
    protected readonly object _lock = new();
    private int LashHash { get; set; }
    protected DateTime LastUpdate { get; set; } = DateTime.MinValue;
    private static readonly TimeSpan GracePeriod = TimeSpan.FromMilliseconds(300);
    virtual public TimeSpan TimerPeriod { get; } = TimeSpan.FromMilliseconds(2000);

    public abstract FreshAppProcesses? GetProcesses(IEnumerable<IAvApp> apps);
    public abstract FreshAppProcesses? UpdateSingleApp(IAvApp app);
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "MA0064:Avoid locking on publicly accessible instance", Justification = "<Pending>")]
    protected FreshAppProcesses DictToFAP()
    {
        var fap = new FreshAppProcesses(_dict);
        return fap;
    }

    public bool IsScheduledUpdateNear()
    {
        var timePassed = DateTime.UtcNow - LastUpdate;
        var diff = TimerPeriod - timePassed;
        return diff < GracePeriod && diff > TimeSpan.Zero;
    }
    protected void MarkUpdate()
    {
        LastUpdate = DateTime.UtcNow;
    }
    protected void Clear()
    {
        foreach (var key in _dict.Keys)
        {
            _dict[key].Clear();
        }
    }
}

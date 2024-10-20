using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace OkapiLauncher.Models.Messages;
internal class RecentFilesChangedMessage : ValueChangedMessage<IEnumerable<RecentlyOpenedFileFacade>>
{
    List<RecentlyOpenedFileFacade> _recentlyOpenedFiles;
    public IReadOnlyCollection<RecentlyOpenedFileFacade> RecentlyOpenedFiles => _recentlyOpenedFiles.AsReadOnly();
    public RecentFilesChangedMessage(IEnumerable<RecentlyOpenedFileFacade> value) : base(value)
    {
        _recentlyOpenedFiles = value.ToList();
    }
}

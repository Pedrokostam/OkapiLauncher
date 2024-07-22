using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AuroraVisionLauncher.Models.Messages
{
    public record AppProcessChangedMessage(string AppPath)
    {
        public bool AffectedAppPresent(IList<AvAppFacade> facades,[NotNullWhen(true)] out AvAppFacade? app)
        {
            app = facades.FirstOrDefault(x=>AppPath.Equals(x?.Path, StringComparison.OrdinalIgnoreCase));
            return app is not null;
        }
        public bool AffectedAppPresent(AvAppFacade? facade)
        {
            return AppPath.Equals(facade?.Path, StringComparison.OrdinalIgnoreCase);
        }
    }
}

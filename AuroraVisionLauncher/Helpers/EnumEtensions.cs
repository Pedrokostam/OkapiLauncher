using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AuroraVisionLauncher.Contracts.Services;

namespace AuroraVisionLauncher.Helpers;
public static class EnumEtensions
{
    public static  bool IsRegisteredApp(this IApplicationInfoService.InstallationScope scope)
    {
        return scope == IApplicationInfoService.InstallationScope.LocalMachine || scope == IApplicationInfoService.InstallationScope.CurrentUser;
    }
}

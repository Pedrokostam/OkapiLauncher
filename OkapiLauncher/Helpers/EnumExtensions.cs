using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OkapiLauncher.Contracts.Services;

namespace OkapiLauncher.Helpers;
public static class EnumExtensions
{
    public static bool IsRegisteredApp(this IApplicationInfoService.InstallationScope scope)
    {
        return scope == IApplicationInfoService.InstallationScope.LocalMachine || scope == IApplicationInfoService.InstallationScope.CurrentUser;
    }
    /// <summary>
    /// Excludes value 0
    /// </summary>
    /// <param name="enumType"></param>
    /// <returns></returns>
    private static IEnumerable<int> GetAllStandaloneFlags_impl(Type enumType)
    {
        var baseOptions = Enum.GetValues(enumType)
          .Cast<int>()
          .Where(v => v != 0 && IsPowerOfTwo(v));
        return baseOptions;
    }
    public static IReadOnlyList<int> GetAllStandaloneFlags(Type enumType)
    {
        return GetAllStandaloneFlags_impl(enumType).ToList().AsReadOnly();
    }
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2021:Do not call Enumerable.Cast<T> or Enumerable.OfType<T> with incompatible types", Justification = "It will work, mate :)")]
    public static IReadOnlyList<T> GetAllStandaloneFlags<T>() where T : Enum
    {
        return GetAllStandaloneFlags_impl(typeof(T)).Cast<T>().ToList().AsReadOnly();
    }
    private static bool IsPowerOfTwo(int x)
    {
        return (x & (x - 1)) == 0;
    }
}
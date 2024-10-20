using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace OkapiLauncher.Helpers;
public static class TimerHelper
{
    /// <summary>
    /// Returns a standard <see cref="DispatcherTimer"/>. Not started.
    /// </summary>
    /// <returns>Unstarted <see cref="DispatcherTimer"/></returns>
    public static DispatcherTimer GetTimer()
    {
        return new DispatcherTimer()
        {
            Interval = TimeSpan.FromSeconds(3),
        };
    }
    /// <summary>
    /// Returns a standard <see cref="DispatcherTimer"/>. Not started.
    /// </summary>
    /// <returns>Unstarted <see cref="DispatcherTimer"/></returns>
    public static DispatcherTimer GetTimer(int milliseconds)
    {
        return new DispatcherTimer()
        {
            Interval = TimeSpan.FromMilliseconds(milliseconds),
        };
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace OkapiLauncher.Exceptions;
internal class PageException : Exception
{
    /// <summary>
    /// Throws <see cref="PageDuplicateKeyException"/> if there is already a key in <paramref name="pages"/> that matches <code>typeof(<typeparamref name="VM"/>).FullName</code>.
    /// </summary>
    /// <typeparam name="VM">Type of the view model of the page</typeparam>
    /// <param name="pages">Dictionary of existing pages</param>
    /// <exception cref="PageDuplicateKeyException"></exception>
    public static void ThrowOnDuplicateKey<VM>(IDictionary<string, Type> pages)
    {
        var key = typeof(VM).FullName!;
        if (pages.ContainsKey((key)))
        {
            throw new PageDuplicateKeyException(key);
        }
    }
    /// <summary>
    /// Throws <see cref="PageDuplicateViewException"/> if any element in <paramref name="pages"/> has the same value as <typeparamref name="V"/>.
    /// </summary>
    /// <typeparam name="VM">Type of the view of the page</typeparam>
    /// <param name="pages">Dictionary of existing pages</param>
    /// <exception cref="PageDuplicateViewException"></exception>
    public static void ThrowOnDuplicateView<V>(IDictionary<string, Type> pages)
    {
        var type = typeof(V);
        if (pages.Any(p => p.Value == type))
        {
            throw new PageDuplicateViewException($"This type is already configured with key {pages.First(p => p.Value == type).Key}");
        }
    }
}

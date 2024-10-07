using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuroraVisionLauncher.Core.Helpers;
public static class CollectionExtensions
{
    public static int FindInsertionIndex<T>(this IList<T> collection, Predicate<T> condition)
    {
        for (int i = 0; i < collection.Count; i++)
        {
            if (condition.Invoke(collection[i]))
            {
                return i;
            }
        }
        return collection.Count;
    }
    /// <summary>
    /// Adds the item if it is not null.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <param name="item"></param>
    public static void AddNotNull<T>(this ICollection<T> list, T? item)
    {
        if (item is null)
        {
            return;
        }
        list.Add(item);
    }
}

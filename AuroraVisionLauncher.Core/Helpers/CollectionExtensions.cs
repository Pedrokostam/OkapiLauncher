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
}

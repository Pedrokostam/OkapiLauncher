using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OkapiLauncher.Core.Helpers;
public static class CollectionExtensions
{
    public static int FindInsertionIndex<T>(this IList<T> collection, Func<T,bool> condition)
    {
        for (int i = 0; i < collection.Count; i++)
        {
            if (condition(collection[i]))
            {
                return i;
            }
        }
        return collection.Count;
    }
    public static int IndexOfMax<T>(this IEnumerable<T> collection, IComparer<T>? comparer = null)
    {
        if (!collection.Any())
        {
            return -1;
        }
        return collection
             .Select((item, index) => (item, index))
             .MaxBy(x => x.item,comparer).index;
    }
    public static int IndexOfMin<T>(this IEnumerable<T> collection,IComparer<T>? comparer=null)
    {
        if (!collection.Any())
        {
            return -1;
        }
        return collection
             .Select((item, index) => (item, index))
             .MinBy(x => x.item,comparer).index;
    }
    public static int IndexOfMax<T,TKey>(this IEnumerable<T> collection,Func<T,TKey> selector, IComparer<TKey>? comparer = null)
    {
        if (!collection.Any())
        {
            return -1;
        }
        return collection
            .Select((item, index) => (item, index))
            .MaxBy(x => selector(x.item),comparer).index;
    }
    public static int IndexOfMin<T, TKey>(this IEnumerable<T> collection, Func<T, TKey> selector, IComparer<TKey>? comparer = null)
    {
        if (!collection.Any())
        {
            return -1;
        }
        return collection
             .Select((item, index) => (item, index))
             .MinBy(x => selector(x.item),comparer).index;
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

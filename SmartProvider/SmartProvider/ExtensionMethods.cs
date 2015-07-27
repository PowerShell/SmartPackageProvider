using System.Collections.Generic;
using System.Linq;

namespace SmartProvider
{
    internal static class ExtensionMethods
    {
        internal static IEnumerable<T> AsNotNull<T>(this IEnumerable<T> source)
        {
            return source ?? Enumerable.Empty<T>();
        }

        internal static IEnumerable<T> AsEnumerable<T>(this T item)
        {
            yield return item;
        }
    }
}

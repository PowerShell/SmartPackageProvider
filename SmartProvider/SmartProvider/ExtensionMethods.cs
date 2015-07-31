using System;
using System.Collections.Generic;
using System.Linq;

namespace SmartProvider
{
    public static class ExtensionMethods
    {
        public static IEnumerable<T> AsNotNull<T>(this IEnumerable<T> source)
        {
            return source ?? Enumerable.Empty<T>();
        }

        public static IEnumerable<T> AsEnumerable<T>(this T item)
        {
            yield return item;
        }

        public static bool IsValidUri(this string uri)
        {
            if (uri.StartsWith("http") || uri.StartsWith("http") || uri.StartsWith("ftp"))
            {
                return true;
            }

            return false;
        }

        public static bool FuzzyContains(this IEnumerable<string> items, string value)
        {
            foreach (var item in items)
            {
                if (item.Contains(value))
                {
                    return true;
                }
            }

            return false;
        }
    }
}

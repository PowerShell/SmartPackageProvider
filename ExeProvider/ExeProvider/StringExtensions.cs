using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExeProvider
{
    internal static class StringExtensions
    {
        internal static string ToBase64(this string text)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(text));
        }

        internal static string FromBase64(this string text)
        {
            if (text == null)
            {
                return null;
            }
            return Convert.FromBase64String(text).ToUtf8String();
        }

        internal static string ToUtf8String(this IEnumerable<byte> bytes)
        {
            var data = bytes.ToArray();
            try
            {
                return Encoding.UTF8.GetString(data);
            }
            finally
            {
                Array.Clear(data, 0, data.Length);
            }
        }
    }
}

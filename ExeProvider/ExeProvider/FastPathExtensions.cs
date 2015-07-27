using System;
using System.Text.RegularExpressions;

namespace ExeProvider
{
    internal static class FastPathExtensions
    {
        private static readonly Regex RxFastPath = new Regex(@"\$(?<source>[\w,\+,\/,=]*)\\(?<id>[\w,\+,\/,=]*)\\(?<version>[\w,\+,\/,=]*)");

        internal static string MakeFastPath(this PackageSource source, string id, string version)
        {
            return String.Format(@"${0}\{1}\{2}", source.Serialized, id.ToBase64(), version.ToBase64());
        }

        internal static bool TryParseFastPath(this string fastPath, out string source, out string id, out string version)
        {
            var match = RxFastPath.Match(fastPath);
            source = match.Success ? match.Groups["source"].Value.FromBase64() : null;
            id = match.Success ? match.Groups["id"].Value.FromBase64() : null;
            version = match.Success ? match.Groups["version"].Value.FromBase64() : null;
            return match.Success;
        }
    }
}

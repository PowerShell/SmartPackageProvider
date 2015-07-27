using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExeProvider
{
    internal class PackageItem
    {
        internal PackageItem(PackageSource source, string id, string version)
        {
            Id = id;
            Version = version;
            PackageSource = source;

            FastPath = source.MakeFastPath(id, version);
        }

        public string FastPath { get; private set; }

        public PackageSource PackageSource { get; private set; }

        public string Id { get; private set; }

        public string Version { get; private set; }

        public string Title;

        public string Summary;

        public string Description;

        public string FullPath;

        public string PackageFilename;

        public string Language;

        public string Tags;

        public string Copyright;

        public string LicenseUrl;

        public string ProjectUrl;
    }
}

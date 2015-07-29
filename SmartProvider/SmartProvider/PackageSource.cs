using Newtonsoft.Json;
using System;
using System.IO;

namespace SmartProvider
{
    public class PackageSource
    {
        // this shouldn't be used, it's only for JSON deserializer
        public PackageSource()
        {
        }

        public PackageSource(string name, string location)
        {
            Name = name;
            Location = location;
        }

        [JsonProperty]
        internal string Name { get; set; }

        [JsonProperty]
        internal string Location { get; set; }

        [JsonProperty]
        internal bool Trusted { get; set; }

        [JsonProperty]
        internal bool IsRegistered { get; set; }

        [JsonProperty]
        internal bool IsValidated { get; set; }

        internal string Serialized
        {
            get { return Location.ToBase64(); }
        }
    }
}
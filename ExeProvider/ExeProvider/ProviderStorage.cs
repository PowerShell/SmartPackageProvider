using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OneGet.Sdk;

namespace ExeProvider
{
    internal static class ProviderStorage
    {
        private const string DefaultConfig = @"{
  ""Google"": {
    ""Name"": ""Google"",
    ""Location"": ""https://google.com"",
    ""Trusted"": false,
    ""IsRegistered"": true,
    ""IsValidated"": true
  },

""Bing"": {
    ""Name"": ""Bing"",
    ""Location"": ""https://bing.com"",
    ""Trusted"": false,
    ""IsRegistered"": true,
    ""IsValidated"": true
  },

""ProductsWeb"": {
    ""Name"": ""ProductsWeb"",
    ""Location"": ""http://productsweb/"",
    ""Trusted"": false,
    ""IsRegistered"": true,
    ""IsValidated"": true
  }
}";

        internal static IDictionary<string, PackageSource> GetPackageSources(Request request)
        {
            var filePath = GetConfigurationFileLocation(request);
            var packageSources = JsonConvert.DeserializeObject<IDictionary<string, PackageSource>>(File.ReadAllText(filePath));
            return packageSources ?? new Dictionary<string, PackageSource>();
        }

        internal static void AddPackageSource(string name, string location, bool trusted, Request request)
        {
            IDictionary<string, PackageSource> packageSources = GetPackageSources(request);

            if (packageSources.ContainsKey(name))
            {
                request.Error(ErrorCategory.ResourceExists.ToString(), name, Strings.PackageSourceExists, name);
                return;
            }

            packageSources.Add(name, new PackageSource() { Name = name, Location = location, Trusted = trusted, IsRegistered = true, IsValidated = true });
            SavePackageSources(packageSources, request);
        }

        internal static void RemovePackageSource(string name, Request request)
        {
            IDictionary<string, PackageSource> packageSources = GetPackageSources(request);

            if (!packageSources.ContainsKey(name))
            {
                request.Error(ErrorCategory.ResourceUnavailable.ToString(), name, Strings.PackageSourceNotFound, name);
                return;
            }

            packageSources.Remove(name);
            SavePackageSources(packageSources, request);
        }

        private static void SavePackageSources(IDictionary<string, PackageSource> packageSources, Request request)
        {
            var filePath = GetConfigurationFileLocation(request);
            string json = JsonConvert.SerializeObject(packageSources, Formatting.Indented);
            File.WriteAllText(filePath, json);
        }

        private static string GetConfigurationFileLocation(Request request)
        {
            var filePath = request.GetOptionValue("ConfigFile");
            if (String.IsNullOrEmpty(filePath))
            {
                //otherwise, use %APPDATA%/EXE/Smart.Config
                filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "EXE", "Smart.config");
            }

            Directory.CreateDirectory(Path.GetDirectoryName(filePath));

            // TODO: this can fail if we don't have permissions - need to verify if OneGet would display a good error message
            using (FileStream fs = new FileStream(filePath, FileMode.OpenOrCreate))
            {
                if (fs.Length == 0)
                {
                    using (var sw = new StreamWriter(fs))
                    {
                        sw.WriteLine(DefaultConfig);
                        sw.Flush();
                    }
                }
                fs.Close();
            }

            return filePath;
        }
    }
}

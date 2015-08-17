using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using OneGet.Sdk;
using System.Text.RegularExpressions;
using Constants = OneGet.Sdk.Constants;
using ErrorCategory = OneGet.Sdk.ErrorCategory;
using System.Diagnostics;

namespace ExeProvider
{
    /// <summary>
    /// EXE Package Provider
    /// </summary>
    public class ExeProvider
    {
        //private const int SearchPageSize = 40;

        /// <summary>
        /// The features that this package supports.
        /// </summary>
        protected static Dictionary<string, string[]> Features = new Dictionary<string, string[]> {
            {Constants.Features.SupportedSchemes, new[] {"http", "https", "file"}},
            {Constants.Features.SupportedExtensions, new[] {"exe"}},
            {Constants.Features.MagicSignatures, new[] {"4d5a"}},
        };


        /// <summary>
        /// Returns the name of the Provider.
        /// </summary>
        /// <returns>The name of this provider</returns>
        public string PackageProviderName
        {
            get { return "EXE"; }
        }

        /// <summary>
        /// Returns the version of the Provider. 
        /// </summary>
        /// <returns>The version of this provider </returns>
        public string ProviderVersion
        {
            get
            {
                return "1.0.0.0";
            }
        }

        /// <summary>
        /// This is just here as to give us some possibility of knowing when an unexception happens...
        /// At the very least, we'll write it to the system debug channel, so a developer can find it if they are looking for it.
        /// </summary>
        public void OnUnhandledException(string methodName, Exception exception)
        {
            Debug.WriteLine("Unexpected Exception thrown in '{0}::{1}' -- {2}\\{3}\r\n{4}", PackageProviderName, methodName, exception.GetType().Name, exception.Message, exception.StackTrace);
        }

        /// <summary>
        /// Performs one-time initialization of the $provider.
        /// </summary>
        /// <param name="request">An object passed in from the CORE that contains functions that can be used to interact with the CORE and HOST</param>
        public void InitializeProvider(Request request)
        {
            request.Debug("Calling '{0}::InitializeProvider'", PackageProviderName);
        }

        /// <summary>
        /// Returns a collection of strings to the client advertising features this provider supports.
        /// </summary>
        /// <param name="request">An object passed in from the CORE that contains functions that can be used to interact with the CORE and HOST</param>
        public void GetFeatures(Request request)
        {
            request.Debug("Calling '{0}::GetFeatures' ", PackageProviderName);

            request.Yield(Features);
        }

        /// <summary>
        /// Returns dynamic option definitions to the HOST
        ///
        /// example response:
        ///     request.YieldDynamicOption( "MySwitch", OptionType.String.ToString(), false);
        ///
        /// </summary>
        /// <param name="category">The category of dynamic options that the HOST is interested in</param>
        /// <param name="request">An object passed in from the CORE that contains functions that can be used to interact with the CORE and HOST</param>
        public void GetDynamicOptions(string category, Request request)
        {
            request.Debug("Calling '{0}::GetDynamicOptions' {1}", PackageProviderName, category);

            switch ((category ?? string.Empty).ToLowerInvariant())
            {
                case "package":
                    //request.YieldDynamicOption("FilterOnTag", Constants.OptionType.StringArray, false);
                    //request.YieldDynamicOption("Contains", Constants.OptionType.String, false);
                    //request.YieldDynamicOption("AllowPrereleaseVersions", Constants.OptionType.Switch, false);
                    break;

                case "source":
                    request.YieldDynamicOption("ConfigFile", Constants.OptionType.String, false);
                    //request.YieldDynamicOption("SkipValidate", Constants.OptionType.Switch, false);
                    break;

                // applies to Get-Package, Install-Package, Uninstall-Package
                case "install":
                    request.YieldDynamicOption("Destination", "Folder", false);
                    //request.YieldDynamicOption("SkipDependencies", Constants.OptionType.Switch, false);
                    //request.YieldDynamicOption("ContinueOnFailure", Constants.OptionType.Switch, false);
                    //request.YieldDynamicOption("ExcludeVersion", Constants.OptionType.Switch, false);
                    break;
                default:
                    request.Debug("Unknown category for '{0}::GetDynamicOptions': {1}", PackageProviderName, category);
                    break;
            }
        }

        /// <summary>
        /// Resolves and returns Package Sources to the client.
        /// 
        /// Specified sources are passed in via the request object (<c>request.GetSources()</c>). 
        /// 
        /// Sources are returned using <c>request.YieldPackageSource(...)</c>
        /// </summary>
        /// <param name="request">An object passed in from the CORE that contains functions that can be used to interact with the CORE and HOST</param>
        public void ResolvePackageSources(Request request)
        {
            request.Debug("Calling '{0}::ResolvePackageSources'", PackageProviderName);

            if (request.Sources.Any())
            {
                // the system is requesting sources that match the values passed.
                // if the value passed can be a legitimate source, but is not registered, return a package source marked unregistered.
                var packageSources = ProviderStorage.GetPackageSources(request);

                if (request.IsCanceled)
                {
                    return;
                }

                foreach (var source in request.Sources.AsNotNull())
                {
                    if (packageSources.ContainsKey(source))
                    {
                        var packageSource = packageSources[source];

                        // YieldPackageSource returns false when operation was cancelled
                        if (!request.YieldPackageSource(packageSource.Name, packageSource.Location, packageSource.Trusted, packageSource.IsRegistered, packageSource.IsValidated))
                        {
                            return;
                        }
                    }
                    else
                    {
                        request.Warning("Package Source '{0}' not found.", source);
                    }
                }
            }
            else
            {
                // the system is requesting all the registered sources
                var packageSources = ProviderStorage.GetPackageSources(request);
                foreach (var entry in packageSources.AsNotNull())
                {
                    var packageSource = entry.Value;

                    // YieldPackageSource returns false when operation was cancelled
                    if (!request.YieldPackageSource(packageSource.Name, packageSource.Location, packageSource.Trusted, packageSource.IsRegistered, packageSource.IsValidated))
                    {
                        return;
                    }
                }
            }
        }


        /// <summary>
        /// This is called when the user is adding (or updating) a package source
        /// </summary>
        /// <param name="name">The name of the package source. If this parameter is null or empty the PROVIDER should use the location as the name (if the PROVIDER actually stores names of package sources)</param>
        /// <param name="location">The location (ie, directory, URL, etc) of the package source. If this is null or empty, the PROVIDER should use the name as the location (if valid)</param>
        /// <param name="trusted">A boolean indicating that the user trusts this package source. Packages returned from this source should be marked as 'trusted'</param>
        /// <param name="request">An object passed in from the CORE that contains functions that can be used to interact with the CORE and HOST</param>
        public void AddPackageSource(string name, string location, bool trusted, Request request)
        {
            request.Debug("Calling '{0}::AddPackageSource' '{1}','{2}','{3}'", PackageProviderName, name, location, trusted);
            ProviderStorage.AddPackageSource(name, location, trusted, request);
        }

        /// <summary>
        /// Removes/Unregisters a package source
        /// </summary>
        /// <param name="name">The name or location of a package source to remove.</param>
        /// <param name="request">An object passed in from the CORE that contains functions that can be used to interact with the CORE and HOST</param>
        public void RemovePackageSource(string name, Request request)
        {
            request.Debug("Calling '{0}::RemovePackageSource' '{1}'", PackageProviderName, name);
            ProviderStorage.RemovePackageSource(name, request);
        }


        /// <summary>
        /// Searches package sources given name and version information 
        /// 
        /// Package information must be returned using <c>request.YieldPackage(...)</c> function.
        /// </summary>
        /// <param name="name">a name or partial name of the package(s) requested</param>
        /// <param name="requiredVersion">A specific version of the package. Null or empty if the user did not specify</param>
        /// <param name="minimumVersion">A minimum version of the package. Null or empty if the user did not specify</param>
        /// <param name="maximumVersion">A maximum version of the package. Null or empty if the user did not specify</param>
        /// <param name="id">if this is greater than zero (and the number should have been generated using <c>StartFind(...)</c>, the core is calling this multiple times to do a batch search request. The operation can be delayed until <c>CompleteFind(...)</c> is called</param>
        /// <param name="request">An object passed in from the CORE that contains functions that can be used to interact with the CORE and HOST</param>
        public void FindPackage(string name, string requiredVersion, string minimumVersion, string maximumVersion, int id, Request request)
        {
            request.Debug("Calling '{0}::FindPackage' '{1}','{2}','{3}','{4}'", PackageProviderName, requiredVersion, minimumVersion, maximumVersion, id);

            List<PackageSource> sources;
            var providerPackageSources = ProviderStorage.GetPackageSources(request);

            if (request.PackageSources != null && request.PackageSources.Any())
            {
                sources = new List<PackageSource>();

                foreach (var userRequestedSource in request.PackageSources)
                {
                    if (providerPackageSources.ContainsKey(userRequestedSource))
                    {
                        sources.Add(providerPackageSources[userRequestedSource]);
                    }
                }
            }
            else
            {
                sources = providerPackageSources.Select(i => i.Value).ToList();
            }

            if (request.IsCanceled)
            {
                return;
            }


            var packageItem = new PackageItem(sources.FirstOrDefault(), "notepad", "1.0");

            // YieldPackage returns false when operation was cancelled
            if (!request.YieldPackage(packageItem, name))
            {
                return;
            }
        }

        /// <summary>
        /// Finds packages given a locally-accessible filename
        /// 
        /// Package information must be returned using <c>request.YieldPackage(...)</c> function.
        /// </summary>
        /// <param name="file">the full path to the file to determine if it is a package</param>
        /// <param name="id">if this is greater than zero (and the number should have been generated using <c>StartFind(...)</c>, the core is calling this multiple times to do a batch search request. The operation can be delayed until <c>CompleteFind(...)</c> is called</param>
        /// <param name="request">An object passed in from the CORE that contains functions that can be used to interact with the CORE and HOST</param>
        public void FindPackageByFile(string file, int id, Request request)
        {
            request.Debug("Calling '{0}::FindPackageByFile' '{1}','{2}'", PackageProviderName, file, id);

            /*List<PackageSource> sources;
            var providerPackageSources = ProviderStorage.GetPackageSources(request);

            if (request.PackageSources != null && request.PackageSources.Any())
            {
                sources = new List<PackageSource>();

                foreach (var userRequestedSource in request.PackageSources)
                {
                    if (providerPackageSources.ContainsKey(userRequestedSource))
                    {
                        sources.Add(providerPackageSources[userRequestedSource]);
                    }
                }
            }
            else
            {
                sources = providerPackageSources.Select(i => i.Value).ToList();
            }

            var source = sources.FirstOrDefault();*/

            var source = new PackageSource("Local Filesystem", file) { Trusted = true };

            request.YieldPackage(new PackageItem(source, file, ""), file);
        }

        /// <summary>
        /// Finds packages given a URI. 
        /// 
        /// The function is responsible for downloading any content required to make this work
        /// 
        /// Package information must be returned using <c>request.YieldPackage(...)</c> function.
        /// </summary>
        /// <param name="uri">the URI the client requesting a package for.</param>
        /// <param name="id">if this is greater than zero (and the number should have been generated using <c>StartFind(...)</c>, the core is calling this multiple times to do a batch search request. The operation can be delayed until <c>CompleteFind(...)</c> is called</param>
        /// <param name="request">An object passed in from the CORE that contains functions that can be used to interact with the CORE and HOST</param>
        public void FindPackageByUri(Uri uri, int id, Request request)
        {
            request.Debug("Calling '{0}::FindPackageByUri' '{1}','{2}'", PackageProviderName, uri, id);
        }

        /// <summary>
        /// Downloads a remote package file to a local location.
        /// </summary>
        /// <param name="fastPackageReference"></param>
        /// <param name="location"></param>
        /// <param name="request">An object passed in from the CORE that contains functions that can be used to interact with the CORE and HOST</param>
        public void DownloadPackage(string fastPackageReference, string location, Request request)
        {
            request.Debug("Calling '{0}::DownloadPackage' '{1}','{2}'", PackageProviderName, fastPackageReference, location);

            string source;
            string id;
            string version;
            if (!fastPackageReference.TryParseFastPath(out source, out id, out version))
            {
                request.Error(ErrorCategory.InvalidArgument.ToString(), fastPackageReference, Strings.InvalidFastPath, fastPackageReference);
            }

            // TODO: download
        }

        /// <summary>
        /// THIS API WILL BE DEPRECATED
        /// Returns package references for all the dependent packages
        /// This is called by Find-Package -IncludeDependencies and returned as a flat list
        /// As well as Install-Package -WhatIf
        /// </summary>
        /// <param name="fastPackageReference"></param>
        /// <param name="request">An object passed in from the CORE that contains functions that can be used to interact with the CORE and HOST</param>
        public void GetPackageDependencies(string fastPackageReference, Request request)
        {
            request.Debug("Calling '{0}::GetPackageDependencies' '{1}'", PackageProviderName, fastPackageReference);
        }

        // 
        static readonly Dictionary<string, string> KnowInstallerInfo = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            {"git", @"/VERYSILENT /NORESTART /NOCANCEL /SP- /NOICONS /COMPONENTS=""icons,icons\quicklaunch,ext,ext\reg,ext\reg\shellhere,ext\reg\guihere,assoc,assoc_sh"" /LOG" },
            {"npp", "/S" },
            {"firefox", "-ms" },
            //need prefix comparer for adobe reader
            {"AcroRdrDC1500720033_MUI","/sAll /msi /norestart /quiet ALLUSERS=1 EULA_ACCEPT=YES" },
            {"vlc", "/S" },
            {"Atom","--silent" },
            //same prefix comparer for jre
            {"jre8","/s REBOOT=Suppress" },
            {"ccsetup508", "/S" },
            {"rubyinstaller", @"/verysilent /tasks=""assocfiles,modpath""" },
            {"filezilla", "/S" },
            {"gvim", "/S" },
            {"dropbox", "/S" }
        };

        private Tuple<string, string> FindAppNameSilentArgs(string fullPath)
        {
            var name = Path.GetFileNameWithoutExtension(fullPath);
            //sometimes name can be a previx with . or _ seperated with version and other info
            // try get the only name part
            if (!string.IsNullOrEmpty(name))
            {
                var names = name.Split('.','_','-');
                if(names.Length >= 1)
                {
                    var basicName = names[0];
                    string appSilentOpt;
                    if(KnowInstallerInfo.TryGetValue(basicName, out appSilentOpt))
                    {
                        return new Tuple<string, string>(basicName,appSilentOpt);
                    }
                }
            }
            return null;
        }
        /// <summary>
        /// Installs a given package.
        /// </summary>
        /// <param name="fastPackageReference">A provider supplied identifier that specifies an exact package</param>
        /// <param name="request">An object passed in from the CORE that contains functions that can be used to interact with the CORE and HOST</param>
        public void InstallPackage(string fastPackageReference, Request request)
        {
            request.Debug("Calling '{0}::InstallPackage' '{1}'", PackageProviderName, fastPackageReference);

            string source; //ignore
            string id; //url or file path
            string version; 
            if (!fastPackageReference.TryParseFastPath(out source, out id, out version))
            {
                request.Error("", ErrorCategory.InvalidArgument.ToString(), fastPackageReference, Strings.InvalidFastPath);
            }

            var error = DscInvoker.SetLCMToDisabled();
            if(error != null)
            {
                request.Error("", ErrorCategory.InvalidOperation.ToString(), fastPackageReference, error.ErrorDetails.Message);
            }

            request.Debug("set the local LCM to disabled mode");

            request.Debug("try getting silent install option");

            var option = FindAppNameSilentArgs(id);

            if (option != null)
            {
                error = DscInvoker.InvokeDscPackegeResource(option.Item1, option.Item2, id);
                if (error != null)
                {
                    request.Error("", ErrorCategory.InvalidOperation.ToString(), fastPackageReference, error.ErrorDetails.Message);
                }
            }
            else
            {
                // Prepare the process to run
                ProcessStartInfo start = new ProcessStartInfo();
                // Enter the executable to run, including the complete path
                start.FileName = id;
                // Do you want to show a console window?
                start.WindowStyle = ProcessWindowStyle.Normal;
                //start.CreateNoWindow = true;

                // Run the external process & wait for it to finish
                using (Process proc = Process.Start(start))
                {
                    proc.WaitForExit();

                    // Retrieve the app's exit code
                    var exitCode = proc.ExitCode;
                    if(exitCode != 0)
                    {
                        request.Error("", ErrorCategory.InvalidOperation.ToString(), fastPackageReference, string.Format("running installation package {0} failed, error detail is {1}",id, proc.StandardError.ReadToEnd()));
                    }
                }
            }
        }

        /// <summary>
        /// Uninstalls a package 
        /// </summary>
        /// <param name="fastPackageReference"></param>
        /// <param name="request">An object passed in from the CORE that contains functions that can be used to interact with the CORE and HOST</param>
        public void UninstallPackage(string fastPackageReference, Request request)
        {
            request.Debug("Calling '{0}::UninstallPackage' '{1}'", PackageProviderName, fastPackageReference);

            string source;
            string id;
            string version;
            if (!fastPackageReference.TryParseFastPath(out source, out id, out version))
            {
                request.Error(ErrorCategory.InvalidArgument.ToString(), fastPackageReference, Strings.InvalidFastPath, fastPackageReference);
            }

            // TODO: uninstall
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="request">An object passed in from the CORE that contains functions that can be used to interact with the CORE and HOST</param>
        public void GetInstalledPackages(string name, string requiredVersion, string minimumVersion, string maximumVersion, Request request)
        {
            // TODO: improve this debug message that tells us what's going on.
            request.Debug("Calling '{0}::GetInstalledPackages' '{1}'", PackageProviderName, name);

            // TODO: list all installed packages
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fastPackageReference"></param>
        /// <param name="request">An object passed in from the CORE that contains functions that can be used to interact with the CORE and HOST</param>
        public void GetPackageDetails(string fastPackageReference, Request request)
        {
            // TODO: improve this debug message that tells us what's going on.
            request.Debug("Calling '{0}::GetPackageDetails' '{1}'", PackageProviderName, fastPackageReference);

            // TODO: This method is for fetching details that are more expensive than FindPackage* (if you don't need that, remove this method)
        }

        /// <summary>
        /// Initializes a batch search request.
        /// </summary>
        /// <param name="request">An object passed in from the CORE that contains functions that can be used to interact with the CORE and HOST</param>
        /// <returns></returns>
        public int StartFind(Request request)
        {
            // TODO: improve this debug message that tells us what's going on.
            request.Debug("Calling '{0}::StartFind'", PackageProviderName);

            // TODO: batch search implementation
            return default(int);
        }

        /// <summary>
        /// Finalizes a batch search request.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="request">An object passed in from the CORE that contains functions that can be used to interact with the CORE and HOST</param>
        /// <returns></returns>
        public void CompleteFind(int id, Request request)
        {
            // TODO: improve this debug message that tells us what's going on.
            request.Debug("Calling '{0}::CompleteFind' '{1}'", PackageProviderName, id);
            // TODO: batch search implementation
        }
    }
}

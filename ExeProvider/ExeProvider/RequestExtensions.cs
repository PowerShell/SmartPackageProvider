using System;
using System.Linq;
using OneGet.Sdk;

namespace ExeProvider
{
    internal static class RequestExtensions
    {
        internal static bool YieldPackage(this Request request, PackageItem pkg, string searchKey)
        {
            try
            {
                if (request.YieldSoftwareIdentity(pkg.FastPath, pkg.Id, pkg.Version.ToString(), "semver", pkg.Summary, pkg.PackageSource.Name, searchKey, pkg.FullPath, pkg.PackageFilename) != null)
                {
                    if (request.AddMetadata(pkg.FastPath, "FromTrustedSource", pkg.PackageSource.Trusted.ToString()) == null)
                    {
                        return false;
                    }

                    /*
                    // AddMetadata seems to return null for good values, commenting out the below
                    if (pkg.Copyright != null)
                    {
                        if (request.AddMetadata(pkg.FastPath, "copyright", pkg.Copyright) != null)
                        {
                            return false;
                        }
                    }

                    if (pkg.Description != null)
                    {
                        if (request.AddMetadata(pkg.FastPath, "description", pkg.Description) != null)
                        {
                            return false;
                        }
                    }

                    if (pkg.Language != null)
                    {
                        if (request.AddMetadata(pkg.FastPath, "language", pkg.Language) != null)
                        {
                            return false;
                        }
                    }

                    if (pkg.Tags != null)
                    {
                        if (request.AddMetadata(pkg.FastPath, "tags", pkg.Tags) != null)
                        {
                            return false;
                        }
                    }

                    if (pkg.Title != null)
                    {
                        if (request.AddMetadata(pkg.FastPath, "title", pkg.Title) != null)
                        {
                            return false;
                        }
                    }

                    if (pkg.PackageSource != null)
                    {
                        if (request.AddMetadata(pkg.FastPath, "FromTrustedSource", pkg.PackageSource.Trusted.ToString()) != null)
                        {
                            return false;
                        }
                    }
                    */
                }
                else
                {
                    return false;
                }
            }
            catch (NullReferenceException)
            {
                request.Error("", ErrorCategory.InvalidData.ToString(), pkg.Id, Strings.PackageMissingProperty);
            }

            return true;
        }
    }
}

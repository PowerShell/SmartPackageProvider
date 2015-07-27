using System;
using System.Linq;
using OneGet.Sdk;

namespace SmartProvider
{
    internal static class RequestExtensions
    {
        internal static bool YieldPackage(this Request request, PackageItem pkg, string searchKey)
        {
            try
            {
                if (request.YieldSoftwareIdentity(pkg.FastPath, pkg.Id, pkg.Version.ToString(), "semver", pkg.Summary, pkg.PackageSource.Name, searchKey, pkg.FullPath, pkg.PackageFilename))
                {
                    if (!request.YieldSoftwareMetadata(pkg.FastPath, "copyright", pkg.Copyright))
                    {
                        return false;
                    }
                    if (!request.YieldSoftwareMetadata(pkg.FastPath, "description", pkg.Description))
                    {
                        return false;
                    }
                    if (!request.YieldSoftwareMetadata(pkg.FastPath, "language", pkg.Language))
                    {
                        return false;
                    }
                    if (!request.YieldSoftwareMetadata(pkg.FastPath, "tags", pkg.Tags))
                    {
                        return false;
                    }
                    if (!request.YieldSoftwareMetadata(pkg.FastPath, "title", pkg.Title))
                    {
                        return false;
                    }
                    if (
                        !request.YieldSoftwareMetadata(pkg.FastPath, "FromTrustedSource", pkg.PackageSource.Trusted.ToString()))
                    {
                        return false;
                    }
                    if (pkg.LicenseUrl != null && !String.IsNullOrEmpty(pkg.LicenseUrl.ToString()))
                    {
                        if (
                            !request.YieldLink(pkg.FastPath, pkg.LicenseUrl.ToString(), "license", null, null,
                                null, null, null))
                        {
                            return false;
                        }
                    }
                    if (pkg.ProjectUrl != null && !String.IsNullOrEmpty(pkg.ProjectUrl.ToString()))
                    {
                        if (
                            !request.YieldLink(pkg.FastPath, pkg.ProjectUrl.ToString(), "project", null, null,
                                null, null, null))
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    return false;
                }
            }
            catch (NullReferenceException)
            {
                request.Error(ErrorCategory.InvalidData, pkg.Id, Strings.PackageMissingProperty, pkg.Id);
            }

            return true;
        }
    }
}

using System;
using System.Linq;
using Microsoft.PackageManagement.Internal.Api;
using System.Globalization;
using Constants = Microsoft.PackageManagement.Internal.Constants;
using Microsoft.PackageManagement.Internal;
using System.Collections.Generic;

namespace SmartProvider
{
    internal static class RequestExtensions
    {
        internal static bool YieldPackage(this IRequest request, PackageItem pkg, string searchKey)
        {
            try
            {
                if (request.YieldSoftwareIdentity(pkg.FastPath, pkg.Id, pkg.Version.ToString(), "semver", pkg.Summary, pkg.PackageSource.Name, searchKey, pkg.FullPath, pkg.PackageFilename) != null)
                {
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
                request.Error(ErrorCategory.InvalidData, pkg.Id, Strings.PackageMissingProperty, pkg.Id);
            }

            return true;
        }

        internal static void Debug(this IRequest request, string messageText, params object[] args)
        {
            request.Debug(request.FormatMessageString(messageText, args));
        }

        internal static void Warning(this IRequest request, string messageText, params object[] args)
        {
            request.Warning(request.FormatMessageString(messageText, args));
        }

        internal static void Verbose(this IRequest request, string messageText, params object[] args)
        {
            request.Verbose(request.FormatMessageString(messageText, args));
        }

        internal static void Error(this IRequest request, ErrorCategory category, string targetObjectValue, string messageText, params object[] args)
        {
            request.Error(messageText, category.ToString(), targetObjectValue, FormatMessageString(request, messageText, args));
        }

        internal static void Yield(this IRequest request, KeyValuePair<string, string[]> pair)
        {
            if (pair.Value.Length == 0)
            {
                request.YieldKeyValuePair(pair.Key, null);
            }
            pair.Value.All(each => request.YieldKeyValuePair(pair.Key, each));
        }

        internal static string GetOptionValue(this IRequest request, string name)
        {
            // get the value from the request
            return (request.GetOptionValues(name) ?? Enumerable.Empty<string>()).LastOrDefault();
        }

        private static string FormatMessageString(this IRequest request, string messageText, params object[] args)
        {
            if (string.IsNullOrWhiteSpace(messageText))
            {
                return string.Empty;
            }

            if (messageText.StartsWith(Constants.MSGPrefix, true, CultureInfo.CurrentCulture))
            {
                // check with the caller first, then with the local resources, and fallback to using the messageText itself.
                messageText = request.GetMessageString(messageText.Substring(Constants.MSGPrefix.Length), messageText) ?? messageText;
            }

            // if it doesn't look like we have the correct number of parameters
            // let's return a fix-me-format string.
            var c = messageText.ToCharArray().Where(each => each == '{').Count();
            if (c < args.Length)
            {
                return FixMeFormat(messageText, args);
            }
            return string.Format(CultureInfo.CurrentCulture, messageText, args);
        }

        private static string FixMeFormat(string formatString, object[] args)
        {
            if (args == null || args.Length == 0)
            {
                // not really any args, and not really expectng any
                return formatString.Replace('{', '\u00ab').Replace('}', '\u00bb');
            }
            return args.Aggregate(formatString.Replace('{', '\u00ab').Replace('}', '\u00bb'), (current, arg) => current + string.Format(CultureInfo.CurrentCulture, " \u00ab{0}\u00bb", arg));
        }
    }
}

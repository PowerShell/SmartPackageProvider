using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Runspaces;

namespace ExeProvider
{
    class DscInvoker
    {
        static internal Runspace GetNewRunspace()
        {
            Runspace runspace = RunspaceFactory.CreateRunspace(InitialSessionState.CreateDefault());
            runspace.ThreadOptions = PSThreadOptions.UseCurrentThread;
            runspace.InitialSessionState.Formats.Clear();
            runspace.Open();
            runspace.SessionStateProxy.SetVariable("DebugPreference", "Continue");
            return runspace;
        }
        static internal void CloseRunspace(Runspace runspace)
        {
            runspace.Dispose();
        }

        const string LCMSettings = @"/*
@TargetNode='localhost'
@GeneratedBy=admin
@GenerationDate=07/27/2015 15:22:47
@GenerationHost=localhost
*/

instance of MSFT_DSCMetaConfiguration as $MSFT_DSCMetaConfiguration1ref
{
RefreshMode = ""Disabled"";

};

instance of OMI_ConfigurationDocument
{
 Version=""2.0.0"";
 MinimumCompatibleVersion = ""1.0.0"";
 CompatibleVersionAdditionalProperties= { ""MSFT_DSCMetaConfiguration:StatusRetentionTimeInDays"" };
 Author=""admin"";
 GenerationDate=""07/27/2015 15:22:47"";
 GenerationHost=""localhost"";
 Name=""LCMSettings"";
};
";
        const string LCMSettingsName = ".\\localhost.meta.mof";

        static internal ErrorRecord SetLCMToDisabled()
        {
            if (!File.Exists(LCMSettingsName))
            {
                File.WriteAllText(LCMSettingsName, LCMSettings);
            }

            using (PowerShell powerShell = PowerShell.Create())
            {
                try
                {
                    powerShell.Runspace = GetNewRunspace();
                    powerShell.AddCommand("Set-DscLocalConfigurationManager").AddParameter("Path", ".\\");
                    Collection<PSObject> results = powerShell.Invoke();
                    if (powerShell.Streams.Error.Count > 0)
                    {
                        return powerShell.Streams.Error[0];
                    }
                }
                catch (Exception exception)
                {
                    return CreateErrorRecord("Set-DscLocalConfigurationManager", ErrorCategory.InvalidOperation, exception, "LCM", new object[0]);
                }
            }
            return null;

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fullyQualifiedErrorId"></param>
        /// <param name="errorCategory"></param>
        /// <param name="innerException"></param>
        /// <param name="resourceId"></param>
        /// <param name="resourceParms"></param>
        /// <returns></returns>
        internal static ErrorRecord CreateErrorRecord(
            string fullyQualifiedErrorId,
            ErrorCategory errorCategory,
            Exception innerException,
            string resourceId,
            params object[] resourceParms)
        {
            InvalidOperationException invalidOperationException;

            string errorMessage = string.Format(
                CultureInfo.CurrentCulture,
                resourceId,
                resourceParms);


            if (innerException != null)
            {
                invalidOperationException = new InvalidOperationException(errorMessage, innerException);
            }
            else
            {
                invalidOperationException = new InvalidOperationException(errorMessage);
            }

            ErrorRecord errorRecord = new ErrorRecord(
                invalidOperationException,
                fullyQualifiedErrorId,
                errorCategory,
                null);

            return errorRecord;
        }

        static internal ErrorRecord InvokeDscPackegeResource(string name, string arguments, string path)
        {
            Dictionary<string, string> packageProperty = new Dictionary<string, string>()
            {
                {"Ensure", "Present"},
                {"Name", name},
                {"Arguments", arguments},
                {"Path", path},
                {"ProductId", ""}
            };

            ErrorRecord error;
            PSObject outputResult;
            using (PowerShell powerShell = PowerShell.Create())
            {
                try
                {
                    powerShell.Runspace = GetNewRunspace();
                    powerShell.AddCommand("Invoke-DscResource").AddParameter("Name", "Package").AddParameter("Method", "Set").AddParameter("Property", packageProperty);
                    Collection<PSObject> collection = powerShell.Invoke();
                    if (collection.Count == 1)
                    {
                        outputResult = collection[0];
                    }
                    if (powerShell.Streams.Error.Count > 0)
                    {
                        return powerShell.Streams.Error[0];
                    }
                }
                catch (Exception exception)
                {
                    return CreateErrorRecord("Invoke-DscResource", ErrorCategory.InvalidOperation, exception, "Package", new object[0]);
                }
            }

            return null;
        }
    }
}

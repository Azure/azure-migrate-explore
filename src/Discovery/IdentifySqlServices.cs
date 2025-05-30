// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using System;
using Newtonsoft.Json;

using Azure.Migrate.Explore.Common;
using Azure.Migrate.Explore.HttpRequestHelper;
using Azure.Migrate.Explore.Models;

namespace Azure.Migrate.Explore.Discovery
{
    public class IdentifySqlServices
    {
        public bool IdentifyPresenceOfSqlServices(string machineId, string machineName, UserInput userInputObj)
        {
            bool result = false;

            string url = Routes.ProtocolScheme + Routes.AzureManagementApiHostname + machineId + Routes.ForwardSlash +
                         Routes.ApplicationsPath + Routes.QueryStringQuestionMark +
                         Routes.QueryParameterApiVersion + Routes.QueryStringEquals + Routes.DiscoverySitesApiVersion;

            string jsonResponse = "";
            try
            {
                jsonResponse = new HttpClientHelper().GetHttpRequestJsonStringResponse(url, userInputObj).Result;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (AggregateException aeSqlServices)
            {
                string errorMessage = "";
                foreach (var e in aeSqlServices.Flatten().InnerExceptions)
                {
                    if (e is OperationCanceledException)
                        throw e;
                    else
                    {
                        errorMessage = errorMessage + e.Message + " ";
                    }
                }
                userInputObj.LoggerObj.LogError($"Failed to obtain applications' data for {machineName}: {errorMessage}");
                return result;
            }
            catch (Exception exJsonResponse)
            {
                userInputObj.LoggerObj.LogError($"Failed to obtain applications' data for {machineName}: {exJsonResponse.Message}");
                return result;
            }

            ApplicationsJSON applicationsObj = JsonConvert.DeserializeObject<ApplicationsJSON>(jsonResponse);

            if (applicationsObj == null)
                return result;

            if (applicationsObj.Properties == null)
                return result;

            if (applicationsObj.Properties.AppsAndRoles == null)
                return result;

            if (applicationsObj.Properties.AppsAndRoles.Applications == null)
                return result;

            foreach (var application in applicationsObj.Properties.AppsAndRoles.Applications)
            {
                string applicationName = application.Name;
                if (string.IsNullOrEmpty(applicationName))
                    continue;

                if (applicationName.ToLower().Contains("sql") && (applicationName.ToLower().Contains("analysis services") ||
                                                                  applicationName.ToLower().Contains("reporting services") ||
                                                                  applicationName.ToLower().Contains("integration services"))
                   )
                {
                    result = true;
                    break;
                }
            }

            return result;
        }
    }
}
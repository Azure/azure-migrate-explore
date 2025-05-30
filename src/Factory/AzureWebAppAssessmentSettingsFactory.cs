// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

using Azure.Migrate.Explore.Common;
using Azure.Migrate.Explore.Models;

namespace Azure.Migrate.Explore.Factory
{
    public class AzureWebAppAssessmentSettingsFactory
    {
        public List<AssessmentInformation> GetAzureWebAppAssessmentSettings(UserInput userInputObj, string groupName)
        {
            List<AssessmentInformation> result = new List<AssessmentInformation>();

            if (userInputObj == null)
                throw new Exception("Received invalid null user input.");

            if (string.IsNullOrEmpty(groupName))
                throw new Exception("Received invalid group name.");

            userInputObj.LoggerObj.LogInformation($"Obtaining Azure WepApp assessment settings for group {groupName}");

            if (!groupName.Contains("WebApp"))
                throw new Exception($"Invalid group name {groupName} for Azure WebApp assessment settings factory.");

            if (groupName.Contains("Prod"))
                result = GetAzureWebAppProdAssessmentSettings(userInputObj, groupName);

            else if (groupName.Contains("Dev"))
                result = GetAzureWebAppDevAssessmentSettings(userInputObj, groupName);

            if (result.Count <= 0)
                throw new Exception($"Azure SQL WebApp assessment factory provided no settings for group {groupName}.");

            return result;
        }

        private List<AssessmentInformation> GetAzureWebAppProdAssessmentSettings(UserInput userInputObj, string groupName)
        {
            List<AssessmentInformation> result = new List<AssessmentInformation>();

            // Pay as you go
            AzureWebAppAssessmentSettingsJSON obj1 = new AzureWebAppAssessmentSettingsJSON();
            obj1.Properties.AzureLocation = userInputObj.TargetRegion.Key;
            obj1.Properties.ReservedInstance = "None";
            obj1.Properties.Currency = userInputObj.Currency.Key;
            obj1.Properties.AzureOfferCode = "MSAZR0003P";
            result.Add(new AssessmentInformation(groupName, "WebApp-Prod-AzMigExport-1", AssessmentType.WebAppAssessment, AssessmentTag.PerformanceBased, JsonConvert.SerializeObject(obj1)));

            // Pay as you go + RI 3 year
            AzureWebAppAssessmentSettingsJSON obj2 = new AzureWebAppAssessmentSettingsJSON();
            obj2.Properties.AzureLocation = userInputObj.TargetRegion.Key;
            obj2.Properties.ReservedInstance = "RI3Year";
            obj2.Properties.Currency = userInputObj.Currency.Key;
            obj2.Properties.AzureOfferCode = "MSAZR0003P";
            result.Add(new AssessmentInformation(groupName, "WebApp-Prod-AzMigExport-2", AssessmentType.WebAppAssessment, AssessmentTag.PerformanceBased_RI3year, JsonConvert.SerializeObject(obj2)));

            // Pay as you go + ASP 3 year
            AzureWebAppAssessmentSettingsJSON obj3 = new AzureWebAppAssessmentSettingsJSON();
            obj3.Properties.AzureLocation = userInputObj.TargetRegion.Key;
            obj3.Properties.ReservedInstance = "None";
            obj3.Properties.Currency = userInputObj.Currency.Key;
            obj3.Properties.AzureOfferCode = "SavingsPlan3Year";
            result.Add(new AssessmentInformation(groupName, "WebApp-Prod-AzMigExport-3", AssessmentType.WebAppAssessment, AssessmentTag.PerformanceBased_ASP3year, JsonConvert.SerializeObject(obj3)));

            return result;
        }

        private List<AssessmentInformation> GetAzureWebAppDevAssessmentSettings(UserInput userInputObj, string groupName)
        {
            List<AssessmentInformation> result = new List<AssessmentInformation>();

            // Pay as you go
            AzureWebAppAssessmentSettingsJSON obj1 = new AzureWebAppAssessmentSettingsJSON();
            obj1.Properties.AzureLocation = userInputObj.TargetRegion.Key;
            obj1.Properties.ReservedInstance = "None";
            obj1.Properties.Currency = userInputObj.Currency.Key;
            obj1.Properties.AzureOfferCode = "MSAZR0023P";
            result.Add(new AssessmentInformation(groupName, "WebApp-Dev-AzMigExport-1", AssessmentType.WebAppAssessment, AssessmentTag.PerformanceBased, JsonConvert.SerializeObject(obj1)));

            return result;
        }
    }
}
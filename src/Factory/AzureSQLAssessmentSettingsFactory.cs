// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

using Azure.Migrate.Explore.Common;
using Azure.Migrate.Explore.Models;

namespace Azure.Migrate.Explore.Factory
{
    public class AzureSQLAssessmentSettingsFactory
    {
        public List<AssessmentInformation> GetAzureSQLAssessmentSettings(UserInput userInputObj, string groupName)
        {
            List<AssessmentInformation> result = new List<AssessmentInformation>();

            if (userInputObj == null)
                throw new Exception("Received invalid null user input.");

            if (string.IsNullOrEmpty(groupName))
                throw new Exception("Received invalid group name.");

            userInputObj.LoggerObj.LogInformation($"Obtaining Azure SQL assessment settings for group {groupName}");

            if (!groupName.Contains("SQL"))
                throw new Exception($"Invalid group name {groupName} for Azure SQL assessment settings factory.");

            if (groupName.Contains("Prod"))
                result = GetAzureSQLProdAssessmentSettings(userInputObj, groupName);

            else if (groupName.Contains("Dev"))
                result = GetAzureSQLDevAssessmentSettings(userInputObj, groupName);

            if (result.Count <= 0)
                throw new Exception($"Azure SQL assessment factory provided no settings for group {groupName}.");

            return result;
        }

        private List<AssessmentInformation> GetAzureSQLProdAssessmentSettings(UserInput userInputObj, string groupName)
        {
            List<AssessmentInformation> result = new List<AssessmentInformation>();

            // Performance based - Pay as you go
            AzureSQLAssessmentSettingsJSON obj1 = new AzureSQLAssessmentSettingsJSON();
            obj1.Properties.AzureLocation = userInputObj.TargetRegion.Key;
            obj1.Properties.EnvrionmentType = "Production";
            obj1.Properties.ReservedInstance = "None";
            obj1.Properties.ReservedInstanceForVM = "None";
            obj1.Properties.Currency = userInputObj.Currency.Key;
            obj1.Properties.TimeRange = userInputObj.AssessmentDuration.Key;
            obj1.Properties.OSLicense = "No";
            obj1.Properties.SQLServerLicense = "No";
            result.Add(new AssessmentInformation("SQL-Prod-AzMigExport-1", AssessmentType.SQLAssessment, AssessmentTag.PerformanceBased, JsonConvert.SerializeObject(obj1), groupName));

            // Performance based - Pay as you go + RI 3 year
            AzureSQLAssessmentSettingsJSON obj2 = new AzureSQLAssessmentSettingsJSON();
            obj2.Properties.AzureLocation = userInputObj.TargetRegion.Key;
            obj2.Properties.EnvrionmentType = "Production";
            obj2.Properties.ReservedInstance = "RI3Year";
            obj2.Properties.ReservedInstanceForVM = "RI3Year";
            obj2.Properties.Currency = userInputObj.Currency.Key;
            obj2.Properties.TimeRange = userInputObj.AssessmentDuration.Key;
            obj2.Properties.OSLicense = "No";
            obj2.Properties.SQLServerLicense = "No";
            result.Add(new AssessmentInformation("SQL-Prod-AzMigExport-2", AssessmentType.SQLAssessment, AssessmentTag.PerformanceBased_RI3year, JsonConvert.SerializeObject(obj2), groupName));

            // Performance based - Pay as you go + AHUB
            AzureSQLAssessmentSettingsJSON obj3 = new AzureSQLAssessmentSettingsJSON();
            obj3.Properties.AzureLocation = userInputObj.TargetRegion.Key;
            obj3.Properties.EnvrionmentType = "Production";
            obj3.Properties.ReservedInstance = "None";
            obj3.Properties.ReservedInstanceForVM = "None";
            obj3.Properties.Currency = userInputObj.Currency.Key;
            obj3.Properties.TimeRange = userInputObj.AssessmentDuration.Key;
            obj3.Properties.OSLicense = "Yes";
            obj3.Properties.SQLServerLicense = "Yes";
            result.Add(new AssessmentInformation("SQL-Prod-AzMigExport-3", AssessmentType.SQLAssessment, AssessmentTag.PerformanceBased_AHUB, JsonConvert.SerializeObject(obj3), groupName));

            // Performance based - Pay as you go + RI 3 year + AHUB
            AzureSQLAssessmentSettingsJSON obj4 = new AzureSQLAssessmentSettingsJSON();
            obj4.Properties.AzureLocation = userInputObj.TargetRegion.Key;
            obj4.Properties.EnvrionmentType = "Production";
            obj4.Properties.ReservedInstance = "RI3Year";
            obj4.Properties.ReservedInstanceForVM = "RI3Year";
            obj4.Properties.Currency = userInputObj.Currency.Key;
            obj4.Properties.TimeRange = userInputObj.AssessmentDuration.Key;
            obj4.Properties.OSLicense = "Yes";
            obj4.Properties.SQLServerLicense = "Yes";
            result.Add(new AssessmentInformation("SQL-Prod-AzMigExport-4", AssessmentType.SQLAssessment, AssessmentTag.PerformanceBased_AHUB_RI3year, JsonConvert.SerializeObject(obj4), groupName));

            // Performance based - Pay as you go + ASP 3 year
            AzureSQLAssessmentSettingsJSON obj5 = new AzureSQLAssessmentSettingsJSON();
            obj5.Properties.AzureLocation = userInputObj.TargetRegion.Key;
            obj5.Properties.EnvrionmentType = "Production";
            obj5.Properties.ReservedInstance = "None";
            obj5.Properties.ReservedInstanceForVM = "None";
            obj5.Properties.AzureOfferCodeForVM = "SavingsPlan3Year";
            obj5.Properties.Currency = userInputObj.Currency.Key;
            obj5.Properties.TimeRange = userInputObj.AssessmentDuration.Key;
            obj5.Properties.OSLicense = "No";
            obj5.Properties.SQLServerLicense = "No";
            result.Add(new AssessmentInformation("SQL-Prod-AzMigExport-5", AssessmentType.SQLAssessment, AssessmentTag.PerformanceBased_ASP3year, JsonConvert.SerializeObject(obj5), groupName));

            return result;
        }

        private List<AssessmentInformation> GetAzureSQLDevAssessmentSettings(UserInput userInputObj, string groupName)
        {
            List<AssessmentInformation> result = new List<AssessmentInformation>();

            // Performance based - Pay as you go
            AzureSQLAssessmentSettingsJSON obj1 = new AzureSQLAssessmentSettingsJSON();
            obj1.Properties.AzureLocation = userInputObj.TargetRegion.Key;
            obj1.Properties.EnvrionmentType = "Test";
            obj1.Properties.ReservedInstance = "None";
            obj1.Properties.ReservedInstanceForVM = "None";
            obj1.Properties.Currency = userInputObj.Currency.Key;
            obj1.Properties.TimeRange = userInputObj.AssessmentDuration.Key;
            obj1.Properties.OSLicense = "No";
            obj1.Properties.SQLServerLicense = "No";
            result.Add(new AssessmentInformation("SQL-Dev-AzMigExport-1", AssessmentType.SQLAssessment, AssessmentTag.PerformanceBased, JsonConvert.SerializeObject(obj1), groupName));

            // Performance based - Pay as you go + AHUB
            AzureSQLAssessmentSettingsJSON obj2 = new AzureSQLAssessmentSettingsJSON();
            obj2.Properties.AzureLocation = userInputObj.TargetRegion.Key;
            obj2.Properties.EnvrionmentType = "Test";
            obj2.Properties.ReservedInstance = "None";
            obj2.Properties.ReservedInstanceForVM = "None";
            obj2.Properties.Currency = userInputObj.Currency.Key;
            obj2.Properties.TimeRange = userInputObj.AssessmentDuration.Key;
            obj2.Properties.OSLicense = "Yes";
            obj2.Properties.SQLServerLicense = "Yes";
            result.Add(new AssessmentInformation("SQL-Dev-AzMigExport-2", AssessmentType.SQLAssessment, AssessmentTag.PerformanceBased_AHUB, JsonConvert.SerializeObject(obj2), groupName));

            return result;
        }
    }
}
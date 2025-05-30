// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using Newtonsoft.Json;
using System;

using Azure.Migrate.Explore.Common;
using Azure.Migrate.Explore.Models;

namespace Azure.Migrate.Explore.Factory
{
    public class BusinessCaseSettingsFactory
    {
        public BusinessCaseInformation GetBusinessCaseSettings(UserInput userInputObj, string sessionId)
        {
            if (userInputObj == null)
                throw new Exception("Received invalid null user input.");

            if (string.IsNullOrEmpty(sessionId))
                throw new Exception("Received invalid session ID.");

            userInputObj.LoggerObj.LogInformation($"Obtaining Business case settings");

            BusinessCaseSettingsJSON obj = new BusinessCaseSettingsJSON();
            obj.Name = "bizcase-ame-" + sessionId;
            obj.Properties.Settings.AzureSettings.TargetLocation = userInputObj.TargetRegion.Key;
            obj.Properties.Settings.AzureSettings.Currency = userInputObj.Currency.Key;

            BusinessCaseTypes type = BusinessCaseTypes.OptimizeForPaas;
            if (userInputObj.PreferredOptimizationObj.OptimizationPreference.Key.Equals("MinimizetimewithAzureVM"))
                type = BusinessCaseTypes.IaaSOnly;
            else if (userInputObj.PreferredOptimizationObj.OptimizationPreference.Key.Equals("MigrateToAvs"))
                type = BusinessCaseTypes.AVSOnly;

            obj.Properties.Settings.AzureSettings.BusinessCaseType = type.ToString();
            obj.Properties.Settings.AzureSettings.WorkloadDiscoverySource = BusinessCaseWorkloadDiscoverySource.Appliance.ToString();
            if (userInputObj.AzureMigrateSourceAppliances.Contains("import"))
                obj.Properties.Settings.AzureSettings.WorkloadDiscoverySource = BusinessCaseWorkloadDiscoverySource.Import.ToString();

            obj.Properties.Settings.AzureSettings.SavingsOption = "SavingsPlan3Year";
            if (userInputObj.BusinessProposal == BusinessProposal.AVS.ToString())
            {
                obj.Properties.Settings.AzureSettings.SavingsOption = "RI3Year";
                obj.Properties.Settings.AzureSettings.PerYearMigrationCompletionPercentage =
                    AvsAssessmentConstants.perYearMigrationCompletionPercentage;
            }

            return new BusinessCaseInformation(obj.Name, JsonConvert.SerializeObject(obj));
        }
    }
}
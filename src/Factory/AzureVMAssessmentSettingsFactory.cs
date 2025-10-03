// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

using Azure.Migrate.Explore.Common;
using Azure.Migrate.Explore.Models;

namespace Azure.Migrate.Explore.Factory
{
    public class AzureVMAssessmentSettingsFactory
    {
        public List<AssessmentInformation> GetAzureVMAssessmentSettings(UserInput userInputObj, string groupName)
        {
            List<AssessmentInformation> result = new List<AssessmentInformation>();

            if (userInputObj == null)
                throw new Exception("Received invalid null user input.");

            if (string.IsNullOrEmpty(groupName))
                throw new Exception("Received invalid group name.");

            userInputObj.LoggerObj.LogInformation($"Obtaining AzureVM assessment settings for group {groupName}");

            if (!groupName.Contains("AzureVM"))
                throw new Exception($"Invalid group name {groupName} for AzureVM assessment settings factory.");

            if (groupName.Contains("Prod"))
                result = GetAzureVMProdAssessmentSettings(userInputObj, groupName);

            else if (groupName.Contains("Dev"))
                result = GetAzureVMDevAssessmentSettings(userInputObj, groupName);

            if (result.Count <= 0)
                throw new Exception($"AzureVM assessment factory provided no settings for group {groupName}.");

            return result;
        }

        private List<AssessmentInformation> GetAzureVMProdAssessmentSettings(UserInput userInputObj, string groupName)
        {
            List<AssessmentInformation> result = new List<AssessmentInformation>();

            // As on premises - Pay as you go
            AzureVMAssessmentSettingsJSON obj1 = new AzureVMAssessmentSettingsJSON();
            obj1.Properties.SizingCriterion = "AsOnPremises";
            obj1.Properties.AzureHybridUseBenefit = "No";
            obj1.Properties.ReservedInstance = "None";
            obj1.Properties.Currency = userInputObj.Currency.Key;
            obj1.Properties.AzureOfferCode = "MSAZR0003P";
            obj1.Properties.AzureLocation = userInputObj.TargetRegion.Key;
            obj1.Properties.TimeRange = userInputObj.AssessmentDuration.Key;
            result.Add(new AssessmentInformation("AzureVM-Prod-AzMigExport-1", AssessmentType.MachineAssessment, AssessmentTag.AsOnPremises, JsonConvert.SerializeObject(obj1), groupName));

            // Performance based - Pay as you go
            AzureVMAssessmentSettingsJSON obj2 = new AzureVMAssessmentSettingsJSON();
            obj2.Properties.SizingCriterion = "PerformanceBased";
            obj2.Properties.AzureHybridUseBenefit = "No";
            obj2.Properties.ReservedInstance = "None";
            obj2.Properties.Currency = userInputObj.Currency.Key;
            obj2.Properties.AzureOfferCode = "MSAZR0003P";
            obj2.Properties.AzureLocation = userInputObj.TargetRegion.Key;
            obj2.Properties.TimeRange = userInputObj.AssessmentDuration.Key;
            result.Add(new AssessmentInformation("AzureVM-Prod-AzMigExport-2", AssessmentType.MachineAssessment, AssessmentTag.PerformanceBased, JsonConvert.SerializeObject(obj2), groupName));

            // Performance based - Pay as you go + RI 3 year
            AzureVMAssessmentSettingsJSON obj3 = new AzureVMAssessmentSettingsJSON();
            obj3.Properties.SizingCriterion = "PerformanceBased";
            obj3.Properties.AzureHybridUseBenefit = "No";
            obj3.Properties.ReservedInstance = "RI3Year";
            obj3.Properties.Currency = userInputObj.Currency.Key;
            obj3.Properties.AzureOfferCode = "MSAZR0003P";
            obj3.Properties.AzureLocation = userInputObj.TargetRegion.Key;
            obj3.Properties.TimeRange = userInputObj.AssessmentDuration.Key;
            result.Add(new AssessmentInformation("AzureVM-Prod-AzMigExport-3", AssessmentType.MachineAssessment, AssessmentTag.PerformanceBased_RI3year, JsonConvert.SerializeObject(obj3), groupName));

            // Performance based - Pay as you go + AHUB
            AzureVMAssessmentSettingsJSON obj4 = new AzureVMAssessmentSettingsJSON();
            obj4.Properties.SizingCriterion = "PerformanceBased";
            obj4.Properties.AzureHybridUseBenefit = "Yes";
            obj4.Properties.ReservedInstance = "None";
            obj4.Properties.Currency = userInputObj.Currency.Key;
            obj4.Properties.AzureOfferCode = "MSAZR0003P";
            obj4.Properties.AzureLocation = userInputObj.TargetRegion.Key;
            obj4.Properties.TimeRange = userInputObj.AssessmentDuration.Key;
            result.Add(new AssessmentInformation("AzureVM-Prod-AzMigExport-4", AssessmentType.MachineAssessment, AssessmentTag.PerformanceBased_AHUB, JsonConvert.SerializeObject(obj4), groupName));

            // Performance based - Pay as you go + AHUB + RI 3 year
            AzureVMAssessmentSettingsJSON obj5 = new AzureVMAssessmentSettingsJSON();
            obj5.Properties.SizingCriterion = "PerformanceBased";
            obj5.Properties.AzureHybridUseBenefit = "Yes";
            obj5.Properties.ReservedInstance = "RI3Year";
            obj5.Properties.Currency = userInputObj.Currency.Key;
            obj5.Properties.AzureOfferCode = "MSAZR0003P";
            obj5.Properties.AzureLocation = userInputObj.TargetRegion.Key;
            obj5.Properties.TimeRange = userInputObj.AssessmentDuration.Key;
            result.Add(new AssessmentInformation("AzureVM-Prod-AzMigExport-5", AssessmentType.MachineAssessment, AssessmentTag.PerformanceBased_AHUB_RI3year, JsonConvert.SerializeObject(obj5), groupName));

            // Performance based - Pay as you go + ASP 3 year
            AzureVMAssessmentSettingsJSON obj6 = new AzureVMAssessmentSettingsJSON();
            obj6.Properties.SizingCriterion = "PerformanceBased";
            obj6.Properties.AzureHybridUseBenefit = "No";
            obj6.Properties.ReservedInstance = "None";
            obj6.Properties.Currency = userInputObj.Currency.Key;
            obj6.Properties.AzureOfferCode = "SavingsPlan3Year";
            obj6.Properties.AzureLocation = userInputObj.TargetRegion.Key;
            obj6.Properties.TimeRange = userInputObj.AssessmentDuration.Key;
            result.Add(new AssessmentInformation("AzureVM-Prod-AzMigExport-6", AssessmentType.MachineAssessment, AssessmentTag.PerformanceBased_ASP3year, JsonConvert.SerializeObject(obj6), groupName));

            return result;
        }

        private List<AssessmentInformation> GetAzureVMDevAssessmentSettings(UserInput userInputObj, string groupName)
        {
            List<AssessmentInformation> result = new List<AssessmentInformation>();

            // As on premises - Pay as you go
            AzureVMAssessmentSettingsJSON obj1 = new AzureVMAssessmentSettingsJSON();
            obj1.Properties.SizingCriterion = "AsOnPremises";
            obj1.Properties.AzureHybridUseBenefit = "No";
            obj1.Properties.ReservedInstance = "None";
            obj1.Properties.Currency = userInputObj.Currency.Key;
            obj1.Properties.AzureOfferCode = "MSAZR0023P";
            obj1.Properties.AzureLocation = userInputObj.TargetRegion.Key;
            obj1.Properties.TimeRange = userInputObj.AssessmentDuration.Key;
            result.Add(new AssessmentInformation("AzureVM-Dev-AzMigExport-1", AssessmentType.MachineAssessment, AssessmentTag.AsOnPremises, JsonConvert.SerializeObject(obj1), groupName));

            // Performance based - Pay as you go
            AzureVMAssessmentSettingsJSON obj2 = new AzureVMAssessmentSettingsJSON();
            obj2.Properties.SizingCriterion = "PerformanceBased";
            obj2.Properties.AzureHybridUseBenefit = "No";
            obj2.Properties.ReservedInstance = "None";
            obj2.Properties.Currency = userInputObj.Currency.Key;
            obj2.Properties.AzureOfferCode = "MSAZR0023P";
            obj2.Properties.AzureLocation = userInputObj.TargetRegion.Key;
            obj2.Properties.TimeRange = userInputObj.AssessmentDuration.Key;
            result.Add(new AssessmentInformation("AzureVM-Dev-AzMigExport-2", AssessmentType.MachineAssessment, AssessmentTag.PerformanceBased, JsonConvert.SerializeObject(obj2), groupName));

            // Performance based - Pay as you go + AHUB
            AzureVMAssessmentSettingsJSON obj3 = new AzureVMAssessmentSettingsJSON();
            obj3.Properties.SizingCriterion = "PerformanceBased";
            obj3.Properties.AzureHybridUseBenefit = "Yes";
            obj3.Properties.ReservedInstance = "None";
            obj3.Properties.Currency = userInputObj.Currency.Key;
            obj3.Properties.AzureOfferCode = "MSAZR0023P";
            obj3.Properties.AzureLocation = userInputObj.TargetRegion.Key;
            obj3.Properties.TimeRange = userInputObj.AssessmentDuration.Key;
            result.Add(new AssessmentInformation("AzureVM-Dev-AzMigExport-3", AssessmentType.MachineAssessment, AssessmentTag.PerformanceBased_AHUB, JsonConvert.SerializeObject(obj3), groupName));

            return result;
        }
    }
}
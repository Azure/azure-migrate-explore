// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

using Azure.Migrate.Explore.Common;
using Azure.Migrate.Explore.Models;

namespace Azure.Migrate.Explore.Factory
{
    public class AzureVMWareSolutionAssessmentSettingsFactory
    {
        public List<AssessmentInformation> GetAzureVMWareSolutionAssessmentSettings(UserInput userInputObj, string groupName)
        {
            List<AssessmentInformation> result = new List<AssessmentInformation>();

            if (userInputObj == null)
                throw new Exception("Received invalid null user input.");

            if (string.IsNullOrEmpty(groupName))
                throw new Exception("Received invalid group name.");

            userInputObj.LoggerObj.LogInformation($"Obtaining Azure VMWare solution assessment settings for group {groupName}");

            if (!groupName.Contains("Azure-VMWareSolution"))
                throw new Exception($"Invalid group name {groupName} for Azure VMWare solution assessment settings factory.");

            // AVS only has Prod pricing
            result = GetAzureVMWareSolutionProdAssessmentSettings(userInputObj, groupName);

            if (result.Count <= 0)
                throw new Exception($"Azure VMWare solution assessment factory provided no settings for group {groupName}.");

            return result;
        }

        private List<AssessmentInformation> GetAzureVMWareSolutionProdAssessmentSettings(UserInput userInputObj, string groupName)
        {
            List<AssessmentInformation> result = new List<AssessmentInformation>();
            List<string> nodeTypes = AvsAssessmentConstants.RegionToAvsNodeTypeMap[userInputObj.TargetRegion.Key];
            List<string> externalStorageTypes = new List<string>();
            if (AvsAssessmentConstants.anfStandardStorageRegionList.Contains(userInputObj.TargetRegion.Key))
            {
                externalStorageTypes.Add("AnfStandard");
            }

            if (AvsAssessmentConstants.anfPremiumStorageRegionList.Contains(userInputObj.TargetRegion.Key))
            {
                externalStorageTypes.Add("AnfPremium");
            }

            if (AvsAssessmentConstants.anfUltraStorageRegionLis.Contains(userInputObj.TargetRegion.Key))
            {
                externalStorageTypes.Add("AnfUltra");
            }

            // Performance based - Pay as you go
            AzureVMWareSolutionAssessmentSettingsJSON obj1 = new AzureVMWareSolutionAssessmentSettingsJSON();
            obj1.Properties.ReservedInstance = "None";
            obj1.Properties.Currency = userInputObj.Currency.Key;
            obj1.Properties.AzureLocation = userInputObj.TargetRegion.Key;
            obj1.Properties.NodeTypes = nodeTypes;
            obj1.Properties.ExternalStorageTypes = externalStorageTypes;
            result.Add(new AssessmentInformation(
                groupName, "AVS-Prod-AzMigExport-1",
                AssessmentType.AVSAssessment,
                AssessmentTag.PerformanceBased,
                JsonConvert.SerializeObject(obj1)
            ));

            // Performance based - Pay as you go + RI 1 year
            AzureVMWareSolutionAssessmentSettingsJSON obj2 = new AzureVMWareSolutionAssessmentSettingsJSON();
            obj2.Properties.ReservedInstance = "RI1Year";
            obj2.Properties.Currency = userInputObj.Currency.Key;
            obj2.Properties.AzureLocation = userInputObj.TargetRegion.Key;
            obj2.Properties.NodeTypes = nodeTypes;
            obj2.Properties.ExternalStorageTypes = externalStorageTypes;
            result.Add(new AssessmentInformation(
                groupName,
                "AVS-Prod-AzMigExport-2",
                AssessmentType.AVSAssessment,
                AssessmentTag.PerformanceBased_RI1year,
                JsonConvert.SerializeObject(obj2)
            ));

            // Performance based - Pay as you go + RI 3 year
            AzureVMWareSolutionAssessmentSettingsJSON obj3 = new AzureVMWareSolutionAssessmentSettingsJSON();
            obj3.Properties.ReservedInstance = "RI3Year";
            obj3.Properties.Currency = userInputObj.Currency.Key;
            obj3.Properties.AzureLocation = userInputObj.TargetRegion.Key;
            obj3.Properties.NodeTypes = nodeTypes;
            obj3.Properties.ExternalStorageTypes = externalStorageTypes;
            result.Add(new AssessmentInformation(
                groupName, "AVS-Prod-AzMigExport-3",
                AssessmentType.AVSAssessment,
                AssessmentTag.PerformanceBased_RI3year,
                JsonConvert.SerializeObject(obj3)
            ));

            // As- Onpremises - Pay as you go
            AzureVMWareSolutionAssessmentSettingsJSON obj4 = new AzureVMWareSolutionAssessmentSettingsJSON();
            obj4.Properties.SizingCriterion = "AsOnPremises";
            obj4.Properties.ReservedInstance = "None";
            obj4.Properties.Currency = userInputObj.Currency.Key;
            obj4.Properties.AzureLocation = userInputObj.TargetRegion.Key;
            obj4.Properties.NodeTypes = nodeTypes;
            obj4.Properties.ExternalStorageTypes = externalStorageTypes;
            result.Add(new AssessmentInformation(
                groupName, "AVS-Prod-AzMigExport-4",
                AssessmentType.AVSAssessment,
                AssessmentTag.AsOnPremises,
                JsonConvert.SerializeObject(obj4)
            ));

            // As- Onpremises - Pay as you go + RI 1 Year
            AzureVMWareSolutionAssessmentSettingsJSON obj5 = new AzureVMWareSolutionAssessmentSettingsJSON();
            obj5.Properties.SizingCriterion = "AsOnPremises";
            obj5.Properties.ReservedInstance = "RI1Year";
            obj5.Properties.Currency = userInputObj.Currency.Key;
            obj5.Properties.AzureLocation = userInputObj.TargetRegion.Key;
            obj5.Properties.NodeTypes = nodeTypes;
            obj5.Properties.ExternalStorageTypes = externalStorageTypes;
            result.Add(new AssessmentInformation(
                groupName, "AVS-Prod-AzMigExport-5",
                AssessmentType.AVSAssessment,
                AssessmentTag.AsOnPremises_RI1Year,
                JsonConvert.SerializeObject(obj5)
            ));

            // As- Onpremises - Pay as you go + RI 3 Year
            AzureVMWareSolutionAssessmentSettingsJSON obj6 = new AzureVMWareSolutionAssessmentSettingsJSON();
            obj6.Properties.SizingCriterion = "AsOnPremises";
            obj6.Properties.ReservedInstance = "RI3Year";
            obj6.Properties.Currency = userInputObj.Currency.Key;
            obj6.Properties.AzureLocation = userInputObj.TargetRegion.Key;
            obj6.Properties.NodeTypes = nodeTypes;
            obj6.Properties.ExternalStorageTypes = externalStorageTypes;
            result.Add(new AssessmentInformation(
                groupName, "AVS-Prod-AzMigExport-6",
                AssessmentType.AVSAssessment,
                AssessmentTag.AsOnPremises_RI3Year,
                JsonConvert.SerializeObject(obj6)
            ));

            return result;
        }
    }
}
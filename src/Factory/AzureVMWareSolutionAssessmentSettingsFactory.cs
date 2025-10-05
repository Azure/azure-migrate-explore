// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

using Azure.Migrate.Explore.Common;
using Azure.Migrate.Explore.Models;
using System.Linq;
using System.Text;
using ColorCode.Parsing;

namespace Azure.Migrate.Explore.Factory
{
    public class AzureVMWareSolutionAssessmentSettingsFactory
    {
        public List<AssessmentInformation> GetAzureVMWareSolutionAssessmentSettings(UserInput userInputObj, List<string> scopedMachineIds)
        {
            List<AssessmentInformation> result = new List<AssessmentInformation>();

            if (userInputObj == null)
                throw new Exception("Received invalid null user input.");

            // AVS only has Prod pricing
            result = GetAzureVMWareSolutionProdAssessmentSettings(userInputObj, scopedMachineIds);

            return result;
        }

        
        private string GenerateArgQuery(UserInput userInputObj, List<string> scopedMachineIds)
        {
            try
            {
                // Get the discovery source filter
                string discoverySourceFilter = GetDiscoverySourceFilter(userInputObj);
                
                // Create the machine IDs filter (using discovery machine ARM IDs)
                var machineIdsList = scopedMachineIds.Select(id => $"\"{id}\"").ToArray();
                string machineIdsFilter = string.Join(", ", machineIdsList);

                // Construct the ARG query
                var argQuery = new StringBuilder();
                argQuery.AppendLine("(migrateresources");
                argQuery.AppendLine("     | where ['type'] in (\"microsoft.offazure/vmwaresites/machines\", \"microsoft.offazure/serversites/machines\", \"microsoft.offazure/hypervsites/machines\", \"microsoft.offazure/importsites/machines\", \"microsoft.offazure/mastersites/sqlsites/sqlservers\", \"microsoft.offazure/mastersites/webappsites/iiswebapplications\", \"microsoft.offazure/mastersites/webappsites/tomcatwebapplications\", \"microsoft.offazure/importsites/machines\")");
                argQuery.AppendLine("     | extend type=tolower(type)");
                argQuery.AppendLine("     | extend properties_machineArmIds = iif(array_length(properties.machineArmIds) == 0, pack_array(id), properties.machineArmIds)");
                argQuery.AppendLine("     | mv-expand properties_machineArmIds");
                argQuery.AppendLine("     | extend machineArmIds=tostring(properties_machineArmIds)");
                argQuery.AppendLine("     | extend parentId = case(type contains \"/machines\", id, type contains \"/sqlservers\", machineArmIds, type contains \"/webappsites\", machineArmIds, \"\")");
                argQuery.AppendLine($"     | where parentId has \"/subscriptions/{userInputObj.Subscription.Key}/resourceGroups/{userInputObj.ResourceGroupName.Value}\"");
                argQuery.AppendLine("     | extend id = tolower(id), siteId = case(id has \"machines\", tostring(split(tolower(id),\"/machines/\")[0]), id has \"sqlsites\", tostring(split(tolower(id),\"/sqlsites/\")[0]), id has \"webappsites\", tostring(split(tolower(id),\"/webappsites/\")[0]), \"\")");
                argQuery.AppendLine("     | extend disks = iff(array_length(properties.disks) > 0, properties.disks, pack_array(pack_dictionary(\"\",\"\")))");
                argQuery.AppendLine("     | mv-expand disks");
                argQuery.AppendLine("     | extend diskSize = tolong(disks.maxSizeInBytes)");
                argQuery.AppendLine("     | summarize serverSizeInGB = sum(diskSize)/1073741824, properties = take_any(properties), type = take_any(type), siteId = take_any(siteId), parentId = take_any(parentId) by id");
                argQuery.AppendLine("     | project id, type, siteId, parentId, properties, serverSizeInGB");
                argQuery.AppendLine("     | extend parentId = tolower(parentId),");
                argQuery.AppendLine("         armId = id,");
                argQuery.AppendLine("         resourceType = type,");
                argQuery.AppendLine("         resourceTags = properties.tags,");
                argQuery.AppendLine("         host = case(");
                argQuery.AppendLine("                id contains \"microsoft.offazure/vmwaresites\", \"VMware\",");
                argQuery.AppendLine("                id contains \"microsoft.offazure/hypervsites\", \"Hyper-V\",");
                argQuery.AppendLine("                id contains \"microsoft.offazure/serversites\", \"Physical\",");
                argQuery.AppendLine("                id contains \"microsoft.offazure/importsites\" and properties.hypervisor =~ \"VMWare\", \"VMware\",");
                argQuery.AppendLine("                id contains \"microsoft.offazure/importsites\" and strlen(properties.hypervisor) > 0 and properties.hypervisor !~ \"VMWare\", properties.hypervisor,");
                argQuery.AppendLine("                \"-\"),");
                argQuery.AppendLine("         resourceName = tostring(iff(type contains \"/sqlservers\", properties.sqlServerName, properties.displayName)),");
                argQuery.AppendLine("         version = tostring(case(id has \"/machines/\", coalesce(properties.guestOSDetails.osName, properties.operatingSystemDetails.osName), id has \"/sqlsites/\", \"\", id has \"/webappsites/\", properties.version, \"\")),");
                argQuery.AppendLine("         edition = tostring(case(id has \"/machines/\", coalesce(properties.guestOSDetails.osVersion, properties.operatingSystemDetails.osVersion), id has \"/sqlsites/\", properties.edition, id has \"/webappsites/\", properties.version, \"\")),");
                argQuery.AppendLine("         osType = tostring(coalesce(properties.guestOSDetails.osType, properties.operatingSystemDetails.osType)),");
                argQuery.AppendLine("         powerOnStatus = case(properties.powerStatus == \"ON\" or properties.powerStatus == \"Running\", \"On\", properties.powerStatus == \"OFF\" or properties.powerStatus == \"PowerOff\" or properties.powerStatus == \"Saved\" or properties.powerStatus == \"Paused\", \"Off\", \"-\"),");
                argQuery.AppendLine("         discoverySource = case(id contains \"microsoft.offazure/importsites\", \"Import\", id contains \"/sqlsites/\" and properties.discoveryState == \"Imported\", \"Import\", \"Appliance\"),");
                argQuery.AppendLine("         dbProperties = case(id has \"/sqlsites/\", properties, parse_json(\"\")),");
                argQuery.AppendLine("         dbEngineStatus =  tostring(case(id has \"/sqlsites/\", properties.status, \"\")),");
                argQuery.AppendLine("         userdatabases = tostring(case(id has \"/sqlsites/\", properties.numberOfUserDatabases, \"\")),");
                argQuery.AppendLine("         totalSizeInGB =  case(id has \"/sqlsites/\", tolong(properties.sumOfUserDatabasesSizeInMb)/1024, serverSizeInGB),");
                argQuery.AppendLine("         memoryInMB = case(id has \"/sqlsites/\", tolong(properties.maxServerMemoryInUseInMb), tolong(properties.allocatedMemoryInMB)),");
                argQuery.AppendLine("         dbhadrConfiguration = tostring(case(id has \"/sqlsites/\", (case(toboolean(properties.isClustered) and toboolean(properties.isHighAvailabilityEnabled), \"Both\", case(toboolean(properties.isClustered), \"FailoverClusterInstance\", case(toboolean(properties.isHighAvailabilityEnabled), \"AvailabilityGroup\", \"\")))), \"\")),");
                argQuery.AppendLine("         diskCount = array_length(properties.disks),");
                argQuery.AppendLine("         supportEndsIn= datetime_diff(\"day\", todatetime(properties.productSupportStatus.supportEndDate), todatetime(now())),");
                argQuery.AppendLine("         depmapErrorCount = array_length(properties.dependencyMapDiscovery.errors)");
                argQuery.AppendLine($"    | where tostring(discoverySource) in ({discoverySourceFilter})");
                argQuery.Append($"  | where (id in~ ({machineIdsFilter})))");

                return argQuery.ToString();
            }
            catch (Exception ex)
            {
                userInputObj.LoggerObj.LogError($"Failed to generate ARG query: {ex.Message}");
                return "";
            }
        }

        private string GetDiscoverySourceFilter(UserInput userInputObj)
        {
            var sources = new List<string>();
            
            if (userInputObj.AzureMigrateSourceAppliances.Contains("vmware"))
            {
                sources.Add("\"Appliance\"");
            }
            
            if (userInputObj.AzureMigrateSourceAppliances.Contains("import"))
            {
                sources.Add("\"Import\"");
            }

            return sources.Any() ? string.Join(", ", sources) : "\"Appliance\"";
        }

        private List<AssessmentInformation> GetAzureVMWareSolutionProdAssessmentSettings(UserInput userInputObj, List<string> scopedMachineIds)
        {
            var ScopeDetails = new ScopeDetails();
            if (scopedMachineIds != null && scopedMachineIds.Any())
            {
                ScopeDetails.AzureResourceGraphQuery = GenerateArgQuery(userInputObj, scopedMachineIds);
                ScopeDetails.ScopeType = "AzureResourceGraphQuery";
            }
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

            if (AvsAssessmentConstants.anfUltraStorageRegionList.Contains(userInputObj.TargetRegion.Key))
            {
                externalStorageTypes.Add("AnfUltra");
            }

            if (AvsAssessmentConstants.AzureElasticSanBaseStorageRegionList.Contains(userInputObj.TargetRegion.Key))
            {
                externalStorageTypes.Add("AzureElasticSanBase");
            }

            if (AvsAssessmentConstants.AzureElasticSanCapacityStorageRegionList.Contains(userInputObj.TargetRegion.Key))
            {
                externalStorageTypes.Add("AzureElasticSanCapacity");
            }

            // Performance based - Pay as you go
            AzureVMWareSolutionAssessmentSettingsJSON obj1 = new AzureVMWareSolutionAssessmentSettingsJSON();
            obj1.Properties.Settings.SavingsSettings.SavingsOption = "None";
            obj1.Properties.Scope = ScopeDetails;
            obj1.Properties.Settings.Currency = userInputObj.Currency.Key;
            obj1.Properties.Settings.AzureLocation = userInputObj.TargetRegion.Key;
            obj1.Properties.Settings.NodeTypes = nodeTypes;
            obj1.Properties.Settings.BillingSettings.SubscriptionId = userInputObj.Subscription.Key;
            obj1.Properties.Settings.PerformanceData.TimeRange = userInputObj.AssessmentDuration.Key;
            obj1.Properties.Settings.ExternalStorageTypes = externalStorageTypes;
            result.Add(new AssessmentInformation(
                "AVS-Prod-AzMigExport-1",
                AssessmentType.AVSAssessment,
                AssessmentTag.PerformanceBased,
                JsonConvert.SerializeObject(obj1)
            ));

            // Performance based - Pay as you go + RI 1 year
            AzureVMWareSolutionAssessmentSettingsJSON obj2 = new AzureVMWareSolutionAssessmentSettingsJSON();
            obj2.Properties.Settings.SavingsSettings.SavingsOption = "RI1Year";
            obj2.Properties.Scope = ScopeDetails;
            obj2.Properties.Settings.Currency = userInputObj.Currency.Key;
            obj2.Properties.Settings.AzureLocation = userInputObj.TargetRegion.Key;
            obj2.Properties.Settings.BillingSettings.SubscriptionId = userInputObj.Subscription.Key;
            obj2.Properties.Settings.PerformanceData.TimeRange = userInputObj.AssessmentDuration.Key;
            obj2.Properties.Settings.NodeTypes = nodeTypes;
            obj2.Properties.Settings.ExternalStorageTypes = externalStorageTypes;
            result.Add(new AssessmentInformation(
                "AVS-Prod-AzMigExport-2",
                AssessmentType.AVSAssessment,
                AssessmentTag.PerformanceBased_RI1year,
                JsonConvert.SerializeObject(obj2)
            ));

            // Performance based - Pay as you go + RI 3 year
            AzureVMWareSolutionAssessmentSettingsJSON obj3 = new AzureVMWareSolutionAssessmentSettingsJSON();
            obj3.Properties.Settings.SavingsSettings.SavingsOption = "RI3Year";
            obj3.Properties.Scope = ScopeDetails;
            obj3.Properties.Settings.Currency = userInputObj.Currency.Key;
            obj3.Properties.Settings.AzureLocation = userInputObj.TargetRegion.Key;
            obj3.Properties.Settings.BillingSettings.SubscriptionId = userInputObj.Subscription.Key;
            obj3.Properties.Settings.PerformanceData.TimeRange = userInputObj.AssessmentDuration.Key;
            obj3.Properties.Settings.NodeTypes = nodeTypes;
            obj3.Properties.Settings.ExternalStorageTypes = externalStorageTypes;
            result.Add(new AssessmentInformation(
                "AVS-Prod-AzMigExport-3",
                AssessmentType.AVSAssessment,
                AssessmentTag.PerformanceBased_RI3year,
                JsonConvert.SerializeObject(obj3)
            ));

            // As- Onpremises - Pay as you go
            AzureVMWareSolutionAssessmentSettingsJSON obj4 = new AzureVMWareSolutionAssessmentSettingsJSON();
            obj4.Properties.Settings.SizingCriterion = "AsOnPremises";
            obj4.Properties.Scope = ScopeDetails;
            obj4.Properties.Settings.SavingsSettings.SavingsOption = "None";
            obj4.Properties.Settings.Currency = userInputObj.Currency.Key;
            obj4.Properties.Settings.AzureLocation = userInputObj.TargetRegion.Key;
            obj4.Properties.Settings.BillingSettings.SubscriptionId = userInputObj.Subscription.Key;
            obj4.Properties.Settings.PerformanceData.TimeRange = userInputObj.AssessmentDuration.Key;
            obj4.Properties.Settings.NodeTypes = nodeTypes;
            obj4.Properties.Settings.ExternalStorageTypes = externalStorageTypes;
            result.Add(new AssessmentInformation(
                "AVS-Prod-AzMigExport-4",
                AssessmentType.AVSAssessment,
                AssessmentTag.AsOnPremises,
                JsonConvert.SerializeObject(obj4)
            ));

            // As- Onpremises - Pay as you go + RI 1 Year
            AzureVMWareSolutionAssessmentSettingsJSON obj5 = new AzureVMWareSolutionAssessmentSettingsJSON();
            obj5.Properties.Settings.SizingCriterion = "AsOnPremises";
            obj5.Properties.Scope = ScopeDetails;
            obj5.Properties.Settings.SavingsSettings.SavingsOption = "RI1Year";
            obj5.Properties.Settings.Currency = userInputObj.Currency.Key;
            obj5.Properties.Settings.AzureLocation = userInputObj.TargetRegion.Key;
            obj5.Properties.Settings.BillingSettings.SubscriptionId = userInputObj.Subscription.Key;
            obj5.Properties.Settings.PerformanceData.TimeRange = userInputObj.AssessmentDuration.Key;
            obj5.Properties.Settings.NodeTypes = nodeTypes;
            obj5.Properties.Settings.ExternalStorageTypes = externalStorageTypes;
            result.Add(new AssessmentInformation(
                "AVS-Prod-AzMigExport-5",
                AssessmentType.AVSAssessment,
                AssessmentTag.AsOnPremises_RI1Year,
                JsonConvert.SerializeObject(obj5)
            ));

            // As- Onpremises - Pay as you go + RI 3 Year
            AzureVMWareSolutionAssessmentSettingsJSON obj6 = new AzureVMWareSolutionAssessmentSettingsJSON();
            obj6.Properties.Settings.SizingCriterion = "AsOnPremises";
            obj6.Properties.Scope = ScopeDetails;
            obj6.Properties.Settings.SavingsSettings.SavingsOption = "RI3Year";
            obj6.Properties.Settings.Currency = userInputObj.Currency.Key;
            obj6.Properties.Settings.AzureLocation = userInputObj.TargetRegion.Key;
            obj6.Properties.Settings.BillingSettings.SubscriptionId = userInputObj.Subscription.Key;
            obj6.Properties.Settings.PerformanceData.TimeRange = userInputObj.AssessmentDuration.Key;
            obj6.Properties.Settings.NodeTypes = nodeTypes;
            obj6.Properties.Settings.ExternalStorageTypes = externalStorageTypes;
            result.Add(new AssessmentInformation(
                "AVS-Prod-AzMigExport-6",
                AssessmentType.AVSAssessment,
                AssessmentTag.AsOnPremises_RI3Year,
                JsonConvert.SerializeObject(obj6)
            ));

            return result;
        }
    }
}
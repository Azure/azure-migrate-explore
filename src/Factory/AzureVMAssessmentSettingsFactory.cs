// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

using Azure.Migrate.Explore.Common;
using Azure.Migrate.Explore.Models;
using System.Linq;
using System.Text;

namespace Azure.Migrate.Explore.Factory
{
    public class AzureVMAssessmentSettingsFactory
    {
        public List<AssessmentInformation> GetAzureVMAssessmentSettings(UserInput userInputObj, string assessmentType, List<string> scopedMachineIds)
        {
            List<AssessmentInformation> result = new List<AssessmentInformation>();

            if (userInputObj == null)
                throw new Exception("Received invalid null user input.");

            if (string.IsNullOrEmpty(assessmentType))
                throw new Exception("Received invalid assessment type.");

            userInputObj.LoggerObj.LogInformation($"Obtaining Azure VM assessment settings for assessment type {assessmentType}");

            if (assessmentType.Contains("Prod"))
                result = GetAzureVMProdAssessmentSettings(userInputObj, scopedMachineIds);

            else if (assessmentType.Contains("Dev"))
                result = GetAzureVMDevAssessmentSettings(userInputObj, scopedMachineIds);

            if (result.Count <= 0)
                throw new Exception($"Azure VM assessment factory provided no settings for assessment type {assessmentType}.");

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
            
            if (userInputObj.AzureMigrateSourceAppliances.Contains("physical") || userInputObj.AzureMigrateSourceAppliances.Contains("hyperv") || userInputObj.AzureMigrateSourceAppliances.Contains("vmware"))
            {
                sources.Add("\"Appliance\"");
            }
            
            if (userInputObj.AzureMigrateSourceAppliances.Contains("import"))
            {
                sources.Add("\"Import\"");
            }

            return sources.Any() ? string.Join(", ", sources) : "\"Appliance\"";
        }

        private List<AssessmentInformation> GetAzureVMProdAssessmentSettings(UserInput userInputObj, List<string> scopedMachineIds)
        {
            var ScopeDetails = new ScopeDetails();
            if (scopedMachineIds != null && scopedMachineIds.Any())
            {
                ScopeDetails.AzureResourceGraphQuery = GenerateArgQuery(userInputObj, scopedMachineIds);
                ScopeDetails.ScopeType = "AzureResourceGraphQuery";
            }
            List<AssessmentInformation> result = new List<AssessmentInformation>();

            // As on premises - Pay as you go
            AzureVMAssessmentSettingsJSON obj1 = new AzureVMAssessmentSettingsJSON();
            obj1.Properties.Scope = ScopeDetails;
            obj1.Properties.Settings.SizingCriterion = "AsOnPremises";
            obj1.Properties.Settings.BillingSettings.SubscriptionId = userInputObj.Subscription.Key;
            obj1.Properties.Settings.SavingsSettings.AzureOfferCode = "MSAZR0003P";
            obj1.Properties.Settings.SavingsSettings.SavingsOption = "None";
            obj1.Properties.Settings.Currency = userInputObj.Currency.Key;
            obj1.Properties.Settings.AzureLocation = userInputObj.TargetRegion.Key;
            obj1.Properties.Settings.PerformanceData.TimeRange = userInputObj.AssessmentDuration.Key;
            result.Add(new AssessmentInformation("AzureVM-Prod-AzMigExport-1", AssessmentType.MachineAssessment, AssessmentTag.AsOnPremises, JsonConvert.SerializeObject(obj1)));

            // Performance based - Pay as you go
            AzureVMAssessmentSettingsJSON obj2 = new AzureVMAssessmentSettingsJSON();
            obj2.Properties.Scope = ScopeDetails;
            obj2.Properties.Settings.BillingSettings.SubscriptionId = userInputObj.Subscription.Key;
            obj2.Properties.Settings.SavingsSettings.AzureOfferCode = "MSAZR0003P";
            obj2.Properties.Settings.SavingsSettings.SavingsOption = "None";
            obj2.Properties.Settings.Currency = userInputObj.Currency.Key;
            obj2.Properties.Settings.AzureLocation = userInputObj.TargetRegion.Key;
            obj2.Properties.Settings.PerformanceData.TimeRange = userInputObj.AssessmentDuration.Key;
            result.Add(new AssessmentInformation("AzureVM-Prod-AzMigExport-2", AssessmentType.MachineAssessment, AssessmentTag.PerformanceBased, JsonConvert.SerializeObject(obj2)));

            // Performance based - Pay as you go + RI 3 year
            AzureVMAssessmentSettingsJSON obj3 = new AzureVMAssessmentSettingsJSON();
            obj3.Properties.Scope = ScopeDetails;
            obj3.Properties.Settings.BillingSettings.SubscriptionId = userInputObj.Subscription.Key;
            obj3.Properties.Settings.SavingsSettings.SavingsOption = "RI3Year";
            obj3.Properties.Settings.SavingsSettings.AzureOfferCode = "MSAZR0003P";
            obj3.Properties.Settings.Currency = userInputObj.Currency.Key;
            obj3.Properties.Settings.AzureLocation = userInputObj.TargetRegion.Key;
            obj3.Properties.Settings.PerformanceData.TimeRange = userInputObj.AssessmentDuration.Key;
            result.Add(new AssessmentInformation("AzureVM-Prod-AzMigExport-3", AssessmentType.MachineAssessment, AssessmentTag.PerformanceBased_RI3year, JsonConvert.SerializeObject(obj3)));

            // Performance based - Pay as you go + AHUB
            AzureVMAssessmentSettingsJSON obj4 = new AzureVMAssessmentSettingsJSON();
            obj4.Properties.Scope = ScopeDetails;
            obj4.Properties.Settings.BillingSettings.SubscriptionId = userInputObj.Subscription.Key;
            obj4.Properties.Settings.SavingsSettings.SavingsOption = "None";
            obj4.Properties.Settings.SavingsSettings.AzureOfferCode = "MSAZR0003P";
            obj4.Properties.Settings.AzureHybridUseBenefit = "Yes";
            obj4.Properties.Settings.Currency = userInputObj.Currency.Key;
            obj4.Properties.Settings.AzureLocation = userInputObj.TargetRegion.Key;
            obj4.Properties.Settings.PerformanceData.TimeRange = userInputObj.AssessmentDuration.Key;
            result.Add(new AssessmentInformation("AzureVM-Prod-AzMigExport-4", AssessmentType.MachineAssessment, AssessmentTag.PerformanceBased_AHUB, JsonConvert.SerializeObject(obj4)));

            // Performance based - Pay as you go + AHUB + RI 3 year
            AzureVMAssessmentSettingsJSON obj5 = new AzureVMAssessmentSettingsJSON();
            obj5.Properties.Scope = ScopeDetails;
            obj5.Properties.Settings.BillingSettings.SubscriptionId = userInputObj.Subscription.Key;
            obj5.Properties.Settings.SavingsSettings.SavingsOption = "RI3Year";
            obj5.Properties.Settings.SavingsSettings.AzureOfferCode = "MSAZR0003P";
            obj5.Properties.Settings.AzureHybridUseBenefit = "Yes";
            obj5.Properties.Settings.Currency = userInputObj.Currency.Key;
            obj5.Properties.Settings.AzureLocation = userInputObj.TargetRegion.Key;
            obj5.Properties.Settings.PerformanceData.TimeRange = userInputObj.AssessmentDuration.Key;
            result.Add(new AssessmentInformation("AzureVM-Prod-AzMigExport-5", AssessmentType.MachineAssessment, AssessmentTag.PerformanceBased_AHUB_RI3year, JsonConvert.SerializeObject(obj5)));

            // Performance based - Pay as you go + ASP 3 year
            AzureVMAssessmentSettingsJSON obj6 = new AzureVMAssessmentSettingsJSON();
            obj6.Properties.Scope = ScopeDetails;
            obj6.Properties.Settings.BillingSettings.SubscriptionId = userInputObj.Subscription.Key;
            obj6.Properties.Settings.SavingsSettings.SavingsOption = "SavingsPlan3Year";
            obj6.Properties.Settings.SavingsSettings.AzureOfferCode = "SavingsPlan3Year";
            obj6.Properties.Settings.AzureHybridUseBenefit = "Yes";
            obj6.Properties.Settings.Currency = userInputObj.Currency.Key;
            obj6.Properties.Settings.AzureLocation = userInputObj.TargetRegion.Key;
            obj6.Properties.Settings.PerformanceData.TimeRange = userInputObj.AssessmentDuration.Key;
            result.Add(new AssessmentInformation("AzureVM-Prod-AzMigExport-6", AssessmentType.MachineAssessment, AssessmentTag.PerformanceBased_ASP3year, JsonConvert.SerializeObject(obj6)));

            return result;
        }

        private List<AssessmentInformation> GetAzureVMDevAssessmentSettings(UserInput userInputObj, List<string> scopedMachineIds)
        {
            var ScopeDetails = new ScopeDetails();
            if (scopedMachineIds != null && scopedMachineIds.Any())
            {
                ScopeDetails.AzureResourceGraphQuery = GenerateArgQuery(userInputObj, scopedMachineIds);
                ScopeDetails.ScopeType = "AzureResourceGraphQuery";
            }
            List<AssessmentInformation> result = new List<AssessmentInformation>();

            // As on premises - Pay as you go
            AzureVMAssessmentSettingsJSON obj1 = new AzureVMAssessmentSettingsJSON();
            obj1.Properties.Scope = ScopeDetails;
            obj1.Properties.Settings.EnvironmentType = "Test";
            obj1.Properties.Settings.SizingCriterion = "AsOnPremises";
            obj1.Properties.Settings.BillingSettings.SubscriptionId = userInputObj.Subscription.Key;
            obj1.Properties.Settings.SavingsSettings.AzureOfferCode = "MSAZR0023P";
            obj1.Properties.Settings.SavingsSettings.SavingsOption = "None";
            obj1.Properties.Settings.Currency = userInputObj.Currency.Key;
            obj1.Properties.Settings.AzureLocation = userInputObj.TargetRegion.Key;
            obj1.Properties.Settings.PerformanceData.TimeRange = userInputObj.AssessmentDuration.Key;
            result.Add(new AssessmentInformation("AzureVM-Dev-AzMigExport-1", AssessmentType.MachineAssessment, AssessmentTag.AsOnPremises, JsonConvert.SerializeObject(obj1)));

            // Performance based - Pay as you go
            AzureVMAssessmentSettingsJSON obj2 = new AzureVMAssessmentSettingsJSON();
            obj2.Properties.Scope = ScopeDetails;
            obj2.Properties.Settings.EnvironmentType = "Test";
            obj2.Properties.Settings.BillingSettings.SubscriptionId = userInputObj.Subscription.Key;
            obj2.Properties.Settings.SavingsSettings.AzureOfferCode = "MSAZR0023P";
            obj2.Properties.Settings.SavingsSettings.SavingsOption = "None";
            obj2.Properties.Settings.Currency = userInputObj.Currency.Key;
            obj2.Properties.Settings.AzureLocation = userInputObj.TargetRegion.Key;
            obj2.Properties.Settings.PerformanceData.TimeRange = userInputObj.AssessmentDuration.Key;
            result.Add(new AssessmentInformation("AzureVM-Dev-AzMigExport-2", AssessmentType.MachineAssessment, AssessmentTag.PerformanceBased, JsonConvert.SerializeObject(obj2)));

            // Performance based - Pay as you go + AHUB
            AzureVMAssessmentSettingsJSON obj3 = new AzureVMAssessmentSettingsJSON();
            obj3.Properties.Scope = ScopeDetails;
            obj3.Properties.Settings.EnvironmentType = "Test";
            obj3.Properties.Settings.BillingSettings.SubscriptionId = userInputObj.Subscription.Key;
            obj3.Properties.Settings.SavingsSettings.SavingsOption = "None";
            obj3.Properties.Settings.AzureHybridUseBenefit = "Yes";
            obj3.Properties.Settings.Currency = userInputObj.Currency.Key;
            obj3.Properties.Settings.SavingsSettings.AzureOfferCode = "MSAZR0023P";
            obj3.Properties.Settings.AzureLocation = userInputObj.TargetRegion.Key;
            obj3.Properties.Settings.PerformanceData.TimeRange = userInputObj.AssessmentDuration.Key;
            result.Add(new AssessmentInformation("AzureVM-Dev-AzMigExport-3", AssessmentType.MachineAssessment, AssessmentTag.PerformanceBased, JsonConvert.SerializeObject(obj3)));

            return result;
        }
    }
}
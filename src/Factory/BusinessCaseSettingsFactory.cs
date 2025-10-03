// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Azure.Migrate.Explore.Common;
using Azure.Migrate.Explore.Models;

namespace Azure.Migrate.Explore.Factory
{
    public class BusinessCaseSettingsFactory
    {
        public BusinessCaseInformation GetBusinessCaseSettings(UserInput userInputObj, string sessionId, List<string>? scopedMachineIds = null)
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

            // Generate ARG query if scoped machines are provided
            if (scopedMachineIds != null && scopedMachineIds.Any())
            {
                obj.Properties.BusinessCaseScope.AzureResourceGraphQuery = GenerateArgQuery(userInputObj, scopedMachineIds);
                obj.Properties.BusinessCaseScope.ScopeType = "AzureResourceGraphQuery";
                userInputObj.LoggerObj.LogInformation($"Generated scoped ARG query for {scopedMachineIds.Count} machines");
            }

            return new BusinessCaseInformation(obj.Name, JsonConvert.SerializeObject(obj));
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
            
            if (userInputObj.AzureMigrateSourceAppliances.Contains("vmware") || 
                userInputObj.AzureMigrateSourceAppliances.Contains("hyperv") || 
                userInputObj.AzureMigrateSourceAppliances.Contains("physical"))
            {
                sources.Add("\"Appliance\"");
            }
            
            if (userInputObj.AzureMigrateSourceAppliances.Contains("import"))
            {
                sources.Add("\"Import\"");
            }

            return sources.Any() ? string.Join(", ", sources) : "\"Appliance\"";
        }
    }
}
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Azure.Migrate.Explore.Common;
using Azure.Migrate.Explore.Models;
using Microsoft.VisualBasic.ApplicationServices;

namespace Azure.Migrate.Explore.Factory
{
    public class BusinessCaseSettingsFactory
    {
        public BusinessCaseInformation GetBusinessCaseSettings(UserInput userInputObj, string sessionId, List<string>? scopedMachineIds = null, List<string>? discoveryHubIds = null, List<string>? sites = null, List<string>? applicationIds = null, List<string>? workloadIds = null)
        {
            if (userInputObj == null)
                throw new Exception("Received invalid null user input.");

            if (string.IsNullOrEmpty(sessionId))
                throw new Exception("Received invalid session ID.");

            userInputObj.LoggerObj.LogInformation($"Obtaining Business case settings");

            BusinessCaseSettingsJSON obj = new BusinessCaseSettingsJSON();
            obj.Name = "bizcase-ame-" + sessionId;
            obj.Properties.Settings.CommonSettings.TargetLocation = userInputObj.TargetRegion.Key;
            obj.Properties.Settings.CommonSettings.Currency = userInputObj.Currency.Key;
            obj.Properties.Settings.BillingSettings.LicensingProgram = MapProgramOfferToLicensingProgram(userInputObj.ProgramOffer.Key);
            obj.Properties.Settings.BillingSettings.SubscriptionId = userInputObj.EamcaSubscription.Key;

            BusinessCaseTypes type = BusinessCaseTypes.OptimizeForPaas;


            if (userInputObj.PreferredOptimizationObj.OptimizationPreference.Key.Equals("MinimizetimewithAzureVM"))
                type = BusinessCaseTypes.IaaSOnly;
            else if (userInputObj.PreferredOptimizationObj.OptimizationPreference.Key.Equals("MigrateToAvs"))
                type = BusinessCaseTypes.AVSOnly;

            obj.Properties.Settings.CommonSettings.BusinessCaseType = type.ToString();
            obj.Properties.Settings.CommonSettings.WorkloadDiscoverySource = BusinessCaseWorkloadDiscoverySource.Appliance.ToString();
            if (userInputObj.AzureMigrateSourceAppliances.Contains("import"))
                obj.Properties.Settings.CommonSettings.WorkloadDiscoverySource = BusinessCaseWorkloadDiscoverySource.Import.ToString();

            obj.Properties.Settings.AzureSettings.SavingsOption = "RI3Year";
            if (userInputObj.BusinessProposal == BusinessProposal.AVS.ToString())
            {
                obj.Properties.Settings.AzureSettings.SavingsOption = "RI3Year";
                obj.Properties.Settings.AzureSettings.PerYearMigrationCompletionPercentage =
                    AvsAssessmentConstants.perYearMigrationCompletionPercentage;
            }

            // Generate ARG query if scoped machines are provided
            if (scopedMachineIds != null && scopedMachineIds.Any())
            {
                obj.Properties.BusinessCaseScope.AzureResourceGraphQuery = GenerateArgQuery(userInputObj, scopedMachineIds, discoveryHubIds, sites, applicationIds, workloadIds);
                obj.Properties.BusinessCaseScope.ScopeType = "AzureResourceGraphQuery";
                userInputObj.LoggerObj.LogInformation($"Generated scoped ARG query for {scopedMachineIds.Count} machines");
            }

            return new BusinessCaseInformation(obj.Name, JsonConvert.SerializeObject(obj));
        }

        private string GenerateArgQuery(UserInput userInputObj, List<string> scopedMachineIds, List<string>? discoveryHubIds = null, List<string>? sites = null, List<string>? applicationIds = null, List<string>? workloadIds = null)
        {
            try
            {
                // Combine machine IDs and workload IDs
                var allResourceIds = new List<string>(scopedMachineIds);
                if (workloadIds != null && workloadIds.Any())
                {
                    allResourceIds.AddRange(workloadIds);
                    userInputObj.LoggerObj.LogInformation($"Added {workloadIds.Count} workload IDs to scope");
                }

                // Create the resource IDs filter (using discovery machine ARM IDs + workload IDs)
                var resourceIdsList = allResourceIds.Select(id => $"\"{id}\"").ToArray();
                string machineIdsFilter = string.Join(", ", resourceIdsList);

                // Construct the ARG query following the new format
                var argQuery = new StringBuilder();

                // Part 1: Discovery Hub Applications Query (if there are discovery hubs and application IDs)
                if (discoveryHubIds != null && discoveryHubIds.Any() && applicationIds != null && applicationIds.Any())
                {
                    var hubConditions = string.Join(" or ", discoveryHubIds.Select(hub =>
                        $"['id'] has '{hub.Replace("'", "''")}'"));

                    argQuery.Append("(migrateresources\n");
                    argQuery.Append("                | where type =~ \"microsoft.applicationmigration/discoveryhubs/applications\"\n");
                    argQuery.Append($"                | where {hubConditions}\n");
                    argQuery.Append("                | where properties.applicationType has \"\"\n");
                    argQuery.Append("                | extend appId = tolower(id)\n");
                    argQuery.Append("                | where true\n");
                    argQuery.Append("                | join kind = leftouter (migrateresources\n");
                    argQuery.Append("                        | where type =~ \"microsoft.applicationmigration/discoveryhubs/applications/members\"\n");
                    argQuery.Append($"                        | where {hubConditions}\n");
                    argQuery.Append("                        | extend appId = tolower(tostring(split(id,\"/members/\")[0]))\n");
                    argQuery.Append("                        | summarize memberCount = count(), memberResourceIds = make_set(properties.memberResourceId) by appId\n");
                    argQuery.Append("                        )\n");
                    argQuery.Append("                        on $left.appId == $right.appId\n");
                    argQuery.Append("                        | project armId = tolower(id), id = tolower(id), type, appId, memberCount, memberResourceIds, properties, name, systemData.CreatedAt");

                    // Filter by application IDs
                    var appIdsList = applicationIds.Select(id => $"\"{id}\"").ToArray();
                    string appIdsFilter = string.Join(", ", appIdsList);
                    argQuery.Append($" | where id in~ ({appIdsFilter})");

                    // Add union for machine resources
                    argQuery.Append(" | union (migrateresources\n");
                }
                else
                {
                    argQuery.Append("(migrateresources\n");
                }

                // Part 2: Machines/Resources Query
                argQuery.Append("        | where type in (\"microsoft.offazure/vmwaresites/machines\", \"microsoft.offazure/serversites/machines\", \"microsoft.offazure/hypervsites/machines\", \"microsoft.offazure/importsites/machines\", \"microsoft.offazure/mastersites/sqlsites/sqlservers\", \"microsoft.applicationmigration/pgsqlsites/pgsqlinstances\", \"microsoft.offazure/mastersites/webappsites/iiswebapplications\", \"microsoft.offazure/mastersites/webappsites/tomcatwebapplications\", \"microsoft.offazure/importsites/machines\")\n");
                if (sites != null && sites.Any())
                {
                    var siteConditions = string.Join(" or ", sites.Select(site =>
                        $"id has '{site.Replace("'", "''")}'"));
                    argQuery.Append($"        | where {siteConditions}\n");
                }
                else
                {
                    argQuery.Append($"        | where id has \"/subscriptions/{userInputObj.Subscription.Key}/resourceGroups/{userInputObj.ResourceGroupName.Value}\"\n");
                }

                argQuery.Append("        | extend type=tolower(type)\n");
                argQuery.Append("        | extend id = tolower(id)\n");
                argQuery.Append("        | join kind = leftouter (\n");
                argQuery.Append("            migrateresources\n");
                argQuery.Append("            | where type =~ \"microsoft.applicationmigration/discoveryhubs/applications/members\"\n");

                // Filter by sites that don't contain "mastersites"
                if (sites != null && sites.Any())
                {
                    var nonMasterSites = sites.Where(site => !site.Contains("mastersites", StringComparison.OrdinalIgnoreCase)).ToList();
                    if (nonMasterSites.Any())
                    {
                        var siteConditions = string.Join(" or ", nonMasterSites.Select(site =>
                            $"properties.memberResourceId has '{site.Replace("'", "''")}'"));
                        argQuery.Append($"            | where {siteConditions}\n");
                    }
                    else
                    {
                        // If all sites are mastersites, use subscription/resource group filter
                        argQuery.Append($"            | where properties.memberResourceId has \"/subscriptions/{userInputObj.Subscription.Key}/resourceGroups/{userInputObj.ResourceGroupName.Value}\"\n");
                    }
                }
                else
                {
                    argQuery.Append($"            | where properties.memberResourceId has \"/subscriptions/{userInputObj.Subscription.Key}/resourceGroups/{userInputObj.ResourceGroupName.Value}\"\n");
                }

                argQuery.Append("            | extend memberResourceId = tolower(properties.memberResourceId)\n");
                argQuery.Append("            | parse kind=regex id with applicationId '/members/'\n");
                argQuery.Append("            | project memberResourceId, applicationId\n");
                argQuery.Append("            )\n");
                argQuery.Append("            on $left.id == $right.memberResourceId\n");
                argQuery.Append("        | summarize applicationId = strcat_array(make_set(applicationId), \", \"), properties = take_any(properties), type = take_any(type) by id\n");
                argQuery.Append("        | extend properties_machineArmIds = iif(array_length(properties.machineArmIds) == 0, pack_array(id), properties.machineArmIds)\n");
                argQuery.Append("        | mv-expand properties_machineArmIds\n");
                argQuery.Append("        | extend machineArmIds=tostring(properties_machineArmIds)\n");
                argQuery.Append("        | extend parentId = case(type contains \"/machines\", id, machineArmIds)\n");
                argQuery.Append("        | extend id = tolower(id), siteId = case(id has \"machines\", tostring(split(tolower(id),\"/machines/\")[0]), id has \"sqlsites\", tostring(split(tolower(id),\"/sqlsites/\")[0]), id has \"pgsqlsites\", tostring(split(tolower(id),\"/pgsqlsites/\")[0]), id has \"webappsites\", tostring(split(tolower(id),\"/webappsites/\")[0]), \"\")\n");
                argQuery.Append("        | extend parentId = tolower(parentId),\n");
                argQuery.Append("            armId = id,\n");
                argQuery.Append("            resourceType = type,\n");
                argQuery.Append("            host = case(\n");
                argQuery.Append("            id contains \"microsoft.offazure/vmwaresites\", \"VMware\",\n");
                argQuery.Append("            id contains \"microsoft.offazure/hypervsites\", \"Hyper-V\",\n");
                argQuery.Append("            id contains \"microsoft.offazure/serversites\", \"Physical\",\n");
                argQuery.Append("            id contains \"microsoft.offazure/importsites\" and properties.hypervisor =~ \"VMWare\", \"VMware\",\n");
                argQuery.Append("            id contains \"microsoft.offazure/importsites\" and strlen(properties.hypervisor) > 0 and properties.hypervisor !~ \"VMWare\", properties.hypervisor,\n");
                argQuery.Append("            \"-\"),\n");
                argQuery.Append("            resourceTags = properties.tags,\n");
                argQuery.Append("            resourceName = tostring(case(type contains \"/sqlservers\", properties.sqlServerName, case(type contains \"/pgsqlinstances\", strcat(properties.hostName, \":\", properties.portNumber), properties.displayName))),\n");
                argQuery.Append("            osName = tostring(case(id has \"/machines/\", coalesce(properties.guestOSDetails.osName, properties.operatingSystemDetails.osName), id has \"/sqlsites/\", \"\", id has \"/webappsites/\", properties.version, properties.version)),\n");
                argQuery.Append("            databaseEdition = tostring(case(id has \"/machines/\", \"-\", id has \"/sqlsites/\", properties.edition, id has \"/webappsites/\", properties.version, properties.edition)),\n");
                argQuery.Append("            version = tostring(case(id has \"/machines/\", coalesce(properties.guestOSDetails.osName, properties.operatingSystemDetails.osName), id has \"/sqlsites/\", \"\", id has \"/webappsites/\" or id has \"/pgsqlsites/\", properties.version, \"\")),\n");
                argQuery.Append("            edition = tostring(case(id has \"/machines/\", coalesce(properties.guestOSDetails.osVersion, properties.operatingSystemDetails.osVersion), id has \"/sqlsites/\", properties.edition, id has \"/pgsqlsites/\", properties.edition, id has \"/webappsites/\", properties.version, \"\")),\n");
                argQuery.Append("            osType = tostring(coalesce(properties.guestOSDetails.osType, properties.operatingSystemDetails.osType)),\n");
                argQuery.Append("            osArchitecture = tostring(coalesce(properties.guestOSDetails.osArchitecture, properties.operatingSystemDetails.osArchitecture)),\n");
                argQuery.Append("            powerOnStatus = case(properties.powerStatus == \"ON\" or properties.powerStatus == \"Running\", \"On\", properties.powerStatus == \"OFF\" or properties.powerStatus == \"PowerOff\" or properties.powerStatus == \"Saved\" or properties.powerStatus == \"Paused\", \"Off\", \"-\"),\n");
                argQuery.Append("            source = case(type contains \"vmwaresites\", properties.vCenterFQDN, type contains \"hypervsites\", coalesce(properties.clusterFqdn, properties.hostFqdn), \"\"),\n");
                argQuery.Append("            numberOfUserDatabases = tostring(case(id has \"/sqlsites/\", properties.numberOfUserDatabases, \"\")),\n");
                argQuery.Append("            discoverySource = case(id contains \"microsoft.offazure/importsites\", \"Import\", id contains \"/sqlsites/\" and properties.discoveryState == \"Imported\", \"Import\", id contains \"/pgsqlsites/\" and properties.discoveryState == \"Imported\", \"Import\", \"Appliance\"),\n");
                argQuery.Append("            dbProperties = case(id has \"/sqlsites/\", properties, id has \"/pgsqlsites/\", properties, parse_json(\"\")),\n");
                argQuery.Append("            dbEngineStatus =  tostring(case(id has \"/sqlsites/\", properties.status, id has \"/pgsqlsites/\", properties.status, \"\")),\n");
                argQuery.Append("            userdatabases = tostring(case(id has \"/sqlsites/\", properties.numberOfUserDatabases, id has \"/pgsqlsites/\", properties.numberOfUserDatabases, \"\")),\n");
                argQuery.Append("            totalSizeInGB =  properties.totalDiskSizeInGB,\n");
                argQuery.Append("            ipAddressList = properties.ipAddresses,\n");
                argQuery.Append("            totalWebAppCount = tolong(case(id has \"/machines/\", case(coalesce(tolong(properties.webAppDiscovery.totalWebApplicationCount), 0) == 0, coalesce(tolong(properties.iisDiscovery.totalWebApplicationCount), 0) + coalesce(tolong(properties.tomcatDiscovery.totalWebApplicationCount), 0), coalesce(tolong(properties.webAppDiscovery.totalWebApplicationCount), 0)), 0)),\n");
                argQuery.Append("            totalDatabaseInstances = tolong(case(id has \"/machines/\", coalesce(tolong(properties.totalInstanceCount), 0), 0)),\n");
                argQuery.Append("            memoryInMB = case(id has \"/sqlsites/\", tolong(properties.maxServerMemoryInUseInMb), id has \"/pgsqlsites/\", tolong(properties.maxServerMemoryInUseInMb), tolong(properties.allocatedMemoryInMB)),\n");
                argQuery.Append("            serverName = tostring(case(type contains \"/sqlservers\", properties.machineOverviewList[0].displayName, type has \"/webappsites/\", properties.machineDisplayName, \"\")),\n");
                argQuery.Append("            frameworkVersion = tostring(case(id has \"/webappsites/\", properties.frameworks[0].version, \"-\")),\n");
                argQuery.Append("            dbhadrConfiguration = tostring(case(id has \"/sqlsites/\", (case(toboolean(properties.isClustered) and toboolean(properties.isHighAvailabilityEnabled), \"Both\", case(toboolean(properties.isClustered), \"FailoverClusterInstance\", case(toboolean(properties.isHighAvailabilityEnabled), \"AvailabilityGroup\", \"\")))), id has \"/pgsqlsites/\", (case(toboolean(properties.isClustered) and toboolean(properties.isHighAvailabilityEnabled), \"Both\", case(toboolean(properties.isClustered), \"FailoverClusterInstance\", case(toboolean(properties.isHighAvailabilityEnabled), \"AvailabilityGroup\", \"\")))), \"\")),\n");
                argQuery.Append("            diskCount = array_length(properties.disks),\n");
                argQuery.Append("            supportEndsIn= datetime_diff(\"day\", todatetime(properties.productSupportStatus.supportEndDate), todatetime(now())),\n");
                argQuery.Append("            depmapErrorCount = array_length(properties.dependencyMapDiscovery.errors),\n");
                argQuery.Append("            numberOfSoftware=tolong(properties.numberOfSoftware),\n");
                argQuery.Append($"            numberOfSecurityRisks=tolong(properties.numberOfSecurityRisks) | where id in~ ({machineIdsFilter})");

                // Close the union if we have discovery hubs and applications
                if (discoveryHubIds != null && discoveryHubIds.Any() && applicationIds != null && applicationIds.Any())
                {
                    argQuery.Append(")");
                }

                argQuery.Append(") | sort by id");

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

        private static string MapProgramOfferToLicensingProgram(string programOfferKey)
        {
            if (string.IsNullOrWhiteSpace(programOfferKey))
                return "Retail";

            return programOfferKey.ToLowerInvariant() switch
            {
                "payasyougo" => "Retail",
                "enterpriseagreementsupport" => "EA",
                "microsoftcustomeragreement" => "MCA",
                _ => "Retail"
            };
        }
    }
}

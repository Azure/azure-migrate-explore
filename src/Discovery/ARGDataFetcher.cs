using Azure.Migrate.Explore.Common;
using Azure.Migrate.Explore.HttpRequestHelper;
using Azure.Migrate.Explore.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace AzureMigrateExplore.Discovery
{
    public class ARGDataFetcher
    {
        public static string AllInventoryWithWebApps = @"
                migrateresources
                | where ['type'] in~ (""microsoft.offazure/vmwaresites/machines"",
                    ""microsoft.offazure/hypervsites/machines"",""microsoft.offazure/serversites/machines"",
                    ""microsoft.offazure/importsites/machines"",""microsoft.offazure/mastersites/sqlsites/sqlservers"",
                    ""microsoft.offazure/mastersites/webappsites/iiswebapplications"",""Microsoft.ApplicationMigration/PGSQLSites/PGSQLInstances"",
                    ""microsoft.offazure/mastersites/webappsites/tomcatwebapplications"")
                | where {0}
                | extend type=tolower(type)
                | extend id = tolower(id)
                | extend properties_webServerId = tostring(properties.webServerId)
                | join kind = leftouter (
                    migrateresources
                    | where type =~ ""microsoft.applicationmigration/discoveryhubs/applications/members""
                    | where {1}
                    | extend memberResourceId = tolower(properties.memberResourceId)
                    | parse kind=regex id with applicationId '/members/'
                    | project memberResourceId, applicationId
                    )
                on $left.id == $right.memberResourceId
                | summarize applicationId = strcat_array(make_set(applicationId), "", ""), properties = take_any(properties), type = take_any(type), take_any(properties_webServerId) by id
                | extend properties_machineArmIds = iif(array_length(properties.machineArmIds) == 0, pack_array(id), properties.machineArmIds)
                | mv-expand properties_machineArmIds
                | extend machineArmIds=tostring(properties_machineArmIds)
                | extend parentId = case(type contains ""/machines"", id, machineArmIds)
                // webapp
                | join kind = leftouter (
                    migrateresources
                    | where type in (""microsoft.offazure/mastersites/webappsites/iiswebservers"", ""microsoft.offazure/mastersites/webappsites/tomcatwebservers"")
                    | project ['id'], properties.version, properties.serverType
                    | project-rename webServerId = ['id'], webServerVersion = properties_version, webServerType = properties_serverType
                )
                on $left.properties_webServerId == $right.webServerId
                //webapp
                | extend id = tolower(id), siteId = case(id has ""machines"", tostring(split(tolower(id),""/machines/"")[0]), id has ""sqlsites"", tostring(split(tolower(id),""/sqlsites/"")[0]), id has ""webappsites"", tostring(split(tolower(id),""/webappsites/"")[0]), """")
                | extend parentId = tolower(parentId),
                    armId = id,
                    resourceType = type,
                    resourceTags = properties.tags,
                    resourceName = tostring(case(type contains ""/sqlservers"", properties.sqlServerName, case(type contains ""/pgsqlinstances"", strcat(properties.hostName, "":"", properties.portNumber), properties.displayName))),
                    version = tostring(case(id has ""/machines/"", coalesce(properties.guestOSDetails.osName, properties.operatingSystemDetails.osName), id has ""/sqlsites/"", """", id has ""/webappsites/"", properties.version, properties.version)),
                    edition = tostring(case(id has ""/machines/"", coalesce(properties.guestOSDetails.osVersion, properties.operatingSystemDetails.osVersion), id has ""/sqlsites/"", properties.edition, id has ""/webappsites/"", properties.version, properties.edition)),
                    osType = tostring(coalesce(properties.guestOSDetails.osType, properties.operatingSystemDetails.osType)),
                    powerOnStatus = case(properties.powerStatus == ""ON"" or properties.powerStatus == ""Running"", ""On"", properties.powerStatus == ""OFF"" or properties.powerStatus == ""PowerOff"" or properties.powerStatus == ""Saved"" or properties.powerStatus == ""Paused"", ""Off"", ""-""),
                    discoverySource = case(id contains ""microsoft.offazure/importsites"", ""Import"", id contains ""/sqlsites/"" and properties.discoveryState == ""Imported"", ""Import"", ""Appliance""),
                    dbProperties = case(id has ""/sqlsites/"", properties, parse_json("""")),
                    dbEngineStatus =  tostring(case(id has ""/sqlsites/"" or id has ""/pgsqlinstances/"", properties.status, """")),
                    userdatabases = tostring(case(id has ""/sqlsites/"", properties.numberOfUserDatabases, case(id has ""/pgsqlinstances/"", properties.numberOfDatabase, """"))),
                    totalSizeInGB =  properties.totalDiskSizeInGB,
                    ipAddressList = properties.ipAddresses,
                    totalWebAppCount = tolong(case(id has ""/machines/"", case(coalesce(tolong(properties.webAppDiscovery.totalWebApplicationCount), 0) == 0, coalesce(tolong(properties.iisDiscovery.totalWebApplicationCount), 0) + coalesce(tolong(properties.tomcatDiscovery.totalWebApplicationCount), 0), coalesce(tolong(properties.webAppDiscovery.totalWebApplicationCount), 0)), 0)),
                    totalDatabaseInstances = tolong(case(id has ""/machines/"", coalesce(tolong(properties.totalInstanceCount), 0), 0)),
                    memoryInMB = case(id has ""/sqlsites/"", tolong(properties.maxServerMemoryInUseInMb), case(id has ""/pgsqlinstances/"", tolong(properties.hostMachineProperties.allocatedMemoryInMb), tolong(properties.allocatedMemoryInMB))),
                    dbhadrConfiguration = tostring(case(id has ""/sqlsites/"", (case(toboolean(properties.isClustered) and toboolean(properties.isHighAvailabilityEnabled), ""Both"", case(toboolean(properties.isClustered), ""FailoverClusterInstance"", case(toboolean(properties.isHighAvailabilityEnabled), ""AvailabilityGroup"", """")))), """")),
                    diskCount = array_length(properties.disks),
                    supportEndsIn = case(datetime_diff(""day"", todatetime(properties.productSupportStatus.supportEndDate), todatetime(now())) < 0, "" "",tostring(datetime_diff(""day"", todatetime(properties.productSupportStatus.supportEndDate), todatetime(now())))),
                    depmapErrorCount = array_length(properties.dependencyMapDiscovery.errors),
                    source = case(['type'] contains ""vmwaresites"", properties.vCenterFQDN, ['type'] contains ""hypervsites"", coalesce(properties.clusterFqdn, properties.hostFqdn), """"),
                    numberOfSecurityRisks = properties.numberOfSecurityRisks,
                    numberOfApplications = properties.numberOfApplications
                | order by tolower(resourceName) asc
                | project id, parentId, resourceName, resourceType, edition, version, properties.dependencyMapping, properties.productSupportStatus.supportStatus, discoverySource, source, properties.tags, properties, dbProperties, properties.numberOfProcessorCore, memoryInMB, diskCount, totalSizeInGB, osType, supportEndsIn, powerOnStatus, siteId, dbProperties.status, dbProperties.numberOfUserDatabases, dbhadrConfiguration, depmapErrorCount, properties.dependencyMapDiscovery.discoveryScopeStatus, properties.autoEnableDependencyMapping, ipAddressList, totalDatabaseInstances, totalWebAppCount, webServerId, webServerVersion, webServerType
                | project-rename armId=id, dependencyMapping=properties_dependencyMapping, supportStatus=properties_productSupportStatus_supportStatus, resourceTags=properties_tags, cores=properties_numberOfProcessorCore, dbEngineStatus=dbProperties_status, userdatabases=dbProperties_numberOfUserDatabases, depMapDiscoveryScopeStatus=properties_dependencyMapDiscovery_discoveryScopeStatus, autoEnableDependencyMapping=properties_autoEnableDependencyMapping";

        public static async Task<string> GetAllInventoryDataFromDiscoveryAsync(
            UserInput userInputObj,
            string[] subscriptions,
            List<string> siteUrls)
        {
            var idHas = string.Join(" or ", siteUrls.Select(id => $"id has \"{id}\""));
            var memberIdHas = string.Join(" or ", siteUrls.Select(id => $"properties.memberResourceId has \"{id}\""));

            var query = string.Format(AllInventoryWithWebApps, idHas, memberIdHas);
            var argPayload = CreateArgPayload(subscriptions, query);

            try
            {
                // Execute query
                var httpHelper = new HttpClientHelper();
                HttpResponseMessage response = await httpHelper.GetHttpResponseForARGQuery(userInputObj, argPayload);

                if (!response.IsSuccessStatusCode)
                {
                    string errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception($"ARG query failed: {response.StatusCode}: {errorContent}");
                }

                // Parse response
                string jsonResponse = await response.Content.ReadAsStringAsync();
                return jsonResponse;
            }
            catch (Exception ex)
            {
                // Treating as non-fatal for now.
                userInputObj.LoggerObj?.LogError($"Failed to execute ARG Query: {ex.Message}");
                userInputObj.LoggerObj?.LogDebug($"Full exception details: {ex}");
            }

            return string.Empty;
        }

        private static string CreateArgPayload(string[] subscriptions, string query)
        {
            var payload = new
            {
                subscriptions = subscriptions,
                query = query
            };
            return JsonConvert.SerializeObject(payload);
        }
    }
}

using Azure.Migrate.Explore.Common;
using Azure.Migrate.Explore.HttpRequestHelper;
using Azure.Migrate.Explore.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
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

        private static string WebAppTomCatSupportStatus = @"
                migrateresources
                | where type contains ""microsoft.offazure/mastersites/webappsites/tomcatwebservers""
                | where {0}
                | extend webApps = properties.webApplications
                | extend normalizedJvmVersion = extract(@""(\d+\.\d+\.\d+)"", 1, tostring(properties.jvmVersion))
                | project serverId = id, serverName = name,
                          serverType = properties.serverType,
                          tomcatVersion = properties.version,
                          jvmVersion = normalizedJvmVersion,  machineId = tostring(split(properties.machineIds[0], ""/"")[10]),
                          webApps
                | mv-expand webAppId = webApps
                | extend webAppId = coalesce(tostring(webAppId['id']), tostring(webAppId))
                | where isnotempty(webAppId)
                | join kind=leftouter (
                    migrateresources
                    | where type contains ""microsoft.offazure/mastersites/webappsites/tomcatwebapplications""
                    | extend webAppId = id
                    | project webAppId, webAppName = name,
                              displayName = properties.displayName,
                              webServerName = properties.webServerName,
                              frameworks = properties.frameworks
                ) on webAppId
                | join kind=leftouter (
                    machinesinventoryinsightsresources
                    | where type =~ ""microsoft.offazure/vmwaresites/machines/inventoryinsights/software""
                    | where {1} // Update with customer subscription
                    | extend softwareType = case(
                        properties.softwareName has ""tomcat"", ""Tomcat"",
                        properties.softwareName has ""Jre-"", ""JVM"",
                        ""Other""
                    )
                    | where softwareType in (""Tomcat"", ""JVM"")
                    | extend normalizedJvmVersion = extract(@""(\d+\.\d+\.\d+)"", 1, tostring(properties.version))
                    | project machineId = tostring(split(id, ""/"")[10]), softwareType,
                              supportStatus = properties.supportStatus,
                              version = tostring(properties.version),
                              softwareName = tostring(properties.softwareName),
                              providerName =  tostring(properties.provider)
                ) on $left.machineId  == $right.machineId
                | summarize tomcatSupportStatus = anyif(supportStatus, softwareType == ""Tomcat"" and version == tostring(tomcatVersion)),
                            jvmSupportStatus = anyif(supportStatus, softwareType == ""JVM"" and version == tostring(jvmVersion))
                            by serverId, webAppId, serverName, tostring(serverType), tostring(tomcatVersion), jvmVersion, webAppName, tostring(displayName), tostring(webServerName), tostring(frameworks), machineId, tostring(providerName)
                | extend tomcatSupportStatus = case(
                    isnotempty(tomcatSupportStatus), tomcatSupportStatus,
                    tomcatVersion startswith ""9."", ""Supported"",
                    tomcatVersion startswith ""10.1"", ""Supported"",
                    tomcatVersion startswith ""10.0"", ""NotSupported"",
                    tomcatVersion startswith ""11.0"", ""Supported"",
                    tomcatVersion startswith ""8."", ""NotSupported"",
                    tomcatVersion startswith ""7."", ""NotSupported"",
                    tomcatVersion startswith ""6."", ""NotSupported"",
                    tomcatVersion startswith ""5."", ""NotSupported"",
                    ""Unknown""
                )
                | project serverId, webAppId, serverName, webAppName, serverType, displayName, tomcatVersion, jvmVersion, machineId, providerName, tomcatSupportStatus, jvmSupportStatus, IISSupportStatus=""NA"", IISframeworkName=""NA"", IISframeworkVersion=""NA""";

        private static string WebAppIISSupportStatus = @"
                migrateresources
                | where type contains ""microsoft.offazure/mastersites/webappsites/iiswebservers""
                | where {0}
                | extend serverId = id
                | project serverId, serverName = name, serverVersion = properties.version, displayName = properties.displayName
                // Join with IIS Web Applications
                | join kind=leftouter (
                    migrateresources
                    | where type contains ""microsoft.offazure/mastersites/webappsites/iiswebapplications""
                    | where {1} // Update with customer subscription
                    | extend serverId = tostring(properties.webServerId)
                    | project serverId, webAppId = id, webAppName = name, displayNameApp = properties.displayName, serverType = properties.serverType, frameworks = properties.frameworks, properties.machineArmIds,  machineId = tostring(split(properties.machineArmIds[0], ""/"")[10])
                ) on serverId
                | join kind=leftouter (
                    machinesinventoryinsightsresources
                    | where type =~ ""microsoft.offazure/hypervsites/machines/inventoryinsights/software"" // change it to respective fabric site
                    | where {2} // Update with customer subscription
                    | extend softwareType = case(
                        properties.softwareName contains "".net framework"", ""NETRUNTIME"",
                        ""Other""
                    )
                   | where softwareType in (""NETRUNTIME"")
                   | project machineId = tostring(split(id, ""/"")[10]), softwareType,
                              supportStatus = properties.supportStatus,
                              version = tostring(properties.version),
                              softwareName = tostring(properties.softwareName),
                              providerName =  tostring(properties.provider)
                ) on $left.machineId  == $right.machineId
                | extend frameworkName = tostring(frameworks[0].name),
                         frameworkVersion = tostring(frameworks[0].version),
                         inventoryMajor = todouble(extract(@""^(\d+\.\d+)"", 1, version))
                | extend normalizedSupportStatus = iff(supportStatus == ""Unknown"" or isempty(supportStatus), """", supportStatus)      
                | extend supportStatus = case(
                    isnotempty(normalizedSupportStatus), normalizedSupportStatus,
                    frameworkName contains "".NET"" and frameworkVersion contains ""4.0"" and inventoryMajor > 5.0, ""Supported"",
                    frameworkName contains "".NET"" and frameworkVersion contains ""4.0"" and tostring(version) contains ""4.6.2"", ""Supported"",
                    frameworkName contains "".NET"" and frameworkVersion contains ""4.0"" and inventoryMajor > 4.6, ""Supported"",
                    frameworkName contains "".NET"" and frameworkVersion contains ""4.0"" and (inventoryMajor < 4.0 or inventoryMajor == 4.0 or inventoryMajor == 4.5 or inventoryMajor < 4.6), ""NotSupported"",
                    frameworkName contains "".NET"" and frameworkVersion startswith ""3."" , ""NotSupported"",
                    frameworkName contains "".NET"" and frameworkVersion startswith ""2."" , ""NotSupported"",
                    ""Unknown""
                )
                | project serverId, webAppId, serverName, webAppName, serverType, displayName, tomcatVersion=""NA"", jvmVersion=""NA"", machineId, providerName, tomcatSupportStatus=""NA"", jvmSupportStatus=""NA"", IISSupportStatus=supportStatus, IISframeworkName=frameworkName, IISframeworkVersion=frameworkVersion";

        private const string SoftwareInventoryInsights = @"
                machinesinventoryinsightsresources
                | where type in~ (
                    ""Microsoft.OffAzure/vmwareSites/machines/inventoryInsights/software"",
                    ""Microsoft.OffAzure/hypervSites/machines/inventoryInsights/software"",
                    ""Microsoft.OffAzure/serverSites/machines/inventoryInsights/software""
                )
                | where {0}
                | parse id with machineId ""/inventoryInsights/default"" *
                | extend Category = tostring(properties.category),
                    Software = tostring(properties.softwareName),
                    Version = tostring(properties.version),
                    SupportStatus = tostring(properties.supportStatus),
                    PotentialTargets = tostring(properties.potentialTargets)
                | where isnotempty(Category) and isnotempty(Software)
                | where Category in ('Security & Compliance', 'Monitoring & Operations', 'Infrastructure Management', 'Productivity & Collaboration', 'Business Applications')
                | summarize Coverage = dcount(machineId), Machine_SoftwareEoS = dcountif(machineId, SupportStatus =~ ""End of Support"") by Category, Software, PotentialTargets
                | extend category_rank = case(
                    Category == ""Security & Compliance"", 0,
                    Category == ""Monitoring & Operations"", 1,
                    Category == ""Infrastructure Management"", 2,
                    Category == ""Productivity & Collaboration"", 3,
                    Category == ""Business Applications"", 4,
                    5
                )
                | where category_rank != 5
                | sort by category_rank asc, Coverage desc
                | extend rank = row_number(1, prev(Category) != Category)
                | where rank <= 10
                | extend internal_rank = iif(rank <= 2, (2 * category_rank) + rank, (10 * (category_rank + 1)) + rank -2)
                | order by internal_rank asc
                | top 10 by internal_rank asc
                | project Category, category_rank, Software, Coverage, PotentialTargets, Machine_SoftwareEoS, internal_rank
                | extend Machine_SoftwareEoS_Percentage = (Machine_SoftwareEoS/Coverage)*100
                | sort by category_rank asc, Coverage desc";

        public static async Task<string> GetSoftwareInventoryInsightsAsync(
            UserInput userInputObj,
            string[] subscriptions,
            List<string> siteUrls)
        {
            var idHas = string.Join(" or ", siteUrls.Select(id => $"id has \"{id}\""));

            var query = string.Format(SoftwareInventoryInsights, idHas);
            var argPayload = CreateArgPayload(subscriptions, query);

            try
            {
                // Execute query
                var httpHelper = new HttpClientHelper();
                string jsonResponse = await httpHelper.GetHttpResponseForARGQueryWithPagination(userInputObj, argPayload);
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


        public static async Task<string> GetWebAppSupportStatusAsync(
            UserInput userInputObj,
            string[] subscriptions,
            List<string> webAppSiteUrls)
        {
            string webAppIIS = await GetWebAppIISSupportStatusAsync(userInputObj, subscriptions, webAppSiteUrls);
            string webAppTomcat = await GetWebAppTomCatSupportStatusAsync(userInputObj, subscriptions, webAppSiteUrls);

            if(string.IsNullOrEmpty(webAppIIS) && string.IsNullOrEmpty(webAppTomcat))
            {
                return string.Empty;
            }

            if (string.IsNullOrEmpty(webAppIIS))
            {
                return webAppTomcat;
            }

            if (string.IsNullOrEmpty(webAppTomcat))
            {
                return webAppIIS;
            }

            try
            {
                // Parse the JSON into JObject
                JObject obj1 = JObject.Parse(webAppIIS);
                JObject obj2 = JObject.Parse(webAppTomcat);

                // Get the 'data' arrays
                JArray data1 = (JArray)obj1["data"];
                JArray data2 = (JArray)obj2["data"];

                data1.Merge(data2);
                obj1["totalRecords"] = data1.Count();
                obj1["count"] = data1.Count();

                return obj1.ToString();
            }
            catch (Exception ex)
            {
                userInputObj.LoggerObj?.LogError($"Failed to merge WebApp supportability data from ARG: {ex.Message}");
                userInputObj.LoggerObj?.LogDebug($"Full exception details: {ex}");
            }

            return string.Empty;
        }

        public static async Task<string> GetWebAppTomCatSupportStatusAsync(
            UserInput userInputObj,
            string[] subscriptions,
            List<string> webAppSiteUrls)
        {
            var idHasSub = string.Join(" or ", subscriptions.Select(id => $"id has \"{id}\""));
            var idHas = string.Join(" or ", webAppSiteUrls.Select(id => $"id has \"{id}\""));

            var query = string.Format(WebAppTomCatSupportStatus, idHas, idHasSub);
            var argPayload = CreateArgPayload(subscriptions, query);

            try
            {
                // Execute query
                var httpHelper = new HttpClientHelper();
                string jsonResponse = await httpHelper.GetHttpResponseForARGQueryWithPagination(userInputObj, argPayload);
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

        public static async Task<string> GetWebAppIISSupportStatusAsync(
            UserInput userInputObj,
            string[] subscriptions,
            List<string> webAppSiteUrls)
        {
            var idHasSub = string.Join(" or ", subscriptions.Select(id => $"id has \"{id}\""));
            var idHas = string.Join(" or ", webAppSiteUrls.Select(id => $"id has \"{id}\""));

            var query = string.Format(WebAppIISSupportStatus, idHas, idHasSub, idHasSub);
            var argPayload = CreateArgPayload(subscriptions, query);

            try
            {
                // Execute query
                var httpHelper = new HttpClientHelper();
                string jsonResponse = await httpHelper.GetHttpResponseForARGQueryWithPagination(userInputObj, argPayload);
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
                string jsonResponse = await httpHelper.GetHttpResponseForARGQueryWithPagination(userInputObj, argPayload);
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

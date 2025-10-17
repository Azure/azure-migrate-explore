using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.Identity.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.ComponentModel;
using System.Reflection.PortableExecutable;
using Windows.ApplicationModel.Background;
using Windows.Media.AppBroadcasting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Azure.Migrate.Explore.HttpRequestHelper;
using Azure.Migrate.Explore.Models;
using AzureMigrateExplore.Models;

namespace AzureMigrateExplore.Assessment
{
    public class ARGQueryBuilder
    {
        // Centralized configuration for supported resource types
        private static readonly Dictionary<string, string> SupportedResourceTypes = new Dictionary<string, string>
        {
            // Server category - machine sites
            { "microsoft.offazure/vmwaresites/machines", "Server" },
            { "microsoft.offazure/serversites/machines", "Server" },
            { "microsoft.offazure/hypervsites/machines", "Server" },
            { "microsoft.offazure/importsites/machines", "Server" },
            
            // Database category - SQL sites
            { "microsoft.offazure/mastersites/sqlsites/sqlservers", "Database" },
            
            // Web Application category - web app sites
            { "microsoft.offazure/mastersites/webappsites/iiswebapplications", "Web Application" },
            { "microsoft.offazure/mastersites/webappsites/tomcatwebapplications", "Web Application" }
        };

        // Helper methods to get resource type lists
        private static string GetResourceTypesForQuery()
        {
            return string.Join(", ", SupportedResourceTypes.Keys.Select(k => $"\"{k}\""));
        }

        private static string GetInventoryInsightsTypesForQuery()
        {
            var inventoryInsightsTypes = SupportedResourceTypes.Keys.Select(type => 
                type.Replace("/machines", "/machines/inventoryinsights")
                    .Replace("/sqlservers", "/sqlservers/inventoryinsights")
                    .Replace("/iiswebapplications", "/iiswebapplications/inventoryinsights")
                    .Replace("/tomcatwebapplications", "/tomcatwebapplications/inventoryinsights"));
            
            return string.Join(" or ", inventoryInsightsTypes.Select(t => $"type =~ \"{t}\""));
        }

        private static string GetCategoryFromResourceType(string resourceType)
        {
            if (string.IsNullOrEmpty(resourceType)) return "Server";
            
            var normalizedType = resourceType.ToLower();
            var matchingType = SupportedResourceTypes.FirstOrDefault(kvp => 
                normalizedType.Contains(kvp.Key.ToLower()));
            
            return matchingType.Key != null ? matchingType.Value : "Server";
        }

        private static string GetTypeFromResourceType(string resourceType)
        {
            if (string.IsNullOrEmpty(resourceType)) return "Unknown";
            
            var normalizedType = resourceType.ToLower();
            
            if (normalizedType.Contains("/serversites/"))
                return "Physical Server";
            else if (normalizedType.Contains("/vmwaresites/"))
                return "VMware Virtual Machine";
            else if (normalizedType.Contains("/hypervsites/"))
                return "Hyper-V Virtual Machine";
            else if (normalizedType.Contains("/importsites/"))
                return "Imported Machine";
            else if (normalizedType.Contains("/sqlsites/"))
                return "SQL Server";
            else if (normalizedType.Contains("/iiswebapplications"))
                return "IIS Web Application";
            else if (normalizedType.Contains("/tomcatwebapplications"))
                return "Tomcat Web Application";
            else if (normalizedType.Contains("/webappsites/"))
                return "Web Application";
                
            return "Unknown";
        }

        const string SoftwareAnalysisQuery = @"
            machinesinventoryinsightsresources
            | where type contains 'Microsoft.OffAzure/serversites/machines/inventoryinsights/software'
                or type contains 'Microsoft.OffAzure/vmwaresites/machines/inventoryinsights/software'
                or type contains 'Microsoft.OffAzure/hypervsites/machines/inventoryinsights/software'
                or type contains 'Microsoft.OffAzure/importsites/machines/inventoryinsights/software'
            | extend id = tolower(id)
            | extend type = tolower(type)
            | extend softwareId = tostring(properties.softwareId)
            | extend machineId = tolower(tostring(split(id, '/inventoryinsights')[0]))
            | where machineId in ({0})
            | extend vulnerabilities = properties.vulnerabilityIds
            | join kind=inner (
                migrateresources
                | where type contains 'Microsoft.OffAzure/serversites/machines'
                    or type contains 'Microsoft.OffAzure/vmwaresites/machines'
                    or type contains 'Microsoft.OffAzure/hypervsites/machines'
                    or type contains 'Microsoft.OffAzure/importsites/machines'
                | extend machineResourceId = tolower(id)
                | where machineResourceId in ({0})
            ) on $left.machineId == $right.machineResourceId
            | summarize
                vulnerabilitiesSet = make_set(vulnerabilities),
                machinesSet = make_set(machineId),
                properties = take_any(properties),
                id = take_any(id)
                by softwareId
            | extend
                name = properties.softwareName,
                category = properties.category,
                provider = properties.provider,
                subcategory = strcat_array(properties.subCategories, ', '),
                supportStatus = properties.supportStatus,
                version = properties.version,
                recommendations = strcat_array(properties.potentialTargets, ', '),
                vulnerabilityCount = array_length(vulnerabilitiesSet),
                machineCount = array_length(machinesSet)
            | project
                softwareId,
                name,
                provider,
                category,
                subcategory,
                version,
                supportStatus,
                recommendations,
                vulnerabilityCount,
                machineCount,
                machinesSet
            ";

        const string SoftwareVulnerabilitiesQuery = @"
            machinesinventoryinsightsresources
            | where type contains 'Microsoft.OffAzure/serversites/machines/inventoryinsights/software'
                or type contains 'Microsoft.OffAzure/vmwaresites/machines/inventoryinsights/software'
                or type contains 'Microsoft.OffAzure/hypervsites/machines/inventoryinsights/software'
                or type contains 'Microsoft.OffAzure/importsites/machines/inventoryinsights/software'
            | extend machineId = tolower(tostring(split(id, '/inventoryInsights')[0]))
            | where machineId in ({0})
            | join kind=inner (
                migrateresources
                | where type contains 'Microsoft.OffAzure/serversites/machines'
                    or type contains 'Microsoft.OffAzure/vmwaresites/machines'
                    or type contains 'Microsoft.OffAzure/hypervsites/machines'
                    or type contains 'Microsoft.OffAzure/importsites/machines'
                | extend machineResourceId = tolower(id)
                | where machineResourceId in ({0})
            ) on $left.machineId == $right.machineResourceId
            | mv-expand vulnerabilityId = properties.vulnerabilityIds
            | extend vulnerabilityId = tostring(vulnerabilityId)
            | summarize
                serverCount = count_distinct(machineId),
                machinesSet = make_set(machineId),
                softwareName = take_any(properties.softwareName),
                softwareVersion = take_any(properties.version)
                by vulnerabilityId
            | join kind=inner (
                machinesinventoryinsightsresources
                | where type contains 'Microsoft.OffAzure/serversites/machines/inventoryinsights/vulnerabilities'
                    or type contains 'Microsoft.OffAzure/vmwaresites/machines/inventoryinsights/vulnerabilities'
                    or type contains 'Microsoft.OffAzure/hypervsites/machines/inventoryinsights/vulnerabilities'
                    or type contains 'Microsoft.OffAzure/importsites/machines/inventoryinsights/vulnerabilities'
                | extend id = tolower(id)
                | where {1}
                | extend cveId = tostring(properties.cveId)
            ) on $left.vulnerabilityId == $right.cveId
            | project
                softwareName,
                softwareVersion,
                vulnerabilityId,
                cveId = properties.cve,
                riskLevel = properties.baseSeverity,
                serverCount
            ";

        const string InventoryInsightsQuery = @"
            machinesinventoryinsightsresources
            | where {1}
            | where {0}
            | extend machineId = tolower(tostring(split(id, '/inventoryInsights')[0]))
            | join kind=inner (
                migrateresources
                | where type in ({2})
                | extend machineResourceId = tolower(id)
                | extend resourceName = properties.displayName
                | extend osType = tostring(coalesce(properties.guestOSDetails.osType, properties.operatingSystemDetails.osType))
                | extend version = tostring(coalesce(properties.guestOSDetails.osName, properties.operatingSystemDetails.osName))
                | extend resourceType = type
            ) on $left.machineId == $right.machineResourceId
            | project
                resourceName,
                osType,
                version,
                resourceType,
                supportStatus = tostring(properties.productSupportStatus.supportStatus),
                vulnerabilityCount = toint(properties.vulnerabilityCount),
                criticalVulnerabilityCount = toint(properties.criticalVulnerabilityCount),
                pendingUpdateCount = toint(properties.pendingUpdateCount),
                pendingSecurityCriticalUpdateCount = toint(properties.pendingSecurityCriticalUpdateCount),
                endOfSupportSoftwareCount = toint(properties.endOfSupportSoftwareCount),
                hasSecuritySoftware = tobool(properties.hasSecuritySoftware),
                hasPatchingSoftware = tobool(properties.hasPatchingSoftware)";

        const string PendingUpdatesServerCountQuery = @"
            machinesinventoryinsightsresources
            | where type in~ (
                ""Microsoft.OffAzure/vmwareSites/machines/inventoryInsights/pendingUpdates"",
                ""Microsoft.OffAzure/hypervSites/machines/inventoryInsights/pendingUpdates"",
                ""Microsoft.OffAzure/serverSites/machines/inventoryInsights/pendingUpdates""
            )
            | where {0}
            | parse id with machineId ""/inventoryInsights/default"" *
            | extend updateTitle = tostring(properties['title']),
                updateId = tostring(properties.updateId),
                severity = tostring(properties.severity)
            | where severity in (""Security"", ""Critical"")
            | summarize serverCount = count_distinct(machineId) by updateTitle
            ";

        // Helper methods to create ARG API compatible JSON payloads
        public static string CreateSoftwareAnalysisArgPayload(string[] subscriptions, string machineIdsList)
        {
            var query = string.Format(SoftwareAnalysisQuery, machineIdsList);
            
            // Debug logging to see the generated query
            System.Diagnostics.Debug.WriteLine("Generated Software Analysis ARG Query:");
            System.Diagnostics.Debug.WriteLine(query);
            System.Diagnostics.Debug.WriteLine($"Machine IDs List: {machineIdsList}");
            
            return CreateArgPayload(subscriptions, query);
        }

        public static string CreateSoftwareVulnerabilitiesArgPayload(string[] subscriptions, string siteIds, string machineIdsList)
        {
            var query = string.Format(SoftwareVulnerabilitiesQuery, machineIdsList, siteIds);
            return CreateArgPayload(subscriptions, query);
        }

        public static string CreateInventoryInsightsArgPayload(string[] subscriptions, string siteFilter)
        {
            var inventoryInsightsTypes = GetInventoryInsightsTypesForQuery();
            var resourceTypes = GetResourceTypesForQuery();
            var query = string.Format(InventoryInsightsQuery, siteFilter, inventoryInsightsTypes, resourceTypes);
            
            // Debug logging to see the generated query
            System.Diagnostics.Debug.WriteLine("Generated ARG Query:");
            System.Diagnostics.Debug.WriteLine(query);
            
            return CreateArgPayload(subscriptions, query);
        }

        private static string CreateArgPayload(string[] subscriptions, string query)
        {
            var payload = new
            {
                subscriptions = subscriptions,
                query = query
            };
            return Newtonsoft.Json.JsonConvert.SerializeObject(payload);
        }

        // Methods to execute ARG queries and return data
        public static async Task<List<SoftwareInsights>> GetSoftwareAnalysisData(
            UserInput userInputObj, 
            string[] subscriptions, 
            List<string> machineIds)
        {
            try
            {
                // Log input parameters
                userInputObj.LoggerObj?.LogInformation($"Software Analysis query - Machine IDs count: {machineIds.Count}");
                userInputObj.LoggerObj?.LogInformation($"Software Analysis query - Subscriptions: {string.Join(", ", subscriptions)}");
                
                // Format machine IDs for KQL
                var machineIdsList = string.Join(", ", machineIds.Select(id => $"\"{id.ToLower()}\""));
                
                // Create ARG payload
                string payload = CreateSoftwareAnalysisArgPayload(subscriptions, machineIdsList);
                
                // Execute query with automatic pagination handled by HttpClientHelper
                var httpHelper = new HttpClientHelper();
                string jsonResponse = await httpHelper.GetHttpResponseForARGQueryWithPagination(userInputObj, payload);
                
                // Parse consolidated response
                var results = ParseSoftwareAnalysisResponse(jsonResponse);
                userInputObj.LoggerObj?.LogInformation($"Software Analysis completed - Total results: {results.Count}");
                
                return results;
            }
            catch (Exception ex)
            {
                userInputObj.LoggerObj?.LogError($"Error executing software analysis query: {ex.Message}");
                throw;
            }
        }

        public static async Task<List<SoftwareVulnerabilities>> GetSoftwareVulnerabilitiesData(
            UserInput userInputObj, 
            string[] subscriptions,
            List<string> siteIds,
            List<string> machineIds)
        {
            try
            {
                // Format machine IDs for KQL
                var machineIdsList = string.Join(", ", machineIds.Select(id => $"\"{id.ToLower()}\""));
                var siteFilters = string.Join(" or ", siteIds.Select(siteId => $"id has \"{siteId.ToLower()}\"").ToList());

                // Create ARG payload
                string payload = CreateSoftwareVulnerabilitiesArgPayload(subscriptions, siteFilters, machineIdsList);
                
                // Execute query with automatic pagination handled by HttpClientHelper
                var httpHelper = new HttpClientHelper();
                string jsonResponse = await httpHelper.GetHttpResponseForARGQueryWithPagination(userInputObj, payload);
                
                // Parse consolidated response
                return ParseSoftwareVulnerabilitiesResponse(jsonResponse);
            }
            catch (Exception ex)
            {
                userInputObj.LoggerObj?.LogError($"Error executing software vulnerabilities query: {ex.Message}");
                throw;
            }
        }

        public static async Task<List<InventoryInsights>> GetInventoryInsightsDataAsync(
            UserInput userInputObj, 
            string[] subscriptions, 
            List<string> siteIds)
        {
            try
            {
                // Create site filter for KQL - build OR condition for site IDs
                var siteFilters = siteIds.Select(siteId => $"id has \"{siteId.ToLower()}\"").ToList();
                var siteFilter = string.Join(" or ", siteFilters);
                
                userInputObj.LoggerObj?.LogInformation($"Site filter: {siteFilter}");
                
                // Create ARG payload
                string payload = CreateInventoryInsightsArgPayload(subscriptions, siteFilter);
                
                // Execute query with automatic pagination handled by HttpClientHelper
                var httpHelper = new HttpClientHelper();
                string jsonResponse = await httpHelper.GetHttpResponseForARGQueryWithPagination(userInputObj, payload);
                
                // Parse consolidated response
                return ParseInventoryInsightsResponse(jsonResponse);
            }
            catch (Exception ex)
            {
                userInputObj.LoggerObj?.LogError($"Error executing inventory insights query: {ex.Message}");
                throw;
            }
        }

        public static async Task<List<PendingUpdatesServerCounts>> GetPendingUpdatesServerCountDataAsync(
            UserInput userInputObj,
            string[] subscriptions,
            List<string> siteIds)
        {
            try
            {
                // Create site filter for KQL - build OR condition for site IDs
                var siteFilters = siteIds.Select(siteId => $"id has \"{siteId.ToLower()}\"").ToList();
                var siteFilter = string.Join(" or ", siteFilters);
                var query = string.Format(PendingUpdatesServerCountQuery, siteFilter);

                // Create ARG payload
                string payload = CreateArgPayload(subscriptions, query);

                // Execute query with automatic pagination handled by HttpClientHelper
                var httpHelper = new HttpClientHelper();
                string jsonResponse = await httpHelper.GetHttpResponseForARGQueryWithPagination(userInputObj, payload);

                // Parse consolidated response
                return ParsePendingUpdatesServerCountsResponse(jsonResponse);
            }
            catch (Exception ex)
            {
                userInputObj.LoggerObj?.LogError($"Error executing pending updates query: {ex.Message}");
                throw;
            }
        }

        // Helper methods to parse ARG response JSON
        private static List<SoftwareInsights> ParseSoftwareAnalysisResponse(string jsonResponse)
        {
            var results = new List<SoftwareInsights>();
            
            try
            {
                var responseObj = JObject.Parse(jsonResponse);
                
                // Try different possible structures for ARG response
                JArray? dataArray = null;
                
                // First try: data.rows structure (common for ARG)
                if (responseObj["data"] is JObject dataObj && dataObj["rows"] is JArray rowsArray)
                {
                    dataArray = rowsArray;
                }
                // Second try: data is directly an array
                else if (responseObj["data"] is JArray directArray)
                {
                    dataArray = directArray;
                }
                
                if (dataArray == null) 
                {
                    // Log the actual response structure for debugging
                    System.Diagnostics.Debug.WriteLine($"Unexpected ARG response structure for software analysis: {jsonResponse}");
                    return results;
                }
                
                System.Diagnostics.Debug.WriteLine($"Found {dataArray.Count} rows in software analysis response");
                if (dataArray.Count > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"First row type: {dataArray[0].GetType()}, content: {dataArray[0]}");
                }
                
                foreach (var row in dataArray)
                {
                    // Handle JObject row format (ARG returns objects with named properties)
                    if (row is JObject rowObject)
                    {
                        // Get recommendations as comma-separated string directly
                        var recommendationsStr = rowObject["recommendations"]?.ToString() ?? string.Empty;
                        
                        results.Add(new SoftwareInsights
                        {
                            Name = rowObject["name"]?.ToString() ?? string.Empty,
                            Provider = rowObject["provider"]?.ToString() ?? string.Empty,
                            Category = rowObject["category"]?.ToString() ?? string.Empty,
                            SubCategory = rowObject["subcategory"]?.ToString() ?? string.Empty,
                            Version = rowObject["version"]?.ToString() ?? string.Empty,
                            SupportStatus = rowObject["supportStatus"]?.ToString() ?? "Unknown",
                            Recommendations = recommendationsStr,
                            Vulnerabilities = rowObject["vulnerabilityCount"]?.ToObject<int>() ?? 0,
                            ServersCount = rowObject["machineCount"]?.ToObject<int>() ?? 0
                        });
                    }
                    else
                    {
                        // Log unexpected row format
                        System.Diagnostics.Debug.WriteLine($"Unexpected row format in software analysis: {row.GetType()}, content: {row}");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error parsing software analysis response: {ex.Message}");
            }
            
            return results;
        }

        private static List<SoftwareVulnerabilities> ParseSoftwareVulnerabilitiesResponse(string jsonResponse)
        {
            var results = new List<SoftwareVulnerabilities>();
            
            try
            {
                var responseObj = JObject.Parse(jsonResponse);
                
                // Try different possible structures for ARG response
                JArray? dataArray = null;
                
                // First try: data.rows structure (common for ARG)
                if (responseObj["data"] is JObject dataObj && dataObj["rows"] is JArray rowsArray)
                {
                    dataArray = rowsArray;
                }
                // Second try: data is directly an array
                else if (responseObj["data"] is JArray directArray)
                {
                    dataArray = directArray;
                }
                
                if (dataArray == null) 
                {
                    // Log the actual response structure for debugging
                    System.Diagnostics.Debug.WriteLine($"Unexpected ARG response structure for software vulnerabilities: {jsonResponse}");
                    return results;
                }
                
                System.Diagnostics.Debug.WriteLine($"Found {dataArray.Count} rows in vulnerabilities response");
                if (dataArray.Count > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"First row type: {dataArray[0].GetType()}, content: {dataArray[0]}");
                }
                
                foreach (var row in dataArray)
                {
                    // Handle JObject row format (ARG returns objects with named properties)
                    if (row is JObject rowObject)
                    {
                        results.Add(new SoftwareVulnerabilities
                        {
                            SoftwareName = rowObject["softwareName"]?.ToString() ?? string.Empty,
                            Version = rowObject["softwareVersion"]?.ToString() ?? string.Empty,
                            Vulnerability = rowObject["vulnerabilityId"]?.ToString() ?? string.Empty,
                            CveId = rowObject["cveId"]?.ToString() ?? string.Empty,
                            Severity = rowObject["riskLevel"]?.ToString() ?? string.Empty,
                            ServerCount = int.TryParse(rowObject["serverCount"]?.ToString(), out var serverCount) ? serverCount : 0
                        });
                    }
                    else
                    {
                        // Log unexpected row format
                        System.Diagnostics.Debug.WriteLine($"Unexpected row format in software vulnerabilities: {row.GetType()}, content: {row}");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error parsing software vulnerabilities response: {ex.Message}");
            }
            
            return results;
        }

        private static List<InventoryInsights> ParseInventoryInsightsResponse(string jsonResponse)
        {
            var results = new List<InventoryInsights>();
            
            try
            {
                var responseObj = JObject.Parse(jsonResponse);
                
                // Try different possible structures for ARG response
                JArray? dataArray = null;
                
                // First try: data.rows structure (common for ARG)
                if (responseObj["data"] is JObject dataObj && dataObj["rows"] is JArray rowsArray)
                {
                    dataArray = rowsArray;
                }
                // Second try: data is directly an array
                else if (responseObj["data"] is JArray directArray)
                {
                    dataArray = directArray;
                }
                
                if (dataArray == null) 
                {
                    // Log the actual response structure for debugging
                    System.Diagnostics.Debug.WriteLine($"Unexpected ARG response structure for inventory insights: {jsonResponse}");
                    return results;
                }
                
                System.Diagnostics.Debug.WriteLine($"Found {dataArray.Count} rows in inventory insights response");
                if (dataArray.Count > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"First row type: {dataArray[0].GetType()}, content: {dataArray[0]}");
                }
                
                foreach (var row in dataArray)
                {
                    // Handle JObject row format (ARG returns objects with named properties)
                    if (row is JObject rowObject)
                    {
                        // Extract data from the query results by property names
                        var resourceName = rowObject["resourceName"]?.ToString() ?? string.Empty;
                        var osType = rowObject["osType"]?.ToString() ?? string.Empty;
                        var version = rowObject["version"]?.ToString() ?? string.Empty;
                        var resourceType = rowObject["resourceType"]?.ToString() ?? string.Empty;
                        var supportStatus = rowObject["supportStatus"]?.ToString() ?? string.Empty;
                        var vulnerabilityCount = int.TryParse(rowObject["vulnerabilityCount"]?.ToString(), out var vulnVal) ? vulnVal : 0;
                        var criticalVulnerabilityCount = int.TryParse(rowObject["criticalVulnerabilityCount"]?.ToString(), out var critVal) ? critVal : 0;
                        var pendingUpdateCount = int.TryParse(rowObject["pendingUpdateCount"]?.ToString(), out var pendingVal) ? pendingVal : 0;
                        var pendingSecurityCriticalUpdateCount = int.TryParse(rowObject["pendingSecurityCriticalUpdateCount"]?.ToString(), out var secVal) ? secVal : 0;
                        var endOfSupportSoftwareCount = int.TryParse(rowObject["endOfSupportSoftwareCount"]?.ToString(), out var eosVal) ? eosVal : 0;

                        var hasSecuritySoftware = bool.TryParse(rowObject["hasSecuritySoftware"]?.ToString(), out var hasSecVal) && hasSecVal;
                        var hasPatchingSoftware = bool.TryParse(rowObject["hasPatchingSoftware"]?.ToString(), out var hasPatchVal) && hasPatchVal;

                        // Determine category based on resource type using centralized logic
                        var category = GetCategoryFromResourceType(resourceType);
                        
                        results.Add(new InventoryInsights
                        {
                            WorkloadName = resourceName,
                            OperatingSystem = version ?? string.Empty, // version maps to OperatingSystem
                            Category = category,
                            Type = GetTypeFromResourceType(resourceType),
                            SupportStatus = supportStatus == "" ? "Unknown" : supportStatus,
                            VulnerabilityCount = vulnerabilityCount,
                            CriticalVulnerabilityCount = criticalVulnerabilityCount,
                            PendingUpdateCount = pendingUpdateCount,
                            PendingSecurityCriticalUpdateCount = pendingSecurityCriticalUpdateCount,
                            EndOfSupportSoftwareCount = endOfSupportSoftwareCount,
                            HasSecuritySoftware = hasSecuritySoftware,
                            HasPatchingSoftware = hasPatchingSoftware
                        });
                    }
                    else
                    {
                        // Log unexpected row format
                        System.Diagnostics.Debug.WriteLine($"Unexpected row format: {row.GetType()}, content: {row}");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error parsing inventory insights response: {ex.Message}");
            }
            
            return results;
        }

        private static List<PendingUpdatesServerCounts> ParsePendingUpdatesServerCountsResponse(string jsonResponse)
        {
            var results = new List<PendingUpdatesServerCounts>();

            try
            {
                var responseObj = JObject.Parse(jsonResponse);

                // Try different possible structures for ARG response
                JArray? dataArray = null;

                // First try: data.rows structure (common for ARG)
                if (responseObj["data"] is JObject dataObj && dataObj["rows"] is JArray rowsArray)
                {
                    dataArray = rowsArray;
                }
                // Second try: data is directly an array
                else if (responseObj["data"] is JArray directArray)
                {
                    dataArray = directArray;
                }

                if (dataArray == null)
                {
                    // Log the actual response structure for debugging
                    System.Diagnostics.Debug.WriteLine($"Unexpected ARG response structure for pending updates: {jsonResponse}");
                    return results;
                }

                System.Diagnostics.Debug.WriteLine($"Found {dataArray.Count} rows in pending updates");
                if (dataArray.Count > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"First row type: {dataArray[0].GetType()}, content: {dataArray[0]}");
                }

                foreach (var row in dataArray)
                {
                    // Handle JObject row format (ARG returns objects with named properties)
                    if (row is JObject rowObject)
                    {
                        // Extract data from the query results by property names
                        var updateTitle = rowObject["updateTitle"]?.ToString() ?? string.Empty;
                        var serverCount = int.TryParse(rowObject["serverCount"]?.ToString(), out var vulnVal) ? vulnVal : 0;

                        results.Add(new PendingUpdatesServerCounts
                        {
                            UpdateTitle = updateTitle,
                            ServerCount = serverCount
                        });
                    }
                    else
                    {
                        // Log unexpected row format
                        System.Diagnostics.Debug.WriteLine($"Unexpected row format: {row.GetType()}, content: {row}");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error parsing pending updates response: {ex.Message}");
            }

            return results;
        }
    }
}

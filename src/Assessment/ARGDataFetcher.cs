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
        const string SoftwareAnalysisQuery = @"
            migrateinventoryinsightsresources
            | where type contains 'Microsoft.OffAzure/serverSites/machines/inventoryinsights/software'
                or type contains 'Microsoft.OffAzure/vmwareSites/machines/inventoryinsights/software'
                or type contains 'Microsoft.OffAzure/hyperVSites/machines/inventoryinsights/software'
                or type contains 'Microsoft.OffAzure/importsites/machines/inventoryinsights/software'
            | extend id = tolower(id)
            | extend type = tolower(type)
            | extend softwareId = tostring(properties.softwareId)
            | extend machineId = tolower(tostring(split(id, '/inventoryinsights')[0]))
            | where machineId in ({0})
            | extend vulnerabilities = properties.vulnerabilityIds
            | join kind=inner (
                migrateresources
                | where type contains 'Microsoft.OffAzure/serverSites/machines'
                    or type contains 'Microsoft.OffAzure/vmwareSites/machines'
                    or type contains 'Microsoft.OffAzure/hyperVSites/machines'
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
                subcategory = strcat_array(properties.subCategories, ', '),
                supportStatus = properties.supportStatus,
                version = properties.version,
                recommendations = strcat_array(properties.potentialTargets, ', '),
                vulnerabilityCount = array_length(vulnerabilitiesSet),
                machineCount = array_length(machinesSet)
            | project
                softwareId,
                name,
                category,
                subcategory,
                version,
                supportStatus,
                recommendations,
                vulnerabilityCount,
                machineCount
            ";

        const string SoftwareVulnerabilitiesQuery = @"
            migrateinventoryinsightsresources
            | where type contains 'Microsoft.OffAzure/serverSites/machines/inventoryinsights/software'
                or type contains 'Microsoft.OffAzure/vmwareSites/machines/inventoryinsights/software'
                or type contains 'Microsoft.OffAzure/hyperVSites/machines/inventoryinsights/software'
                or type contains 'Microsoft.OffAzure/importsites/machines/inventoryinsights/software'
            | extend machineId = tolower(tostring(split(id, '/inventoryinsights')[0]))
            | where machineId in ({0})
            | join kind=inner (
                migrateresources
                | where type contains 'Microsoft.OffAzure/serverSites/machines'
                    or type contains 'Microsoft.OffAzure/vmwareSites/machines'
                    or type contains 'Microsoft.OffAzure/hyperVSites/machines'
                    or type contains 'Microsoft.OffAzure/importsites/machines'
                | extend machineResourceId = tolower(id)
                | where machineResourceId in ({0})
            ) on $left.machineId == $right.machineResourceId
            | mv-expand vulnerabilityId = properties.vulnerabilityIds
            | extend vulnerabilityId = tostring(vulnerabilityId)
            | summarize
                machinesSet = make_set(machineId),
                softwareName = take_any(properties.softwareName),
                softwareVersion = take_any(properties.version)
                by vulnerabilityId
            | join kind=inner (
                migrateinventoryinsightsresources
                | where type contains 'Microsoft.OffAzure/serverSites/machines/inventoryinsights/vulnerabilities'
                    or type contains 'Microsoft.OffAzure/vmwareSites/machines/inventoryinsights/vulnerabilities'
                    or type contains 'Microsoft.OffAzure/hyperVSites/machines/inventoryinsights/vulnerabilities'
                    or type contains 'Microsoft.OffAzure/importsites/machines/inventoryinsights/vulnerabilities'
                | extend cveId = tostring(properties.cveId)
            ) on $left.vulnerabilityId == $right.cveId
            | project
                softwareName,
                softwareVersion,
                vulnerabilityId,
                riskLevel = properties.baseSeverity
            ";

        const string InventoryInsightsForServerMachineQuery = @"
            ";

        // Helper methods to create ARG API compatible JSON payloads
        public static string CreateSoftwareAnalysisArgPayload(string[] subscriptions, string machineIdsList)
        {
            var query = string.Format(SoftwareAnalysisQuery, machineIdsList);
            return CreateArgPayload(subscriptions, query);
        }

        public static string CreateSoftwareVulnerabilitiesArgPayload(string[] subscriptions, string machineIdsList)
        {
            var query = string.Format(SoftwareVulnerabilitiesQuery, machineIdsList);
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
                // Format machine IDs for KQL
                var machineIdsList = string.Join(", ", machineIds.Select(id => $"\"{id.ToLower()}\""));
                
                // Create ARG payload
                string payload = CreateSoftwareAnalysisArgPayload(subscriptions, machineIdsList);
                
                // Execute query
                var httpHelper = new HttpClientHelper();
                HttpResponseMessage response = await httpHelper.GetHttpResponseForARGQuery(userInputObj, payload);
                
                if (!response.IsSuccessStatusCode)
                {
                    string errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception($"ARG Software Analysis query failed: {response.StatusCode}: {errorContent}");
                }
                
                // Parse response
                string jsonResponse = await response.Content.ReadAsStringAsync();
                return ParseSoftwareAnalysisResponse(jsonResponse);
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
            List<string> machineIds)
        {
            try
            {
                // Format machine IDs for KQL
                var machineIdsList = string.Join(", ", machineIds.Select(id => $"\"{id.ToLower()}\""));
                
                // Create ARG payload
                string payload = CreateSoftwareVulnerabilitiesArgPayload(subscriptions, machineIdsList);
                
                // Execute query
                var httpHelper = new HttpClientHelper();
                HttpResponseMessage response = await httpHelper.GetHttpResponseForARGQuery(userInputObj, payload);
                
                if (!response.IsSuccessStatusCode)
                {
                    string errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception($"ARG Software Vulnerabilities query failed: {response.StatusCode}: {errorContent}");
                }
                
                // Parse response
                string jsonResponse = await response.Content.ReadAsStringAsync();
                return ParseSoftwareVulnerabilitiesResponse(jsonResponse);
            }
            catch (Exception ex)
            {
                userInputObj.LoggerObj?.LogError($"Error executing software vulnerabilities query: {ex.Message}");
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
                var dataArray = responseObj["data"]?["rows"] as JArray;
                
                if (dataArray == null) return results;
                
                foreach (var row in dataArray)
                {
                    var rowArray = row as JArray;
                    if (rowArray == null || rowArray.Count < 9) continue;
                    
                    // Parse recommendations from comma-separated string to List<string>
                    var recommendationsStr = rowArray[6]?.ToString() ?? string.Empty;
                    var recommendations = string.IsNullOrEmpty(recommendationsStr) 
                        ? new List<string>() 
                        : recommendationsStr.Split(',').Select(r => r.Trim()).ToList();
                    
                    results.Add(new SoftwareInsights
                    {
                        Name = rowArray[1]?.ToString() ?? string.Empty,
                        Category = rowArray[2]?.ToString() ?? string.Empty,
                        SubCategory = rowArray[3]?.ToString() ?? string.Empty,
                        Version = rowArray[4]?.ToString() ?? string.Empty,
                        SupportStatus = rowArray[5]?.ToString() ?? string.Empty,
                        Recommendations = recommendations,
                        Vulnerabilities = rowArray[7]?.ToObject<int>() ?? 0,
                        ServersCount = rowArray[8]?.ToObject<int>() ?? 0
                    });
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
                var dataArray = responseObj["data"]?["rows"] as JArray;
                
                if (dataArray == null) return results;
                
                foreach (var row in dataArray)
                {
                    var rowArray = row as JArray;
                    if (rowArray == null || rowArray.Count < 4) continue;
                    
                    results.Add(new SoftwareVulnerabilities
                    {
                        SoftwareName = rowArray[0]?.ToString() ?? string.Empty,
                        Version = rowArray[1]?.ToString() ?? string.Empty,
                        Vulnerability = rowArray[2]?.ToString() ?? string.Empty,
                        Severity = rowArray[3]?.ToString() ?? string.Empty
                    });
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error parsing software vulnerabilities response: {ex.Message}");
            }
            
            return results;
        }
    }
}

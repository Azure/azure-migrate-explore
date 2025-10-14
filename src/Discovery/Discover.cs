// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Azure.Migrate.Explore.Common;
using Azure.Migrate.Explore.Excel;
using Azure.Migrate.Explore.HttpRequestHelper;
using Azure.Migrate.Explore.Models;
using AzureMigrateExplore.Discovery;
using Newtonsoft.Json;

namespace Azure.Migrate.Explore.Discovery
{
    public class Discover
    {
        private UserInput UserInputObj;
        public List<DiscoveryData> DiscoveredData;
        public vCenterHostDiscovery VCenterHostData;

        public Discover(UserInput userInputObj)
        {
            UserInputObj = userInputObj;
            DiscoveredData = new List<DiscoveryData>();
            VCenterHostData = new vCenterHostDiscovery();
        }

        public bool BeginDiscovery()
        {
            if (UserInputObj == null)
                throw new Exception("User input provided is null");

            UserInputObj.LoggerObj.LogInformation("Initiating discovered data retrieval");

            DeletePreviousDiscoveryReport();

            string masterSitesUrl = Routes.ProtocolScheme + Routes.AzureManagementApiHostname + Routes.ForwardSlash +
                                    Routes.SubscriptionPath + Routes.ForwardSlash + UserInputObj.Subscription.Key + Routes.ForwardSlash +
                                    Routes.ResourceGroupPath + Routes.ForwardSlash + UserInputObj.ResourceGroupName.Value + Routes.ForwardSlash +
                                    Routes.ProvidersPath + Routes.ForwardSlash + Routes.OffAzureProvidersPath + Routes.ForwardSlash +
                                    Routes.MasterSitesPath + Routes.ForwardSlash + UserInputObj.DiscoverySiteName +
                                    Routes.QueryStringQuestionMark + Routes.QueryParameterApiVersion + Routes.QueryStringEquals + Routes.MasterSiteApiVersion;

            string masterSitesJsonResponse = "";
            try
            {
                masterSitesJsonResponse = new HttpClientHelper().GetHttpRequestJsonStringResponse(masterSitesUrl, UserInputObj).Result;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (AggregateException aeMasterSites)
            {
                string errorMessage = "";
                foreach (var e in aeMasterSites.Flatten().InnerExceptions)
                {
                    if (e is OperationCanceledException)
                        throw e;
                    else
                    {
                        errorMessage = errorMessage + e.Message + " ";
                    }
                }
                UserInputObj.LoggerObj.LogError($"Failed to retrieve master sites: {errorMessage}");
                return false;
            }
            catch (Exception exMasterSitesHttpResponse)
            {
                UserInputObj.LoggerObj.LogError($"Failed to retrieve master sites: {exMasterSitesHttpResponse.Message}");
                return false;
            }

            MasterSitesJSON masterSitesObj = JsonConvert.DeserializeObject<MasterSitesJSON>(masterSitesJsonResponse);
            UserInputObj.LoggerObj.LogInformation(3, "Received discovery sites"); // 8 % complete

            if (UserInputObj.CancellationContext.IsCancellationRequested)
                UtilityFunctions.InitiateCancellation(UserInputObj);

            int excelCreationPercentProgress = 0;
            if (UserInputObj.AzureMigrateSourceAppliances.Contains("import"))
                excelCreationPercentProgress = PerformImportDiscovery(masterSitesObj);
            else
                excelCreationPercentProgress = PerformApplianceDiscovery(masterSitesObj);

            if (DiscoveredData.Count == 0)
            {
                UserInputObj.LoggerObj.LogError($"Consolidated discovery data has no discovered machines");
                DiscoveredMachinesCount.DiscoveredDataCount = DiscoveredData.Count;
                return false;
            }

            // Fetch all inventory data for all sites from ARG.
            var allSites = masterSitesObj.Properties.Sites.Concat(masterSitesObj.Properties.NestedSites).ToList();
            var argData = ARGDataFetcher.GetAllInventoryDataFromDiscoveryAsync(
                UserInputObj,
                new string[] { UserInputObj.Subscription.Key },
                allSites).GetAwaiter().GetResult();

            UserInputObj.LoggerObj.LogInformation($"Retrieved discovery data for {DiscoveredData.Count.ToString()} machines");

            DiscoveryProperties discoveryProperties = new DiscoveryProperties();
            CreateDiscoveryPropertiesModel(discoveryProperties);

            ExportDiscoveryReport exporter = new ExportDiscoveryReport(DiscoveredData, VCenterHostData, discoveryProperties, argData);
            exporter.GenerateDiscoveryReportExcel();

            UserInputObj.LoggerObj.LogInformation(excelCreationPercentProgress, "Discovery report excel created successfully"); // IsExpressWorkflow ? 20 : 100 % Complete

            DiscoveredMachinesCount.DiscoveredDataCount = DiscoveredData.Count;
            return true;
        }

        private void DeletePreviousDiscoveryReport()
        {
            UserInputObj.LoggerObj.LogInformation("Deleting previous discovery reports, if any");
            var directory = UtilityFunctions.GetReportsDirectory();

            if (!Directory.Exists(directory))
            {
                UserInputObj.LoggerObj.LogInformation("No discovery reports found");
                return;
            }

            UserInputObj.LoggerObj.LogInformation("Discovery report found, please ensure the file is closed otherwise deleting it won't be possible and process will terminate");
            Directory.Delete(directory, true);
        }

        private void CreateDiscoveryPropertiesModel(DiscoveryProperties discoveryProperites)
        {
            UserInputObj.LoggerObj.LogInformation("Generating model for discovery properties");
            discoveryProperites.TenantId = UserInputObj.TenantId;
            discoveryProperites.Subscription = UserInputObj.Subscription.Value;
            discoveryProperites.ResourceGroup = UserInputObj.ResourceGroupName.Value;
            discoveryProperites.AzureMigrateProjectName = UserInputObj.AzureMigrateProjectName.Value;
            discoveryProperites.DiscoverySiteName = UserInputObj.DiscoverySiteName;
            discoveryProperites.Workflow = UserInputObj.WorkflowObj.IsExpressWorkflow ? "Express" : "Custom - Discovery";
            discoveryProperites.SourceAppliances = string.Join(",", UserInputObj.AzureMigrateSourceAppliances);
        }

        private int PerformApplianceDiscovery(MasterSitesJSON masterSitesObj)
        {
            List<string> vmwareSites = new List<string>();
            List<string> hypervSites = new List<string>();
            List<string> serverSites = new List<string>();

            foreach (string site in masterSitesObj.Properties.Sites)
            {
                if (site.Contains("vmwaresites"))
                    vmwareSites.Add(site);
                else if (site.Contains("hypervsites"))
                    hypervSites.Add(site);
                else if (site.Contains("serversites"))
                    serverSites.Add(site);
            }

            UserInputObj.LoggerObj.LogInformation(2, "Finished parsing discovery sites"); // 10 % complete

            int discoveryDataPerSitePercentProgress = UserInputObj.WorkflowObj.IsExpressWorkflow ? 3 : 25;
            int excelCreationPercentProgress = UserInputObj.WorkflowObj.IsExpressWorkflow ? 1 : 15;

            if (UserInputObj.AzureMigrateSourceAppliances.Count == 2)
            {
                discoveryDataPerSitePercentProgress = UserInputObj.WorkflowObj.IsExpressWorkflow ? 4 : 40;
                excelCreationPercentProgress = UserInputObj.WorkflowObj.IsExpressWorkflow ? 2 : 10;
            }
            else if (UserInputObj.AzureMigrateSourceAppliances.Count == 1)
            {
                discoveryDataPerSitePercentProgress = UserInputObj.WorkflowObj.IsExpressWorkflow ? 7 : 70;
                excelCreationPercentProgress = UserInputObj.WorkflowObj.IsExpressWorkflow ? 3 : 20;
            }

            List<DiscoveryData> vmwareMachinesDiscovery = new List<DiscoveryData>();
            List<DiscoveryData> hypervMachinesDiscovery = new List<DiscoveryData>();
            List<DiscoveryData> physicalMachinesDiscovery = new List<DiscoveryData>();

            try
            {
                Parallel.For(0, UserInputObj.AzureMigrateSourceAppliances.Count, i =>
                {
                    if (UserInputObj.AzureMigrateSourceAppliances[i].Equals("vmware"))
                    {
                        UserInputObj.LoggerObj.LogInformation("Retrieving discovery data from VMWare sites");
                        vmwareMachinesDiscovery = new RetrieveDiscoveryData().BeginRetrieval(UserInputObj, vmwareSites, DiscoverySites.VMWare);
                        CalculateVCenterHostDiscoveryData(UserInputObj, vmwareSites);
                        UserInputObj.LoggerObj.LogInformation(discoveryDataPerSitePercentProgress, $"Retrieved discovery data for {vmwareMachinesDiscovery.Count.ToString()} machines from VMWare sites");
                    }
                    else if (UserInputObj.AzureMigrateSourceAppliances[i].Equals("hyperv"))
                    {
                        UserInputObj.LoggerObj.LogInformation("Retrieving discovery data from HyperV sites");
                        hypervMachinesDiscovery = new RetrieveDiscoveryData().BeginRetrieval(UserInputObj, hypervSites, DiscoverySites.HyperV);
                        UserInputObj.LoggerObj.LogInformation(discoveryDataPerSitePercentProgress, $"Retrieved discovery data for {hypervMachinesDiscovery.Count.ToString()} machines from HyperV sites");
                    }
                    else if (UserInputObj.AzureMigrateSourceAppliances[i].Equals("physical"))
                    {
                        UserInputObj.LoggerObj.LogInformation("Retrieving discovery data from Physical sites");
                        physicalMachinesDiscovery = new RetrieveDiscoveryData().BeginRetrieval(UserInputObj, serverSites, DiscoverySites.Physical);
                        UserInputObj.LoggerObj.LogInformation(discoveryDataPerSitePercentProgress, $"Retrieved discovery data for {physicalMachinesDiscovery.Count.ToString()} machines from Physical sites");
                    }
                });
            }
            catch (AggregateException ae)
            {
                string errorMessage = "";
                foreach (var e in ae.Flatten().InnerExceptions)
                {
                    if (e is OperationCanceledException)
                        throw e;
                    else
                    {
                        errorMessage = errorMessage + e.Message + " ";
                    }
                }
                throw new Exception(errorMessage);
            }
            catch (Exception exDiscover)
            {
                throw exDiscover;
            }

            try
            {
                DiscoveredData.AddRange(vmwareMachinesDiscovery);
            }
            catch (Exception exConsolidateVmware)
            {
                UserInputObj.LoggerObj.LogWarning($"Failed data consolidation from VMWare sites: {exConsolidateVmware.Message}");
            }
            try
            {
                DiscoveredData.AddRange(hypervMachinesDiscovery);
            }
            catch (Exception exConsolidateHyperv)
            {
                UserInputObj.LoggerObj.LogWarning($"Failed data consolidation from HyperV sites: {exConsolidateHyperv.Message}");
            }
            try
            {
                DiscoveredData.AddRange(physicalMachinesDiscovery);
            }
            catch (Exception exConsolidatePhysical)
            {
                UserInputObj.LoggerObj.LogWarning($"Failed data consolidation from Physical sites: {exConsolidatePhysical.Message}");
            }

            return excelCreationPercentProgress;
        }

        private int PerformImportDiscovery(MasterSitesJSON masterSitesObj)
        {
            List<string> importSites = new List<string>();

            foreach (string site in masterSitesObj.Properties.Sites)
            {
                if (site.Contains("importsites"))
                    importSites.Add(site);
            }

            UserInputObj.LoggerObj.LogInformation(2, "Finished parsing discovery sites"); // 10 % complete

            int discoveryDataPerSitePercentProgress = UserInputObj.WorkflowObj.IsExpressWorkflow ? 7 : 70;
            int excelCreationPercentProgress = UserInputObj.WorkflowObj.IsExpressWorkflow ? 3 : 20;
            List<DiscoveryData> importMachinesDiscovery = new List<DiscoveryData>();

            try
            {
                UserInputObj.LoggerObj.LogInformation("Retrieving discovery data from Import sites");
                importMachinesDiscovery = new RetrieveDiscoveryData().BeginRetrieval(UserInputObj, importSites, DiscoverySites.Import);
                UserInputObj.LoggerObj.LogInformation(discoveryDataPerSitePercentProgress, $"Retrieved discovery data for {importMachinesDiscovery.Count.ToString()} machines from Import sites");
            }
            catch (AggregateException ae)
            {
                string errorMessage = "";
                foreach (var e in ae.Flatten().InnerExceptions)
                {
                    if (e is OperationCanceledException)
                        throw e;
                    else
                    {
                        errorMessage = errorMessage + e.Message + " ";
                    }
                }
                throw new Exception(errorMessage);
            }
            catch (Exception exDiscover)
            {
                throw exDiscover;
            }

            try
            {
                DiscoveredData.AddRange(importMachinesDiscovery);
            }
            catch (Exception exConsolidateImport)
            {
                UserInputObj.LoggerObj.LogWarning($"Failed data consolidation from Import sites: {exConsolidateImport.Message}");
            }

            return excelCreationPercentProgress;
        }

        private void CalculateVCenterHostDiscoveryData(UserInput userInputObj, List<string> vmwareSiteUrls)
        {
            foreach (string siteUrl in vmwareSiteUrls)
            {
                List<KeyValuePair<string, int>> hostAndVCenterList = RetrieveDiscoveryData.RetrieveHostvCenterData(userInputObj, siteUrl);
                VCenterHostData.Hosts += hostAndVCenterList[0].Value;
                VCenterHostData.vCenters += hostAndVCenterList[1].Value;
            };
        }

        public static class DiscoveredMachinesCount
        {
            public static int DiscoveredDataCount { get; set; }
        }
    }
}
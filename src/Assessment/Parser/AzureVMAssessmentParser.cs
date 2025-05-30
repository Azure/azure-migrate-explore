// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

using Azure.Migrate.Explore.Common;
using Azure.Migrate.Explore.HttpRequestHelper;
using Azure.Migrate.Explore.Models;

namespace Azure.Migrate.Explore.Assessment.Parser
{
    public class AzureVMAssessmentParser
    {
        private readonly Dictionary<AssessmentInformation, AssessmentPollResponse> AzureVMAssessmentStatusMap;

        public AzureVMAssessmentParser(Dictionary<AssessmentInformation, AssessmentPollResponse> azureVMAssessmentStatusMap)
        {
            AzureVMAssessmentStatusMap = azureVMAssessmentStatusMap;
        }

        public void ParseVMAssessments(Dictionary<string, AzureVMPerformanceBasedDataset> AzureVMPerformanceBasedMachinesData, Dictionary<string, AzureVMAsOnPremDataset> AzureVMAsOnPremMachinesData, UserInput userInputObj)
        {
            if (userInputObj == null)
                throw new Exception("Received null user input object.");

            if (AzureVMAssessmentStatusMap == null)
            {
                userInputObj.LoggerObj.LogError($"Azure VM assessment status map is null, terminating parsing");
                return;
            }

            if (AzureVMAssessmentStatusMap.Count <= 0)
            {
                userInputObj.LoggerObj.LogError($"Azure VM assessment status map is empty, terminating parsing");
                return;
            }
            

            foreach (var kvp in AzureVMAssessmentStatusMap)
            {
                if (!UtilityFunctions.IsAssessmentCompleted(kvp))
                {
                    userInputObj.LoggerObj.LogWarning($"Skipping parsing assessment {kvp.Key.AssessmentName} as it is in {new EnumDescriptionHelper().GetEnumDescription(kvp.Value)} state");
                    continue;
                }

                userInputObj.LoggerObj.LogInformation($"Parsing Azure VM assessment {kvp.Key.AssessmentName}");

                string url = Routes.ProtocolScheme + Routes.AzureManagementApiHostname + Routes.ForwardSlash +
                             Routes.SubscriptionPath + Routes.ForwardSlash + userInputObj.Subscription.Key + Routes.ForwardSlash +
                             Routes.ResourceGroupPath + Routes.ForwardSlash + userInputObj.ResourceGroupName.Value + Routes.ForwardSlash +
                             Routes.ProvidersPath + Routes.ForwardSlash + Routes.MigrateProvidersPath + Routes.ForwardSlash +
                             Routes.AssessmentProjectsPath + Routes.ForwardSlash + userInputObj.AssessmentProjectName + Routes.ForwardSlash +
                             Routes.GroupsPath + Routes.ForwardSlash + kvp.Key.GroupName + Routes.ForwardSlash +
                             new EnumDescriptionHelper().GetEnumDescription(kvp.Key.AssessmentType) + Routes.ForwardSlash + kvp.Key.AssessmentName + Routes.ForwardSlash +
                             Routes.AssessedMachinesPath +
                             Routes.QueryStringQuestionMark +
                             Routes.QueryParameterApiVersion + Routes.QueryStringEquals + Routes.AssessmentMachineListApiVersion;

                while (!string.IsNullOrEmpty(url))
                {
                    if (userInputObj.CancellationContext.IsCancellationRequested)
                        UtilityFunctions.InitiateCancellation(userInputObj);

                    string assessedMachinesResponse = "";
                    try
                    {
                        assessedMachinesResponse = new HttpClientHelper().GetHttpRequestJsonStringResponse(url, userInputObj).Result;
                    }
                    catch (OperationCanceledException)
                    {
                        throw;
                    }
                    catch (AggregateException aeAssessedMachines)
                    {
                        string errorMessage = "";
                        foreach (var e in aeAssessedMachines.Flatten().InnerExceptions)
                        {
                            if (e is OperationCanceledException)
                                throw e;
                            else
                            {
                                errorMessage = errorMessage + e.Message + " ";
                            }
                        }
                        userInputObj.LoggerObj.LogError($"Failed to retrieve machine data from assessment: {errorMessage}");
                        url = "";
                        continue;
                    }
                    catch (Exception exAssessedMachines)
                    {
                        userInputObj.LoggerObj.LogError($"Failed to retrieve machine data from assessment: {exAssessedMachines.Message}");
                        url = "";
                        continue;
                    }

                    AzureVMAssessedMachinesJSON obj = JsonConvert.DeserializeObject<AzureVMAssessedMachinesJSON>(assessedMachinesResponse);
                    url = obj.NextLink;

                    foreach (var value in obj.Values)
                    {
                        string key = value.Properties.DatacenterMachineArmId?.ToLower();

                        if (kvp.Key.AssessmentTag == AssessmentTag.AsOnPremises)
                        {
                            UpdateAsOnPremDataset(AzureVMAsOnPremMachinesData, key, value, kvp.Key);
                            continue;
                        }
                        
                        UpdatePerformanceBasedDataset(AzureVMPerformanceBasedMachinesData, key, value, kvp.Key, userInputObj.Currency.Key);

                        double monthlyCostEstimate = value.Properties.MonthlyComputeCostForRecommendedSize;
                        if (kvp.Key.AssessmentTag == AssessmentTag.PerformanceBased)
                            AzureVMPerformanceBasedMachinesData[key].MonthlyComputeCostEstimate = monthlyCostEstimate;
                        if (kvp.Key.AssessmentTag == AssessmentTag.PerformanceBased_RI3year)
                            AzureVMPerformanceBasedMachinesData[key].MonthlyComputeCostEstimate_RI3year = monthlyCostEstimate;
                        if (kvp.Key.AssessmentTag == AssessmentTag.PerformanceBased_AHUB)
                            AzureVMPerformanceBasedMachinesData[key].MonthlyComputeCostEstimate_AHUB = monthlyCostEstimate;
                        if (kvp.Key.AssessmentTag == AssessmentTag.PerformanceBased_AHUB_RI3year)
                            AzureVMPerformanceBasedMachinesData[key].MonthlyComputeCostEstimate_AHUB_RI3year = monthlyCostEstimate;
                        if (kvp.Key.AssessmentTag == AssessmentTag.PerformanceBased_ASP3year)
                            AzureVMPerformanceBasedMachinesData[key].MonthlyComputeCostEstimate_ASP3year = monthlyCostEstimate;
                    }
                }
            }
        }

        private void UpdateAsOnPremDataset(Dictionary<string, AzureVMAsOnPremDataset> AzureVMAsOnPremMachinesData, string key, AzureVMAssessedMachineValue value, AssessmentInformation assessmentInfo)
        {
            if (AzureVMAsOnPremMachinesData.ContainsKey(key))
                return;

            AzureVMAsOnPremMachinesData.Add(key, new AzureVMAsOnPremDataset());

            AzureVMAsOnPremMachinesData[key].DisplayName = value.Properties.DisplayName;
            AzureVMAsOnPremMachinesData[key].DatacenterMachineArmId = value.Properties.DatacenterMachineArmId?.ToLower();
            AzureVMAsOnPremMachinesData[key].DatacenterManagementServerName = value.Properties.DatacenterManagementServerName;

            if (assessmentInfo.AssessmentName.Contains("Dev"))
                AzureVMAsOnPremMachinesData[key].Environment = "Dev";
            else if (assessmentInfo.AssessmentName.Contains("Prod"))
                AzureVMAsOnPremMachinesData[key].Environment = "Prod";

            AzureVMAsOnPremMachinesData[key].Suitability = value.Properties.Suitability;
            AzureVMAsOnPremMachinesData[key].SuitabilityExplanation = value.Properties.SuitabilityExplanation;
            AzureVMAsOnPremMachinesData[key].OperatingSystem = value.Properties.OperatingSystemName;
            AzureVMAsOnPremMachinesData[key].SupportStatus = value.Properties.ProductSupportStatus == null ? new EnumDescriptionHelper().GetEnumDescription(SupportabilityStatus.Unknown) : new EnumDescriptionHelper().GetEnumDescription(value.Properties.ProductSupportStatus.SupportStatus);
            AzureVMAsOnPremMachinesData[key].BootType = value.Properties.BootType;
            AzureVMAsOnPremMachinesData[key].NumberOfCores = value.Properties.NumberOfCores;
            AzureVMAsOnPremMachinesData[key].MegabytesOfMemory = value.Properties.MegabytesOfMemory;
            AzureVMAsOnPremMachinesData[key].NetworkAdapters = value.Properties.NetworkAdapters.Count;
            AzureVMAsOnPremMachinesData[key].NetworkAdapterList = GetAssessedNetworkAdapterList(value.Properties.NetworkAdapters);
            AzureVMAsOnPremMachinesData[key].RecommendedVMSize = value.Properties.RecommendedSize;
            AzureVMAsOnPremMachinesData[key].Disks = GetAssessedDiskList(value.Properties.Disks);
            AzureVMAsOnPremMachinesData[key].StorageMonthlyCost = value.Properties.MonthlyStorageCost;
            AzureVMAsOnPremMachinesData[key].MonthlyComputeCostEstimate = value.Properties.MonthlyComputeCostForRecommendedSize;
            AzureVMAsOnPremMachinesData[key].MonthlySecurityCost = UtilityFunctions.GetSecurityCost(value.Properties.CostComponents);
            AzureVMAsOnPremMachinesData[key].GroupName = assessmentInfo.GroupName;
        }

        private void UpdatePerformanceBasedDataset(Dictionary<string, AzureVMPerformanceBasedDataset> AzureVMPerformanceBasedMachinesData, string key, AzureVMAssessedMachineValue value, AssessmentInformation assessmentInfo, string currencySymbol)
        {
            if (AzureVMPerformanceBasedMachinesData.ContainsKey(key))
                return;

            AzureVMPerformanceBasedMachinesData.Add(key, new AzureVMPerformanceBasedDataset());

            AzureVMPerformanceBasedMachinesData[key].DisplayName = value.Properties.DisplayName;
            AzureVMPerformanceBasedMachinesData[key].DatacenterMachineArmId = value.Properties.DatacenterMachineArmId?.ToLower();
            AzureVMPerformanceBasedMachinesData[key].DatacenterManagementServerName = value.Properties.DatacenterManagementServerName;

            if (assessmentInfo.AssessmentName.Contains("Dev"))
                AzureVMPerformanceBasedMachinesData[key].Environment = "Dev";
            else if (assessmentInfo.AssessmentName.Contains("Prod"))
                AzureVMPerformanceBasedMachinesData[key].Environment = "Prod";

            AzureVMPerformanceBasedMachinesData[key].Suitability = value.Properties.Suitability;
            AzureVMPerformanceBasedMachinesData[key].SuitabilityExplanation = value.Properties.SuitabilityExplanation;
            AzureVMPerformanceBasedMachinesData[key].OperatingSystem = value.Properties.OperatingSystemName;
            AzureVMPerformanceBasedMachinesData[key].SupportStatus = value.Properties.ProductSupportStatus == null ? new EnumDescriptionHelper().GetEnumDescription(SupportabilityStatus.Unknown) : new EnumDescriptionHelper().GetEnumDescription(value.Properties.ProductSupportStatus.SupportStatus);
            AzureVMPerformanceBasedMachinesData[key].BootType = value.Properties.BootType;
            AzureVMPerformanceBasedMachinesData[key].NumberOfCores = value.Properties.NumberOfCores;
            AzureVMPerformanceBasedMachinesData[key].MegabytesOfMemory = value.Properties.MegabytesOfMemory;
            AzureVMPerformanceBasedMachinesData[key].PercentageOfCoresUtilization = value.Properties.PercentageCoresUtilization;
            AzureVMPerformanceBasedMachinesData[key].PercentageOfMemoryUtilization = value.Properties.PercentageMemoryUtilization;
            AzureVMPerformanceBasedMachinesData[key].NetworkAdapters = value.Properties.NetworkAdapters.Count;
            AzureVMPerformanceBasedMachinesData[key].NetworkAdapterList = GetAssessedNetworkAdapterList(value.Properties.NetworkAdapters);
            AzureVMPerformanceBasedMachinesData[key].RecommendedVMSize = value.Properties.RecommendedSize;
            AzureVMPerformanceBasedMachinesData[key].Disks = GetAssessedDiskList(value.Properties.Disks);
            AzureVMPerformanceBasedMachinesData[key].StorageMonthlyCost = value.Properties.MonthlyStorageCost;
            AzureVMPerformanceBasedMachinesData[key].MonthlySecurityCost = UtilityFunctions.GetSecurityCost(value.Properties.CostComponents);
            AzureVMPerformanceBasedMachinesData[key].GroupName = assessmentInfo.GroupName;

            if (AzureVMPerformanceBasedMachinesData[key].Environment.Equals("Dev"))
            {
                AzureVMPerformanceBasedMachinesData[key].AzureBackupMonthlyCostEstimate = 0;
                AzureVMPerformanceBasedMachinesData[key].AzureSiteRecoveryMonthlyCostEstimate = 0;
                return;
            }

            AzureVMPerformanceBasedMachinesData[key].AzureSiteRecoveryMonthlyCostEstimate = UtilityFunctions.GetAzureSiteRecoveryMonthlyCostEstimate(currencySymbol);
            AzureVMPerformanceBasedMachinesData[key].AzureBackupMonthlyCostEstimate = UtilityFunctions.GetAzureBackupMonthlyCostEstimate(AzureVMPerformanceBasedMachinesData[key].Disks, currencySymbol);

        }

        #region Utilities
        private List<AssessedDisk> GetAssessedDiskList(Dictionary<string, AzureVMAssessedMachineDisk> disks)
        {
            List<AssessedDisk> diskList = new List<AssessedDisk>();
            foreach (var disk in disks)
            {
                AssessedDisk obj = new AssessedDisk();
                obj.DisplayName = disk.Value.DisplayName;
                obj.GigabytesProvisioned = disk.Value.GigabytesProvisioned;
                obj.Suitability = disk.Value.Suitability;
                obj.RecommendedDiskSku = disk.Value.RecommendedDiskSize;
                obj.DiskType = disk.Value.RecommendedDiskType;
                obj.DiskCost = disk.Value.MonthlyStorageCost;
                obj.MegabytesPerSecondOfRead = disk.Value.MegabytesPerSecondOfRead;
                obj.MegabytesPerSecondOfWrite = disk.Value.MegabytesPerSecondOfWrite;
                obj.NumberOfReadOperationsPerSecond = disk.Value.NumberOfReadOperationsPerSecond;
                obj.NumberOfWriteOperationsPerSecond = disk.Value.NumberOfWriteOperationsPerSecond;

                diskList.Add(obj);
            }

            return diskList;
        }

        private List<AssessedNetworkAdapter> GetAssessedNetworkAdapterList(Dictionary<string, AzureVMAssessedMachineNetworkAdapter> networkAdapters)
        {
            List<AssessedNetworkAdapter> networkAdapterList = new List<AssessedNetworkAdapter>();
            foreach (var networkAdapter in networkAdapters)
            {
                AssessedNetworkAdapter obj = new AssessedNetworkAdapter();
                obj.MacAddress = networkAdapter.Value.MacAddress;
                obj.IpAddresses = networkAdapter.Value.IpAddresses;
                obj.DisplayName = networkAdapter.Value.DisplayName;
                obj.MegabytesPerSecondReceived = networkAdapter.Value.MegabytesPerSecondReceived;
                obj.MegaytesPerSecondTransmitted = networkAdapter.Value.MegabytesPerSecondTransmitted;

                networkAdapterList.Add(obj);
            }

            return networkAdapterList;
        }
        #endregion
    }
}
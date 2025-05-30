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
    public class AzureSQLAssessedMachinesParser
    {
        private readonly Dictionary<AssessmentInformation, AssessmentPollResponse> AzureSQLAssessmentStatusMap;
        public AzureSQLAssessedMachinesParser(Dictionary<AssessmentInformation, AssessmentPollResponse> azureSQLAssessmentStatusMap)
        {
            AzureSQLAssessmentStatusMap = azureSQLAssessmentStatusMap;
        }

        public void ParseAssessedSQLMachines(Dictionary<string, AzureSQLMachineDataset> AzureSQLMachinesData, UserInput userInputObj)
        {
            if (userInputObj == null)
                throw new Exception("Received null user input object.");

            if (AzureSQLAssessmentStatusMap == null)
            {
                userInputObj.LoggerObj.LogError("Azure SQL assessment status map is null, terminating parsing");
                return;
            }

            if (AzureSQLAssessmentStatusMap.Count <= 0)
            {
                userInputObj.LoggerObj.LogError("Azure SQL Assessment status map is empty, terminating parsing");
                return;
            }

            foreach (var kvp in AzureSQLAssessmentStatusMap)
            {
                if (!UtilityFunctions.IsAssessmentCompleted(kvp))
                {
                    userInputObj.LoggerObj.LogWarning($"Skipping parsing assessment {kvp.Key.AssessmentName} as it is in {new EnumDescriptionHelper().GetEnumDescription(kvp.Value)} state");
                    continue;
                }

                userInputObj.LoggerObj.LogInformation($"Parsing Azure SQL assessment {kvp.Key.AssessmentName} for SQL Machines data");

                string url = Routes.ProtocolScheme + Routes.AzureManagementApiHostname + Routes.ForwardSlash +
                             Routes.SubscriptionPath + Routes.ForwardSlash + userInputObj.Subscription.Key + Routes.ForwardSlash +
                             Routes.ResourceGroupPath + Routes.ForwardSlash + userInputObj.ResourceGroupName.Value + Routes.ForwardSlash +
                             Routes.ProvidersPath + Routes.ForwardSlash + Routes.MigrateProvidersPath + Routes.ForwardSlash +
                             Routes.AssessmentProjectsPath + Routes.ForwardSlash + userInputObj.AssessmentProjectName + Routes.ForwardSlash +
                             Routes.GroupsPath + Routes.ForwardSlash + kvp.Key.GroupName + Routes.ForwardSlash +
                             new EnumDescriptionHelper().GetEnumDescription(kvp.Key.AssessmentType) + Routes.ForwardSlash + kvp.Key.AssessmentName + Routes.ForwardSlash +
                             Routes.AssessedSQLMachinesPath +
                             Routes.QueryStringQuestionMark +
                             Routes.QueryParameterApiVersion + Routes.QueryStringEquals + Routes.AssessmentMachineListApiVersion;

                while (!string.IsNullOrEmpty(url))
                {
                    string assessedMachinesResponse = "";
                    try
                    {
                        assessedMachinesResponse = new HttpClientHelper().GetHttpRequestJsonStringResponse(url, userInputObj).Result;
                    }
                    catch (OperationCanceledException)
                    {
                        throw;
                    }
                    catch (AggregateException aeAssessedSQLMachines)
                    {
                        string errorMessage = "";
                        foreach (var e in aeAssessedSQLMachines.Flatten().InnerExceptions)
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
                    catch (Exception exAssessedSQLMachines)
                    {
                        userInputObj.LoggerObj.LogError($"Failed to retrieve machine data from assessment: {exAssessedSQLMachines.Message}");
                        url = "";
                        continue;
                    }

                    AzureSQLAssessedMachinesJSON obj = JsonConvert.DeserializeObject<AzureSQLAssessedMachinesJSON>(assessedMachinesResponse);
                    url = obj.NextLink;

                    foreach (var value in obj.Values)
                    {
                        string key = value.Properties.DatacenterMachineArmId?.ToLower();

                        UpdateAssessedSQLMachinesDataset(AzureSQLMachinesData, key, value, kvp.Key, userInputObj.Currency.Key);

                        double monthlyComputeCost = value.Properties.MonthlyComputeCost;
                        if (kvp.Key.AssessmentTag == AssessmentTag.PerformanceBased)
                            AzureSQLMachinesData[key].MonthlyComputeCost = monthlyComputeCost;
                        else if (kvp.Key.AssessmentTag == AssessmentTag.PerformanceBased_RI3year)
                            AzureSQLMachinesData[key].MonthlyComputeCostEstimate_RI3year = monthlyComputeCost;
                        else if (kvp.Key.AssessmentTag == AssessmentTag.PerformanceBased_AHUB)
                            AzureSQLMachinesData[key].MonthlyComputeCostEstimate_AHUB = monthlyComputeCost;
                        else if (kvp.Key.AssessmentTag == AssessmentTag.PerformanceBased_AHUB_RI3year)
                            AzureSQLMachinesData[key].MonthlyComputeCostEstimate_AHUB_RI3year = monthlyComputeCost;
                        else if (kvp.Key.AssessmentTag == AssessmentTag.PerformanceBased_ASP3year)
                            AzureSQLMachinesData[key].MonthlyComputeCostEstimate_ASP3year = monthlyComputeCost;
                    }
                }
            }
        }

        private void UpdateAssessedSQLMachinesDataset(Dictionary<string, AzureSQLMachineDataset> AzureSQLMachinesData, string key, AzureSQLAssessedMachineValue value, AssessmentInformation assessmentInfo, string currencySymbol)
        {
            if (AzureSQLMachinesData.ContainsKey(key))
                return;

            AzureSQLMachinesData.Add(key, new AzureSQLMachineDataset());

            AzureSQLMachinesData[key].DisplayName = value.Properties.DisplayName;
            AzureSQLMachinesData[key].DatacenterMachineArmId = value.Properties.DatacenterMachineArmId?.ToLower();
            AzureSQLMachinesData[key].DatacenterManagementServerArmId = value.Properties.DatacenterManagementServerArmId;
            AzureSQLMachinesData[key].DatacenterManagementServerName = value.Properties.DatacenterManagementServerName;

            if (assessmentInfo.AssessmentName.Contains("Dev"))
                AzureSQLMachinesData[key].Environment = "Dev";
            else if (assessmentInfo.AssessmentName.Contains("Prod"))
                AzureSQLMachinesData[key].Environment = "Prod";

            AzureSQLMachinesData[key].SuitabilityDetail = value.Properties.SuitabilityDetail;
            AzureSQLMachinesData[key].SuitabilityExplanation = value.Properties.SuitabilityExplanation;
            AzureSQLMachinesData[key].RecommendedVMSize = value.Properties.RecommendedVmSize;
            AzureSQLMachinesData[key].RecommendedVMFamily = value.Properties.RecommendedVmFamily;
            AzureSQLMachinesData[key].RecommendedVMSizeNumberOfCores = value.Properties.RecommendedVmSizeNumberOfCores;
            AzureSQLMachinesData[key].RecommendedVMSizeMegabytesOfMemory = value.Properties.RecommendedVmSizeMegabytesOfMemory;
            AzureSQLMachinesData[key].Disks = GetAssessedDiskList(value.Properties.Disks);
            AzureSQLMachinesData[key].NetworkAdapters = value.Properties.NetworkAdapters.Count;
            AzureSQLMachinesData[key].NetworkAdapterList = GetAssessedNetworkAdapterList(value.Properties.NetworkAdapters);
            AzureSQLMachinesData[key].MonthlyBandwidthCost = value.Properties.MonthlyBandwidthCost;
            AzureSQLMachinesData[key].MonthlyStorageCost = value.Properties.MonthlyStorageCost;
            AzureSQLMachinesData[key].Type = value.Properties.Type;
            AzureSQLMachinesData[key].BootType = value.Properties.BootType;
            AzureSQLMachinesData[key].OperatingSystemType = value.Properties.OperatingSystemType;
            AzureSQLMachinesData[key].OperatingSystemName = value.Properties.OperatingSystemName;
            AzureSQLMachinesData[key].SupportStatus = value.Properties.ProductSupportStatus == null ? new EnumDescriptionHelper().GetEnumDescription(SupportabilityStatus.Unknown) : new EnumDescriptionHelper().GetEnumDescription(value.Properties.ProductSupportStatus.SupportStatus);
            AzureSQLMachinesData[key].OperatingSystemVersion = value.Properties.OperatingSystemVersion;
            AzureSQLMachinesData[key].OperatingSystemArchitecture = value.Properties.OperatingSystemArchitecture;
            AzureSQLMachinesData[key].CreatedTimestamp = value.Properties.CreatedTimestamp;
            AzureSQLMachinesData[key].MonthlySecurityCost = UtilityFunctions.GetSecurityCost(value.Properties.CostComponents);
            AzureSQLMachinesData[key].MegabytesOfMemory = value.Properties.MegabytesOfMemory;
            AzureSQLMachinesData[key].NumberOfCores = value.Properties.NumberOfCores;
            AzureSQLMachinesData[key].ConfidenceRatingInPercentage = value.Properties.ConfidenceRatingInPercentage;
            AzureSQLMachinesData[key].PercentageCoresUtilization = value.Properties.PercentageCoresUtilization;
            AzureSQLMachinesData[key].PercentageMemoryUtilization = value.Properties.PercentageMemoryUtilization;
            AzureSQLMachinesData[key].Suitability = value.Properties.Suitability;
            AzureSQLMachinesData[key].GroupName = assessmentInfo.GroupName;

            if (AzureSQLMachinesData[key].Environment.Equals("Dev"))
            {
                AzureSQLMachinesData[key].AzureBackupMonthlyCostEstimate = 0;
                AzureSQLMachinesData[key].AzureSiteRecoveryMonthlyCostEstimate = 0;
                return;
            }

            AzureSQLMachinesData[key].AzureSiteRecoveryMonthlyCostEstimate = UtilityFunctions.GetAzureSiteRecoveryMonthlyCostEstimate(currencySymbol);
            AzureSQLMachinesData[key].AzureBackupMonthlyCostEstimate = UtilityFunctions.GetAzureBackupMonthlyCostEstimate(AzureSQLMachinesData[key].Disks, currencySymbol);
        }

        #region Utilities
        private List<AssessedDisk> GetAssessedDiskList(Dictionary<string, AzureSQLAssessedMachineDisk> disks)
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

        private List<AssessedNetworkAdapter> GetAssessedNetworkAdapterList(Dictionary<string, AzureSQLAssessedMachineNetworkAdapter> networkAdapters)
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
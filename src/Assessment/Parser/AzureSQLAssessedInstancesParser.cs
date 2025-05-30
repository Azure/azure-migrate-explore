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
    public class AzureSQLAssessedInstancesParser
    {
        private readonly Dictionary<AssessmentInformation, AssessmentPollResponse> AzureSQLAssessmentStatusMap;

        public AzureSQLAssessedInstancesParser(Dictionary<AssessmentInformation, AssessmentPollResponse> azureSQLAssessmentStatusMap)
        {
            AzureSQLAssessmentStatusMap = azureSQLAssessmentStatusMap;
        }

        public void ParseAssessedSQLInstances(Dictionary<string, AzureSQLInstanceDataset> AzureSQLInstancesData, UserInput userInputObj)
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

                userInputObj.LoggerObj.LogInformation($"Parsing Azure SQL assessment {kvp.Key.AssessmentName} for SQL instance data");

                string url = Routes.ProtocolScheme + Routes.AzureManagementApiHostname + Routes.ForwardSlash +
                             Routes.SubscriptionPath + Routes.ForwardSlash + userInputObj.Subscription.Key + Routes.ForwardSlash +
                             Routes.ResourceGroupPath + Routes.ForwardSlash + userInputObj.ResourceGroupName.Value + Routes.ForwardSlash +
                             Routes.ProvidersPath + Routes.ForwardSlash + Routes.MigrateProvidersPath + Routes.ForwardSlash +
                             Routes.AssessmentProjectsPath + Routes.ForwardSlash + userInputObj.AssessmentProjectName + Routes.ForwardSlash +
                             Routes.GroupsPath + Routes.ForwardSlash + kvp.Key.GroupName + Routes.ForwardSlash +
                             new EnumDescriptionHelper().GetEnumDescription(kvp.Key.AssessmentType) + Routes.ForwardSlash + kvp.Key.AssessmentName + Routes.ForwardSlash +
                             Routes.AssessedSQLInstancesPath +
                             Routes.QueryStringQuestionMark +
                             Routes.QueryParameterApiVersion + Routes.QueryStringEquals + Routes.AssessmentMachineListApiVersion;

                while (!string.IsNullOrEmpty(url))
                {
                    if (userInputObj.CancellationContext.IsCancellationRequested)
                        UtilityFunctions.InitiateCancellation(userInputObj);

                    string assessedInstancesResponse = "";
                    try
                    {
                        assessedInstancesResponse = new HttpClientHelper().GetHttpRequestJsonStringResponse(url, userInputObj).Result;
                    }
                    catch (OperationCanceledException)
                    {
                        throw;
                    }
                    catch (AggregateException aeAssessedSqlInstances)
                    {
                        string errorMessage = "";
                        foreach (var e in aeAssessedSqlInstances.Flatten().InnerExceptions)
                        {
                            if (e is OperationCanceledException)
                                throw e;
                            else
                            {
                                errorMessage = errorMessage + e.Message + " ";
                            }
                        }
                        userInputObj.LoggerObj.LogError($"Failed to retrieve assessed SQL instances data from assessment: {errorMessage}");
                        url = "";
                        continue;
                    }
                    catch (Exception exAssessedSqlInstances)
                    {
                        userInputObj.LoggerObj.LogError($"Failed to retrieve assessed SQL instances data from assessment: {exAssessedSqlInstances.Message}");
                        url = "";
                        continue;
                    }

                    AzureSQLAssessedInstancesJSON assessedInstancesObj = JsonConvert.DeserializeObject<AzureSQLAssessedInstancesJSON>(assessedInstancesResponse);
                    url = assessedInstancesObj.NextLink;

                    foreach (var value in assessedInstancesObj.Values)
                    {
                        string key = value.Properties.SqlInstanceSDSArmId?.ToLower();
                        UpdateAssessedInstancesDataset(AzureSQLInstancesData, key, value, kvp.Key, userInputObj.Currency.Key);

                        double monthlySqlMIComputeCost = value.Properties.AzureSqlMISuitabilityDetails.MonthlyComputeCost;
                        double monthlySqlVMComputeCost = value.Properties.AzureSqlVMSuitabilityDetails.MonthlyComputeCost;

                        if (kvp.Key.AssessmentTag == AssessmentTag.PerformanceBased)
                        {
                            AzureSQLInstancesData[key].AzureSQLMIMonthlyComputeCost = monthlySqlMIComputeCost;
                            AzureSQLInstancesData[key].AzureSQLVMMonthlyComputeCost = monthlySqlVMComputeCost;
                        }
                        else if (kvp.Key.AssessmentTag == AssessmentTag.PerformanceBased_RI3year)
                        {
                            AzureSQLInstancesData[key].AzureSQLMIMonthlyComputeCost_RI3year = monthlySqlMIComputeCost;
                            AzureSQLInstancesData[key].AzureSQLVMMonthlyComputeCost_RI3year = monthlySqlVMComputeCost;
                        }
                        else if (kvp.Key.AssessmentTag == AssessmentTag.PerformanceBased_AHUB)
                        {
                            AzureSQLInstancesData[key].AzureSQLMIMonthlyComputeCost_AHUB = monthlySqlMIComputeCost;
                            AzureSQLInstancesData[key].AzureSQLVMMonthlyComputeCost_AHUB = monthlySqlVMComputeCost;
                        }
                        else if (kvp.Key.AssessmentTag == AssessmentTag.PerformanceBased_AHUB_RI3year)
                        {
                            AzureSQLInstancesData[key].AzureSQLMIMonthlyComputeCost_AHUB_RI3year = monthlySqlMIComputeCost;
                            AzureSQLInstancesData[key].AzureSQLVMMonthlyComputeCost_AHUB_RI3year = monthlySqlVMComputeCost;
                        }
                        else if (kvp.Key.AssessmentTag == AssessmentTag.PerformanceBased_ASP3year)
                        {
                            AzureSQLInstancesData[key].AzureSQLVMMonthlyComputeCost_ASP3year = monthlySqlVMComputeCost;
                        }
                    }
                }
            }
        }

        private void UpdateAssessedInstancesDataset(Dictionary<string, AzureSQLInstanceDataset> AzureSQLInstancesData, string key, AzureSQLAssessedInstanceValue value, AssessmentInformation assessmentInfo, string currencySymbol)
        {
            if (AzureSQLInstancesData.ContainsKey(key))
                return;

            AzureSQLInstancesData.Add(key, new AzureSQLInstanceDataset());

            AzureSQLInstancesData[key].MemoryInUseMB = value.Properties.MemoryInUseInMB;
            AzureSQLInstancesData[key].RecommendedAzureSqlTargetType = value.Properties.RecommendedAzureSqlTargetType;
            AzureSQLInstancesData[key].RecommendedSuitability = value.Properties.RecommendedSuitability;

            if (assessmentInfo.AssessmentName.Contains("Dev"))
                AzureSQLInstancesData[key].Environment = "Dev";
            else if (assessmentInfo.AssessmentName.Contains("Prod"))
                AzureSQLInstancesData[key].Environment = "Prod";

            AzureSQLInstancesData[key].AzureSQLMISkuServiceTier = value.Properties.AzureSqlMISuitabilityDetails.AzureSqlSku?.AzureSqlServiceTier;
            AzureSQLInstancesData[key].AzureSQLMISkuComputeTier = value.Properties.AzureSqlMISuitabilityDetails.AzureSqlSku?.AzureSqlComputeTier;
            AzureSQLInstancesData[key].AzureSQLMISkuHardwareGeneration = value.Properties.AzureSqlMISuitabilityDetails.AzureSqlSku?.AzureSqlHardwareGeneration;
            AzureSQLInstancesData[key].AzureSQLMISkuStorageMaxSizeInMB = value.Properties.AzureSqlMISuitabilityDetails.AzureSqlSku == null ? 0.0 : value.Properties.AzureSqlMISuitabilityDetails.AzureSqlSku.StorageMaxSizeInMB;
            AzureSQLInstancesData[key].AzureSQLMISkuPredictedDataSizeInMB = value.Properties.AzureSqlMISuitabilityDetails.AzureSqlSku == null ? 0.0 : value.Properties.AzureSqlMISuitabilityDetails.AzureSqlSku.PredictedDataSizeInMB;
            AzureSQLInstancesData[key].AzureSQLMISkuPredictedLogSizeInMB = value.Properties.AzureSqlMISuitabilityDetails.AzureSqlSku == null ? 0.0 : value.Properties.AzureSqlMISuitabilityDetails.AzureSqlSku.PredictedLogSizeInMB;
            AzureSQLInstancesData[key].AzureSQLMISkuCores = value.Properties.AzureSqlMISuitabilityDetails.AzureSqlSku == null ? 0 : value.Properties.AzureSqlMISuitabilityDetails.AzureSqlSku.Cores;
            AzureSQLInstancesData[key].AzureSQLMITargetType = value.Properties.AzureSqlMISuitabilityDetails.AzureSqlSku == null ? 0 : value.Properties.AzureSqlMISuitabilityDetails.AzureSqlSku.AzureSqlTargetType;
            AzureSQLInstancesData[key].AzureSQLMIMonthlyStorageCost = value.Properties.AzureSqlMISuitabilityDetails.MonthlyStorageCost;
            AzureSQLInstancesData[key].AzureSQLMIMonthlyLicenseCost = value.Properties.AzureSqlMISuitabilityDetails.MonthlyLicenseCost;
            AzureSQLInstancesData[key].AzureSQLMIMigrationTargetPlatform = value.Properties.AzureSqlMISuitabilityDetails.MigrationTargetPlatform;
            AzureSQLInstancesData[key].AzureSQLMISuitability = value.Properties.AzureSqlMISuitabilityDetails.Suitability;
            AzureSQLInstancesData[key].AzureSQLMIMonthlySecurityCost = UtilityFunctions.GetSecurityCost(value.Properties.AzureSqlMISuitabilityDetails.CostComponents);
            AzureSQLInstancesData[key].AzureSQLMIMigrationIssues = GetMigrationIssueList(value.Properties.AzureSqlMISuitabilityDetails.MigrationIssues);

            AzureSQLInstancesData[key].AzureSQLVMFamily = value.Properties.AzureSqlVMSuitabilityDetails.AzureSqlSku?.VirtualMachineSize?.AzureVmFamily;
            AzureSQLInstancesData[key].AzureSQLVMCores = value.Properties.AzureSqlVMSuitabilityDetails.AzureSqlSku == null ? 0 : value.Properties.AzureSqlVMSuitabilityDetails.AzureSqlSku.VirtualMachineSize.Cores;
            AzureSQLInstancesData[key].AzureSQLVMSkuName = value.Properties.AzureSqlVMSuitabilityDetails.AzureSqlSku?.VirtualMachineSize?.AzureSkuName;
            AzureSQLInstancesData[key].AzureSQLVMAvailableCores = value.Properties.AzureSqlVMSuitabilityDetails.AzureSqlSku == null ? 0 : value.Properties.AzureSqlVMSuitabilityDetails.AzureSqlSku.VirtualMachineSize.AvailableCores;
            AzureSQLInstancesData[key].AzureSQLVMMaxNetworkInterfaces = value.Properties.AzureSqlVMSuitabilityDetails.AzureSqlSku == null ? 0 : value.Properties.AzureSqlVMSuitabilityDetails.AzureSqlSku.VirtualMachineSize.MaxNetworkInterfaces;
            AzureSQLInstancesData[key].AzureSQLVMDataDisks = value.Properties.AzureSqlVMSuitabilityDetails.AzureSqlSku == null ? new List<AssessedDisk>() : ConvertDataAndLogDiskToAssessedDiskList(value.Properties.AzureSqlVMSuitabilityDetails.AzureSqlSku.DataDiskSizes);
            AzureSQLInstancesData[key].AzureSQLVMLogDisks = value.Properties.AzureSqlVMSuitabilityDetails.AzureSqlSku == null ? new List<AssessedDisk>() : ConvertDataAndLogDiskToAssessedDiskList(value.Properties.AzureSqlVMSuitabilityDetails.AzureSqlSku.LogDiskSizes);
            AzureSQLInstancesData[key].AzureSQLVMTargetType = value.Properties.AzureSqlVMSuitabilityDetails.AzureSqlSku == null ? 0 : value.Properties.AzureSqlVMSuitabilityDetails.AzureSqlSku.AzureSqlTargetType;
            AzureSQLInstancesData[key].AzureSQLVMMonthlyLicenseCost = value.Properties.AzureSqlVMSuitabilityDetails.MonthlyLicenseCost;
            AzureSQLInstancesData[key].AzureSQLVMMonthlyStorageCost = value.Properties.AzureSqlVMSuitabilityDetails.MonthlyStorageCost;
            AzureSQLInstancesData[key].AzureSQLVMMigrationTargetPlatform = value.Properties.AzureSqlVMSuitabilityDetails.MigrationTargetPlatform;
            AzureSQLInstancesData[key].AzureSQLVMMonthlySecurityCost = UtilityFunctions.GetSecurityCost(value.Properties.AzureSqlVMSuitabilityDetails.CostComponents);
            AzureSQLInstancesData[key].AzureSQLVMSuitability = value.Properties.AzureSqlVMSuitabilityDetails.Suitability;
            AzureSQLInstancesData[key].AzureSQLVMMigrationIssues = GetMigrationIssueList(value.Properties.AzureSqlVMSuitabilityDetails.MigrationIssues);

            AzureSQLInstancesData[key].MachineArmId = value.Properties.MachineArmId?.ToLower();
            AzureSQLInstancesData[key].MachineName = value.Properties.MachineName;
            AzureSQLInstancesData[key].InstanceName = value.Properties.InstanceName;
            AzureSQLInstancesData[key].SQLInstanceSDSArmId = value.Properties.SqlInstanceSDSArmId?.ToLower();
            AzureSQLInstancesData[key].SQLEdition = value.Properties.SqlEdition;
            AzureSQLInstancesData[key].SQLVersion = value.Properties.SqlVersion;
            AzureSQLInstancesData[key].SupportStatus = value.Properties.ProductSupportStatus == null ? new EnumDescriptionHelper().GetEnumDescription(SupportabilityStatus.Unknown) : new EnumDescriptionHelper().GetEnumDescription(value.Properties.ProductSupportStatus.SupportStatus);
            AzureSQLInstancesData[key].NumberOfCoresAllocated = value.Properties.NumberOfCoresAllocated;
            AzureSQLInstancesData[key].PercentageCoresUtilization = value.Properties.PercentageCoresUtilization;
            AzureSQLInstancesData[key].LogicalDisks = GetAssessedLogicalDiskList(value.Properties.LogicalDisks);
            AzureSQLInstancesData[key].ConfidenceRatingInPercentage = value.Properties.ConfidenceRatingInPercentage;
            AzureSQLInstancesData[key].CreatedTimestamp = value.Properties.CreatedTimestamp;

            AzureSQLInstancesData[key].DatabaseSummaryNumberOfUserDatabases = value.Properties.DatabaseSummary == null ? 0 : value.Properties.DatabaseSummary.NumberOfUserDatabases;
            AzureSQLInstancesData[key].DatabaseSummaryTotalDiscoveredUserDatabases = value.Properties.DatabaseSummary == null ? 0 : value.Properties.DatabaseSummary.TotalDiscoveredUserDatabases;
            AzureSQLInstancesData[key].DatabaseSummaryTotalDatabaseSizeInMB = value.Properties.DatabaseSummary == null ? 0 : value.Properties.DatabaseSummary.TotalDatabaseSizeInMB;
            AzureSQLInstancesData[key].DatabaseSumaryLargestDatabaseSizeInMB = value.Properties.DatabaseSummary == null ? 0 : value.Properties.DatabaseSummary.LargestDatabaseSizeInMB;

            AzureSQLInstancesData[key].GroupName = assessmentInfo.GroupName;

            if (AzureSQLInstancesData[key].Environment.Equals("Dev"))
            {
                AzureSQLInstancesData[key].MonthlyAzureBackupCostEstimate = 0;
                AzureSQLInstancesData[key].MonthlyAzureSiteRecoveryCostEstimate = 0;
                return;
            }

            AzureSQLInstancesData[key].MonthlyAzureSiteRecoveryCostEstimate = UtilityFunctions.GetAzureSiteRecoveryMonthlyCostEstimate(currencySymbol);
            AzureSQLInstancesData[key].MonthlyAzureBackupCostEstimate = UtilityFunctions.GetAzureBackupMonthlyCostEstimate(AzureSQLInstancesData[key].LogicalDisks, currencySymbol);
        }

        #region Utilities
        private List<AssessedMigrationIssue> GetMigrationIssueList(List<AzureSqlMigrationIssueInfo> migrationIssues)
        {
            List<AssessedMigrationIssue> migrationIssueList = new List<AssessedMigrationIssue>();

            foreach (var migrationIssue in migrationIssues)
            {
                AssessedMigrationIssue obj = new AssessedMigrationIssue();
                obj.IssueId = migrationIssue.IssueId;
                obj.IssueCategory = migrationIssue.IssueCategory;
                obj.ImpactedObjects = migrationIssue.ImpactedObjects;

                migrationIssueList.Add(obj);
            }

            return migrationIssueList;
        }

        private List<AssessedDisk> GetAssessedLogicalDiskList(List<AzureSqlLogicalDisk> disks)
        {
            List<AssessedDisk> diskList = new List<AssessedDisk>();

            foreach (var disk in disks)
            {
                AssessedDisk obj = new AssessedDisk();
                obj.DisplayName = disk.DiskId;
                obj.GigabytesProvisioned = disk.DiskSizeInMB / 1024.0;
                obj.MegabytesPerSecondOfRead = disk.MegabytesPerSecondOfRead;
                obj.MegabytesPerSecondOfWrite = disk.MegabytesPerSecondOfWrite;
                obj.NumberOfReadOperationsPerSecond = disk.NumberOfReadOperationsPerSecond;
                obj.NumberOfWriteOperationsPerSecond = disk.NumberOfWriteOperationsPerSecond;

                diskList.Add(obj);
            }

            return diskList;
        }

        private List<AssessedDisk> ConvertDataAndLogDiskToAssessedDiskList(List<AzureSQLVMDiskInfo> disks)
        {
            List<AssessedDisk> diskList = new List<AssessedDisk>();

            foreach (var disk in disks)
            {
                AssessedDisk obj = new AssessedDisk();

                obj.DiskType = disk.DiskType;
                obj.RecommendedDiskSku = disk.DiskSize;
                obj.DiskCost = disk.StorageCost;

                diskList.Add(obj);
            }

            return diskList;
        }
        #endregion
    }
}
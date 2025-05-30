// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using System.Collections.Generic;

using Azure.Migrate.Explore.Common;

namespace Azure.Migrate.Explore.Models
{
    public class AzureSQLInstanceDataset
    {
        public double MemoryInUseMB { get; set; }
        public AzureSQLTargetType RecommendedAzureSqlTargetType { get; set; }
        public string RecommendedSuitability { get; set; }
        public string Environment { get; set; }

        public string AzureSQLMISkuServiceTier { get; set; }
        public string AzureSQLMISkuComputeTier { get; set; }
        public string AzureSQLMISkuHardwareGeneration { get; set; }
        public double AzureSQLMISkuStorageMaxSizeInMB { get; set; }
        public double AzureSQLMISkuPredictedDataSizeInMB { get; set; }
        public double AzureSQLMISkuPredictedLogSizeInMB { get; set; }
        public int AzureSQLMISkuCores { get; set; }
        public AzureSQLTargetType AzureSQLMITargetType { get; set; }
        public double AzureSQLMIMonthlyComputeCost { get; set; }
        public double AzureSQLMIMonthlyComputeCost_RI3year { get; set; }
        public double AzureSQLMIMonthlyComputeCost_AHUB { get; set; }
        public double AzureSQLMIMonthlyComputeCost_AHUB_RI3year { get; set; }
        public double AzureSQLMIMonthlySecurityCost { get; set; }
        public double AzureSQLMIMonthlyStorageCost { get; set; }
        public double AzureSQLMIMonthlyLicenseCost { get; set; }
        public AzureSQLTargetType AzureSQLMIMigrationTargetPlatform { get; set; }
        public Suitabilities AzureSQLMISuitability { get; set; }
        public List<AssessedMigrationIssue> AzureSQLMIMigrationIssues { get; set; }

        public string AzureSQLVMFamily { get; set; }
        public int AzureSQLVMCores { get; set; }
        public string AzureSQLVMSkuName { get; set; }
        public int AzureSQLVMAvailableCores { get; set; }
        public int AzureSQLVMMaxNetworkInterfaces { get; set; }
        public List<AssessedDisk> AzureSQLVMDataDisks { get; set; }
        public List<AssessedDisk> AzureSQLVMLogDisks { get; set; }
        public AzureSQLTargetType AzureSQLVMTargetType { get; set; }
        public double AzureSQLVMMonthlyComputeCost { get; set; }
        public double AzureSQLVMMonthlyComputeCost_RI3year { get; set; }
        public double AzureSQLVMMonthlyComputeCost_AHUB { get; set; }
        public double AzureSQLVMMonthlyComputeCost_AHUB_RI3year { get; set; }
        public double AzureSQLVMMonthlyComputeCost_ASP3year { get; set; }
        public double AzureSQLVMMonthlySecurityCost { get; set; }
        public double AzureSQLVMMonthlyLicenseCost { get; set; }
        public double AzureSQLVMMonthlyStorageCost { get; set; }
        public AzureSQLTargetType AzureSQLVMMigrationTargetPlatform { get; set; }
        public Suitabilities AzureSQLVMSuitability { get; set; }
        public List<AssessedMigrationIssue> AzureSQLVMMigrationIssues { get; set; }

        public string MachineArmId { get; set; }
        public string MachineName { get; set; }
        public string InstanceName { get; set; }
        public string SQLInstanceSDSArmId { get; set; }
        public string SQLEdition { get; set; }
        public string SQLVersion { get; set; }
        public string SupportStatus { get; set; }
        public int NumberOfCoresAllocated { get; set; }
        public double PercentageCoresUtilization { get; set; }
        public List<AssessedDisk> LogicalDisks { get; set; }
        public double ConfidenceRatingInPercentage { get; set; }
        public string CreatedTimestamp { get; set; }

        public int DatabaseSummaryNumberOfUserDatabases { get; set; }
        public double DatabaseSummaryTotalDatabaseSizeInMB { get; set; }
        public double DatabaseSumaryLargestDatabaseSizeInMB { get; set; }
        public int DatabaseSummaryTotalDiscoveredUserDatabases { get; set; }

        public double MonthlyAzureSiteRecoveryCostEstimate { get; set; }
        public double MonthlyAzureBackupCostEstimate { get; set; }

        public string GroupName { get; set ;}
    }
}
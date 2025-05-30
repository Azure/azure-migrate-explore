// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
namespace Azure.Migrate.Explore.Models
{
    public class SQL_MI_Opportunity
    {
        public string MachineName { get; set; }
        public string SQLInstance { get; set; }
        public string Environment { get; set; }
        public string AzureSQLMIReadiness { get; set; }
        public string AzureSQLMIReadiness_Warnings { get; set; }
        public string AzureSQLMIReadiness_Issues { get; set; }
        public string RecommendedDeploymentType { get; set; }
        public string AzureSQLMIConfiguration { get; set; }
        public double MonthlyComputeCostEstimate { get; set; }
        public double MonthlyComputeCostEstimate_RI3year { get; set; }
        public double MonthlyComputeCostEstimate_AHUB { get; set; }
        public double MonthlyComputeCostEstimate_AHUB_RI3year { get; set; }
        public double MonthlyStorageCostEstimate { get; set; }
        public double MonthlySecurityCostEstimate { get; set; }
        public int UserDatabases { get; set; }
        public string SQLEdition { get; set; }
        public string SQLVersion { get; set; }
        public string SupportStatus { get; set; }
        public double TotalDBSizeInMB { get; set; }
        public double LargestDBSizeInMB { get; set; }
        public int VCoresAllocated { get; set; }
        public double CpuUtilizationInPercentage { get; set; }
        public double MemoryInUseInMB { get; set; }
        public int NumberOfDisks { get; set; }
        public double DiskReadInOPS { get; set; }
        public double DiskWriteInOPS { get; set; }
        public double DiskReadInMBPS { get; set; }
        public double DiskWriteInMBPS { get; set; }
        public double ConfidenceRatingInPercentage { get; set; }
        public string AzureSQLMIConfigurationTargetServiceTier { get; set; }
        public string AzureSQLMIConfigurationTargetComputeTier { get; set; }
        public string AzureSQLMIConfigurationTargetHardwareType { get; set; }
        public int AzureSQLMIConfigurationTargetCores { get; set; }
        public double AzureSQLMIConfigurationTargetStorageInGB { get; set; }
        public string GroupName { get; set; }
        public string MachineId { get; set; }
    }
}
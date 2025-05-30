// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
namespace Azure.Migrate.Explore.Models
{
    public class SQL_IaaS_Instance_Rehost_Perf
    {
        public string MachineName { get; set; }
        public string SQLInstance { get; set; }
        public string Environment { get; set; }
        public string SQLServerOnAzureVMReadiness { get; set; }
        public string SQLServerOnAzureVMReadiness_Warnings { get; set; }
        public string SQLServerOnAzureVMConfiguration { get; set; }
        public double MonthlyComputeCostEstimate { get; set; }
        public double MonthlyComputeCostEstimate_RI3year { get; set; }
        public double MonthlyComputeCostEstimate_AHUB { get; set; }
        public double MonthlyComputeCostEstimate_AHUB_RI3year { get; set; }
        public double MonthlyComputeCostEstimate_ASP3year { get; set; }
        public double MonthlyStorageCostEstimate { get; set; }
        public double MonthlySecurityCostEstimate { get; set; }
        public string SQLServerONAzureVMManagedDiskConfiguration { get; set; }
        public int UserDatabases { get; set; }
        public string RecommendedDeploymentType { get; set; }
        public int StandardHddDisks { get; set; }
        public int StandardSsdDisks { get; set; }
        public int PremiumDisks { get; set; }
        public int UltraDisks { get; set; }
        public double MonthlyStorageCostForStandardHddDisks { get; set; }
        public double MonthlyStorageCostForStandardSsdDisks { get; set; }
        public double MonthlyStorageCostForPremiumDisks { get; set; }
        public double MonthlyStorageCostForUltraDisks { get; set; }
        public string SQLEdition { get; set; }
        public string SQLVersion { get; set; }
        public string SupportStatus { get; set; }
        public double TotalDBSizeInMB { get; set; }
        public double LargestDBSizeInMB { get; set; }
        public int VCoresAllocated { get; set; }
        public double CpuUtilizationPercentage { get; set; }
        public double MemoryInUseInMB { get; set; }
        public int NumberOfDisks { get; set; }
        public double DiskReadInOPS { get; set; }
        public double DiskWriteInOPS { get; set; }
        public double DiskReadInMBPS { get; set; }
        public double DiskWriteInMBPS { get; set; }
        public double ConfidenceRatingInPercentage { get; set; }
        public int SQLServerOnAzureVMConfigurationTargetCores { get; set; }
        public double MonthlyAzureSiteRecoveryCostEstimate { get; set; }
        public double MonthlyAzureBackupCostEstimate { get; set; }
        public string GroupName { get; set; }
        public string MachineId { get; set; }
    }
}
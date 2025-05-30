// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
namespace Azure.Migrate.Explore.Models
{
    public class All_VM_IaaS_Server_Rehost_Perf
    {
        public string MachineName { get; set; }
        public string Environment { get; set; }
        public string AzureVMReadiness { get; set; }
        public string AzureVMReadiness_Warnings { get; set; }
        public string RecommendedVMSize { get; set; }
        public double MonthlyComputeCostEstimate { get; set; }
        public double MonthlyStorageCostEstimate { get; set; }
        public double MonthlySecurityCostEstimate { get; set; }
        public string OperatingSystem { get; set; }
        public string SupportStatus { get; set; }
        public string VMHost { get; set; }
        public string BootType { get; set; }
        public int Cores { get; set; }
        public double MemoryInMB { get; set; }
        public double CpuUtilizationPercentage { get; set; }
        public double MemoryUtilizationPercentage { get; set; }
        public double StorageInGB { get; set; }
        public int NetworkAdapters { get; set; }
        public string IpAddresses { get; set; }
        public string MacAddresses { get; set; }
        public string DiskNames { get; set; }
        public string AzureDiskReadiness { get; set; }
        public string RecommendedDiskSKUs { get; set; }
        public int StandardHddDisks { get; set; }
        public int StandardSsdDisks { get; set; }
        public int PremiumDisks { get; set; }
        public int UltraDisks { get; set; }
        public double MonthlyStorageCostForStandardHddDisks { get; set; }
        public double MonthlyStorageCostForStandardSsdDisks { get; set; }
        public double MonthlyStorageCostForPremiumDisks { get; set; }
        public double MonthlyStorageCostForUltraDisks { get; set; }
        public double MonthlyAzureSiteRecoveryCostEstimate { get; set; }
        public double MonthlyAzureBackupCostEstimate { get; set; }
        public string GroupName { get; set; }
        public string MachineId { get; set; }
    }
}
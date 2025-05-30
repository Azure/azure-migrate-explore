// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using System.Collections.Generic;

using Azure.Migrate.Explore.Common;

namespace Azure.Migrate.Explore.Models
{
    public class AzureVMPerformanceBasedDataset
    {
        public string DisplayName { get; set; }
        public string DatacenterMachineArmId { get; set; }
        public string DatacenterManagementServerName { get; set; }
        public string Environment { get; set; }
        public Suitabilities Suitability { get; set; }
        public string SuitabilityExplanation { get; set; }
        public string OperatingSystem { get; set; }
        public string SupportStatus { get; set; }
        public string BootType { get; set; }
        public int NumberOfCores { get; set; }
        public double MegabytesOfMemory { get; set; }
        public double PercentageOfCoresUtilization { get; set; }
        public double PercentageOfMemoryUtilization { get; set; }
        public int NetworkAdapters { get; set; }
        public List<AssessedNetworkAdapter> NetworkAdapterList { get; set; }
        public string RecommendedVMSize { get; set; }
        public List<AssessedDisk> Disks { get; set; }
        public double StorageMonthlyCost { get; set; }
        public double MonthlyComputeCostEstimate { get; set; }
        public double MonthlyComputeCostEstimate_RI3year { get; set; }
        public double MonthlyComputeCostEstimate_AHUB { get; set; }
        public double MonthlyComputeCostEstimate_AHUB_RI3year { get; set; }
        public double MonthlyComputeCostEstimate_ASP3year { get; set; }
        public double MonthlySecurityCost { get; set; }
        public double AzureSiteRecoveryMonthlyCostEstimate { get; set; }
        public double AzureBackupMonthlyCostEstimate { get; set; }
        public string GroupName { get; set; }
    }
}
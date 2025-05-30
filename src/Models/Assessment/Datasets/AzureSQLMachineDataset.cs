// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using System.Collections.Generic;

using Azure.Migrate.Explore.Common;

namespace Azure.Migrate.Explore.Models
{
    public class AzureSQLMachineDataset
    {
        public string DisplayName { get; set; }
        public string DatacenterMachineArmId { get; set; }
        public string DatacenterManagementServerArmId { get; set; }
        public string DatacenterManagementServerName { get; set; }
        public string Environment { get; set; }
        public string SuitabilityDetail { get; set; }
        public string SuitabilityExplanation { get; set; }
        public string RecommendedVMSize { get; set; }
        public string RecommendedVMFamily { get; set; }
        public int RecommendedVMSizeNumberOfCores { get; set; }
        public double RecommendedVMSizeMegabytesOfMemory { get; set; }
        public double MonthlyComputeCost { get; set; }
        public double MonthlyComputeCostEstimate_RI3year { get; set; }
        public double MonthlyComputeCostEstimate_AHUB { get; set; }
        public double MonthlyComputeCostEstimate_AHUB_RI3year { get; set; }
        public double MonthlyComputeCostEstimate_ASP3year { get; set; }
        public List<AssessedDisk> Disks { get; set; }
        public int NetworkAdapters { get; set; }
        public List<AssessedNetworkAdapter> NetworkAdapterList { get; set; }
        public double MonthlyBandwidthCost { get; set; }
        public double MonthlyStorageCost { get; set; }
        public string Type { get; set; }
        public double MonthlySecurityCost { get; set; }
        public string BootType { get; set; }
        public string OperatingSystemType { get; set; }
        public string OperatingSystemName { get; set; }
        public string SupportStatus { get; set; }
        public string OperatingSystemVersion { get; set; }
        public string OperatingSystemArchitecture { get; set; }
        public string CreatedTimestamp { get; set; }
        public double MegabytesOfMemory { get; set; }
        public int NumberOfCores { get; set; }
        public double ConfidenceRatingInPercentage { get; set; }
        public double PercentageCoresUtilization { get; set; }
        public double PercentageMemoryUtilization { get; set; }
        public Suitabilities Suitability { get; set; }
        public double AzureSiteRecoveryMonthlyCostEstimate { get; set; }
        public double AzureBackupMonthlyCostEstimate { get; set; }
        public string GroupName { get; set; }
    }
}
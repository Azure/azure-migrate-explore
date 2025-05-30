// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using Newtonsoft.Json;
using System.Collections.Generic;

using Azure.Migrate.Explore.Common;

namespace Azure.Migrate.Explore.Models
{
    public class AzureVMAssessedMachinesJSON
    {
        [JsonProperty("value")]
        public List<AzureVMAssessedMachineValue> Values { get; set; }

        [JsonProperty("nextLink")]
        public string NextLink { get; set; }
    }

    public class AzureVMAssessedMachineValue
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("name")]
        public string Name;

        [JsonProperty("properties")]
        public AzureVMAssessedMachineProperty Properties { get; set; }
    }

    public class AzureVMAssessedMachineProperty
    {
        [JsonProperty("recommendedSize")]
        public string RecommendedSize { get; set; }

        [JsonProperty("monthlyComputeCostForRecommendedSize")]
        public double MonthlyComputeCostForRecommendedSize { get; set; }

        [JsonProperty("disks")]
        public Dictionary<string, AzureVMAssessedMachineDisk> Disks { get; set; }

        [JsonProperty("monthlyStorageCost")]
        public double MonthlyStorageCost { get; set; }

        [JsonProperty("costComponents")]
        public List<AzureAssessmentCostComponent> CostComponents { get; set; }

        [JsonProperty("monthlyStandardSSDStorageCost")]
        public double MonthlyStandardSSDStorageCost { get; set; }

        [JsonProperty("monthlyPremiumStorageCost")]
        public double MonthlyPremiumStorageCost { get; set; }

        [JsonProperty("monthlyUltraStorageCost")]
        public double MonthlyUltraStorageCost { get; set; }

        [JsonProperty("monthlyBandwidthCost")]
        public double MonthlyBandwidthCost { get; set; }

        [JsonProperty("networkAdapters")]
        public Dictionary<string, AzureVMAssessedMachineNetworkAdapter> NetworkAdapters { get; set; }

        [JsonProperty("numberOfCoresForRecommendedSize")]
        public int NumberOfCoresForRecommendedSize { get; set; }

        [JsonProperty("megabytesOfMemoryForRecommendedSize")]
        public double MegabytesOfMemoryForRecommendedSize { get; set; }

        [JsonProperty("suitability")]
        public Suitabilities Suitability { get; set; }

        [JsonProperty("suitabilityDetail")]
        public string SuitabilityDetail { get; set; }

        [JsonProperty("suitabilityExplanation")]
        public string SuitabilityExplanation { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("bootType")]
        public string BootType { get; set; }

        [JsonProperty("operatingSystemType")]
        public string OperatingSytemType { get; set; }

        [JsonProperty("operatingSystemName")]
        public string OperatingSystemName { get; set; }

        [JsonProperty("productSupportStatus")]
        public ProductSupportState ProductSupportStatus { get; set; }

        [JsonProperty("operatingSystemVersion")]
        public string OperatingSystemVersion { get; set; }

        [JsonProperty("operatingSystemArchitecture")]
        public string OperatingSystemArchitecture { get; set; }

        [JsonProperty("createdTimestamp")]
        public string CreatedTimeStamp { get; set; }

        [JsonProperty("updatedTimestamp")]
        public string UpdatedTimeStamp { get; set; }

        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("datacenterMachineArmId")]
        public string DatacenterMachineArmId { get; set; }

        [JsonProperty("datacenterManagementServerArmId")]
        public string DatacenterManagementServerArmId { get; set; }

        [JsonProperty("datacenterManagementServerName")]
        public string DatacenterManagementServerName { get; set; }

        [JsonProperty("megabytesOfMemory")]
        public double MegabytesOfMemory { get; set; }

        [JsonProperty("numberOfCores")]
        public int NumberOfCores { get; set; }

        [JsonProperty("confidenceRatingInPercentage")]
        public double ConfidenceRatingInPercentage { get; set; }

        [JsonProperty("percentageCoresUtilization")]
        public double PercentageCoresUtilization { get; set; }

        [JsonProperty("percentageMemoryUtilization")]
        public double PercentageMemoryUtilization { get; set; }
    }

    public class AzureVMAssessedMachineDisk
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        [JsonProperty("recommendedDiskSize")]
        public string RecommendedDiskSize { get; set; }

        [JsonProperty("recommendedDiskType")]
        public RecommendedDiskTypes RecommendedDiskType { get; set; }

        [JsonProperty("monthlyStorageCost")]
        public double MonthlyStorageCost { get; set; }

        [JsonProperty("recommendedDiskIops")]
        public double RecommendedDiskIops { get; set; }

        [JsonProperty("recommendedDiskThroughputInMbps")]
        public double RecommendedDiskThroughputInMbps { get; set; }

        [JsonProperty("gigabytesProvisioned")]
        public double GigabytesProvisioned { get; set; }

        [JsonProperty("megabytesPerSecondOfRead")]
        public double MegabytesPerSecondOfRead { get; set; }

        [JsonProperty("megabytesPerSecondOfWrite")]
        public double MegabytesPerSecondOfWrite { get; set; }

        [JsonProperty("numberOfReadOperationsPerSecond")]
        public double NumberOfReadOperationsPerSecond { get; set; }

        [JsonProperty("numberOfWriteOperationsPerSecond")]
        public double NumberOfWriteOperationsPerSecond { get; set; }

        [JsonProperty("gigabytesForRecommendedDiskSize")]
        public int GigabytesForRecommendedDiskSize { get; set; }

        [JsonProperty("suitability")]
        public Suitabilities Suitability { get; set; }

        [JsonProperty("suitabilityExplanation")]
        public string SuitabilityExplanation { get; set; }

        [JsonProperty("suitabilityDetail")]
        public string SuitabilityDetail { get; set; }
    }

    public class AzureVMAssessedMachineNetworkAdapter
    {
        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        [JsonProperty("macAddress")]
        public string MacAddress { get; set; }

        [JsonProperty("ipAddresses")]
        public List<string> IpAddresses { get; set; }

        [JsonProperty("monthlyBandwidthCosts")]
        public double MonthlyBandwidthCosts { get; set; }

        [JsonProperty("megabytesPerSecondReceived")]
        public double MegabytesPerSecondReceived { get; set; }

        [JsonProperty("megabytesPerSecondTransmitted")]
        public double MegabytesPerSecondTransmitted { get; set; }

        [JsonProperty("netGigabytesTransmittedPerMonth")]
        public double NetGigabytesTransmittedPerMonth { get; set; }

        [JsonProperty("suitability")]
        public Suitabilities Suitability { get; set; }

        [JsonProperty("suitabilityExplanation")]
        public string SuitabilityExplanation { get; set; }

        [JsonProperty("suitabilityDetail")]
        public string SuitabilityDetail { get; set; }
    }
}
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using Newtonsoft.Json;
using System.Collections.Generic;

using Azure.Migrate.Explore.Common;

namespace Azure.Migrate.Explore.Models
{
    public class AzureSQLAssessedMachinesJSON
    {
        [JsonProperty("value")]
        public List<AzureSQLAssessedMachineValue> Values { get; set; }

        [JsonProperty("nextLink")]
        public string NextLink { get; set; }
    }

    public class AzureSQLAssessedMachineValue
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("properties")]
        public AzureSQLAssessedMachineProperty Properties { get; set; }
    }

    public class AzureSQLAssessedMachineProperty
    {
        [JsonProperty("sqlInstances")]
        public List<AzureSqlInstanceInfo> SqlInstances { get; set; }

        [JsonProperty("suitabilityDetail")]
        public string SuitabilityDetail { get; set; }

        [JsonProperty("suitabilityExplanation")]
        public string SuitabilityExplanation { get; set; }

        [JsonProperty("recommendedVmSize")]
        public string RecommendedVmSize { get; set; }

        [JsonProperty("recommendedVmFamily")]
        public string RecommendedVmFamily { get; set; }

        [JsonProperty("costComponents")]
        public List<AzureAssessmentCostComponent> CostComponents { get; set; }

        [JsonProperty("recommendedVmSizeNumberOfCores")]
        public int RecommendedVmSizeNumberOfCores { get; set; }

        [JsonProperty("recommendedVmSizeMegabytesOfMemory")]
        public double RecommendedVmSizeMegabytesOfMemory { get; set; }

        [JsonProperty("monthlyComputeCost")]
        public double MonthlyComputeCost { get; set; }

        [JsonProperty("disks")]
        public Dictionary<string, AzureSQLAssessedMachineDisk> Disks { get; set; }

        [JsonProperty("networkAdapters")]
        public Dictionary<string, AzureSQLAssessedMachineNetworkAdapter> NetworkAdapters { get; set; }

        [JsonProperty("monthlyBandwidthCost")]
        public double MonthlyBandwidthCost { get; set; }

        [JsonProperty("monthlyStorageCost")]
        public double MonthlyStorageCost { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("bootType")]
        public string BootType { get; set; }

        [JsonProperty("operatingSystemType")]
        public string OperatingSystemType { get; set; }

        [JsonProperty("operatingSystemName")]
        public string OperatingSystemName { get; set; }

        [JsonProperty("productSupportStatus")]
        public ProductSupportState ProductSupportStatus { get; set; }

        [JsonProperty("operatingSystemVersion")]
        public string OperatingSystemVersion { get; set; }

        [JsonProperty("operatingSystemArchitecture")]
        public string OperatingSystemArchitecture { get; set; }

        [JsonProperty("createdTimestamp")]
        public string CreatedTimestamp { get; set; }

        [JsonProperty("updatedTimestamp")]
        public string UpdatedTimestamp { get; set; }

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

        [JsonProperty("suitability")]
        public Suitabilities Suitability { get; set; }
    }

    public class AzureSqlInstanceInfo
    {
        [JsonProperty("instanceId")]
        public string InstanceId { get; set; }

        [JsonProperty("instanceName")]
        public string InstanceName { get; set; }

        [JsonProperty("sqlInstanceSDSArmId")]
        public string SqlInstanceSDSArmId { get; set; }

        [JsonProperty("sqlInstanceEntityId")]
        public string SqlInstanceEntityId { get; set; }

        [JsonProperty("sqlEdition")]
        public string SqlEdition { get; set; }

        [JsonProperty("sqlVersion")]
        public string SqlVersion { get; set; }
    }

    public class AzureSQLAssessedMachineDisk
    {
        [JsonProperty("suitability")]
        public Suitabilities Suitability { get; set; }

        [JsonProperty("suitabilityExplanation")]
        public string SuitabilityExplanation { get; set; }

        [JsonProperty("suitabilityDetail")]
        public string SuitabilityDetail { get; set; }

        [JsonProperty("recommendedDiskSize")]
        public string RecommendedDiskSize { get; set; }

        [JsonProperty("recommendedDiskType")]
        public RecommendedDiskTypes RecommendedDiskType { get; set; }

        [JsonProperty("recommendedDiskSizeGigabytes")]
        public int RecommendedDiskSizeGigabytes { get; set; }

        [JsonProperty("recommendDiskThroughputInMbps")]
        public double RecommendedDiskThroughputInMbps { get; set; }

        [JsonProperty("recommendedDiskIops")]
        public double RecommendedDiskIops { get; set; }

        [JsonProperty("monthlyStorageCost")]
        public double MonthlyStorageCost { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

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
    }

    public class AzureSQLAssessedMachineNetworkAdapter
    {
        [JsonProperty("suitability")]
        public Suitabilities Suitability { get; set; }

        [JsonProperty("suitabilityDetail")]
        public string SuitabilityDetail { get; set; }

        [JsonProperty("suitabilityExplanation")]
        public string SuitabilityExplanation { get; set; }

        [JsonProperty("monthlyBandwidthCosts")]
        public double MonthlyBandwidthCosts { get; set; }

        [JsonProperty("netGigabytesTransmittedPerMonth")]
        public double NetGigabytesTransmittedPerMonth { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        [JsonProperty("macAddress")]
        public string MacAddress { get; set; }

        [JsonProperty("ipAddresses")]
        public List<string> IpAddresses { get; set; }

        [JsonProperty("megabytesPerSecondReceived")]
        public double MegabytesPerSecondReceived { get; set; }

        [JsonProperty("megabytesPerSecondTransmitted")]
        public double MegabytesPerSecondTransmitted { get; set; }
    }
}
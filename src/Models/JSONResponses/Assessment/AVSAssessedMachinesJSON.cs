// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using Newtonsoft.Json;
using System.Collections.Generic;

using Azure.Migrate.Explore.Common;

namespace Azure.Migrate.Explore.Models
{
    public class AVSAssessedMachinesJSON
    {
        [JsonProperty("value")]
        public List<AVSAssessedMachineValue> Values { get; set; }

        [JsonProperty("nextLink")]
        public string NextLink { get; set; }
    }

    public class AVSAssessedMachineValue
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("properties")]
        public AVSAssessedMachineProperty Properties { get; set; }
    }

    public class AVSAssessedMachineProperty
    {
        [JsonProperty("extendedDetails")]
        public AVSAssessedMachineExtendedDetails ExtendedDetails { get; set; }

        [JsonProperty("workloadType")]
        public string WorkloadType { get; set; }

        [JsonProperty("linkages")]
        public List<Linkage> Linkages { get; set; }

        [JsonProperty("recommendations")]
        public List<Recommendation> Recommendations { get; set; }
    }

    public class Linkage
    {
        [JsonProperty("linkageType")]
        public string LinkageType { get; set; }

        [JsonProperty("kind")]
        public string Kind { get; set; }

        [JsonProperty("armId")]
        public string ArmId { get; set; }
    }

    public class AVSAssessedMachineExtendedDetails
    {
        [JsonProperty("disks")]
        public List<AVSAssessedMachineDisk> Disks { get; set; }

        [JsonProperty("networkAdapters")]
        public List<AVSAssessedMachineNetworkAdapter> NetworkAdapters { get; set; }

        [JsonProperty("storageInUseGB")]
        public double StorageInUseGB { get; set; }

        [JsonProperty("bootType")]
        public string BootType { get; set; }

        [JsonProperty("operatingSystemType")]
        public string OperatingSystemType { get; set; }

        [JsonProperty("operatingSystemName")]
        public string OperatingSystemName { get; set; }

        [JsonProperty("operatingSystemVersion")]
        public string OperatingSystemVersion { get; set; }

        [JsonProperty("operatingSystemArchitecture")]
        public string OperatingSystemArchitecture { get; set; }

        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("datacenterManagementServerName")]
        public string DatacenterManagementServerName { get; set; }

        [JsonProperty("megabytesOfMemory")]
        public double MegabytesOfMemory { get; set; }

        [JsonProperty("numberOfCores")]
        public int NumberOfCores { get; set; }

        [JsonProperty("percentageCoresUtilization")]
        public double PercentageCoresUtilization { get; set; }

        [JsonProperty("percentageMemoryUtilization")]
        public double PercentageMemoryUtilization { get; set; }
    }

    public class Recommendation
    {
        [JsonProperty("migrationSuitability")]
        public MigrationSuitability MigrationSuitability { get; set; }
    }

    public class MigrationSuitability
    {
        [JsonProperty("readiness")]
        public string Readiness { get; set; }
    }

    public class AVSAssessedMachineDisk
    {
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

    public class AVSAssessedMachineNetworkAdapter
    {
        [JsonProperty("macAddress")]
        public string MacAddress { get; set; }

        [JsonProperty("ipAddresses")]
        public List<string> IpAddresses { get; set; }

        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        [JsonProperty("megabytesPerSecondReceived")]
        public double MegabytesPerSecondReceived { get; set; }

        [JsonProperty("megabytesPerSecondTransmitted")]
        public double MegabytesPerSecondTransmitted { get; set; }
    }
}
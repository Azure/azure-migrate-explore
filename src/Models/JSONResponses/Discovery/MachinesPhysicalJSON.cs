// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Azure.Migrate.Explore.Models
{
    public class MachinesPhysicalJSON
    {
        [JsonProperty("value")]
        public List<PhysicalMachinesValue> Values { get; set; }

        [JsonProperty("nextLink")]
        public string NextLink { get; set; }
    }

    public class PhysicalMachinesValue
    {
        [JsonProperty("properties")]
        public PhysicalMachinesProperty Properties { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }
    }

    public class PhysicalMachinesProperty
    {
        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        [JsonProperty("networkAdapters")]
        public List<PhysicalMachinesNetworkAdapter> NetworkAdapters { get; set; }

        [JsonProperty("numberOfApplications")]
        public int NumberOfApplications { get; set; }

        [JsonProperty("sqlDiscovery")]
        public PhysicalMachinesSqlDiscoveryInfo SqlDiscovery { get; set; }

        [JsonProperty("webAppDiscovery")]
        public PhysicalMachinesWebAppDiscoveryInfo WebAppDiscovery { get; set; }

        [JsonProperty("numberOfProcessorCore")]
        public int NumberOfProcessorCore { get; set; }

        [JsonProperty("allocatedMemoryInMB")]
        public double AllocatedMemoryInMB { get; set; }

        [JsonProperty("disks")]
        public List<PhysicalMachinesDisk> Disks { get; set; }

        [JsonProperty("operatingSystemDetails")]
        public PhysicalMachinesOperatingSystemDetailsInfo OperatingSystemDetails { get; set; }

        [JsonProperty("productSupportStatus")]
        public ProductSupportStatusInfo ProductSupportStatus { get; set; }

        [JsonProperty("firmware")]
        public string Firmware { get; set; }

        [JsonProperty("powerStatus")]
        public string PowerStatus { get; set; }

        [JsonProperty("updatedTimestamp")]
        public string UpdatedTimestamp { get; set; }

        [JsonProperty("createdTimestamp")]
        public string CreatedTimeStamp { get; set; }
    }

    public class PhysicalMachinesNetworkAdapter
    {
        [JsonProperty("ipAddressList")]
        public List<string> IpAddressList { get; set; }

        [JsonProperty("macAddress")]
        public string MacAddress { get; set; }
    }

    public class PhysicalMachinesSqlDiscoveryInfo
    {
        [JsonProperty("totalServerCount")]
        public int TotalServerCount { get; set; }
    }

    public class PhysicalMachinesWebAppDiscoveryInfo
    {
        [JsonProperty("totalWebApplicationCount")]
        public int TotalWebApplicationCount { get; set; }
    }

    public class PhysicalMachinesDisk
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("generatedId")]
        public string GeneratedId { get; set; }

        [JsonProperty("maxSizeInBytes")]
        public long MaxSizeInBytes { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("diskType")]
        public string DiskType { get; set; }

        [JsonProperty("lun")]
        public int Lun { get; set; }

        [JsonProperty("path")]
        public string Path { get; set; }
    }

    public class PhysicalMachinesOperatingSystemDetailsInfo
    {
        [JsonProperty("osName")]
        public string OSName { get; set; }
    }
}
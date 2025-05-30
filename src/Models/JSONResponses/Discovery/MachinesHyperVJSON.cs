// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Azure.Migrate.Explore.Models
{
    public class MachinesHyperVJSON
    {
        [JsonProperty("value")]
        public List<HyperVMachinesValue> Values { get; set; }

        [JsonProperty("nextLink")]
        public string NextLink { get; set; }
    }

    public class HyperVMachinesValue
    {
        [JsonProperty("properties")]
        public HyperVMachinesProperty Properties { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }
    }

    public class HyperVMachinesProperty
    {
        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        [JsonProperty("networkAdapters")]
        public List<HyperVMachinesNetworkAdapter> NetworkAdapters { get; set; }

        [JsonProperty("numberOfApplications")]
        public int NumberOfApplications { get; set; }

        [JsonProperty("sqlDiscovery")]
        public HyperVMachinesSqlDiscoveryInfo SqlDiscovery { get; set; }

        [JsonProperty("webAppDiscovery")]
        public HyperVMachinesWebAppDiscoveryInfo WebAppDiscovery { get; set; }

        [JsonProperty("numberOfProcessorCore")]
        public int NumberOfProcessorCore { get; set; }

        [JsonProperty("allocatedMemoryInMB")]
        public double AllocatedMemoryInMB { get; set; }

        [JsonProperty("disks")]
        public List<HyperVMachinesDisk> Disks { get; set; }

        [JsonProperty("operatingSystemDetails")]
        public HyperVMachinesOperatingSystemDetailsInfo OperatingSystemDetails { get; set; }

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

    public class HyperVMachinesNetworkAdapter
    {
        [JsonProperty("ipAddressList")]
        public List<string> IpAddressList { get; set; }

        [JsonProperty("macAddress")]
        public string MacAddress { get; set; }
    }

    public class HyperVMachinesSqlDiscoveryInfo
    {
        [JsonProperty("totalServerCount")]
        public int TotalServerCount { get; set; }
    }

    public class HyperVMachinesWebAppDiscoveryInfo
    {
        [JsonProperty("totalWebApplicationCount")]
        public int TotalWebApplicationCount { get; set; }
    }

    public class HyperVMachinesDisk
    {
        [JsonProperty("instanceId")]
        public string InstanceId { get; set; }

        [JsonProperty("vhdId")]
        public string VhdId { get; set; }

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

    public class HyperVMachinesOperatingSystemDetailsInfo
    {
        [JsonProperty("osName")]
        public string OSName { get; set; }
    }
}
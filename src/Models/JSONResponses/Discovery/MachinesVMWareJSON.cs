// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Azure.Migrate.Explore.Models
{
    public class MachinesVMWareJSON
    {
        [JsonProperty("value")]
        public List<VMWareMachinesValue> Values { get; set; }

        [JsonProperty("nextLink")]
        public string NextLink { get; set; }
    }

    public class VMWareMachinesValue
    {
        [JsonProperty("properties")]
        public VMWareMachinesProperty Properties { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }
    }

    public class VMWareMachinesProperty
    {
        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        [JsonProperty("networkAdapters")]
        public List<VMWareMachinesNetworkAdapter> NetworkAdapters { get; set; }

        [JsonProperty("numberOfApplications")]
        public int NumberOfApplications { get; set; }

        [JsonProperty("sqlDiscovery")]
        public VMWareMachinesSqlDiscoveryInfo SqlDiscovery { get; set; }

        [JsonProperty("webAppDiscovery")]
        public VMWareMachinesWebAppDiscoveryInfo WebAppDiscovery { get; set; }

        [JsonProperty("numberOfProcessorCore")]
        public int NumberOfProcessorCore { get; set; }

        [JsonProperty("allocatedMemoryInMB")]
        public double AllocatedMemoryInMB { get; set; }

        [JsonProperty("disks")]
        public List<VMWareMachinesDisk> Disks { get; set; }

        [JsonProperty("operatingSystemDetails")]
        public VMWareMachinesOperatingSystemDetailsInfo OperatingSystemDetails { get; set; }

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

    public class VMWareMachinesNetworkAdapter
    {
        [JsonProperty("ipAddressList")]
        public List<string> IpAddressList { get; set; }

        [JsonProperty("macAddress")]
        public string MacAddress { get; set; }
    }

    public class VMWareMachinesSqlDiscoveryInfo
    {
        [JsonProperty("totalServerCount")]
        public int TotalServerCount { get; set; }
    }

    public class VMWareMachinesWebAppDiscoveryInfo
    {
        [JsonProperty("totalWebApplicationCount")]
        public int TotalWebApplicationCount { get; set; }
    }

    public class VMWareMachinesDisk
    {
        [JsonProperty("uuid")]
        public string Uuid { get; set; }

        [JsonProperty("label")]
        public string Label { get; set; }

        [JsonProperty("diskProvisioningPolicy")]
        public string DiskProvisioningPolicy { get; set; }

        [JsonProperty("diskScrubbingPolicy")]
        public string DiskScrubbingPolicy { get; set; }

        [JsonProperty("diskMode")]
        public string DiskMode { get; set; }

        [JsonProperty("controllerType")]
        public string ControllerType { get; set; }

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

    public class VMWareMachinesOperatingSystemDetailsInfo
    {
        [JsonProperty("osName")]
        public string OSName { get; set; }
    }

    public class ProductSupportStatusInfo
    {
        [JsonProperty("supportStatus")]
        public string SupportStatus { get; set; }
    }
}
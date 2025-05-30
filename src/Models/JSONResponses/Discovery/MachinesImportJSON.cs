// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Azure.Migrate.Explore.Models
{
    public class MachinesImportJSON
    {
        [JsonProperty("value")]
        public List<ImportMachinesValue> Values { get; set; }

        [JsonProperty("nextLink")]
        public string NextLink { get; set; }
    }

    public class ImportMachinesValue
    {
        [JsonProperty("properties")]
        public ImportMachinesProperty Properties { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }
    }

    public class ImportMachinesProperty
    {
        [JsonProperty("firmware")]
        public string Firmware { get; set; }

        [JsonProperty("storageInUseGB")]
        public double? StorageInUseGB { get; set; }

        [JsonProperty("macAddress")]
        public string MacAddress { get; set; }

        [JsonProperty("ipAddresses")]
        public List<string> IPAddresses { get; set; }        

        [JsonProperty("numberOfNetworkAdapters")]
        public int? NumberOfNetworkAdapters { get; set; }

        [JsonProperty("disks")]
        public List<ImportMachinesDisk> Disks { get; set; }        

        [JsonProperty("numberOfProcessorCore")]
        public int? NumberOfProcessorCore { get; set; }

        [JsonProperty("allocatedMemoryInMB")]
        public double? AllocatedMemoryInMB { get; set; }

        [JsonProperty("operatingSystemDetails")]
        public ImportMachinesOperatingSystemDetailsInfo OperatingSystemDetails { get; set; }

        [JsonProperty("productSupportStatus")]
        public ProductSupportStatusInfo ProductSupportStatus { get; set; }

        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        [JsonProperty("createdTimestamp")]
        public string CreatedTimestamp { get; set; }

        [JsonProperty("updatedTimestamp")]
        public string UpdatedTimestamp { get; set; }
    }

    public class ImportMachinesDisk
    {        
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("diskType")]
        public string DiskType { get; set; }
    }

    public class ImportMachinesOperatingSystemDetailsInfo
    {
        [JsonProperty("osName")]
        public string OSName { get; set; }
    }
}
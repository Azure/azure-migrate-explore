// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
namespace Azure.Migrate.Explore.Models
{
    public class DiscoveryData
    {
        public string MachineName { get; set; }
        public string EnvironmentType { get; set; }
        public int SoftwareInventory { get; set; }
        public int SqlDiscoveryServerCount { get; set; }
        public bool IsSqlServicePresent { get; set; }
        public int WebAppCount { get; set; }
        public string OperatingSystem { get; set; }
        public int Cores { get; set; }
        public double MemoryInMB { get; set; }
        public int TotalDisks { get; set; }
        public string IpAddress { get; set; }
        public string MacAddress { get; set; }
        public int TotalNetworkAdapters { get; set; }
        public string BootType { get; set; }
        public string PowerStatus { get; set; }
        public string SupportStatus { get; set; }
        public string FirstDiscoveryTime { get; set; }
        public string LastUpdatedTime { get; set; }
        public string MachineId { get; set; }
        public double? StorageInUseGB { get; set; } = 0.0;
    }
}
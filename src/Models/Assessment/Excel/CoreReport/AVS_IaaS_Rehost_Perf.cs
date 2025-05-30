// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
namespace Azure.Migrate.Explore.Models
{
    public class AVS_IaaS_Rehost_Perf
    {
        public string MachineName { get; set; }
        public string AzureVMWareSolutionReadiness { get; set; }
        public string AzureVMWareSolutionReadiness_Warnings { get; set; }
        public string OperatingSystem { get; set; }
        public string OperatingSystemVersion { get; set; }
        public string OperatingSystemArchitecture { get; set; }
        public string BootType { get; set; }
        public int Cores { get; set; }
        public double MemoryInMB { get; set; }
        public double StorageInGB { get; set; }
        public double StorageInUseInGB { get; set; }
        public double DiskReadInOPS { get; set; }
        public double DiskWriteInOPS { get; set; }
        public double DiskReadInMBPS { get; set; }
        public double DiskWriteInMBPS { get; set; }
        public int NetworkAdapters { get; set; }
        public string IpAddresses { get; set; }
        public string MacAddresses { get; set; }
        public double NetworkInMBPS { get; set; }
        public double NetworkOutMBPS { get; set; }
        public string DiskNames { get; set; }
        public string GroupName { get; set; }
        public string MachineId { get; set; }
    }
}
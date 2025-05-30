// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
namespace Azure.Migrate.Explore.Models
{
    public class Clash_Report
    {
        public string MachineName { get; set; }
        public string Environment { get; set; }
        public string OperatingSystem { get; set; }
        public string BootType { get; set; }
        public string IPAddresses { get; set; }
        public string MacAddresses { get; set; }
        public int VM_IaaS_Server_Rehost_Perf_Clash { get; set; }
        public int SQL_IaaS_Instance_Rehost_Perf_Clash { get; set; }
        public int SQL_MI_PaaS_Clash { get; set; }
        public int SQL_IaaS_Server_Rehost_Perf_Clash { get; set; }
        public int WebApp_PaaS_Clash { get; set; }
        public int WebApp_IaaS_Server_Rehost_Perf_Clash { get; set; }
        public int VM_SS_IaaS_Server_Rehost_Perf_Clash { get; set; }
        public string VMHost { get; set; }
        public string MachineId { get; set; }
    }
}
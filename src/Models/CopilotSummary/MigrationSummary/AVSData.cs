// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using System.Collections.Generic;
using System.ComponentModel;

namespace Azure.Migrate.Explore.Models.CopilotSummary.MigrationSummary
{
    public class AvsData
    {
        [Description("Count of total vCenters scanned")]
        public int TotalvCenters { get; set; }

        [Description("Count of total hosts")]
        public int TotalHosts { get; set; }

        [Description("Average AVS CPU utilization at P95(%)")]
        public double AverageAvsCpuUtilizationP95 { get; set; }

        [Description("Average AVS RAM utilization at P95(%)")]
        public double AverageAvsRamUtilizationP95 { get; set; }

        [Description("Total AVS Compute and Licensing Cost")]
        public double TotalAvsComputeLicenseCost { get; set; }

        [Description("Total AVS Storage Cost")]
        public double TotalAvsStorageCost { get; set; }

        [Description("Total AVS Networking Cost")]
        public double TotalAvsNetworkCost { get; set; }

        [Description("Total AVS IT Staff Cost")]
        public double TotalAvsITStaffCost { get; set; }

        [Description("Total AVS Facilities Cost")]
        public double TotalAvsFacilitiesCost { get; set; }

        [Description("Estimated total AVS cost")]
        public double EstimatedTotalAvsCost { get; set; }

        [Description("Estimated total savings if customer moves its workloads to AVS. This is the difference between total on premises cost and total AVS cost.")]
        public double EstimatedTotalSavings { get; set; }

        [Description("AVS TCO")]
        public double AvsTCO { get; set; }

        [Description("AVS Savings Percentage")]
        public string AvsSavingsPercentage { get; set; }

        [Description("AVS External Storage in TB")]
        public string AvsExternalStorage { get; set; }

        [Description("Recommended Nodes")]
        public string RecommendedNodes { get; set; }

        [Description("Total RAM in TB for AVS")]
        public double TotalAvsNodeRamInTb { get; set; }

        [Description("Total Windows Server Licensing for AVS")]
        public double TotalWindowsServerLicensing { get; set; }

        [Description("Total SQL Server Licensing for AVS")]
        public double TotalSqlServerLicensing { get; set; }

        [Description("Total ESU Savings for AVS")]
        public double TotalEsuSavings { get; set; }

        [Description("Total AHUB Savings for AVS")]
        public double TotalAhubSavingsforAvs { get; set; }

        [Description("vCPU Oversubscription for AVS")]
        public string VCpuOverSubscription { get; set; }

        [Description("Memory Overcommit for AVS")]
        public string MemoryOverCommit { get; set; }

        [Description("Dedupe and Compression factor for AVS")]
        public double DedupeCompression { get; set; }

        [Description("Storage in use(GB)")]
        public double? StorageInUseGB { get; set; }
    }
}
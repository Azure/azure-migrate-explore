// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using System.ComponentModel;

namespace Azure.Migrate.Explore.Models.CopilotSummary.MigrationSummary
{
    public class MigrationIaasData
    {
        [Description("Azure IaaS Recommendations for various existing customer workloads to identify readiness for migration on IaaS targets with re-host strategy.")]
        public string IaasTransformationSummary { get; set; }

        [Description("Total servers suggested for migration to Azure VM")]
        public int TotalMachinesCount { get; set; }

        [Description("Count of servers ready for Migration to Azure")]
        public int ReadyMachinesCount { get; set; }

        [Description("Total cores")]
        public int TotalCores { get; set; }

        [Description("Total memory in GB")]
        public double TotalMemoryinGB { get; set; }

        [Description("Total storage in GB")]
        public double TotalStorageinGB { get; set; }

        [Description("Azure Cost for hosting ASP.NET webapps on Azure VM")]
        public double AspNetAzureVMCost { get; set; }

        [Description("Azure Cost for migrating General Purpose VM")]
        public double AzureVMCost { get; set; }

        [Description("Azure Cost for hosting SQL Servers on Azure VM")]
        public double SqlServerAzureVMCost { get; set; }

        [Description("Windows Compute and Licensing Cost without AHUB")]
        public double WindowsComputeLicenseCost { get; set; }

        [Description("Windows Compute and Licensing Cost with AHUB")]
        public double WindowsComputeLicenseCostAhub { get; set; }

        [Description("SQL Compute and Licensing Cost without AHUB")]
        public double SqlComputeLicenseCost { get; set; }

        [Description("SQL Compute and Licensing Cost with AHUB")]
        public double SqlComputeLicenseCostAhub { get; set; }

        [Description("Estimated cost by recommended offer PAYG+AHUB in Dev/Test")]
        public double AzureVMCostDevAhub { get; set; }

        [Description("Estimated cost by recommended offer 3RI+AHUB in Prod")]
        public double AzureVMCostProdAhub { get; set; }

        [Description("Estimated cost savings with Extended Security Updates (ESU)")]
        public double CostSavingsEsu { get; set; }

        [Description("Comparison between total compute cost for IaaS targets between various Azure offers. This includes right sized, ASP, AHUB and 3Year RI + AHUB compute costs")]
        public string AnnualComputeCostComparison { get; set; }

        [Description("Estimated annual Azure storage cost")]
        public double AnnualStorageCost { get; set; }

        [Description("Top Recommended VM SKUs by cost (Prod)(AHUB)")]
        public string VMTopRecommendationCostProd { get; set; }

        [Description("Top Recommended VM SKUs by cost (Dev/Test)(AHUB)")]
        public string VMTopRecommendationCostDev { get; set; }

        [Description("Top Recommended VM SKUs by count")]
        public string VMSkuRecommendedCount { get; set; }

        [Description("Top readiness warnings by count")]
        public string VMWarnings { get; set; }

        [Description("Standard HDD Drives Count")]
        public int StandardHddDrivesCount { get; set; }

        [Description("Cost of Standard HDD Drives")]
        public double StandardHddDrivesCost { get; set; }

        [Description("Standard SSD Drives Count")]
        public int StandardSsdDrivesCount { get; set; }

        [Description("Cost of Standard SSD Drives")]
        public double StandardSsdDrivesCost { get; set; }

        [Description("Premium Disks Count")]
        public int PremiumDisksCount { get; set; }

        [Description("Cost of Premium Disks")]
        public double PremiumDiskCost { get; set; }

        [Description("Ultra Drives Count")]
        public int UltraDrivesCount { get; set; }

        [Description("Cost of Ultra Disks")]
        public double UltraDisksCost { get; set; }
    }
}
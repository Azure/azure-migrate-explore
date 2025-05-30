// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using System.ComponentModel;

namespace Azure.Migrate.Explore.Models.CopilotSummary.MigrationSummary
{
    public class MigrationOnpremData
    {
        [Description("Selected Azure region for migration")]
        public string AzureRegion { get; set; }

        [Description("Symbol of the currency used in the report for cost calculations")]
        public string CurrencySymbol { get; set; }

        [Description("Count of total On-Prem servers")]
        public int TotalServers { get; set; }

        [Description("Count of On-Premises Windows servers")]
        public int WindowsServers { get; set; }

        [Description("Count of On-Premises Linux servers")]
        public int LinuxServers { get; set; }

        [Description("Count of On-Premises servers having Unknown OS")]
        public int UnknownServers { get; set; }

        [Description("Production Servers Count")] 
        public int ProductionServers { get; set; }

        [Description("Dev/Test Servers Count")] 
        public int DevServers { get; set; }

        [Description("Count of servers running webapps")] 
        public int WebappServers { get; set; }

        [Description("General-purpose servers")] 
        public int OtherWorkloads { get; set; }

        [Description("Servers with SQL Server")] 
        public int SqlServers { get; set; }

        [Description("SQL Server Instances")] 
        public int SqlServerInstances { get; set; }

        [Description("SQL Server Databases")] 
        public int SqlServerDatabases { get; set; }

        [Description("Count of ASP.NET webapps hosted on IIS server")] 
        public int WebappCount { get; set; }

        [Description("Total storage(TB) for SQL Servers")] 
        public double TotalStorageinTB { get; set; }

        [Description("Average CPU utilization at P95(%)")] 
        public double AverageCpuUtilizationP95 { get; set; }

        [Description("Average RAM utilization at P95(%)")] 
        public double AverageRamUtilizationP95 { get; set; }

        [Description("Estimated total On-premises cost covering Compute and Licensing,Storage,Security,Network,Facilities and IT Labor")] 
        public double EstimatedTotalOnpremCost { get; set; }

        [Description("Estimated total Azure cost covering Compute and Licensing,Storage,Security,Network,Facilities and IT Labor. This includes Total Azure IaaS Cost and Total Azure Paas Cost. This cost is after deducting savings from Azure Hybrid Benefits.")] 
        public double EstimatedTotalAzureCost { get; set; }

        [Description("Part of Total Azure Cost attributed by servers moving to Paas targets. This cost is after deducting savings from Azure Hybrid Benefits.")] 
        public double EstimatedTotalAzurePaasCost { get; set; }

        [Description("Part of Total Azure Cost attributed by servers moving to IaaS targets. This cost is after deducting savings from Azure Hybrid Benefits.")] 
        public double EstimatedTotalAzureIaasCost { get; set; }

        [Description("Estimated total savings if customer moves its workloads to Azure. This is the difference between total on premises cost and total Azure cost. This is inclusive of Azure Hybrid Benefits cost savings.")] 
        public double EstimatedTotalSavings { get; set; }

        [Description("On-Premises Compute and Licensing Cost")] 
        public double OnpremComputeLicenseCost { get; set; }

        [Description("On-Premises ESU License Cost")] 
        public double OnpremEsuLicenseCost { get; set; }

        [Description("On-Premises Storage Cost")] 
        public double OnpremStorageCost { get; set; }

        [Description("On-Premises Networking Cost")] 
        public double OnpremNetworkCost { get; set; }

        [Description("On-Premises Security Cost")] 
        public double OnpremSecurityCost { get; set; }

        [Description("On-Premises IT Staff Cost")] 
        public double OnpremITStaffCost { get; set; }

        [Description("On-Premises Facilities Cost")] 
        public double OnpremFacilitiesCost { get; set; }

        [Description("Total Azure Compute and Licensing Cost. This cost is after deducting savings from Azure Hybrid Benefits.")] 
        public double TotalAzureComputeLicenseCost { get; set; }

        [Description("Total Azure ESU License Cost")] 
        public double TotalAzureEsuLicenseCost { get; set; }

        [Description("Total Azure Storage Cost")] 
        public double TotalAzureStorageCost { get; set; }

        [Description("Total Azure Networking Cost")] 
        public double TotalAzureNetworkCost { get; set; }

        [Description("Total Azure Security Cost")] 
        public double TotalAzureSecurityCost { get; set; }

        [Description("Total Azure IT Staff Cost")] 
        public double TotalAzureITStaffCost { get; set; }

        [Description("Total Azure Facilities Cost")] 
        public double TotalAzureFacilitiesCost { get; set; }

        [Description("Azure Compute and Licensing Cost Savings")] 
        public double ComputeLicenseSavings { get; set; }

        [Description("Azure ESU License Cost Savings")] 
        public double EsuLicenseSavings { get; set; }

        [Description("Azure Storage Cost Savings")] 
        public double StorageCostSavings { get; set; }

        [Description("Azure Network Cost Savings")] 
        public double NetworkCostSavings { get; set; }

        [Description("Azure Security Cost Savings")] 
        public double SecurityCostSavings { get; set; }

        [Description("Azure IT Staff Cost Savings")] 
        public double ITStaffCostSavings { get; set; }

        [Description("Azure Facilities Cost Savings")] 
        public double FacilitiesCostSavings { get; set; }

        [Description("AHUB Savings for Windows Servers Licenses")] 
        public double WindowsServerLicenseSavingsAhub { get; set; }

        [Description("AHUB savings for SQL Server Licenses")] 
        public double SqlServerLicenseSavingsAhub { get; set; }
    }
}
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using System.Collections.Generic;
using System;
using System.IO;
namespace Azure.Migrate.Explore.Common
{
    public class OpportunityReportConstants
    {
        public const string AVS_IaaS_Rehost_Perf_TabName = "AVS_IaaS_Rehost_Perf";
        public static readonly string OpportunityReportDirectory = Path.Combine(AppContext.BaseDirectory, "Reports");
        public const string OpportunityReportName = "AzureMigrate_Assessment_Opportunity_Report.xlsx";
        public static readonly string OpportunityReportPath = Path.Combine(OpportunityReportDirectory, OpportunityReportName);

        public const string SQL_MI_Issues_and_Warnings_TabName = "SQL_MI_Issues_and_Warnings";
        public static readonly List<string> SQL_MI_Issues_and_Warnings_Columns = new List<string>
        {
            "Machine Name",
            "SQL Instance",
            "Migration Readiness Target",
            "Category",
            "Title",
            "Impacted Object Type",
            "Impacted Object Name",
            "User Databases",
            "Machine ID"
        };

        public const string SQL_MI_Opportunity_TabName = "SQL_MI_Opportunity";
        public static readonly List<string> SQL_MI_Opportunity_Columns = new List<string>
        {
            "Machine Name",
            "SQL Instance",
            "Environment",
            "Azure SQL MI Readiness",
            "Azure SQL MI Readiness - Warnings",
            "Azure SQL MI Readiness - Issues",
            "Recommended Deployment Type",
            "Azure SQL MI Configuration",
            "Monthly Compute Cost Estimate (Pay as you go)",
            "Monthly Compute Cost Estimate (Pay as you go + RI)",
            "Monthly Compute Cost Estimate (Pay as you go + AHUB)",
            "Monthly Compute Cost Estimate (Pay as you go + AHUB + RI)",
            "Monthly Storage Cost Estimate",
            "Monthly Security Cost Estimate",
            "User Databases",
            "SQL Edition",
            "SQL Version",
            "Support Status",
            "Total DB Size (in MB)",
            "Largest DB Size (in MB)",
            "vCores Allocated",
            "CPU Utilization (in %)",
            "Memory Utilization (in MB)",
            "Number of Disks",
            "Disk Read (in OPS)",
            "Disk Write (in OPS)",
            "Disk Read (in MBPS)",
            "Disk Write (in MBPS)",
            "Confidence Rating (in %)",
            "Azure SQL MI Configuration - Target Service Tier",
            "Azure SQL MI Configuration - Target Compute Tier",
            "Azure SQL MI Configuration - Target Hardware Type",
            "Azure SQL MI Configuration - Target vCores",
            "Azure SQL MI Configuration - Target Storage (in GB)",
            "Group Name",
            "Machine ID"
        };

        public const string VM_Opportunity_AsOnPrem_TabName = "VM_Opportunity_As-is";
        public static readonly List<string> VM_Opportunity_AsOnPrem_Columns = new List<string>
        {
            "Machine Name",
            "Environment",
            "Azure VM Readiness",
            "Recommended VM Size",
            "Monthly Compute Cost Estimate",
            "Monthly Storage Cost Estimate",
            "Monthly Security Cost Estimate",
            "Operating System",
            "Support Status",
            "VM Host",
            "Boot Type",
            "Cores",
            "Memory (in MB)",
            "Storage (in GB)",
            "Network Adapters",
            "IP Addresses",
            "MAC Addresses",
            "Disk Names",
            "Azure Disk Readiness",
            "Recommended Disk SKUs",
            "Standard HDD Disks",
            "Standard SSD Disks",
            "Premium Disks",
            "Ultra Disks",
            "Monthly Storage Cost for Standard HDD Disks",
            "Monthly Storage Cost for Standard SSD Disks",
            "Monthly Storage Cost for Premium Disks",
            "Monthly Storage Cost for Ultra Disks",
            "Group Name",
            "Machine ID"
        };

        public const string VM_Opportunity_Perf_TabName = "VM_Opportunity_Perf";
        public static readonly List<string> VM_Opportunity_Perf_Columns = new List<string>
        {
            "Machine Name",
            "Environment",
            "Azure VM Readiness",
            "Azure VM Readiness - Warnings",
            "Recommended VM Size",
            "Monthly Compute Cost Estimate (Pay as you go)",
            "Monthly Compute Cost Estimate (Pay as you go + RI)",
            "Monthly Compute Cost Estimate (Pay as you go + AHUB)",
            "Monthly Compute Cost Estimate (Pay as you go + AHUB + RI)",
            "Monthly Compute Cost Estimate (Pay as you go + ASP)",
            "Monthly Storage Cost Estimate",
            "Monthly Security Cost Estimate",
            "Operating System",
            "Support Status",
            "VM Host",
            "Boot Type",
            "Cores",
            "Memory (in MB)",
            "CPU Utilization (in %)",
            "Memory Utilization (in %)",
            "Storage (in GB)",
            "Network Adapters",
            "IP Addresses",
            "MAC Addresses",
            "Disk Names",
            "Azure Disk Readiness",
            "Recommended Disk SKUs",
            "Standard HDD Disks",
            "Standard SSD Disks",
            "Premium Disks",
            "Ultra Disks",
            "Monthly Storage Cost for Standard HDD Disks",
            "Monthly Storage Cost for Standard SSD Disks",
            "Monthly Storage Cost for Premium Disks",
            "Monthly Storage Cost for Ultra Disks",
            "Monthly Azure Site Recovery Cost Estimate",
            "Monthly Azure Backup Cost Estimate",
            "Group Name",
            "Machine ID"
        };

        public const string WebApp_Opportunity_TabName = "WebApp_Opportunity";
        public static readonly List<string> WebApp_Opportunity_Columns = new List<string>
        {
            "Machine Name",
            "Web Application Name",
            "Environment",
            "Azure App Service Readiness",
            "Azure App Service Readiness - Issues",
            "Azure Recommended Target",
            "Group Name",
            "Machine ID",
        };
    }
}
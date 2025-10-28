// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using System.Collections.Generic;
using System;
using System.IO;
namespace Azure.Migrate.Explore.Common
{
    public class CoreReportConstants
    {
        public const string CoreReportName = "AzureMigrate_Assessment_Core_Report.xlsx";
        public const string PropertiesTabName = "Properties";
        public static readonly List<string> PropertyList = new List<string>
        {
            "Tenant ID",
            "Subscription",
            "Resource Group Name",
            "Azure Migrate Project Name",
            "Assessment Site Name",
            "Workflow",
            "Business Proposal",
            "Target Region",
            "Currency",
            "Assessment Duration",
            "Optimization Preference",
            "Assess SQL Services",
            "vCPU Oversubscription",
            "Memory Overcommit",
            "Dedupe and Compression factor"
        };

        public const string Business_Case_TabName = "Business_Case";
        public static readonly List<string> Business_Case_Columns = new List<string>
        {
            "Category",
            "Azure Application Cost",
            "On-Premises Application Cost",
            "Azure Cots Application Cost",
            "On-Premises Cots Application Cost",
            "Azure Custom Application Cost",
            "On-Premises Custom Application Cost",
            "Azure Independent Workloads Cost",
            "On-Premises Independent Workloads Cost",
            "Total Azure Cost",
            "Total On-Premises Cost",
            "Azure Arc Enabled On-Premises Cost",
            "Future Azure Arc Enabled On-Premises Cost",
            "Future Cost",
            "Windows AHUB Savings",
            "Linux AHUB Savings",
            "SQL AHUB Savings",
            "SQL Azure Cost",
            "Machine Azure Cost",
            "Azure Arc Enabled On-Premises Cost",
            "Future Cost Including Azure Arc",
            "Future ESU Savings For 4 Years Including Azure Arc",
            "Future Management Cost Savings Including Azure Arc",
            "Future Security Cost Savings Including Azure Arc",
            "Azure Arc Services Cost",
            "Future Azure Arc Enabled On-Premises Cost",
            "Future Azure Arc Services Cost"
        };
        public static readonly List<string> Business_Case_RowTypes = new List<string>
        {
            "Total",
            "Compute",
            "License",
            "Storage",
            "Network",
            "Security",
            "IT Staff",
            "Facilities",
            "Management",
            "Ahub Savings",
            "Esu Savings",
            "Linux Ahub Savings"
        };

        public const string Cash_Flows_TabName = "Cash_Flows";
        public static readonly List<string> Cash_Flows_Years = new List<string>
        {
            "Year 0",
            "Year 1",
            "Year 2",
            "Year 3"
        };
        public static readonly List<string> Cash_Flows_CloudComputingServiceTypes = new List<string>
        {
            "Total",
        };
        public static readonly List<string> Cash_Flows_Types = new List<string>
        {
            "Current state Cash Flow",
            "Future state Cash Flow",
            "Savings"
        };

        public const string Financial_Summary_TabName = "Financial_Summary";
        public static readonly List<string> Financial_Summary_Columns = new List<string>
        {
            "Migration Strategy",
            "Workload",
            "Source Count",
            "Target Count",
            "Storage Cost",
            "Compute Cost",
            "Total Annual Cost"
        };

        public const string AVS_IaaS_Rehost_Perf_TabName = "AVS_IaaS_Rehost_Perf";
        public static readonly List<string> AVS_IaaS_Rehost_Perf_Columns = new List<string>
        {
            "Machine Name",
            "Azure VMWare Solution Readiness",
            "Azure VMWare Solution Readiness - Warnings",
            "Operating System",
            "Operating System Version",
            "Operating System Architecture",
            "Boot Type",
            "Cores",
            "Memory (in MB)",
            "Storage (in GB)",
            "Storage Utilization (in GB)",
            "Disk Read (in OPS)",
            "Disk Write (in OPS)",
            "Disk Read (in MBPS)",
            "Disk Write (in MBPS)",
            "Network Adapters",
            "IP Addresses",
            "MAC Addresses",
            "Network in (in MBPS)",
            "Network out (in MBPS)",
            "Disk Names",
            "Machine ID"
        };

        public const string AVS_Summary_TabName = "AVS_Summary";
        public static readonly List<string> AVS_Summary_Columns = new List<string>
        {
            "Subscription ID",
            "Resource Group",
            "Project Name",
            "Assessment Name",
            "Sizing Criterion",
            "Assessment Type",
            "Created on",
            "Total Machines Assessed",
            "Machines Ready",
            "Machines Ready with Conditions",
            "Machines not Ready",
            "Machines Readiness Unknown",
            "Recommended Number of Nodes",
            "Node Type",
            "Recommended Nodes",
            "Recommended FttRaidLevel",
            "Recommended External Storage",
            "Monthly Total Cost Estimate",
            "Monthly AVS External Storage Cost",
            "Monthly AVS Node Cost",
            "Monthly AVS External Network Cost",
            "Predicted CPU Utilization (in %)",
            "Predicted Memory Utilization (in %)",
            "Predicted Storage Utilization (in %)",
            "Number of CPU Cores - Available",
            "Memory - Available (in TB)",
            "Storage - Available (in TB)",
            "Number of CPU Cores - Used",
            "Memory - Used (in TB)",
            "Storage - Used (in TB)",
            "Number of CPU Cores - Free",
            "Memory - Free (in TB)",
            "Storage - Free (in TB)",
            "Confidence Rating"
        };

        public const string Decommissioned_Machines_TabName = "Decommissioned_Machines";
        public static readonly List<string> Decommissioned_Machines_Columns = new List<string>
        {
            "Machine Name",
            "Machine ID"
        };

        public const string YOY_Emissions_TabName = "YOY_Emissions";
        public static readonly List<string> YOY_Emissions_Columns = new List<string>
        {
            "Source",
            "Year 0",
            "Year 1",
            "Year 2",
            "Year 3"
        };

        public const string Emissions_Details_TabName = "Emissions_Details";
        public static readonly List<string> Emissions_Details_Columns = new List<string>
        {
            "Source",
            "Scope 1 Compute",
            "Scope 1 Storage",
            "Scope 2 Compute",
            "Scope 2 Storage",
            "Scope 3 Compute",
            "Scope 3 Storage"
        };
    }
}
using System.Collections.Generic;

namespace Azure.Migrate.Export.Common
{
    public class CoreReportConstants
    {
        public const string CoreReportDirectory = @".\Core-Report";
        public const string CoreReportName = @"AzureMigrate_Assessment_Core_Report.xlsx";
        public const string BackSlash = @"\";
        public const string CoreReportPath = CoreReportDirectory + BackSlash + CoreReportName;

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

        public const string All_VM_IaaS_Server_Rehost_Perf_TabName = "All_VM_IaaS_Server_Rehost_Perf";
        public static readonly List<string> All_VM_IaaS_Server_Rehost_Perf_Columns = new List<string>
        {
            "Machine Name",
            "Environment",
            "Azure VM Readiness",
            "Azure VM Readiness - Warnings",
            "Recommended VM Size",
            "Monthly Compute Cost Estimate (Pay as you go)",
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

        public const string SQL_All_Instances_TabName = "SQL_All_Instances";
        public static readonly List<string> SQL_All_Instances_Columns = new List<string>
        {
            "Machine Name",
            "SQL Instance",
            "Environment",
            "Azure SQL MI Readiness",
            "Azure SQL MI Readiness - Warnings",
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
            "Azure SQL MI Conifugration - Target Service Tier",
            "Azure SQL MI Conifugration - Target Compute Tier",
            "Azure SQL MI Conifugration - Target Hardware Type",
            "Azure SQL MI Conifugration - Target vCores",
            "Azure SQL MI Conifugration - Target Storage (in GB)",
            "Group Name",
            "Machine ID",
        };

        public const string SQL_IaaS_Instance_Rehost_Perf_TabName = "SQL_IaaS_Instance_Rehost_Perf";
        public static readonly List<string> SQL_IaaS_Instance_Rehost_Perf_Columns = new List<string>
        {
            "Machine Name",
            "SQL Instance",
            "Environment",
            "SQL Server on Azure VM Readiness",
            "SQL Server on Azure VM Readiness - Warnings",
            "SQL Server on Azure VM Configuration",
            "Monthly Compute Cost Estimate (Pay as you go)",
            "Monthly Compute Cost Estimate (Pay as you go + RI)",
            "Monthly Compute Cost Estimate (Pay as you go + AHUB)",
            "Monthly Compute Cost Estimate (Pay as you go + AHUB + RI)",
            "Monthly Compute Cost Estimate (Pay as you go + ASP)",
            "Monthly Storage Cost Estimate",
            "Monthly Security Cost Estimate",
            "SQL Server on Azure VM - Managed Disk Configuration",
            "User Databases",
            "Recommended Deployment Type",
            "Standard HDD Disks",
            "Standard SSD Disks",
            "Premium Disks",
            "Ultra Disks",
            "Monthly Storage Cost for Standard HDD Disks",
            "Monthly Storage Cost for Standard SSD Disks",
            "Monthly Storage Cost for Premium Disks",
            "Monthly Storage Cost for Ultra Disks",
            "SQL Edition",
            "SQL Version",
            "Support Status",
            "Total DB Size (in MB)",
            "Largest DB Size (in MB)",
            "vCores Allocated",
            "CPU Utilization (in %)",
            "Memory Utilization (in MB)",
            "Number Of Disks",
            "Disk Read (in OPS)",
            "Disk Write (in OPS)",
            "Disk Read (in MBPS)",
            "Disk Write (in MBPS)",
            "Confidence Rating (in %)",
            "SQL Server on Azure VM Configuration - Target vCores",
            "Monthly Azure Site Recovery Cost Estimate",
            "Monthly Azure Backup Cost Estimate",
            "Group Name",
            "Machine ID"
        };

        public const string SQL_IaaS_Server_Rehost_AsOnPrem_TabName = "SQL_IaaS_Server_Rehost_As-is";
        public static readonly List<string> SQL_IaaS_Server_Rehost_AsOnPrem_Columns = new List<string>
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

        public const string SQL_IaaS_Server_Rehost_Perf_TabName = "SQL_IaaS_Server_Rehost_Perf";
        public static readonly List<string> SQL_IaaS_Server_Rehost_Perf_Columns = new List<string>
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

        public const string SQL_MI_PaaS_TabName = "SQL_MI_PaaS";
        public static readonly List<string> SQL_MI_PaaS_Columns = new List<string>
        {
            "Machine Name",
            "SQL Instance",
            "Environment",
            "Azure SQL MI Readiness",
            "Azure SQL MI Readiness - Warnings",
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
            "Azure SQL MI Conifugration - Target Service Tier",
            "Azure SQL MI Conifugration - Target Compute Tier",
            "Azure SQL MI Conifugration - Target Hardware Type",
            "Azure SQL MI Conifugration - Target vCores",
            "Azure SQL MI Conifugration - Target Storage (in GB)",
            "Group Name",
            "Machine ID"
        };

        public const string VM_IaaS_Server_Rehost_AsOnPrem_TabName = "VM_IaaS_Server_Rehost_As-is";
        public static readonly List<string> VM_IaaS_Server_Rehost_AsOnPrem_Columns = new List<string>
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

        public const string VM_IaaS_Server_Rehost_Perf_TabName = "VM_IaaS_Server_Rehost_Perf";
        public static readonly List<string> VM_IaaS_Server_Rehost_Perf_Columns = new List<string>
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

        public const string VM_SS_IaaS_Server_Rehost_AsOnPrem_TabName = "VM_SS_IaaS_Server_Rehost_As-is";
        public static readonly List<string> VM_SS_IaaS_Server_Rehost_AsOnPrem_Columns = new List<string>
        {
            "Machine Name",
            "Environment",
            "Azure VM Readiness",
            "Recommended VM Size",
            "Monthly Compute Cost Estimate",
            "Monthly Storage Cost Estimate",
            "Operating System",
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

        public const string VM_SS_IaaS_Server_Rehost_Perf_TabName = "VM_SS_IaaS_Server_Rehost_Perf";
        public static readonly List<string> VM_SS_IaaS_Server_Rehost_Perf_Columns = new List<string>
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
            "Operating System",
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

        public const string WebApp_IaaS_Server_Rehost_AsOnPrem_TabName = "WebApp_IaaS_Server_Rehost_As-is";
        public static readonly List<string> WebApp_IaaS_Server_Rehost_AsOnPrem_Columns = new List<string>
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

        public const string WebApp_IaaS_Server_Rehost_Perf_TabName = "WebApp_IaaS_Server_Rehost_Perf";
        public static readonly List<string> WebApp_IaaS_Server_Rehost_Perf_Columns = new List<string>
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

        public const string WebApp_PaaS_TabName = "WebApp_PaaS";
        public static readonly List<string> WebApp_PaaS_Columns = new List<string>
        {
            "Machine Name",
            "Web Application Name",
            "Environment",
            "Azure App Service Readiness",
            "Azure App Service Readiness - Warnings",
            "App Service Plan",
            "Recommended SKU",
            "Monthly Compute Cost Estimate (Pay as you go)",
            "Monthly Compute Cost Estimate (Pay as you go + RI)",
            "Monthly Compute Cost Estimate (Pay as you go + ASP)",
            "Monthly Security Cost Estimate",
            "Azure Recommended Target",
            "Group Name",
            "Machine ID"
        };

        public const string Business_Case_TabName = "Business_Case";
        public static readonly List<string> Business_Case_Columns = new List<string>
        {
            "Category",
            "On-Prem IaaS Cost",
            "On-Prem PaaS Cost",
            "On-Prem AVS Cost",
            "Total On-Premises Cost",
            "Azure IaaS Cost",
            "Azure PaaS Cost",
            "AVS Cost",
            "Total Azure Cost",
            "Windows Server License Savings",
            "SQL Server License Savings",
            "ESU Savings"
        };
        public static readonly List<string> Business_Case_RowTypes = new List<string>
        {
            "Compute & Licensing",
            "ESU License",
            "Storage",
            "Network",
            "Security",
            "IT Staff",
            "Facilities"
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
            "IaaS",
            "PaaS",
            "AVS"
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
            "Group Name",
            "Machine ID"
        };

        public const string AVS_Summary_TabName = "AVS_Summary";
        public static readonly List<string> AVS_Summary_Columns = new List<string>
        {
            "Subscription ID",
            "Resource Group",
            "Project Name",
            "Group Name",
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
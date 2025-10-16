// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using System.Collections.Generic;
using System;
using System.IO;
namespace Azure.Migrate.Explore.Common
{
    public class DiscoveryReportConstants
    {
        public const string DiscoveryReportName = "AzureMigrate_Discovery_Report.xlsx";
        public const string PropertiesTabName = "Properties";
        public const string Discovery_Report_TabName = "Discovery_Report";
        public const string vCenterHost_Report_TabName = "vCenter_Host_Report";
        public const string ARGDataTabName = "ARGData";
        public const string WebAppSupportabilityDataTabName = "WebAppSupportability";
        public const string SoftwareInventoryInsightsTabName = "SoftwareInventoryInsights";

        public static readonly List<string> PropertiesList = new List<string>
        {
            "Tenant ID",
            "Subscription",
            "Resource Group",
            "Azure Migrate Project Name",
            "Discovery Site Name",
            "Workflow",
            "Source Appliances"
        };

        public static readonly List<string> DiscoveryReportColumns = new List<string>
        {
            "Machine Name",
            "Environment",
            "Software Inventory Count",
            "SQL Discovery Server Count",
            "Is SQL Service Present",
            "Web Applications Count",
            "Operating System",
            "Cores",
            "Memory (in MB)",
            "Total Disks",
            "IP Addresses",
            "MAC Addresses",
            "Total Network Adapters",
            "Boot Type",
            "Power Stats",
            "Support Status",
            "First Discovery Time",
            "Last Updated Time",
            "Machine ID",
            "Total Storage In Use (in GB)"
        };

        public static readonly List<string> VCenterHostReportColumns = new List<string>
        {
            "vCenters",
            "Hosts"
        };
    }
}
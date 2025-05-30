// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using System.Collections.Generic;
using System.IO;
using System;

namespace Azure.Migrate.Explore.Common
{
    public class ClashReportConstants
    {
        public static readonly string ClashReportDirectory = Path.Combine(AppContext.BaseDirectory, "Reports");
        public const string ClashReportName = "AzureMigrate_Assessment_Clash_Report.xlsx";
        public static readonly string ClashReportPath = Path.Combine(ClashReportDirectory, ClashReportName);
        public const string Clash_Report_TabName = "Clash_Report";

        public static readonly List<string> Clash_Report_Columns = new List<string>
        {
            "Machine Name",
            "Environment",
            "Operating System",
            "Boot Type",
            "IP Addresses",
            "MAC Addresses",
            "VM_IaaS_Server_Rehost_Perf",
            "SQL_IaaS_Instance_Rehost_Perf",
            "SQL_MI_PaaS",
            "SQL_IaaS_Server_Rehost_Perf",
            "WebApp_PaaS",
            "WebApp_IaaS_Server_Rehost_Perf",
            "VM_SS_IaaS_Server_Rehost_Perf",
            "VM Host",
            "Machine ID"
        };
    }
}
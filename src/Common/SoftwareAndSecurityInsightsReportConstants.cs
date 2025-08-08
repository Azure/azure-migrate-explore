using System.Collections.Generic;

namespace Azure.Migrate.Explore.Common
{
    public class SoftwareAndSecurityInsightsReportConstants
    {
        public const string SecurityAndSoftwareInsightsReportName = "Software_And_Security_Insights_Report.xlsx";
        public const string InventoryInsightsTabName = "Inventory Insights";
        public const string SoftwareInsightsTabName = "Software Insights";
        public const string SoftwareVulnerabilitiesTabName = "Software Vulnerabilities";
        public static readonly List<string> InventoryInsightsColumns = new List<string>
        {
            "Workload Name",
            "Category",
            "Type",
            "Has Patch Management Software",
            "Missing Security Software",
            "Has Pending Updates",
            "Support Status",
            "Has Vulnerabilities"
        };
        public static readonly List<string> SoftwareInsightsColumns = new List<string>
        {
            "Name",
            "Category",
            "Sub Category",
            "Support Status",
            "Version",
            "Servers Count",
            "Vulnerabilities",
            "Recommendations"
        };
        public static readonly List<string> SoftwareVulnerabilitiesColumns = new List<string>
        {
            "SoftwareName",
            "Version",
            "Vulnerability",
            "Severity"
        };
    }
}

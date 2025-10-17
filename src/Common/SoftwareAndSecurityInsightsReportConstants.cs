using System.Collections.Generic;

namespace Azure.Migrate.Explore.Common
{
    public class SoftwareAndSecurityInsightsReportConstants
    {
        public const string SecurityAndSoftwareInsightsReportName = "Software_And_Security_Insights_Report.xlsx";
        public const string InventoryInsightsTabName = "Inventory Insights";
        public const string SoftwareInsightsTabName = "Software Insights";
        public const string SoftwareVulnerabilitiesTabName = "Software Vulnerabilities";
        public const string PendingUpdatesTabName = "Pending Updates";
        public static readonly List<string> InventoryInsightsColumns = new List<string>
        {
            "Workload Name",
            "Operating System",
            "Category",
            "Type",
            "Support Status",
            "Vulnerability Count",
            "Critical Vulnerability Count",
            "Pending Update Count",
            "End Of Support Software Count",
            "Has Security Software",
            "Has Patching Software"
        };
        public static readonly List<string> SoftwareInsightsColumns = new List<string>
        {
            "Name",
            "Provider",
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
            "CveId",
            "Severity",
            "Servers Count"
        };
        public static readonly List<string> PendingUpdatesTabNameColumns = new List<string>
        {
            "Update Name",
            "Servers"
        };
    }
}

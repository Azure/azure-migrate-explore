using ClosedXML.Excel;
using System.Collections.Generic;
using Azure.Migrate.Explore.Models;
using AzureMigrateExplore.Models;
using Azure.Migrate.Explore.Common;

namespace AzureMigrateExplore.Excel
{
    public class ExportSecurityAndSoftwareInsightReport
    {
        private readonly List<InventoryInsights> Inventory_Insights_List;
        private readonly List<SoftwareInsights> Software_Insights_List;
        private readonly List<SoftwareVulnerabilities> Software_Vulnerabilities_List;

        XLWorkbook SecurityAndSoftwareInsightsWb;

        public ExportSecurityAndSoftwareInsightReport
            (
                List<InventoryInsights> inventory_Insights_List,
                List<SoftwareInsights> software_Insights_List,
                List<SoftwareVulnerabilities> software_Vulnerabilities_List
            )
        {
            Inventory_Insights_List = inventory_Insights_List;
            Software_Insights_List = software_Insights_List;
            Software_Vulnerabilities_List = software_Vulnerabilities_List;
            SecurityAndSoftwareInsightsWb = new XLWorkbook();
        }

        public void GenerateSecurityAndSoftwareInsightReportExcel()
        {
            GenerateInventoryInsightsWorksheet();
            GenerateSoftwareInsightsWorksheet();
            GenerateSoftwareVulnerabilitiesWorksheet();
            SecurityAndSoftwareInsightsWb.SaveAs(UtilityFunctions.GetReportsDirectory() + "\\" + SoftwareAndSecurityInsightsReportConstants.SecurityAndSoftwareInsightsReportName);
        }

        public void GenerateInventoryInsightsWorksheet()
        {
            var dataWs = SecurityAndSoftwareInsightsWb.Worksheets.Add(SoftwareAndSecurityInsightsReportConstants.InventoryInsightsTabName, 1);
            UtilityFunctions.AddColumnHeadersToWorksheet(dataWs, SoftwareAndSecurityInsightsReportConstants.InventoryInsightsColumns);
            if (Inventory_Insights_List != null && Inventory_Insights_List.Count > 0)
                dataWs.Cell(2, 1).InsertData(Inventory_Insights_List);
        }

        public void GenerateSoftwareInsightsWorksheet()
        {
            var dataWs = SecurityAndSoftwareInsightsWb.Worksheets.Add(SoftwareAndSecurityInsightsReportConstants.SoftwareInsightsTabName, 2);
            UtilityFunctions.AddColumnHeadersToWorksheet(dataWs, SoftwareAndSecurityInsightsReportConstants.SoftwareInsightsColumns);
            if (Software_Insights_List != null && Software_Insights_List.Count > 0)
                dataWs.Cell(2, 1).InsertData(Software_Insights_List);
        }

        public void GenerateSoftwareVulnerabilitiesWorksheet()
        {
            var dataWs = SecurityAndSoftwareInsightsWb.Worksheets.Add(SoftwareAndSecurityInsightsReportConstants.SoftwareVulnerabilitiesTabName, 3);
            UtilityFunctions.AddColumnHeadersToWorksheet(dataWs, SoftwareAndSecurityInsightsReportConstants.SoftwareVulnerabilitiesColumns);
            if (Software_Vulnerabilities_List != null && Software_Vulnerabilities_List.Count > 0)
                dataWs.Cell(2, 1).InsertData(Software_Vulnerabilities_List);
        }
    }
}

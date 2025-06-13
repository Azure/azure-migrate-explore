// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.IO;

using Azure.Migrate.Explore.Common;
using Azure.Migrate.Explore.Models;

namespace Azure.Migrate.Explore.Excel
{
    public class ImportOpportunityReport
    {
        private Logger.LogHandler logger;
        public List<SQL_MI_Issues_and_Warnings> SqlMIIssuesAndWarningsList { get; private set; }
        public List<SQL_MI_Opportunity> SqlMIOpportunityList { get; private set; }
        public List<WebApp_Opportunity> WebappOpportunityList { get; private set; }
        public List<VM_Opportunity_Perf> VmOpportunityPerfList { get; private set; }
        public List<VM_Opportunity_AsOnPrem> VmOpportunityAsOnpremList { get; private set; }
        private Dictionary<string, List<string>> opportunityReportWorksheetNameToColumns;
        public ImportOpportunityReport(Logger.LogHandler logger)
        {
            this.logger = logger;
            SqlMIIssuesAndWarningsList = new List<SQL_MI_Issues_and_Warnings>();
            SqlMIOpportunityList = new List<SQL_MI_Opportunity>();
            WebappOpportunityList = new List<WebApp_Opportunity>();
            VmOpportunityPerfList = new List<VM_Opportunity_Perf>();
            VmOpportunityAsOnpremList = new List<VM_Opportunity_AsOnPrem>();
            opportunityReportWorksheetNameToColumns = new Dictionary<string, List<string>>
            {
                { "VM_Opportunity_Perf", OpportunityReportConstants.VM_Opportunity_Perf_Columns },
            };
        }

        public void ImportOpportunityReportData()
        {
            UtilityFunctions.ValidateReportPresence(UtilityFunctions.GetReportsDirectory(), UtilityFunctions.GetReportsDirectory() +
                "\\" + OpportunityReportConstants.OpportunityReportName);
            logger.LogInformation($"Validated the presence of file {UtilityFunctions.GetReportsDirectory() + "\\" + 
                OpportunityReportConstants.OpportunityReportName}");

            using (var fileStream = new FileStream(UtilityFunctions.GetReportsDirectory() + "\\" + 
                OpportunityReportConstants.OpportunityReportName, FileMode.Open, FileAccess.Read)) // only read the data
            {
                using (var opportunityReportWb = new XLWorkbook(fileStream))
                {
                    ValidateOpportunityReport(opportunityReportWb);

                    LoadExcelData(opportunityReportWb);
                }
            }
        }           

        private void ValidateOpportunityReport(XLWorkbook opportunityReportWb)
        {
            int worksheetNumber = 4;
            foreach (KeyValuePair<string, List<string>> kvp in opportunityReportWorksheetNameToColumns)
            {
                ValidateEachWorksheet(opportunityReportWb, kvp, worksheetNumber);                
                worksheetNumber++;
            }
        }

        private void LoadExcelData(XLWorkbook opportunityReportWb)
        {
            Load_VM_Opportunity_Perf_Worksheet(opportunityReportWb);
        }

        private void ValidateEachWorksheet(XLWorkbook opportunityReportWb, KeyValuePair<string, List<string>> kvp, int worksheetNumber)
        {
            if (opportunityReportWb == null)
                throw new ArgumentNullException($"{OpportunityReportConstants.OpportunityReportName} provided is null");

            var opportunityReportDataSheet = opportunityReportWb.Worksheet(worksheetNumber);
            var headerRow = opportunityReportDataSheet.Row(1);

            logger.LogInformation($"Validating columns of worksheet: {kvp.Key} in opportunity report");

            var columns = kvp.Value;

            for (int i = 0; i < columns.Count; i++)
            {
                var column = columns[i];
                if (string.IsNullOrEmpty(column))
                {
                    throw new Exception("Encountered empty column name from app settings");
                }

                var cell = headerRow.Cell(i + 1);

                string sheetColumn = cell.GetValue<string>();

                if (string.IsNullOrEmpty(sheetColumn))
                {
                    throw new Exception($"Expected column {column}, but received empty column name");
                }

                if (!column.Equals(sheetColumn))
                {
                    throw new Exception($"Expected column {column}, but encountered column {sheetColumn}");
                }
            }

            logger.LogInformation($"Validated columns  worksheet: {kvp.Key} in opportunity report excel");
        }

        private void Load_VM_Opportunity_Perf_Worksheet(XLWorkbook opportunityReportWb)
        {
            logger.LogInformation($"Loading data from worksheet: VM_Opportunity_Perf in opportunity report");

            var opportunityReportDataSheet = opportunityReportWb.Worksheet(4);

            int i = 2; // 1st row contains headers
            var row = opportunityReportDataSheet.Row(i);

            while (!row.IsEmpty())
            {               
                VM_Opportunity_Perf obj = new VM_Opportunity_Perf();

                obj.MachineName = row.Cell(1).GetValue<string>();
                obj.Environment = row.Cell(2).GetValue<string>();
                obj.AzureVMReadiness = row.Cell(3).GetValue<string>();
                obj.AzureVMReadiness_Warnings = row.Cell(4).GetValue<string>();
                obj.RecommendedVMSize = row.Cell(5).GetValue<string>();
                obj.MonthlyComputeCostEstimate = row.Cell(6).GetValue<double>();
                obj.MonthlyComputeCostEstimate_RI3year = row.Cell(7).GetValue<double>();
                obj.MonthlyComputeCostEstimate_AHUB = row.Cell(8).GetValue<double>();
                obj.MonthlyComputeCostEstimate_AHUB_RI3year = row.Cell(9).GetValue<double>();
                obj.MonthlyComputeCostEstimate_ASP3year = row.Cell(10).GetValue<double>();
                obj.MonthlyStorageCostEstimate = row.Cell(11).GetValue<double>();
                obj.MonthlySecurityCostEstimate = row.Cell(12).GetValue<double>();
                obj.OperatingSystem = row.Cell(13).GetValue<string>();
                obj.SupportStatus = row.Cell(14).GetValue<string>();
                obj.VMHost = row.Cell(15).GetValue<string>();
                obj.BootType = row.Cell(16).GetValue<string>();
                obj.Cores = row.Cell(17).GetValue<int>();
                obj.MemoryInMB = row.Cell(18).GetValue<double>();
                obj.CpuUtilizationPercentage = row.Cell(19).GetValue<double>();
                obj.MemoryUtilizationPercentage = row.Cell(20).GetValue<double>();
                obj.StorageInGB = row.Cell(21).GetValue<double>();
                obj.NetworkAdapters = row.Cell(22).GetValue<int>();
                obj.IpAddresses = row.Cell(23).GetValue<string>();
                obj.MacAddresses = row.Cell(24).GetValue<string>();
                obj.DiskNames = row.Cell(25).GetValue<string>();
                obj.AzureDiskReadiness = row.Cell(26).GetValue<string>();
                obj.RecommendedDiskSKUs = row.Cell(27).GetValue<string>();
                obj.StandardHddDisks = row.Cell(28).GetValue<int>();
                obj.StandardSsdDisks = row.Cell(29).GetValue<int>();
                obj.PremiumDisks = row.Cell(30).GetValue<int>();
                obj.UltraDisks = row.Cell(31).GetValue<int>();
                obj.MonthlyStorageCostForStandardHddDisks = row.Cell(32).GetValue<double>();
                obj.MonthlyStorageCostForStandardSsdDisks = row.Cell(33).GetValue<double>();
                obj.MonthlyStorageCostForPremiumDisks = row.Cell(34).GetValue<double>();
                obj.MonthlyStorageCostForUltraDisks = row.Cell(35).GetValue<double>();
                obj.MonthlyAzureSiteRecoveryCostEstimate = row.Cell(36).GetValue<double>();
                obj.MonthlyAzureBackupCostEstimate = row.Cell(37).GetValue<double>();
                obj.GroupName = row.Cell(38).GetValue<string>();
                obj.MachineId = row.Cell(39).GetValue<string>();

                VmOpportunityPerfList.Add(obj);

                i += 1;
                row = opportunityReportDataSheet.Row(i);
            }

            logger.LogInformation($"Updated VM_Opportunity_Perf data model of core report");
        }        
    }
}
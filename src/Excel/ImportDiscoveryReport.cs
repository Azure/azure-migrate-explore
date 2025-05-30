// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using ClosedXML.Excel;
using System;
using System.IO;
using System.Collections.Generic;

using Azure.Migrate.Explore.Common;
using Azure.Migrate.Explore.Models;
using DocumentFormat.OpenXml.Bibliography;

namespace Azure.Migrate.Explore.Excel
{
    public class ImportDiscoveryReport
    {
        private Logger.LogHandler logger;
        private List<DiscoveryData> DiscoveredData;
        public List<vCenterHostDiscovery> VcenterHostData;
        private Dictionary<string, List<string>> discoveryReportWorksheetNameToColumns;
        public ImportDiscoveryReport(Logger.LogHandler logger, List<DiscoveryData> discoveredData, List<vCenterHostDiscovery> vCenterHostData)
        {
            this.logger = logger;
            DiscoveredData = discoveredData;
            VcenterHostData = vCenterHostData;
        }

        public void ImportDiscoveryData()
        {

            discoveryReportWorksheetNameToColumns = new Dictionary<string, List<string>>
            {
                { "Properties", DiscoveryReportConstants.PropertiesList },
                { "Discovery_Report", DiscoveryReportConstants.DiscoveryReportColumns},
                { "vCenter_Host_Report", DiscoveryReportConstants.VCenterHostReportColumns}
            };

            ValidateDiscoveryReportPresence();

            using (var fileStream = new FileStream(DiscoveryReportConstants.DiscoveryReportPath, FileMode.Open, FileAccess.Read)) // only read the data
            {
                using (var discoveryWb = new XLWorkbook(fileStream))
                {
                    ValidateDiscoveryReport(discoveryWb);

                    if (DiscoveredData == null)
                        DiscoveredData = new List<DiscoveryData>();

                    if (VcenterHostData == null)
                        VcenterHostData = new List<vCenterHostDiscovery>();

                    LoadExcelData(discoveryWb);
                }
            }
        }

        private void ValidateDiscoveryReportPresence()
        {
            if (!Directory.Exists(DiscoveryReportConstants.DiscoveryReportDirectory))
                throw new Exception($"Discovery report directory {DiscoveryReportConstants.DiscoveryReportDirectory} not found.");
            if (!File.Exists(DiscoveryReportConstants.DiscoveryReportPath))
                throw new Exception($"Discovery report file {DiscoveryReportConstants.DiscoveryReportPath} not found");

            logger.LogInformation("Validated the presence of Discovery Report");
        }

        private void ValidateDiscoveryReport(XLWorkbook discoveryWb)
        {
            int worksheetNumber = 1;
            foreach (KeyValuePair<string, List<string>> kvp in discoveryReportWorksheetNameToColumns)
            {
                ValidateEachWorksheet(discoveryWb, kvp, worksheetNumber);
                worksheetNumber++;
            }
        }

        private void ValidateEachWorksheet(XLWorkbook discoveryWb, KeyValuePair<string, List<string>> kvp, int worksheetNumber)
        {
            if (discoveryWb == null)
                throw new ArgumentNullException($"{DiscoveryReportConstants.DiscoveryReportName} provided is null");

            var discoveryReportDataSheet = discoveryWb.Worksheet(worksheetNumber);

            var headerRow = discoveryReportDataSheet.Row(1);

            logger.LogInformation($"Validating columns of worksheet: {kvp.Key} in discovery report");

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
                    // Check for specific case where the column name is empty
                    // and the column is "Total Storage In Use (in GB)"
                    if (column == "Total Storage In Use (in GB)")
                    {
                        logger.LogWarning($"Column '{column}' is null or empty in the worksheet '{kvp.Key}'");
                        continue;
                    }                   
                    throw new Exception($"Expected column {column}, but received empty column name");
                }

                if (!column.Equals(sheetColumn))
                {
                    throw new Exception($"Expected column {column}, but encountered column {sheetColumn}");
                }
            }

            logger.LogInformation($"Validated columns of worksheet: {kvp.Key} in discovery report excel");
        }

        private void LoadExcelData(XLWorkbook discoveryWb)
        {
            logger.LogInformation("Loading data from discovery report");

            var discoveryDataSheet = discoveryWb.Worksheet(2);

            int i = 2;
            var row = discoveryDataSheet.Row(i);

            while (!row.IsEmpty())
            {
                i += 1;
                DiscoveryData obj = new DiscoveryData();

                obj.MachineName = row.Cell(1).GetValue<string>();
                obj.EnvironmentType = row.Cell(2).GetValue<string>();
                obj.SoftwareInventory = row.Cell(3).GetValue<int>();
                obj.SqlDiscoveryServerCount = row.Cell(4).GetValue<int>();
                obj.IsSqlServicePresent = row.Cell(5).GetValue<bool>();
                obj.WebAppCount = row.Cell(6).GetValue<int>();
                obj.OperatingSystem = row.Cell(7).GetValue<string>();
                obj.Cores = row.Cell(8).GetValue<int>();
                obj.MemoryInMB = row.Cell(9).GetValue<double>();
                obj.TotalDisks = row.Cell(10).GetValue<int>();
                obj.IpAddress = row.Cell(11).GetValue<string>();
                obj.MacAddress = row.Cell(12).GetValue<string>();
                obj.TotalNetworkAdapters = row.Cell(13).GetValue<int>();
                obj.BootType = row.Cell(14).GetValue<string>();
                obj.PowerStatus = row.Cell(15).GetValue<string>();
                obj.SupportStatus = row.Cell(16).GetValue<string>();
                obj.FirstDiscoveryTime = row.Cell(17).GetValue<string>();
                obj.LastUpdatedTime = row.Cell(18).GetValue<string>();
                obj.MachineId = row.Cell(19).GetValue<string>();
                obj.StorageInUseGB = row.Cell(20).GetValue<double?>() ?? 0;

                DiscoveredData.Add(obj);

                row = discoveryDataSheet.Row(i);
            }

            LoadvCenterHostData(discoveryWb);

            logger.LogInformation($"Updated discovery data model with {DiscoveredData.Count} machines");
        }

        private void LoadvCenterHostData(XLWorkbook discoveryWb)
        {
            logger.LogInformation("Loading data from vCenter Host report");

            var vCenterHostDataSheet = discoveryWb.Worksheet(3);

            int i = 2;
            var row = vCenterHostDataSheet.Row(i);
            vCenterHostDiscovery obj = new vCenterHostDiscovery();
            obj.vCenters = row.Cell(1).GetValue<int>();
            obj.Hosts = row.Cell(2).GetValue<int>();

            VcenterHostData.Add(obj);
        }
    }
}
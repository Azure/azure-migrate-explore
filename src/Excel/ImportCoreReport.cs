// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.IO;

using Azure.Migrate.Explore.Common;
using Azure.Migrate.Explore.Models;
using Azure.Migrate.Explore.Logger;

namespace Azure.Migrate.Explore.Excel
{
    public class ImportCoreReport
    {
        private LogHandler logger;
        public CoreProperties CorePropertiesObj { get; private set; }
        public Business_Case BusinessCaseObj { get; private set; }
        public Cash_Flows CashFlowsData { get; private set; }
        public List<AVS_Summary> AvsSummaryList { get; private set; }
        public List<AVS_IaaS_Rehost_Perf> AvsIaaSRehostPerfList { get; private set; }
        public List<Decommissioned_Machines> DecommissionedMachinesList { get; private set; }
        private Dictionary<string, List<string>> coreReportWorksheetNameToColumns;

        public ImportCoreReport(LogHandler logger)
        {
            this.logger = logger;
            CorePropertiesObj = new CoreProperties();
            BusinessCaseObj = new Business_Case();
            CashFlowsData = new Cash_Flows();
            DecommissionedMachinesList = new List<Decommissioned_Machines>();
            AvsSummaryList = new List<AVS_Summary>();
            AvsIaaSRehostPerfList = new List<AVS_IaaS_Rehost_Perf>();

            coreReportWorksheetNameToColumns = new Dictionary<string, List<string>>
            {
                { "Properties", CoreReportConstants.PropertyList },
                { "Business_Case", CoreReportConstants.Business_Case_Columns },
                { "Cash_Flows", CoreReportConstants.Cash_Flows_Years },
                { "AVS_Summary", CoreReportConstants.AVS_Summary_Columns },
                { "AVS_IaaS_Rehost_Perf", CoreReportConstants.AVS_IaaS_Rehost_Perf_Columns },
                { "Decommissioned_Machines", CoreReportConstants.Decommissioned_Machines_Columns },
            };
        }

        public void ImportCoreReportData()
        {
            UtilityFunctions.ValidateReportPresence(UtilityFunctions.GetReportsDirectory(), UtilityFunctions.GetReportsDirectory()
                + "\\" + CoreReportConstants.CoreReportName);
            logger.LogInformation($"Validated the presence of file {UtilityFunctions.GetReportsDirectory() + "\\" 
                + CoreReportConstants.CoreReportName}");

            using (var fileStream = new FileStream(UtilityFunctions.GetReportsDirectory() + "\\"
                + CoreReportConstants.CoreReportName, FileMode.Open, FileAccess.Read)) // only read the data
            {
                using (var coreReportWb = new XLWorkbook(fileStream))
                {
                    ValidateCoreReport(coreReportWb);

                    LoadExcelData(coreReportWb);
                }
            }
        }        

        private void ValidateCoreReport(XLWorkbook coreReportWb)
        {
            int worksheetNumber = 1;
            foreach (KeyValuePair<string, List<string>> kvp in coreReportWorksheetNameToColumns)
            {
                //if (worksheetNumber > 15)
                //{
                //    worksheetNumber += 2;
                //}
                ValidateEachWorksheet(coreReportWb, kvp, worksheetNumber);
                worksheetNumber++;
            }
        }

        private void LoadExcelData(XLWorkbook coreReportWb)
        {
            Load_Properties_Worksheet(coreReportWb);
            Load_Business_Case_Worksheet(coreReportWb);
            Load_Decommissioned_Machines_Worksheet(coreReportWb);
            Load_AVS_Summary_Worksheet(coreReportWb);
            Load_AVS_IaaS_Rehost_Perf_Worksheet(coreReportWb);
        }

        private void ValidateEachWorksheet(XLWorkbook coreReportWb, KeyValuePair<string,List<string>> kvp, int worksheetNumber)
        {
            if (worksheetNumber == 3 || worksheetNumber == 8 || worksheetNumber == 11 || worksheetNumber == 13)
                return;

            if (coreReportWb == null)
                throw new ArgumentNullException($"{CoreReportConstants.CoreReportName} provided is null");

            var coreReportDataSheet = coreReportWb.Worksheet(worksheetNumber);

            if ( kvp.Key == "Business_Case")
            {
                var headerColumn = coreReportDataSheet.Column(1);

                logger.LogInformation($"Validating rows of worksheet: {kvp.Key} in core report");

                List<string> rows = CoreReportConstants.Business_Case_RowTypes;

                for (int i = 0; i < rows.Count; i++)
                {
                    var row = rows[i];
                    if (string.IsNullOrEmpty(row))
                    {
                        throw new Exception("Encountered empty row name from app settings");
                    }

                    var cell = headerColumn.Cell(i + 2);

                    string sheetRow = cell.GetValue<string>();

                    if (string.IsNullOrEmpty(sheetRow))
                    {
                        throw new Exception($"Expected row {row}, but received empty row name");
                    }

                    if (!row.Equals(sheetRow))
                    {
                        throw new Exception($"Expected row {row}, but encountered row {sheetRow}");
                    }
                }

                logger.LogInformation($"Validated rows of worksheet: {kvp.Key} in core report excel");
            }

            var headerRow = coreReportDataSheet.Row(1);

            logger.LogInformation($"Validating columns of worksheet: {kvp.Key} in core report");

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

            logger.LogInformation($"Validated columns of worksheet: {kvp.Key} in core report excel");
        }

        private void Load_Properties_Worksheet(XLWorkbook coreReportWb)
        {
            logger.LogInformation($"Loading data from worksheet: Properties in core report");

            var coreReportDataSheet = coreReportWb.Worksheet(1);

            int i = 2; // 1st row contains headers
            var row = coreReportDataSheet.Row(i);

            while (!row.IsEmpty())
            {               
                CoreProperties obj = new CoreProperties();

                obj.TenantId = row.Cell(1).GetValue<string>();
                obj.Subscription = row.Cell(2).GetValue<string>();
                obj.TargetRegion = row.Cell(8).GetValue<string>();
                obj.Currency = row.Cell(9).GetValue<string>();
                obj.OptimizationPreference = row.Cell(11).GetValue<string>();
                obj.VCpuOverSubscription = row.Cell(13).GetValue<string>();
                obj.MemoryOverCommit = row.Cell(14).GetValue<string>();
                obj.DedupeCompression = row.Cell(15).GetValue<double>();
                CorePropertiesObj = obj;

                i += 1;
                row = coreReportDataSheet.Row(i);
            }

            logger.LogInformation($"Updated Properties data model of core report");
        }

        private void Load_Business_Case_Worksheet(XLWorkbook coreReportWb)
        {
            logger.LogInformation($"Loading data from worksheet: Business_Case in core report");

            var coreReportDataSheet = coreReportWb.Worksheet(2);

            Business_Case obj = new Business_Case();

            obj.TotalOnPremisesCost.ComputeLicenseCost = coreReportDataSheet.Cell(2,5).GetValue<double>();
            obj.TotalOnPremisesCost.EsuLicenseCost = coreReportDataSheet.Cell(3,5).GetValue<double>();
            obj.TotalOnPremisesCost.StorageCost = coreReportDataSheet.Cell(4, 5).GetValue<double>();
            obj.TotalOnPremisesCost.NetworkCost = coreReportDataSheet.Cell(5, 5).GetValue<double>();
            obj.TotalOnPremisesCost.SecurityCost = coreReportDataSheet.Cell(6, 5).GetValue<double>();
            obj.TotalOnPremisesCost.ITStaffCost = coreReportDataSheet.Cell(7, 5).GetValue<double>();
            obj.TotalOnPremisesCost.FacilitiesCost = coreReportDataSheet.Cell(8, 5).GetValue<double>();

            obj.AzureIaaSCost.ComputeLicenseCost = coreReportDataSheet.Cell(2, 6).GetValue<double>();
            obj.AzureIaaSCost.EsuLicenseCost = coreReportDataSheet.Cell(3, 6).GetValue<double>();
            obj.AzureIaaSCost.StorageCost = coreReportDataSheet.Cell(4, 6).GetValue<double>();
            obj.AzureIaaSCost.NetworkCost = coreReportDataSheet.Cell(5, 6).GetValue<double>();
            obj.AzureIaaSCost.SecurityCost = coreReportDataSheet.Cell(6, 6).GetValue<double>();
            obj.AzureIaaSCost.ITStaffCost = coreReportDataSheet.Cell(7, 6).GetValue<double>();
            obj.AzureIaaSCost.FacilitiesCost = coreReportDataSheet.Cell(8, 6).GetValue<double>();

            obj.AzurePaaSCost.ComputeLicenseCost = coreReportDataSheet.Cell(2, 7).GetValue<double>();
            obj.AzurePaaSCost.EsuLicenseCost = coreReportDataSheet.Cell(3, 7).GetValue<double>();
            obj.AzurePaaSCost.StorageCost = coreReportDataSheet.Cell(4, 7).GetValue<double>();
            obj.AzurePaaSCost.NetworkCost = coreReportDataSheet.Cell(5, 7).GetValue<double>();
            obj.AzurePaaSCost.SecurityCost = coreReportDataSheet.Cell(6, 7).GetValue<double>();
            obj.AzurePaaSCost.ITStaffCost = coreReportDataSheet.Cell(7, 7).GetValue<double>();
            obj.AzurePaaSCost.FacilitiesCost = coreReportDataSheet.Cell(8, 7).GetValue<double>();

            obj.OnPremisesAvsCost.ComputeLicenseCost = coreReportDataSheet.Cell(2, 4).GetValue<double>();
            obj.OnPremisesAvsCost.EsuLicenseCost = coreReportDataSheet.Cell(3, 4).GetValue<double>();
            obj.OnPremisesAvsCost.StorageCost = coreReportDataSheet.Cell(4, 4).GetValue<double>();
            obj.OnPremisesAvsCost.NetworkCost = coreReportDataSheet.Cell(5, 4).GetValue<double>();
            obj.OnPremisesAvsCost.SecurityCost = coreReportDataSheet.Cell(6, 4).GetValue<double>();
            obj.OnPremisesAvsCost.ITStaffCost = coreReportDataSheet.Cell(7, 4).GetValue<double>();
            obj.OnPremisesAvsCost.FacilitiesCost = coreReportDataSheet.Cell(8, 4).GetValue<double>();

            obj.AzureAvsCost.ComputeLicenseCost = coreReportDataSheet.Cell(2, 8).GetValue<double>();
            obj.AzureAvsCost.EsuLicenseCost = coreReportDataSheet.Cell(3, 8).GetValue<double>();
            obj.AzureAvsCost.StorageCost = coreReportDataSheet.Cell(4, 8).GetValue<double>();
            obj.AzureAvsCost.NetworkCost = coreReportDataSheet.Cell(5, 8).GetValue<double>();
            obj.AzureAvsCost.SecurityCost = coreReportDataSheet.Cell(6, 8).GetValue<double>();
            obj.AzureAvsCost.ITStaffCost = coreReportDataSheet.Cell(7, 8).GetValue<double>();
            obj.AzureAvsCost.FacilitiesCost = coreReportDataSheet.Cell(8, 8).GetValue<double>();

            obj.TotalAzureCost.ComputeLicenseCost = coreReportDataSheet.Cell(2, 9).GetValue<double>();
            obj.TotalAzureCost.EsuLicenseCost = coreReportDataSheet.Cell(3, 9).GetValue<double>();
            obj.TotalAzureCost.StorageCost = coreReportDataSheet.Cell(4, 9).GetValue<double>();
            obj.TotalAzureCost.NetworkCost = coreReportDataSheet.Cell(5, 9).GetValue<double>();
            obj.TotalAzureCost.ITStaffCost = coreReportDataSheet.Cell(6, 9).GetValue<double>();
            obj.TotalAzureCost.SecurityCost = coreReportDataSheet.Cell(7, 9).GetValue<double>();
            obj.TotalAzureCost.FacilitiesCost = coreReportDataSheet.Cell(8, 9).GetValue<double>();

            obj.WindowsServerLicense.ComputeLicenseCost = coreReportDataSheet.Cell(2, 10).GetValue<double?>() ?? 0.00;
            obj.SqlServerLicense.ComputeLicenseCost = coreReportDataSheet.Cell(2, 11).GetValue<double?>() ?? 0.00;
            obj.EsuSavings.ComputeLicenseCost = coreReportDataSheet.Cell(2, 12).GetValue<double?>() ?? 0.00;

            BusinessCaseObj = obj;

            logger.LogInformation($"Updated Business_Case data model of core report");
        }

        public void Load_AVS_Summary_Worksheet(XLWorkbook coreReportWb)
        {
            logger.LogInformation($"Loading data from worksheet: AVS_IaaS_Rehost_Perf in core report");
            var coreReportDataSheet = coreReportWb.Worksheet(16);
            int i = 2; // 1st row contains headers
            var row = coreReportDataSheet.Row(i);
            while (!row.IsEmpty())
            {
                AVS_Summary obj = new AVS_Summary
                {
                    SubscriptionId = row.Cell(1).GetValue<string>(),
                    ResourceGroup = row.Cell(2).GetValue<string>(),
                    ProjectName = row.Cell(3).GetValue<string>(),
                    AssessmentName = row.Cell(5).GetValue<string>(),
                    SizingCriterion = row.Cell(6).GetValue<string>(),
                    AssessmentType = row.Cell(7).GetValue<string>(),
                    CreatedOn = row.Cell(8).GetValue<string>(),
                    TotalMachinesAssessed = row.Cell(9).GetValue<int>(),
                    MachinesReady = row.Cell(10).GetValue<int>(),
                    MachinesReadyWithConditions = row.Cell(11).GetValue<int>(),
                    MachinesNotReady = row.Cell(12).GetValue<int>(),
                    MachinesReadinessUnknown = row.Cell(13).GetValue<int>(),
                    TotalRecommendedNumberOfNodes = row.Cell(14).GetValue<int>(),
                    NodeTypes = row.Cell(15).GetValue<string>(),
                    RecommendedNodes = row.Cell(16).GetValue<string>(),
                    RecommendedFttRaidLevels = row.Cell(17).GetValue<string>(),
                    RecommendedExternalStorage = row.Cell(18).GetValue<string>(),
                    MonthlyTotalCostEstimate = row.Cell(19).GetValue<double>(),
                    PredictedCpuUtilizationPercentage = row.Cell(20).GetValue<double>(),
                    PredictedMemoryUtilizationPercentage = row.Cell(21).GetValue<double>(),
                    PredictedStorageUtilizationPercentage = row.Cell(22).GetValue<double>(),
                    NumberOfCpuCoresAvailable = row.Cell(23).GetValue<int>(),
                    MemoryInTBAvailable = row.Cell(24).GetValue<double>(),
                    StorageInTBAvailable = row.Cell(25).GetValue<double>(),
                    NumberOfCpuCoresUsed = row.Cell(26).GetValue<int>(),
                    MemoryInTBUsed = row.Cell(27).GetValue<double>(),
                    StorageInTBUsed = row.Cell(28).GetValue<double>(),
                    NumberOfCpuCoresFree = row.Cell(29).GetValue<int>(),
                    MemoryInTBFree = row.Cell(30).GetValue<double>(),
                    StorageInTBFree = row.Cell(31).GetValue<double>(),
                    ConfidenceRating = row.Cell(32).GetValue<string>()
                };

                AvsSummaryList.Add(obj);

                i += 1;
                row = coreReportDataSheet.Row(i);
            }

            logger.LogInformation($"Updated AVS_Summary_List data model of core report");
        }

        public void Load_AVS_IaaS_Rehost_Perf_Worksheet(XLWorkbook coreReportWb)
        {
            logger.LogInformation($"Loading data from worksheet: AVS_IaaS_Rehost_Perf in core report");
            var coreReportDataSheet = coreReportWb.Worksheet(17);
            int i = 2; // 1st row contains headers
            var row = coreReportDataSheet.Row(i);
            while (!row.IsEmpty())
            {
                AVS_IaaS_Rehost_Perf obj = new AVS_IaaS_Rehost_Perf
                {
                    MachineName = row.Cell(1).GetValue<string>(),
                    AzureVMWareSolutionReadiness = row.Cell(2).GetValue<string>(),
                    AzureVMWareSolutionReadiness_Warnings = row.Cell(3).GetValue<string>(),
                    OperatingSystem = row.Cell(4).GetValue<string>(),
                    OperatingSystemVersion = row.Cell(5).GetValue<string>(),
                    OperatingSystemArchitecture = row.Cell(6).GetValue<string>(),
                    BootType = row.Cell(7).GetValue<string>(),
                    Cores = row.Cell(8).GetValue<int>(),
                    MemoryInMB = row.Cell(9).GetValue<double>(),
                    StorageInGB = row.Cell(10).GetValue<double>(),
                    StorageInUseInGB = row.Cell(11).GetValue<double>(),
                    DiskReadInOPS = row.Cell(12).GetValue<double>(),
                    DiskWriteInOPS = row.Cell(13).GetValue<double>(),
                    DiskReadInMBPS = row.Cell(14).GetValue<double>(),
                    DiskWriteInMBPS = row.Cell(15).GetValue<double>(),
                    NetworkAdapters = row.Cell(16).GetValue<int>(),
                    IpAddresses = row.Cell(17).GetValue<string>(),
                    MacAddresses = row.Cell(18).GetValue<string>(),
                    NetworkInMBPS = row.Cell(19).GetValue<double>(),
                    NetworkOutMBPS = row.Cell(20).GetValue<double>(),
                    DiskNames = row.Cell(21).GetValue<string>(),
                    MachineId = row.Cell(23).GetValue<string>()
                };

                AvsIaaSRehostPerfList.Add(obj);

                i += 1;
                row = coreReportDataSheet.Row(i);
            }

            logger.LogInformation($"Updated AVS_IaaS_Rehost_Perf data model of core report");
        }

        public void Load_Decommissioned_Machines_Worksheet(XLWorkbook coreReportWb)
        {
            logger.LogInformation($"Loading data from worksheet: Decommissioned_Machines in core report");

            var coreReportDataSheet = coreReportWb.Worksheet(18);

            int i = 2; // 1st row contains headers
            var row = coreReportDataSheet.Row(i);

            while (!row.IsEmpty())
            {
                Decommissioned_Machines obj = new Decommissioned_Machines();

                obj.MachineName = row.Cell(1).GetValue<string>();
                obj.MachineId = row.Cell(2).GetValue<string>();

                DecommissionedMachinesList.Add(obj);

                i += 1;
                row = coreReportDataSheet.Row(i);
            }

            logger.LogInformation($"Updated Decommissioned_Machines data model of core report");
        }
    }
}
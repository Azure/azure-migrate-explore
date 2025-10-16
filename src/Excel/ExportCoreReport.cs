// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using ClosedXML.Excel;
using System.Collections.Generic;

using Azure.Migrate.Explore.Models;
using Azure.Migrate.Explore.Common;

namespace Azure.Migrate.Explore.Excel
{
    public class ExportCoreReport
    {
        private readonly CoreProperties CorePropertiesObj;
        private readonly Business_Case Business_Case_Data;
        private readonly Cash_Flows Cash_Flows_Data;
        private readonly List<AVS_Summary> AVS_Summary_List;
        private readonly List<AVS_IaaS_Rehost_Perf> AVS_IaaS_Rehost_Perf_List;
        private readonly List<Decommissioned_Machines> Decommissioned_Machines_List;
        private readonly List<EmissionsDetails> EmissionsDetailsList;
        private readonly List<YOY_Emissions> YOY_EmissionsList;

        XLWorkbook CoreWb;

        public ExportCoreReport
            (
                CoreProperties corePropertiesObj,
                Business_Case business_Case_Data,
                Cash_Flows cash_Flows_Data,
                List<AVS_Summary> avs_Summary_List,
                List<AVS_IaaS_Rehost_Perf> avs_IaaS_Rehost_Perf_List,
                List<Decommissioned_Machines> decommissioned_Machines_List,
                List<EmissionsDetails> emissionsDetailsList,
                List<YOY_Emissions> yoy_EmissionsList
            )
        {
            CorePropertiesObj = corePropertiesObj;
            Business_Case_Data = business_Case_Data;
            Cash_Flows_Data = cash_Flows_Data;
            AVS_Summary_List = avs_Summary_List;
            AVS_IaaS_Rehost_Perf_List = avs_IaaS_Rehost_Perf_List;
            Decommissioned_Machines_List = decommissioned_Machines_List;
            EmissionsDetailsList = emissionsDetailsList;
            YOY_EmissionsList = yoy_EmissionsList;

            CoreWb = new XLWorkbook();
        }

        public void GenerateCoreReportExcel()
        {
            Generate_Properties_Worksheet();
            Generate_Business_Case_Worksheet();
            Generate_Cash_Flows_Worksheet();
            Generate_AVS_Summary_Worksheet();
            Generate_AVS_IaaS_Server_Rehost_Perf_Worksheet();
            Generate_Decommissioned_Machines_Worksheet();
            Generate_YOY_Emissions_Worksheet();
            Generate_Emissions_Details_Worksheet();

            CoreWb.SaveAs(UtilityFunctions.GetReportsDirectory() + "\\" + CoreReportConstants.CoreReportName);
        }

        private void Generate_Properties_Worksheet()
        {
            var dataWs = CoreWb.Worksheets.Add(CoreReportConstants.PropertiesTabName, 1);
            var propertyHeaders = CoreReportConstants.PropertyList;

            for (int i = 0; i < propertyHeaders.Count; i++)
                dataWs.Cell(1, i + 1).Value = propertyHeaders[i];

            // Add values: important to add in the same order as above

            dataWs.Cell(2, 1).Value = CorePropertiesObj.TenantId;
            dataWs.Cell(2, 2).Value = CorePropertiesObj.Subscription;
            dataWs.Cell(2, 3).Value = CorePropertiesObj.ResourceGroupName;
            dataWs.Cell(2, 4).Value = CorePropertiesObj.AzureMigrateProjectName;
            dataWs.Cell(2, 5).Value = CorePropertiesObj.AssessmentSiteName;
            dataWs.Cell(2, 6).Value = CorePropertiesObj.Workflow;
            dataWs.Cell(2, 7).Value = CorePropertiesObj.BusinessProposal;
            dataWs.Cell(2, 8).Value = CorePropertiesObj.TargetRegion;
            dataWs.Cell(2, 9).Value = CorePropertiesObj.Currency;
            dataWs.Cell(2, 10).Value = CorePropertiesObj.AssessmentDuration;
            dataWs.Cell(2, 11).Value = CorePropertiesObj.OptimizationPreference;
            dataWs.Cell(2, 12).Value = CorePropertiesObj.AssessSQLServices;
            dataWs.Cell(2, 13).Value = CorePropertiesObj.VCpuOverSubscription;
            dataWs.Cell(2, 14).Value = CorePropertiesObj.MemoryOverCommit;
            dataWs.Cell(2, 15).Value = CorePropertiesObj.DedupeCompression;
        }

        private void Generate_Business_Case_Worksheet()
        {
            var dataWs = CoreWb.Worksheets.Add(CoreReportConstants.Business_Case_TabName, 2);

            UtilityFunctions.AddColumnHeadersToWorksheet(dataWs, CoreReportConstants.Business_Case_Columns);
            for (int i = 0; i < CoreReportConstants.Business_Case_RowTypes.Count; i++)
                dataWs.Cell(i + 2, 1).Value = CoreReportConstants.Business_Case_RowTypes[i];

            if (Business_Case_Data == null)
                return;

            dataWs.Cell(2, 2).Value = Business_Case_Data.OnPremisesIaaSCost.ComputeLicenseCost;
            dataWs.Cell(3, 2).Value = Business_Case_Data.OnPremisesIaaSCost.EsuLicenseCost;
            dataWs.Cell(4, 2).Value = Business_Case_Data.OnPremisesIaaSCost.StorageCost;
            dataWs.Cell(5, 2).Value = Business_Case_Data.OnPremisesIaaSCost.NetworkCost;
            dataWs.Cell(6, 2).Value = Business_Case_Data.OnPremisesIaaSCost.SecurityCost;
            dataWs.Cell(7, 2).Value = Business_Case_Data.OnPremisesIaaSCost.ITStaffCost;
            dataWs.Cell(8, 2).Value = Business_Case_Data.OnPremisesIaaSCost.FacilitiesCost;
            dataWs.Cell(9, 2).Value = Business_Case_Data.OnPremisesIaaSCost.ManagementCost;

            dataWs.Cell(2, 3).Value = Business_Case_Data.OnPremisesPaaSCost.ComputeLicenseCost;
            dataWs.Cell(3, 3).Value = Business_Case_Data.OnPremisesPaaSCost.EsuLicenseCost;
            dataWs.Cell(4, 3).Value = Business_Case_Data.OnPremisesPaaSCost.StorageCost;
            dataWs.Cell(5, 3).Value = Business_Case_Data.OnPremisesPaaSCost.NetworkCost;
            dataWs.Cell(6, 3).Value = Business_Case_Data.OnPremisesPaaSCost.SecurityCost;
            dataWs.Cell(7, 3).Value = Business_Case_Data.OnPremisesPaaSCost.ITStaffCost;
            dataWs.Cell(8, 3).Value = Business_Case_Data.OnPremisesPaaSCost.FacilitiesCost;
            dataWs.Cell(9, 3).Value = Business_Case_Data.OnPremisesPaaSCost.ManagementCost;

            dataWs.Cell(2, 4).Value = Business_Case_Data.OnPremisesAvsCost.ComputeLicenseCost;
            dataWs.Cell(3, 4).Value = Business_Case_Data.OnPremisesAvsCost.EsuLicenseCost;
            dataWs.Cell(4, 4).Value = Business_Case_Data.OnPremisesAvsCost.StorageCost;
            dataWs.Cell(5, 4).Value = Business_Case_Data.OnPremisesAvsCost.NetworkCost;
            dataWs.Cell(6, 4).Value = Business_Case_Data.OnPremisesAvsCost.SecurityCost;
            dataWs.Cell(7, 4).Value = Business_Case_Data.OnPremisesAvsCost.ITStaffCost;
            dataWs.Cell(8, 4).Value = Business_Case_Data.OnPremisesAvsCost.FacilitiesCost;
            dataWs.Cell(9, 4).Value = Business_Case_Data.OnPremisesAvsCost.ManagementCost;

            dataWs.Cell(2, 5).Value = Business_Case_Data.TotalOnPremisesCost.ComputeLicenseCost;
            dataWs.Cell(3, 5).Value = Business_Case_Data.TotalOnPremisesCost.EsuLicenseCost;
            dataWs.Cell(4, 5).Value = Business_Case_Data.TotalOnPremisesCost.StorageCost;
            dataWs.Cell(5, 5).Value = Business_Case_Data.TotalOnPremisesCost.NetworkCost;
            dataWs.Cell(6, 5).Value = Business_Case_Data.TotalOnPremisesCost.SecurityCost;
            dataWs.Cell(7, 5).Value = Business_Case_Data.TotalOnPremisesCost.ITStaffCost;
            dataWs.Cell(8, 5).Value = Business_Case_Data.TotalOnPremisesCost.FacilitiesCost;
            dataWs.Cell(9, 5).Value = Business_Case_Data.TotalOnPremisesCost.ManagementCost;

            dataWs.Cell(2, 6).Value = Business_Case_Data.AzureIaaSCost.ComputeLicenseCost;
            dataWs.Cell(3, 6).Value = Business_Case_Data.AzureIaaSCost.EsuLicenseCost;
            dataWs.Cell(4, 6).Value = Business_Case_Data.AzureIaaSCost.StorageCost;
            dataWs.Cell(5, 6).Value = Business_Case_Data.AzureIaaSCost.NetworkCost;
            dataWs.Cell(6, 6).Value = Business_Case_Data.AzureIaaSCost.SecurityCost;
            dataWs.Cell(7, 6).Value = Business_Case_Data.AzureIaaSCost.ITStaffCost;
            dataWs.Cell(8, 6).Value = Business_Case_Data.AzureIaaSCost.FacilitiesCost;
            dataWs.Cell(9, 6).Value = Business_Case_Data.AzureIaaSCost.ManagementCost;

            dataWs.Cell(2, 7).Value = Business_Case_Data.AzurePaaSCost.ComputeLicenseCost;
            dataWs.Cell(3, 7).Value = Business_Case_Data.AzurePaaSCost.EsuLicenseCost;
            dataWs.Cell(4, 7).Value = Business_Case_Data.AzurePaaSCost.StorageCost;
            dataWs.Cell(5, 7).Value = Business_Case_Data.AzurePaaSCost.NetworkCost;
            dataWs.Cell(6, 7).Value = Business_Case_Data.AzurePaaSCost.SecurityCost;
            dataWs.Cell(7, 7).Value = Business_Case_Data.AzurePaaSCost.ITStaffCost;
            dataWs.Cell(8, 7).Value = Business_Case_Data.AzurePaaSCost.FacilitiesCost;
            dataWs.Cell(9, 7).Value = Business_Case_Data.AzurePaaSCost.ManagementCost;

            dataWs.Cell(2, 8).Value = Business_Case_Data.AzureAvsCost.ComputeLicenseCost;
            dataWs.Cell(3, 8).Value = Business_Case_Data.AzureAvsCost.EsuLicenseCost;
            dataWs.Cell(4, 8).Value = Business_Case_Data.AzureAvsCost.StorageCost;
            dataWs.Cell(5, 8).Value = Business_Case_Data.AzureAvsCost.NetworkCost;
            dataWs.Cell(6, 8).Value = Business_Case_Data.AzureAvsCost.SecurityCost;
            dataWs.Cell(7, 8).Value = Business_Case_Data.AzureAvsCost.ITStaffCost;
            dataWs.Cell(8, 8).Value = Business_Case_Data.AzureAvsCost.FacilitiesCost;
            dataWs.Cell(9, 8).Value = Business_Case_Data.AzureAvsCost.ManagementCost;

            dataWs.Cell(2, 9).Value = Business_Case_Data.AzureArcEnabledOnPremisesCost.ComputeLicenseCost;
            dataWs.Cell(3, 9).Value = Business_Case_Data.AzureArcEnabledOnPremisesCost.EsuLicenseCost;
            dataWs.Cell(4, 9).Value = Business_Case_Data.AzureArcEnabledOnPremisesCost.StorageCost;
            dataWs.Cell(5, 9).Value = Business_Case_Data.AzureArcEnabledOnPremisesCost.NetworkCost;
            dataWs.Cell(6, 9).Value = Business_Case_Data.AzureArcEnabledOnPremisesCost.SecurityCost;
            dataWs.Cell(7, 9).Value = Business_Case_Data.AzureArcEnabledOnPremisesCost.ITStaffCost;
            dataWs.Cell(8, 9).Value = Business_Case_Data.AzureArcEnabledOnPremisesCost.FacilitiesCost;
            dataWs.Cell(9, 9).Value = Business_Case_Data.AzureArcEnabledOnPremisesCost.ManagementCost;

            dataWs.Cell(2, 10).Value = Business_Case_Data.TotalAzureCost.ComputeLicenseCost;
            dataWs.Cell(3, 10).Value = Business_Case_Data.TotalAzureCost.EsuLicenseCost;
            dataWs.Cell(4, 10).Value = Business_Case_Data.TotalAzureCost.StorageCost;
            dataWs.Cell(5, 10).Value = Business_Case_Data.TotalAzureCost.NetworkCost;
            dataWs.Cell(6, 10).Value = Business_Case_Data.TotalAzureCost.SecurityCost;
            dataWs.Cell(7, 10).Value = Business_Case_Data.TotalAzureCost.ITStaffCost;
            dataWs.Cell(8, 10).Value = Business_Case_Data.TotalAzureCost.FacilitiesCost;
            dataWs.Cell(9, 10).Value = Business_Case_Data.TotalAzureCost.ManagementCost;

            dataWs.Cell(2, 11).Value = Business_Case_Data.WindowsServerLicense.ComputeLicenseCost;
            dataWs.Cell(3, 11).Value = 0.00;
            dataWs.Cell(4, 11).Value = 0.00;
            dataWs.Cell(5, 11).Value = 0.00;
            dataWs.Cell(6, 11).Value = 0.00;
            dataWs.Cell(7, 11).Value = 0.00;
            dataWs.Cell(8, 11).Value = 0.00;

            dataWs.Cell(2, 12).Value = Business_Case_Data.SqlServerLicense.ComputeLicenseCost;
            dataWs.Cell(3, 12).Value = 0.00;
            dataWs.Cell(4, 12).Value = 0.00;
            dataWs.Cell(5, 12).Value = 0.00;
            dataWs.Cell(6, 12).Value = 0.00;
            dataWs.Cell(7, 12).Value = 0.00;
            dataWs.Cell(8, 12).Value = 0.00;

            dataWs.Cell(2, 13).Value = Business_Case_Data.EsuSavings.ComputeLicenseCost;
            dataWs.Cell(3, 13).Value = 0.00;
            dataWs.Cell(4, 13).Value = 0.00;
            dataWs.Cell(5, 13).Value = 0.00;
            dataWs.Cell(6, 13).Value = 0.00;
            dataWs.Cell(7, 13).Value = 0.00;
            dataWs.Cell(8, 13).Value = 0.00;
        }

        private void Generate_Cash_Flows_Worksheet()
        {
            var dataWs = CoreWb.Worksheets.Add(CoreReportConstants.Cash_Flows_TabName, 3);

            for (int i = 0; i < CoreReportConstants.Cash_Flows_Years.Count; i++)
                dataWs.Cell(1, i + 3).Value = CoreReportConstants.Cash_Flows_Years[i];

            for (int i = 0; i < CoreReportConstants.Cash_Flows_CloudComputingServiceTypes.Count; i++)
            {
                dataWs.Cell(3 * i + 2, 1).Value = CoreReportConstants.Cash_Flows_CloudComputingServiceTypes[i];
                dataWs.Cell(3 * i + 3, 1).Value = CoreReportConstants.Cash_Flows_CloudComputingServiceTypes[i];
                dataWs.Cell(3 * i + 4, 1).Value = CoreReportConstants.Cash_Flows_CloudComputingServiceTypes[i];
            }

            for (int i = 0; i < CoreReportConstants.Cash_Flows_Types.Count; i++)
            {
                dataWs.Cell(3 * 1 + i - 1, 2).Value = CoreReportConstants.Cash_Flows_Types[i];
                dataWs.Cell(3 * 2 + i - 1, 2).Value = CoreReportConstants.Cash_Flows_Types[i];
                dataWs.Cell(3 * 3 + i - 1, 2).Value = CoreReportConstants.Cash_Flows_Types[i];
                dataWs.Cell(3 * 4 + i - 1, 2).Value = CoreReportConstants.Cash_Flows_Types[i];
            }

            // Total
            // Current state Cash Flow
            dataWs.Cell(2, 3).Value = Cash_Flows_Data.TotalYOYCosts.OnPremisesCostYOY.Year0;
            dataWs.Cell(2, 4).Value = Cash_Flows_Data.TotalYOYCosts.OnPremisesCostYOY.Year1;
            dataWs.Cell(2, 5).Value = Cash_Flows_Data.TotalYOYCosts.OnPremisesCostYOY.Year2;
            dataWs.Cell(2, 6).Value = Cash_Flows_Data.TotalYOYCosts.OnPremisesCostYOY.Year3;

            // Future state Cash Flow
            dataWs.Cell(3, 3).Value = Cash_Flows_Data.TotalYOYCosts.AzureCostYOY.Year0;
            dataWs.Cell(3, 4).Value = Cash_Flows_Data.TotalYOYCosts.AzureCostYOY.Year1;
            dataWs.Cell(3, 5).Value = Cash_Flows_Data.TotalYOYCosts.AzureCostYOY.Year2;
            dataWs.Cell(3, 6).Value = Cash_Flows_Data.TotalYOYCosts.AzureCostYOY.Year3;

            // Savings
            dataWs.Cell(4, 3).Value = Cash_Flows_Data.TotalYOYCosts.SavingsYOY.Year0;
            dataWs.Cell(4, 4).Value = Cash_Flows_Data.TotalYOYCosts.SavingsYOY.Year1;
            dataWs.Cell(4, 5).Value = Cash_Flows_Data.TotalYOYCosts.SavingsYOY.Year2;
            dataWs.Cell(4, 6).Value = Cash_Flows_Data.TotalYOYCosts.SavingsYOY.Year3;

            // IaaS
            // Current state Cash Flow
            dataWs.Cell(5, 3).Value = Cash_Flows_Data.IaaSYOYCosts.OnPremisesCostYOY.Year0;
            dataWs.Cell(5, 4).Value = Cash_Flows_Data.IaaSYOYCosts.OnPremisesCostYOY.Year1;
            dataWs.Cell(5, 5).Value = Cash_Flows_Data.IaaSYOYCosts.OnPremisesCostYOY.Year2;
            dataWs.Cell(5, 6).Value = Cash_Flows_Data.IaaSYOYCosts.OnPremisesCostYOY.Year3;

            // Future state Cash Flow
            dataWs.Cell(6, 3).Value = Cash_Flows_Data.IaaSYOYCosts.AzureCostYOY.Year0;
            dataWs.Cell(6, 4).Value = Cash_Flows_Data.IaaSYOYCosts.AzureCostYOY.Year1;
            dataWs.Cell(6, 5).Value = Cash_Flows_Data.IaaSYOYCosts.AzureCostYOY.Year2;
            dataWs.Cell(6, 6).Value = Cash_Flows_Data.IaaSYOYCosts.AzureCostYOY.Year3;

            // Savings
            dataWs.Cell(7, 3).Value = Cash_Flows_Data.IaaSYOYCosts.SavingsYOY.Year0;
            dataWs.Cell(7, 4).Value = Cash_Flows_Data.IaaSYOYCosts.SavingsYOY.Year1;
            dataWs.Cell(7, 5).Value = Cash_Flows_Data.IaaSYOYCosts.SavingsYOY.Year2;
            dataWs.Cell(7, 6).Value = Cash_Flows_Data.IaaSYOYCosts.SavingsYOY.Year3;

            // PaaS
            // Current state Cash Flow
            dataWs.Cell(8, 3).Value = Cash_Flows_Data.PaaSYOYCosts.OnPremisesCostYOY.Year0;
            dataWs.Cell(8, 4).Value = Cash_Flows_Data.PaaSYOYCosts.OnPremisesCostYOY.Year1;
            dataWs.Cell(8, 5).Value = Cash_Flows_Data.PaaSYOYCosts.OnPremisesCostYOY.Year2;
            dataWs.Cell(8, 6).Value = Cash_Flows_Data.PaaSYOYCosts.OnPremisesCostYOY.Year3;

            // Future state Cash Flow
            dataWs.Cell(9, 3).Value = Cash_Flows_Data.PaaSYOYCosts.AzureCostYOY.Year0;
            dataWs.Cell(9, 4).Value = Cash_Flows_Data.PaaSYOYCosts.AzureCostYOY.Year1;
            dataWs.Cell(9, 5).Value = Cash_Flows_Data.PaaSYOYCosts.AzureCostYOY.Year2;
            dataWs.Cell(9, 6).Value = Cash_Flows_Data.PaaSYOYCosts.AzureCostYOY.Year3;

            // Savings
            dataWs.Cell(10, 3).Value = Cash_Flows_Data.PaaSYOYCosts.SavingsYOY.Year0;
            dataWs.Cell(10, 4).Value = Cash_Flows_Data.PaaSYOYCosts.SavingsYOY.Year1;
            dataWs.Cell(10, 5).Value = Cash_Flows_Data.PaaSYOYCosts.SavingsYOY.Year2;
            dataWs.Cell(10, 6).Value = Cash_Flows_Data.PaaSYOYCosts.SavingsYOY.Year3;

            // AVS
            // Current state Cash Flow
            dataWs.Cell(11, 3).Value = Cash_Flows_Data.AvsYOYCosts.OnPremisesCostYOY.Year0;
            dataWs.Cell(11, 4).Value = Cash_Flows_Data.AvsYOYCosts.OnPremisesCostYOY.Year1;
            dataWs.Cell(11, 5).Value = Cash_Flows_Data.AvsYOYCosts.OnPremisesCostYOY.Year2;
            dataWs.Cell(11, 6).Value = Cash_Flows_Data.AvsYOYCosts.OnPremisesCostYOY.Year3;

            // Future state Cash Flow
            dataWs.Cell(12, 3).Value = Cash_Flows_Data.AvsYOYCosts.AzureCostYOY.Year0;
            dataWs.Cell(12, 4).Value = Cash_Flows_Data.AvsYOYCosts.AzureCostYOY.Year1;
            dataWs.Cell(12, 5).Value = Cash_Flows_Data.AvsYOYCosts.AzureCostYOY.Year2;
            dataWs.Cell(12, 6).Value = Cash_Flows_Data.AvsYOYCosts.AzureCostYOY.Year3;

            // Savings
            dataWs.Cell(13, 3).Value = Cash_Flows_Data.AvsYOYCosts.SavingsYOY.Year0;
            dataWs.Cell(13, 4).Value = Cash_Flows_Data.AvsYOYCosts.SavingsYOY.Year1;
            dataWs.Cell(13, 5).Value = Cash_Flows_Data.AvsYOYCosts.SavingsYOY.Year2;
            dataWs.Cell(13, 6).Value = Cash_Flows_Data.AvsYOYCosts.SavingsYOY.Year3;
        }
        private void Generate_AVS_Summary_Worksheet()
        {
            var dataWs = CoreWb.Worksheets.Add(CoreReportConstants.AVS_Summary_TabName, 16);

            UtilityFunctions.AddColumnHeadersToWorksheet(dataWs, CoreReportConstants.AVS_Summary_Columns);

            if (AVS_Summary_List != null && AVS_Summary_List.Count > 0)
                dataWs.Cell(2, 1).InsertData(AVS_Summary_List);
        }

        private void Generate_AVS_IaaS_Server_Rehost_Perf_Worksheet()
        {
            var dataWs = CoreWb.Worksheets.Add(CoreReportConstants.AVS_IaaS_Rehost_Perf_TabName, 17);

            UtilityFunctions.AddColumnHeadersToWorksheet(dataWs, CoreReportConstants.AVS_IaaS_Rehost_Perf_Columns);

            if (AVS_IaaS_Rehost_Perf_List != null && AVS_IaaS_Rehost_Perf_List.Count > 0)
                dataWs.Cell(2, 1).InsertData(AVS_IaaS_Rehost_Perf_List);
        }

        private void Generate_Decommissioned_Machines_Worksheet()
        {
            var dataWs = CoreWb.Worksheets.Add(CoreReportConstants.Decommissioned_Machines_TabName, 18);

            UtilityFunctions.AddColumnHeadersToWorksheet(dataWs, CoreReportConstants.Decommissioned_Machines_Columns);

            if (Decommissioned_Machines_List != null && Decommissioned_Machines_List.Count > 0)
                dataWs.Cell(2, 1).InsertData(Decommissioned_Machines_List);
        }

        private void Generate_YOY_Emissions_Worksheet()
        {
            var dataWs = CoreWb.Worksheets.Add(CoreReportConstants.YOY_Emissions_TabName, 19);
            UtilityFunctions.AddColumnHeadersToWorksheet(dataWs, CoreReportConstants.YOY_Emissions_Columns);
            if (YOY_EmissionsList != null && YOY_EmissionsList.Count > 0)
                dataWs.Cell(2, 1).InsertData(YOY_EmissionsList);
        }

        private void Generate_Emissions_Details_Worksheet()
        {
            var dataWs = CoreWb.Worksheets.Add(CoreReportConstants.Emissions_Details_TabName, 20);
            UtilityFunctions.AddColumnHeadersToWorksheet(dataWs, CoreReportConstants.Emissions_Details_Columns);
            if (EmissionsDetailsList != null && EmissionsDetailsList.Count > 0)
                dataWs.Cell(2, 1).InsertData(EmissionsDetailsList);
        }
    }
}
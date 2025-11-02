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

            dataWs.Cell(2, 2).Value = Business_Case_Data.ApplicationSummaryCostDetails.AzureCost.TotalCost;
            dataWs.Cell(3, 2).Value = Business_Case_Data.ApplicationSummaryCostDetails.AzureCost.ComputeCost;
            dataWs.Cell(4, 2).Value = Business_Case_Data.ApplicationSummaryCostDetails.AzureCost.LicenseCost;
            dataWs.Cell(5, 2).Value = Business_Case_Data.ApplicationSummaryCostDetails.AzureCost.StorageCost;
            dataWs.Cell(6, 2).Value = Business_Case_Data.ApplicationSummaryCostDetails.AzureCost.NetworkCost;
            dataWs.Cell(7, 2).Value = Business_Case_Data.ApplicationSummaryCostDetails.AzureCost.SecurityCost;
            dataWs.Cell(8, 2).Value = Business_Case_Data.ApplicationSummaryCostDetails.AzureCost.ITLaborCost;
            dataWs.Cell(9, 2).Value = Business_Case_Data.ApplicationSummaryCostDetails.AzureCost.FacilitiesCost;
            dataWs.Cell(10, 2).Value = Business_Case_Data.ApplicationSummaryCostDetails.AzureCost.ManagementCost;
            dataWs.Cell(11, 2).Value = Business_Case_Data.ApplicationSummaryCostDetails.AzureCost.AhubSavings;
            dataWs.Cell(12, 2).Value = Business_Case_Data.ApplicationSummaryCostDetails.AzureCost.EsuSavings;
            dataWs.Cell(13, 2).Value = Business_Case_Data.ApplicationSummaryCostDetails.AzureCost.LinuxAhubSavings;

            dataWs.Cell(2, 3).Value = Business_Case_Data.ApplicationSummaryCostDetails.OnPremCost.TotalCost;
            dataWs.Cell(3, 3).Value = Business_Case_Data.ApplicationSummaryCostDetails.OnPremCost.ComputeCost;
            dataWs.Cell(4, 3).Value = Business_Case_Data.ApplicationSummaryCostDetails.OnPremCost.LicenseCost;
            dataWs.Cell(5, 3).Value = Business_Case_Data.ApplicationSummaryCostDetails.OnPremCost.StorageCost;
            dataWs.Cell(6, 3).Value = Business_Case_Data.ApplicationSummaryCostDetails.OnPremCost.NetworkCost;
            dataWs.Cell(7, 3).Value = Business_Case_Data.ApplicationSummaryCostDetails.OnPremCost.SecurityCost;
            dataWs.Cell(8, 3).Value = Business_Case_Data.ApplicationSummaryCostDetails.OnPremCost.ITLaborCost;
            dataWs.Cell(9, 3).Value = Business_Case_Data.ApplicationSummaryCostDetails.OnPremCost.FacilitiesCost;
            dataWs.Cell(10, 3).Value = Business_Case_Data.ApplicationSummaryCostDetails.OnPremCost.ManagementCost;
            dataWs.Cell(11, 3).Value = Business_Case_Data.ApplicationSummaryCostDetails.OnPremCost.AhubSavings;
            dataWs.Cell(12, 3).Value = Business_Case_Data.ApplicationSummaryCostDetails.OnPremCost.EsuSavings;
            dataWs.Cell(13, 3).Value = Business_Case_Data.ApplicationSummaryCostDetails.OnPremCost.LinuxAhubSavings;

            dataWs.Cell(2, 4).Value = Business_Case_Data.CotsApplicationSummaryCostDetails.AzureCost.TotalCost;
            dataWs.Cell(3, 4).Value = Business_Case_Data.CotsApplicationSummaryCostDetails.AzureCost.ComputeCost;
            dataWs.Cell(4, 4).Value = Business_Case_Data.CotsApplicationSummaryCostDetails.AzureCost.LicenseCost;
            dataWs.Cell(5, 4).Value = Business_Case_Data.CotsApplicationSummaryCostDetails.AzureCost.StorageCost;
            dataWs.Cell(6, 4).Value = Business_Case_Data.CotsApplicationSummaryCostDetails.AzureCost.NetworkCost;
            dataWs.Cell(7, 4).Value = Business_Case_Data.CotsApplicationSummaryCostDetails.AzureCost.SecurityCost;
            dataWs.Cell(8, 4).Value = Business_Case_Data.CotsApplicationSummaryCostDetails.AzureCost.ITLaborCost;
            dataWs.Cell(9, 4).Value = Business_Case_Data.CotsApplicationSummaryCostDetails.AzureCost.FacilitiesCost;
            dataWs.Cell(10, 4).Value = Business_Case_Data.CotsApplicationSummaryCostDetails.AzureCost.ManagementCost;
            dataWs.Cell(11, 4).Value = Business_Case_Data.CotsApplicationSummaryCostDetails.AzureCost.AhubSavings;
            dataWs.Cell(12, 4).Value = Business_Case_Data.CotsApplicationSummaryCostDetails.AzureCost.EsuSavings;
            dataWs.Cell(13, 4).Value = Business_Case_Data.CotsApplicationSummaryCostDetails.AzureCost.LinuxAhubSavings;

            dataWs.Cell(2, 5).Value = Business_Case_Data.CotsApplicationSummaryCostDetails.OnPremCost.TotalCost;
            dataWs.Cell(3, 5).Value = Business_Case_Data.CotsApplicationSummaryCostDetails.OnPremCost.ComputeCost;
            dataWs.Cell(4, 5).Value = Business_Case_Data.CotsApplicationSummaryCostDetails.OnPremCost.LicenseCost;
            dataWs.Cell(5, 5).Value = Business_Case_Data.CotsApplicationSummaryCostDetails.OnPremCost.StorageCost;
            dataWs.Cell(6, 5).Value = Business_Case_Data.CotsApplicationSummaryCostDetails.OnPremCost.NetworkCost;
            dataWs.Cell(7, 5).Value = Business_Case_Data.CotsApplicationSummaryCostDetails.OnPremCost.SecurityCost;
            dataWs.Cell(8, 5).Value = Business_Case_Data.CotsApplicationSummaryCostDetails.OnPremCost.ITLaborCost;
            dataWs.Cell(9, 5).Value = Business_Case_Data.CotsApplicationSummaryCostDetails.OnPremCost.FacilitiesCost;
            dataWs.Cell(10, 5).Value = Business_Case_Data.CotsApplicationSummaryCostDetails.OnPremCost.ManagementCost;
            dataWs.Cell(11, 5).Value = Business_Case_Data.CotsApplicationSummaryCostDetails.OnPremCost.AhubSavings;
            dataWs.Cell(12, 5).Value = Business_Case_Data.CotsApplicationSummaryCostDetails.OnPremCost.EsuSavings;
            dataWs.Cell(13, 5).Value = Business_Case_Data.CotsApplicationSummaryCostDetails.OnPremCost.LinuxAhubSavings;

            dataWs.Cell(2, 6).Value = Business_Case_Data.CustomApplicationSummaryCostDetails.AzureCost.TotalCost;
            dataWs.Cell(3, 6).Value = Business_Case_Data.CustomApplicationSummaryCostDetails.AzureCost.ComputeCost;
            dataWs.Cell(4, 6).Value = Business_Case_Data.CustomApplicationSummaryCostDetails.AzureCost.LicenseCost;
            dataWs.Cell(5, 6).Value = Business_Case_Data.CustomApplicationSummaryCostDetails.AzureCost.StorageCost;
            dataWs.Cell(6, 6).Value = Business_Case_Data.CustomApplicationSummaryCostDetails.AzureCost.NetworkCost;
            dataWs.Cell(7, 6).Value = Business_Case_Data.CustomApplicationSummaryCostDetails.AzureCost.SecurityCost;
            dataWs.Cell(8, 6).Value = Business_Case_Data.CustomApplicationSummaryCostDetails.AzureCost.ITLaborCost;
            dataWs.Cell(9, 6).Value = Business_Case_Data.CustomApplicationSummaryCostDetails.AzureCost.FacilitiesCost;
            dataWs.Cell(10, 6).Value = Business_Case_Data.CustomApplicationSummaryCostDetails.AzureCost.ManagementCost;
            dataWs.Cell(11, 6).Value = Business_Case_Data.CustomApplicationSummaryCostDetails.AzureCost.AhubSavings;
            dataWs.Cell(12, 6).Value = Business_Case_Data.CustomApplicationSummaryCostDetails.AzureCost.EsuSavings;
            dataWs.Cell(13, 6).Value = Business_Case_Data.CustomApplicationSummaryCostDetails.AzureCost.LinuxAhubSavings;

            dataWs.Cell(2, 7).Value = Business_Case_Data.CustomApplicationSummaryCostDetails.OnPremCost.TotalCost;
            dataWs.Cell(3, 7).Value = Business_Case_Data.CustomApplicationSummaryCostDetails.OnPremCost.ComputeCost;
            dataWs.Cell(4, 7).Value = Business_Case_Data.CustomApplicationSummaryCostDetails.OnPremCost.LicenseCost;
            dataWs.Cell(5, 7).Value = Business_Case_Data.CustomApplicationSummaryCostDetails.OnPremCost.StorageCost;
            dataWs.Cell(6, 7).Value = Business_Case_Data.CustomApplicationSummaryCostDetails.OnPremCost.NetworkCost;
            dataWs.Cell(7, 7).Value = Business_Case_Data.CustomApplicationSummaryCostDetails.OnPremCost.SecurityCost;
            dataWs.Cell(8, 7).Value = Business_Case_Data.CustomApplicationSummaryCostDetails.OnPremCost.ITLaborCost;
            dataWs.Cell(9, 7).Value = Business_Case_Data.CustomApplicationSummaryCostDetails.OnPremCost.FacilitiesCost;
            dataWs.Cell(10, 7).Value = Business_Case_Data.CustomApplicationSummaryCostDetails.OnPremCost.ManagementCost;
            dataWs.Cell(11, 7).Value = Business_Case_Data.CustomApplicationSummaryCostDetails.OnPremCost.AhubSavings;
            dataWs.Cell(12, 7).Value = Business_Case_Data.CustomApplicationSummaryCostDetails.OnPremCost.EsuSavings;
            dataWs.Cell(13, 7).Value = Business_Case_Data.CustomApplicationSummaryCostDetails.OnPremCost.LinuxAhubSavings;

            dataWs.Cell(2, 8).Value = Business_Case_Data.IndependentWorkloadsSummaryCostDetails.AzureCost.TotalCost;
            dataWs.Cell(3, 8).Value = Business_Case_Data.IndependentWorkloadsSummaryCostDetails.AzureCost.ComputeCost;
            dataWs.Cell(4, 8).Value = Business_Case_Data.IndependentWorkloadsSummaryCostDetails.AzureCost.LicenseCost;
            dataWs.Cell(5, 8).Value = Business_Case_Data.IndependentWorkloadsSummaryCostDetails.AzureCost.StorageCost;
            dataWs.Cell(6, 8).Value = Business_Case_Data.IndependentWorkloadsSummaryCostDetails.AzureCost.NetworkCost;
            dataWs.Cell(7, 8).Value = Business_Case_Data.IndependentWorkloadsSummaryCostDetails.AzureCost.SecurityCost;
            dataWs.Cell(8, 8).Value = Business_Case_Data.IndependentWorkloadsSummaryCostDetails.AzureCost.ITLaborCost;
            dataWs.Cell(9, 8).Value = Business_Case_Data.IndependentWorkloadsSummaryCostDetails.AzureCost.FacilitiesCost;
            dataWs.Cell(10, 8).Value = Business_Case_Data.IndependentWorkloadsSummaryCostDetails.AzureCost.ManagementCost;
            dataWs.Cell(11, 8).Value = Business_Case_Data.IndependentWorkloadsSummaryCostDetails.AzureCost.AhubSavings;
            dataWs.Cell(12, 8).Value = Business_Case_Data.IndependentWorkloadsSummaryCostDetails.AzureCost.EsuSavings;
            dataWs.Cell(13, 8).Value = Business_Case_Data.IndependentWorkloadsSummaryCostDetails.AzureCost.LinuxAhubSavings;

            dataWs.Cell(2, 9).Value = Business_Case_Data.IndependentWorkloadsSummaryCostDetails.OnPremCost.TotalCost;
            dataWs.Cell(3, 9).Value = Business_Case_Data.IndependentWorkloadsSummaryCostDetails.OnPremCost.ComputeCost;
            dataWs.Cell(4, 9).Value = Business_Case_Data.IndependentWorkloadsSummaryCostDetails.OnPremCost.LicenseCost;
            dataWs.Cell(5, 9).Value = Business_Case_Data.IndependentWorkloadsSummaryCostDetails.OnPremCost.StorageCost;
            dataWs.Cell(6, 9).Value = Business_Case_Data.IndependentWorkloadsSummaryCostDetails.OnPremCost.NetworkCost;
            dataWs.Cell(7, 9).Value = Business_Case_Data.IndependentWorkloadsSummaryCostDetails.OnPremCost.SecurityCost;
            dataWs.Cell(8, 9).Value = Business_Case_Data.IndependentWorkloadsSummaryCostDetails.OnPremCost.ITLaborCost;
            dataWs.Cell(9, 9).Value = Business_Case_Data.IndependentWorkloadsSummaryCostDetails.OnPremCost.FacilitiesCost;
            dataWs.Cell(10, 9).Value = Business_Case_Data.IndependentWorkloadsSummaryCostDetails.OnPremCost.ManagementCost;
            dataWs.Cell(11, 9).Value = Business_Case_Data.IndependentWorkloadsSummaryCostDetails.OnPremCost.AhubSavings;
            dataWs.Cell(12, 9).Value = Business_Case_Data.IndependentWorkloadsSummaryCostDetails.OnPremCost.EsuSavings;
            dataWs.Cell(13, 9).Value = Business_Case_Data.IndependentWorkloadsSummaryCostDetails.OnPremCost.LinuxAhubSavings;

            dataWs.Cell(2, 10).Value = Business_Case_Data.TotalAzureCostDetails.TotalCost;
            dataWs.Cell(3, 10).Value = Business_Case_Data.TotalAzureCostDetails.ComputeCost;
            dataWs.Cell(4, 10).Value = Business_Case_Data.TotalAzureCostDetails.LicenseCost;
            dataWs.Cell(5, 10).Value = Business_Case_Data.TotalAzureCostDetails.StorageCost;
            dataWs.Cell(6, 10).Value = Business_Case_Data.TotalAzureCostDetails.NetworkCost;
            dataWs.Cell(7, 10).Value = Business_Case_Data.TotalAzureCostDetails.SecurityCost;
            dataWs.Cell(8, 10).Value = Business_Case_Data.TotalAzureCostDetails.ITLaborCost;
            dataWs.Cell(9, 10).Value = Business_Case_Data.TotalAzureCostDetails.FacilitiesCost;
            dataWs.Cell(10, 10).Value = Business_Case_Data.TotalAzureCostDetails.ManagementCost;
            dataWs.Cell(11, 10).Value = Business_Case_Data.TotalAzureCostDetails.AhubSavings;
            dataWs.Cell(12, 10).Value = Business_Case_Data.TotalAzureCostDetails.EsuSavings;
            dataWs.Cell(13, 10).Value = Business_Case_Data.TotalAzureCostDetails.LinuxAhubSavings;

            dataWs.Cell(2, 11).Value = Business_Case_Data.TotalOnPremisesCostDetails.TotalCost;
            dataWs.Cell(3, 11).Value = Business_Case_Data.TotalOnPremisesCostDetails.ComputeCost;
            dataWs.Cell(4, 11).Value = Business_Case_Data.TotalOnPremisesCostDetails.LicenseCost;
            dataWs.Cell(5, 11).Value = Business_Case_Data.TotalOnPremisesCostDetails.StorageCost;
            dataWs.Cell(6, 11).Value = Business_Case_Data.TotalOnPremisesCostDetails.NetworkCost;
            dataWs.Cell(7, 11).Value = Business_Case_Data.TotalOnPremisesCostDetails.SecurityCost;
            dataWs.Cell(8, 11).Value = Business_Case_Data.TotalOnPremisesCostDetails.ITLaborCost;
            dataWs.Cell(9, 11).Value = Business_Case_Data.TotalOnPremisesCostDetails.FacilitiesCost;
            dataWs.Cell(10, 11).Value = Business_Case_Data.TotalOnPremisesCostDetails.ManagementCost;
            dataWs.Cell(11, 11).Value = Business_Case_Data.TotalOnPremisesCostDetails.AhubSavings;
            dataWs.Cell(12, 11).Value = Business_Case_Data.TotalOnPremisesCostDetails.EsuSavings;
            dataWs.Cell(13, 11).Value = Business_Case_Data.TotalOnPremisesCostDetails.LinuxAhubSavings;

            dataWs.Cell(2, 12).Value = Business_Case_Data.azureArcEnabledOnPremisesCostDetails.TotalCost;
            dataWs.Cell(3, 12).Value = Business_Case_Data.azureArcEnabledOnPremisesCostDetails.ComputeCost;
            dataWs.Cell(4, 12).Value = Business_Case_Data.azureArcEnabledOnPremisesCostDetails.LicenseCost;
            dataWs.Cell(5, 12).Value = Business_Case_Data.azureArcEnabledOnPremisesCostDetails.StorageCost;
            dataWs.Cell(6, 12).Value = Business_Case_Data.azureArcEnabledOnPremisesCostDetails.NetworkCost;
            dataWs.Cell(7, 12).Value = Business_Case_Data.azureArcEnabledOnPremisesCostDetails.SecurityCost;
            dataWs.Cell(8, 12).Value = Business_Case_Data.azureArcEnabledOnPremisesCostDetails.ITLaborCost;
            dataWs.Cell(9, 12).Value = Business_Case_Data.azureArcEnabledOnPremisesCostDetails.FacilitiesCost;
            dataWs.Cell(10, 12).Value = Business_Case_Data.azureArcEnabledOnPremisesCostDetails.ManagementCost;
            dataWs.Cell(11, 12).Value = Business_Case_Data.azureArcEnabledOnPremisesCostDetails.AhubSavings;
            dataWs.Cell(12, 12).Value = Business_Case_Data.azureArcEnabledOnPremisesCostDetails.EsuSavings;
            dataWs.Cell(13, 12).Value = Business_Case_Data.azureArcEnabledOnPremisesCostDetails.LinuxAhubSavings;

            dataWs.Cell(2, 13).Value = Business_Case_Data.futureAzureArcEnabledOnPremisesCostDetails.TotalCost;
            dataWs.Cell(3, 13).Value = Business_Case_Data.futureAzureArcEnabledOnPremisesCostDetails.ComputeCost;
            dataWs.Cell(4, 13).Value = Business_Case_Data.futureAzureArcEnabledOnPremisesCostDetails.LicenseCost;
            dataWs.Cell(5, 13).Value = Business_Case_Data.futureAzureArcEnabledOnPremisesCostDetails.StorageCost;
            dataWs.Cell(6, 13).Value = Business_Case_Data.futureAzureArcEnabledOnPremisesCostDetails.NetworkCost;
            dataWs.Cell(7, 13).Value = Business_Case_Data.futureAzureArcEnabledOnPremisesCostDetails.SecurityCost;
            dataWs.Cell(8, 13).Value = Business_Case_Data.futureAzureArcEnabledOnPremisesCostDetails.ITLaborCost;
            dataWs.Cell(9, 13).Value = Business_Case_Data.futureAzureArcEnabledOnPremisesCostDetails.FacilitiesCost;
            dataWs.Cell(10, 13).Value = Business_Case_Data.futureAzureArcEnabledOnPremisesCostDetails.ManagementCost;
            dataWs.Cell(11, 13).Value = Business_Case_Data.futureAzureArcEnabledOnPremisesCostDetails.AhubSavings;
            dataWs.Cell(12, 13).Value = Business_Case_Data.futureAzureArcEnabledOnPremisesCostDetails.EsuSavings;
            dataWs.Cell(13, 13).Value = Business_Case_Data.futureAzureArcEnabledOnPremisesCostDetails.LinuxAhubSavings;

            dataWs.Cell(2, 14).Value = Business_Case_Data.futureCostDetails.TotalCost;
            dataWs.Cell(3, 14).Value = Business_Case_Data.futureCostDetails.ComputeCost;
            dataWs.Cell(4, 14).Value = Business_Case_Data.futureCostDetails.LicenseCost;
            dataWs.Cell(5, 14).Value = Business_Case_Data.futureCostDetails.StorageCost;
            dataWs.Cell(6, 14).Value = Business_Case_Data.futureCostDetails.NetworkCost;
            dataWs.Cell(7, 14).Value = Business_Case_Data.futureCostDetails.SecurityCost;
            dataWs.Cell(8, 14).Value = Business_Case_Data.futureCostDetails.ITLaborCost;
            dataWs.Cell(9, 14).Value = Business_Case_Data.futureCostDetails.FacilitiesCost;
            dataWs.Cell(10, 14).Value = Business_Case_Data.futureCostDetails.ManagementCost;
            dataWs.Cell(11, 14).Value = Business_Case_Data.futureCostDetails.AhubSavings;
            dataWs.Cell(12, 14).Value = Business_Case_Data.futureCostDetails.EsuSavings;
            dataWs.Cell(13, 14).Value = Business_Case_Data.futureCostDetails.LinuxAhubSavings;

            dataWs.Cell(2, 15).Value = Business_Case_Data.WindowsAhubSavings;
            dataWs.Cell(2, 16).Value = Business_Case_Data.LinuxAhubSavings;
            dataWs.Cell(2, 17).Value = Business_Case_Data.SqlAhubSavings;
            dataWs.Cell(2, 18).Value = Business_Case_Data.SqlAzureCost;
            dataWs.Cell(2, 19).Value = Business_Case_Data.MachineAzureCost;
            dataWs.Cell(2, 20).Value = Business_Case_Data.AzureArcEnabledOnPremisesCost;
            dataWs.Cell(2, 21).Value = Business_Case_Data.FutureCostIncludingAzureArc;
            dataWs.Cell(2, 22).Value = Business_Case_Data.FutureEsuSavingsFor4YearsIncludingAzureArc;
            dataWs.Cell(2, 23).Value = Business_Case_Data.FutureManagementCostSavingsIncludingAzureArc;
            dataWs.Cell(2, 24).Value = Business_Case_Data.FutureSecurityCostSavingsIncludingAzureArc;
            dataWs.Cell(2, 25).Value = Business_Case_Data.AzureArcServicesCost;
            dataWs.Cell(2, 26).Value = Business_Case_Data.FutureAzureArcEnabledOnPremisesCost;
            dataWs.Cell(2, 27).Value = Business_Case_Data.FutureAzureArcServicesCost;
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
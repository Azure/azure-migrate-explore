// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using ClosedXML.Excel;
using System.Collections.Generic;

using Azure.Migrate.Explore.Common;
using Azure.Migrate.Explore.Models;
using System;
using AzureMigrateExplore.Discovery;

namespace Azure.Migrate.Explore.Excel
{
    public class ExportDiscoveryReport
    {
        private readonly List<DiscoveryData> DiscoveredData;
        private readonly DiscoveryProperties DiscoveryPropertiesData;
        private readonly vCenterHostDiscovery VCenterHostDiscoveryData;
        private readonly string DiscoveryDataFromARG;
        XLWorkbook DiscoveryWb;

        public ExportDiscoveryReport(List<DiscoveryData> discoveredData, vCenterHostDiscovery vCenterHostData, DiscoveryProperties discoveryPropertiesData, string discoveryDataFromARG = "")
        {
            DiscoveredData = discoveredData;
            DiscoveryPropertiesData = discoveryPropertiesData;
            VCenterHostDiscoveryData = vCenterHostData;
            DiscoveryDataFromARG = discoveryDataFromARG;
            DiscoveryWb = new XLWorkbook();
        }

        public void GenerateDiscoveryReportExcel()
        {
            GeneratePropertyWorksheet();
            GenerateDiscoveryReportWorksheet();
            GeneratevCenterHostReportWorksheet();
            GenerateArgDataWorksheet();

            DiscoveryWb.SaveAs(UtilityFunctions.GetReportsDirectory() + "\\" + DiscoveryReportConstants.DiscoveryReportName);
        }

        private void GenerateArgDataWorksheet()
        {
            UtilityFunctions.SaveARGJsonDataToWorksheet(
                DiscoveryDataFromARG,
                DiscoveryWb.Worksheets.Add(DiscoveryReportConstants.ARGDataTabName, 4));
        }

        private void GeneratePropertyWorksheet()
        {
            var propertiesWs = DiscoveryWb.Worksheets.Add(DiscoveryReportConstants.PropertiesTabName, 1);

            var propertyHeaders = DiscoveryReportConstants.PropertiesList;

            for (int i = 0; i < propertyHeaders.Count; i++)
                propertiesWs.Cell(1, i + 1).Value = propertyHeaders[i];

            // Add values: important to add in the same order as above

            propertiesWs.Cell(2, 1).Value = DiscoveryPropertiesData.TenantId;
            propertiesWs.Cell(2, 2).Value = DiscoveryPropertiesData.Subscription;
            propertiesWs.Cell(2, 3).Value = DiscoveryPropertiesData.ResourceGroup;
            propertiesWs.Cell(2, 4).Value = DiscoveryPropertiesData.AzureMigrateProjectName;
            propertiesWs.Cell(2, 5).Value = DiscoveryPropertiesData.DiscoverySiteName;
            propertiesWs.Cell(2, 6).Value = DiscoveryPropertiesData.Workflow;
            propertiesWs.Cell(2, 7).Value = DiscoveryPropertiesData.SourceAppliances;
        }

        private void GenerateDiscoveryReportWorksheet()
        {
            var dataWs = DiscoveryWb.Worksheets.Add(DiscoveryReportConstants.Discovery_Report_TabName, 2);

            UtilityFunctions.AddColumnHeadersToWorksheet(dataWs, DiscoveryReportConstants.DiscoveryReportColumns);

            if (DiscoveredData != null && DiscoveredData.Count > 0)
                dataWs.Cell(2, 1).InsertData(DiscoveredData);
        }

        private void GeneratevCenterHostReportWorksheet()
        {
            var dataWs = DiscoveryWb.Worksheets.Add(DiscoveryReportConstants.vCenterHost_Report_TabName, 3);

            var dataHeaders = DiscoveryReportConstants.VCenterHostReportColumns;

            for (int i = 0; i < dataHeaders.Count; i++)
                dataWs.Cell(1, i + 1).Value = dataHeaders[i];

            dataWs.Cell(2, 1).Value = VCenterHostDiscoveryData.vCenters;
            dataWs.Cell(2, 2).Value = VCenterHostDiscoveryData.Hosts;
        }
    }
}
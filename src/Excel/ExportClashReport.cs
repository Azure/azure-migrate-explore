// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using ClosedXML.Excel;
using System.Collections.Generic;

using Azure.Migrate.Explore.Models;
using Azure.Migrate.Explore.Common;

namespace Azure.Migrate.Explore.Excel
{
    public class ExportClashReport
    {
        private readonly List<Clash_Report> Clash_Report_List;
        XLWorkbook ClashWb;

        public ExportClashReport(List<Clash_Report> clash_Report_List)
        {
            Clash_Report_List = clash_Report_List;
            ClashWb = new XLWorkbook();
        }

        public void GenerateClashReportExcel()
        {
            GenerateClashReportWorksheet();

            ClashWb.SaveAs(UtilityFunctions.GetReportsDirectory() + "\\" + ClashReportConstants.ClashReportName);
        }

        private void GenerateClashReportWorksheet()
        {
            var dataWs = ClashWb.Worksheets.Add(ClashReportConstants.Clash_Report_TabName, 1);

            UtilityFunctions.AddColumnHeadersToWorksheet(dataWs, ClashReportConstants.Clash_Report_Columns);

            if (Clash_Report_List != null && Clash_Report_List.Count > 0)
                dataWs.Cell(2, 1).InsertData(Clash_Report_List);
        }
    }
}
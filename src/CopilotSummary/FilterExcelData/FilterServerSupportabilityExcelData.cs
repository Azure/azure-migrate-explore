// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using System.Collections.Generic;
using System.Linq;

using Azure.Migrate.Explore.Excel;
using Azure.Migrate.Explore.Models.CopilotSummary.MigrationSummary;
using Azure.Migrate.Explore.Models;
using Azure.Migrate.Explore.Common;

namespace Azure.Migrate.Explore.CopilotSummary.FilterExcelData
{
    public static class FilterServerSupportabilityExcelData
    {
        public static List<MigrationDataProperty> FilterServerSupportabilityData(ImportCoreReport coreReportData, ImportOpportunityReport opportunityReportData, Logger.LogHandler logger)
        {
            ServerSupportabilityData filteredData = FilterData(coreReportData, opportunityReportData, logger);
            List<MigrationDataProperty> migrationDataProperties = UtilityFunctions.FetchProperties(typeof(ServerSupportabilityData), filteredData);
            return migrationDataProperties;
        }

        public static ServerSupportabilityData FilterData(ImportCoreReport coreReportData, ImportOpportunityReport opportunityReportData, Logger.LogHandler logger)
        {
            logger.LogInformation("Retrieving data for Legacy servers or servers moving to out of support state");

            double windowsServerExtended = 0.00;
            double windowsServerOutOfSupport = 0.00;
            double sqlServerExtended = 0.00;
            double sqlServerOutOfSupport = 0.00;
            HashSet<string> windowsUniqueMachineId = new HashSet<string>();
            HashSet<string> sqlUniqueMachineId = new HashSet<string>();

            foreach (VM_Opportunity_Perf item in opportunityReportData.VmOpportunityPerfList)
            {
                windowsUniqueMachineId.Add(item.MachineId);
                if (item.SupportStatus == "Extended")
                {
                    windowsServerExtended++;
                }
                else if (item.SupportStatus == "Out of support")
                {
                    windowsServerOutOfSupport++;
                }
            }

            foreach (All_VM_IaaS_Server_Rehost_Perf item in coreReportData.AllVMIaasServerRehostPerfList)
            {
                windowsUniqueMachineId.Add(item.MachineId);
                if (item.SupportStatus == "Extended")
                {
                    windowsServerExtended++;
                }
                else if (item.SupportStatus == "Out of support")
                {
                    windowsServerOutOfSupport++;
                }
            }
            windowsServerExtended = windowsServerExtended / windowsUniqueMachineId.Count();
            windowsServerOutOfSupport = windowsServerOutOfSupport / windowsUniqueMachineId.Count();

            foreach (SQL_All_Instances item in coreReportData.SqlAllInstancesList)
            {
                sqlUniqueMachineId.Add(item.MachineId);
                if (item.SupportStatus == "Extended")
                {
                    sqlServerExtended++;
                }
                else if (item.SupportStatus == "Out of support")
                {
                    sqlServerOutOfSupport++;
                }
            }
            sqlServerExtended = sqlServerExtended / sqlUniqueMachineId.Count();
            sqlServerOutOfSupport = sqlServerOutOfSupport / sqlUniqueMachineId.Count();

            ServerSupportabilityData migrationData = new ServerSupportabilityData
            {
                WindowsServerExtended = windowsServerExtended,
                WindowsServerOutOfSupport = windowsServerOutOfSupport,
                SqlServerExtended = sqlServerExtended,
                SqlServerOutOfSupport = sqlServerOutOfSupport
            };

            logger.LogInformation("Fetched data for Legacy servers or servers moving to out of support state");

            return migrationData;
        }
    }
}
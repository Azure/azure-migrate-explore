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
    public static class FilterAzureBcdrServiceExcelData
    {
        public static List<MigrationDataProperty> FilterAzureBcdrServiceData(ImportCoreReport coreReportData, Logger.LogHandler logger)
        {
            AzureBcdrServicesData filteredData = FilterData(coreReportData, logger);
            List<MigrationDataProperty> migrationDataProperties = UtilityFunctions.FetchProperties(typeof(AzureBcdrServicesData), filteredData);
            return migrationDataProperties;
        }

        public static AzureBcdrServicesData FilterData(ImportCoreReport coreReportData, Logger.LogHandler logger)
        {
            logger.LogInformation("Retrieving data for Azure Backup and Azure Disaster recovery services");

            HashSet<string> asrInstancesCount = new HashSet<string>();
            double annualAsrCost = 0.00;
            double annualBackupCost = 0.00;

            AzureBcdrServicesData migrationData = new AzureBcdrServicesData
            {
                ASRInstancesCount = asrInstancesCount.Count(),
                AnnualASRCost = annualAsrCost,
                AnnualBackupCost = annualBackupCost,
            };

            logger.LogInformation("Fetched data for Azure Backup and Azure Disaster recovery services");

            return migrationData;
        }
    }
}
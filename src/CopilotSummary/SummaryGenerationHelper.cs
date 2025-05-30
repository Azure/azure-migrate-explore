// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using Azure.Migrate.Explore.Assessment;
using Azure.Migrate.Explore.Excel;
using Azure.Migrate.Explore.Logger;
using Azure.Migrate.Explore.Models;
using Azure.Migrate.Explore.Models.CopilotSummary.MigrationSummary;
using Azure.Migrate.Explore.CopilotSummary.FilterExcelData;
using Newtonsoft.Json;
using System.Collections.Generic;
using System;

namespace Azure.Migrate.Explore.Summary
{
    public static class SummaryGenerationHelper
    {
        public static List<string> GetSummaryInsights(LogHandler logger)
        {
            List<string> filteredData = FilterAMEData(logger);

            return filteredData;
        }

        public static List<string> FilterAMEData(LogHandler logger)
        {
            logger.LogInformation("Initiating migration data retrieval");

            (List<DiscoveryData> discoveredData, List<vCenterHostDiscovery> vCenterHostData) = FetchDiscoveryExcelData(logger);

            ImportCoreReport coreReportData = FetchCoreReportExcelData(logger);

            ImportOpportunityReport opportunityReportData = FetchOpportunityReportExcelData(logger);

            List<MigrationDataProperty> migrationDiscoveryData = FilterOnpremExcelData.FilterOnpremData(discoveredData, coreReportData, logger);

            List<MigrationDataProperty> migrationPaaSData = FilterPaasExcelData.FilterPaasData(coreReportData, logger);

            List<MigrationDataProperty> migrationIaaSData = FilterIaasExcelData.FilterIaasData(coreReportData, logger);

            List<MigrationDataProperty> azureBcdrServicesData = FilterAzureBcdrServiceExcelData.FilterAzureBcdrServiceData(coreReportData, logger);

            List<MigrationDataProperty> serverSupportabilityData = FilterServerSupportabilityExcelData.FilterServerSupportabilityData(coreReportData, opportunityReportData, logger);

            //avs data
            List<MigrationDataProperty> avsData = FilterAvsExcelData.FilterAvsData(coreReportData, discoveredData, vCenterHostData, logger);

            List<string> migrateData = new List<string>
            {
                JsonConvert.SerializeObject(migrationDiscoveryData),
                JsonConvert.SerializeObject(migrationPaaSData),
                JsonConvert.SerializeObject(migrationIaaSData),
                JsonConvert.SerializeObject(azureBcdrServicesData),
                JsonConvert.SerializeObject(serverSupportabilityData),
                JsonConvert.SerializeObject(avsData)
            };

            logger.LogInformation("Filtered azure migrate explore migration data");

            return migrateData;
        }

        public static (List<DiscoveryData>, List<vCenterHostDiscovery>) FetchDiscoveryExcelData(Logger.LogHandler logger)
        {
            logger.LogInformation("Initiating discovered data retrieval");

            List<DiscoveryData> discoveredData = new List<DiscoveryData>();
            List<vCenterHostDiscovery> vCenterHostData = new List<vCenterHostDiscovery>();
            new ImportDiscoveryReport(logger, discoveredData, vCenterHostData).ImportDiscoveryData();
            new DiscoveryDataValidation().BeginValidation(logger, discoveredData);

            logger.LogInformation("Fetched discovery data");
            return (discoveredData, vCenterHostData);
        }

        public static ImportCoreReport FetchCoreReportExcelData(Logger.LogHandler logger)
        {
            logger.LogInformation("Initiating core report data retrieval");

            ImportCoreReport importCoreReportObj = new ImportCoreReport(logger);
            importCoreReportObj.ImportCoreReportData();

            logger.LogInformation("Fetched core report data");
            return importCoreReportObj;
        }

        public static ImportOpportunityReport FetchOpportunityReportExcelData(Logger.LogHandler logger)
        {
            logger.LogInformation("Initiating opportunity report data retrieval");

            ImportOpportunityReport importOpportunityReportObj = new ImportOpportunityReport(logger);
            importOpportunityReportObj.ImportOpportunityReportData();

            logger.LogInformation("Fetched opportunity report data");
            return importOpportunityReportObj;
        }
    }
}
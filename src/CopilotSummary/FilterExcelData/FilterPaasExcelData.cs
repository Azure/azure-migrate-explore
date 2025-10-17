// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

using Azure.Migrate.Explore.Excel;
using Azure.Migrate.Explore.Models.CopilotSummary.MigrationSummary;
using Azure.Migrate.Explore.Models;
using Azure.Migrate.Explore.Common;

namespace Azure.Migrate.Explore.CopilotSummary.FilterExcelData
{
    public static class FilterPaasExcelData
    {
        public static List<MigrationDataProperty> FilterPaasData(ImportCoreReport coreReportData, Logger.LogHandler logger)
        {
            MigrationPaasData filteredData = FilterData(coreReportData, logger);
            List<MigrationDataProperty> migrationDataProperties = UtilityFunctions.FetchProperties(typeof(MigrationPaasData), filteredData);
            return migrationDataProperties;
        }

        public static MigrationPaasData FilterData(ImportCoreReport coreReportData, Logger.LogHandler logger)
        {
            logger.LogInformation("Retrieving data for workloads suggested for PaaS targets");

            int workloadAspNetWebappsDev = 0;
            int workloadAspNetWebappsProd = 0;
            int workloadSqlServerDatabaseDev = 0;
            int workloadSqlServerDatabaseProd = 0;
            int azAspNetWebappsDev = 0;
            int azAspNetWebappsProd = 0;
            int azSqlServerDatabaseDev = 0;
            int azSqlServerDatabaseProd = 0;
            HashSet<string> activeSqlInstances = new HashSet<string>();
            double sqlAvgCpuUtilization = 0.00;
            double sqlTotalStorageinTB = 0.00;
            double sqlAnnualComputeCostEstimate = 0.00;
            double sqlAnnualComputeCostEstimateRI3year = 0.00;
            double sqlAnnualComputeCostEstimateAhub = 0.00;
            double sqlAnnualComputeCostEstimateAhubRI3year = 0.00;
            double sqlAnnualStorageCostEstimate = 0.00;
            double webappAnnualComputeCostEstimate = 0.00;
            double webappAnnualComputeCostEstimateRI3year = 0.00;
            double webappAnnualComputeCostEstimateAsp = 0.00;
            double webappAnnualConsumptionCost = 0.00;
            Dictionary<string, double> sqlTopRecommendationToCostProd = new Dictionary<string, double>();
            Dictionary<string, double> sqlTopRecommendationToCostDev = new Dictionary<string, double>();
            Dictionary<string, HashSet<string>> sqlTopRecommendationToServerInstance = new Dictionary<string, HashSet<string>>();
            Dictionary<string, int> sqlTopRecommendationToCount = new Dictionary<string, int>();
            Dictionary<string, int> sqlWarningsToCount = new Dictionary<string, int>();
            Dictionary<string, double> webappTopRecommendationToCostProd = new Dictionary<string, double>();
            Dictionary<string, double> webappTopRecommendationToCostDev = new Dictionary<string, double>();
            Dictionary<string, HashSet<string>> webappTopRecommendationToAppServicePlan = new Dictionary<string, HashSet<string>>();
            Dictionary<string, int> webappTopRecommendationToCount = new Dictionary<string, int>();
            Dictionary<string, int> webappWarningsToCount = new Dictionary<string, int>();

            List<string> paasTransformationSummary = new List<string>
            {
                $"{workloadAspNetWebappsDev} IIS webservers hosting ASP.NET webapps in Dev/Test environment suggested for migration to {azAspNetWebappsDev} App Service Plan (PaaS).",
                $"{workloadAspNetWebappsProd} IIS webservers hosting ASP.NET webapps in Production environment suggested for migration to {azAspNetWebappsProd} App Service Plan (PaaS).",
                $"{workloadSqlServerDatabaseDev} SQL Servers in Dev/Test environment suggested for migration to {azSqlServerDatabaseDev} Azure SQL Managed Instance.",
                $"{workloadSqlServerDatabaseProd} Count of SQL Server in Production environment suggested for migration to {azSqlServerDatabaseProd} Azure SQL Managed Instance."
            };

            double sqlTotalStorageinMB = 0.00;

            foreach (KeyValuePair<string, HashSet<string>> kvp in sqlTopRecommendationToServerInstance)
            {
                sqlTopRecommendationToCount.Add(kvp.Key, kvp.Value.Count());
            }

            webappAnnualConsumptionCost = Math.Min(webappAnnualComputeCostEstimateAsp, webappAnnualComputeCostEstimateRI3year);

            foreach (KeyValuePair<string, HashSet<string>> kvp in webappTopRecommendationToAppServicePlan)
            {
                webappTopRecommendationToCount.Add(kvp.Key, kvp.Value.Count());
            }

            List<string> annualComputeCostComparison = new List<string>
            {
                $"Total compute cost for Right-sized PaaS targets without any Azure offer for SQL is {sqlAnnualComputeCostEstimate}",
                $"Total compute cost for Right-sized PaaS targets considering 3Year RI for SQL is {sqlAnnualComputeCostEstimateRI3year}",
                $"Total compute cost for Right-sized PaaS targets considering AHUB for SQL is {sqlAnnualComputeCostEstimateAhub}",
                $"Total compute cost for Right-sized PaaS targets considering 3Year RI + AHUB for SQL is {sqlAnnualComputeCostEstimateAhubRI3year}",
                $"Total compute cost for Right-sized PaaS targets without any Azure offer for Webapps is {webappAnnualComputeCostEstimate}",
                $"Total compute cost for Right-sized PaaS targets considering 3Year RI for Webapps is {webappAnnualComputeCostEstimateRI3year}",
                $"Total compute cost for Right-sized PaaS targets considering ASP offer for Webapps is {webappAnnualComputeCostEstimateAsp}",
            };

            MigrationPaasData migrationData = new MigrationPaasData
            {
                PaasTransformationSummary = JsonConvert.SerializeObject(paasTransformationSummary),
                ActiveSqlInstances = activeSqlInstances.Count(),
                SqlAvgCpuUtilization = sqlAvgCpuUtilization,
                SqlTotalStorageinTB = sqlTotalStorageinTB,
                WebappAnnualConsumptionCost = webappAnnualConsumptionCost,
                SqlTopRecommendationCostProd = JsonConvert.SerializeObject(sqlTopRecommendationToCostProd),
                SqlTopRecommendationCostDev = JsonConvert.SerializeObject(sqlTopRecommendationToCostDev),
                SqlTopRecommendationCount = JsonConvert.SerializeObject(sqlTopRecommendationToCount),
                SqlWarnings = JsonConvert.SerializeObject(sqlWarningsToCount),
                SqlAnnualStorageCostEstimate = sqlAnnualStorageCostEstimate,
                WebappTopRecommendationCostProd = JsonConvert.SerializeObject(webappTopRecommendationToCostProd),
                WebappTopRecommendationCostDev = JsonConvert.SerializeObject(webappTopRecommendationToCostDev),
                WebappTopRecommendationCount = JsonConvert.SerializeObject(webappTopRecommendationToCount),
                WebappWarnings = JsonConvert.SerializeObject(webappWarningsToCount),
                AnnualComputeCostComparison = JsonConvert.SerializeObject(annualComputeCostComparison)
            };

            logger.LogInformation("Fetched data for workloads suggested for PaaS targets");

            return migrationData;
        }
    }
}
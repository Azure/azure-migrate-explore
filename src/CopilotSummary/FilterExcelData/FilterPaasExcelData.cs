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

            foreach (Financial_Summary item in coreReportData.FinancialSummaryList)
            {
                if (item.MigrationStrategy == "Modernize/Re-Platform(PaaS)")
                {
                    if (item.Workload == "ASP.NET WebApps on IIS - Dev/Test")
                    {
                        workloadAspNetWebappsDev = item.SourceCount;
                        azAspNetWebappsDev = item.TargetCount;
                    }
                    else if (item.Workload == "ASP.NET WebApps on IIS - Prod")
                    {
                        workloadAspNetWebappsProd = item.SourceCount;
                        azAspNetWebappsProd = item.TargetCount;
                    }
                    else if (item.Workload == "SQL Server Database Engine - Dev/Test")
                    {
                        workloadSqlServerDatabaseDev = item.SourceCount;
                        azSqlServerDatabaseDev = item.TargetCount;
                    }
                    else if (item.Workload == "SQL Server Database Engine - Prod")
                    {
                        workloadSqlServerDatabaseProd = item.SourceCount;
                        azSqlServerDatabaseProd = item.TargetCount;
                    }
                }
            }

            List<string> paasTransformationSummary = new List<string>
            {
                $"{workloadAspNetWebappsDev} IIS webservers hosting ASP.NET webapps in Dev/Test environment suggested for migration to {azAspNetWebappsDev} App Service Plan (PaaS).",
                $"{workloadAspNetWebappsProd} IIS webservers hosting ASP.NET webapps in Production environment suggested for migration to {azAspNetWebappsProd} App Service Plan (PaaS).",
                $"{workloadSqlServerDatabaseDev} SQL Servers in Dev/Test environment suggested for migration to {azSqlServerDatabaseDev} Azure SQL Managed Instance.",
                $"{workloadSqlServerDatabaseProd} Count of SQL Server in Production environment suggested for migration to {azSqlServerDatabaseProd} Azure SQL Managed Instance."
            };

            double sqlTotalStorageinMB = 0.00;
            foreach (SQL_MI_PaaS item in coreReportData.SqlMIPaasList)
            {
                string server_Instance = item.MachineId + "." + item.SQLInstance;
                activeSqlInstances.Add(server_Instance);

                sqlAvgCpuUtilization += item.CpuUtilizationInPercentage;
                sqlTotalStorageinMB += item.TotalDBSizeInMB;

                sqlAnnualComputeCostEstimate += item.MonthlyComputeCostEstimate * 12;
                sqlAnnualComputeCostEstimateRI3year += item.MonthlyComputeCostEstimate_RI3year * 12;
                sqlAnnualComputeCostEstimateAhub += item.MonthlyComputeCostEstimate_AHUB * 12;
                sqlAnnualComputeCostEstimateAhubRI3year += item.MonthlyComputeCostEstimate_AHUB_RI3year * 12;
                sqlAnnualStorageCostEstimate += item.MonthlyStorageCostEstimate * 12;

                if (item.AzureSQLMIConfiguration != "")
                {
                    if (item.Environment == "Prod")
                    {
                        if (!sqlTopRecommendationToCostProd.ContainsKey(item.AzureSQLMIConfiguration))
                        {
                            sqlTopRecommendationToCostProd.Add(item.AzureSQLMIConfiguration, 0.00);
                        }
                        sqlTopRecommendationToCostProd[item.AzureSQLMIConfiguration] += item.MonthlyComputeCostEstimate_AHUB_RI3year * 12;
                    }
                    else
                    {
                        if (!sqlTopRecommendationToCostDev.ContainsKey(item.AzureSQLMIConfiguration))
                        {
                            sqlTopRecommendationToCostDev.Add(item.AzureSQLMIConfiguration, 0.00);
                        }
                        sqlTopRecommendationToCostDev[item.AzureSQLMIConfiguration] += item.MonthlyComputeCostEstimate_AHUB_RI3year * 12;
                    }

                    if (!sqlTopRecommendationToServerInstance.ContainsKey(item.AzureSQLMIConfiguration))
                    {
                        sqlTopRecommendationToServerInstance[item.AzureSQLMIConfiguration] = new HashSet<string>();
                    }
                    sqlTopRecommendationToServerInstance[item.AzureSQLMIConfiguration].Add(server_Instance);
                }

                if (item.AzureSQLMIReadiness_Warnings != "")
                {
                    List<string> itemWarnings = new List<string>(item.AzureSQLMIReadiness_Warnings.Split(';'));
                    foreach (string warning in itemWarnings)
                    {
                       if (warning != "")
                       {
                            if (!sqlWarningsToCount.ContainsKey(warning))
                            {
                                sqlWarningsToCount.Add(warning, 0);
                            }
                            sqlWarningsToCount[warning]++;
                        }
                    }             
                }
            }
            sqlAvgCpuUtilization /= coreReportData.SqlMIPaasList.Count();
            sqlTotalStorageinTB = (sqlTotalStorageinMB / 1024.0) / 1024.0;

            foreach (KeyValuePair<string, HashSet<string>> kvp in sqlTopRecommendationToServerInstance)
            {
                sqlTopRecommendationToCount.Add(kvp.Key, kvp.Value.Count());
            }

            foreach (WebApp_PaaS item in coreReportData.WebappPaasList)
            {
                webappAnnualComputeCostEstimate += item.MonthlyComputeCostEstimate * 12;
                webappAnnualComputeCostEstimateAsp += item.MonthlyComputeCostEstimate_ASP3year * 12;
                webappAnnualComputeCostEstimateRI3year += item.MonthlyComputeCostEstimate_RI3year * 12;
                
                double webappMonthlyConsumptionCost = Math.Min(item.MonthlyComputeCostEstimate_ASP3year, item.MonthlyComputeCostEstimate_RI3year);

                if (item.RecommendedSKU != "")
                {
                    if (item.Environment == "Prod")
                    {
                        if (!webappTopRecommendationToCostProd.ContainsKey(item.RecommendedSKU))
                        {
                            webappTopRecommendationToCostProd.Add(item.RecommendedSKU, 0.00);
                        }
                        webappTopRecommendationToCostProd[item.RecommendedSKU] += webappMonthlyConsumptionCost * 12;
                    }
                    else
                    {
                        if (!webappTopRecommendationToCostDev.ContainsKey(item.RecommendedSKU))
                        {
                            webappTopRecommendationToCostDev.Add(item.RecommendedSKU, 0.00);
                        }
                        webappTopRecommendationToCostDev[item.RecommendedSKU] += webappMonthlyConsumptionCost * 12;
                    }

                    if (!webappTopRecommendationToAppServicePlan.ContainsKey(item.RecommendedSKU))
                    {
                        webappTopRecommendationToAppServicePlan.Add(item.RecommendedSKU, new HashSet<string>());
                    }
                    webappTopRecommendationToAppServicePlan[item.RecommendedSKU].Add(item.AppServicePlanName);
                }

                if (item.AzureAppServiceReadiness_Warnings != "")
                {
                    List<string> itemWarnings = new List<string>(item.AzureAppServiceReadiness_Warnings.Split(';'));
                    foreach (string warning in itemWarnings)
                    {
                        if (warning != "")
                        {
                            if (!webappWarningsToCount.ContainsKey(warning))
                            {
                                webappWarningsToCount.Add(warning, 0);
                            }
                            webappWarningsToCount[warning]++;
                        }
                    }
                }
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
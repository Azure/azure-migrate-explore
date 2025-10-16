// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

using Azure.Migrate.Explore.Excel;
using Azure.Migrate.Explore.Models.CopilotSummary.MigrationSummary;
using Azure.Migrate.Explore.Models;
using Azure.Migrate.Explore.Common;

namespace Azure.Migrate.Explore.CopilotSummary.FilterExcelData
{
    public static class FilterIaasExcelData
    {
        public static List<MigrationDataProperty> FilterIaasData(ImportCoreReport coreReportData, Logger.LogHandler logger)
        {
            MigrationIaasData filteredData = FilterData(coreReportData, logger);
            List<MigrationDataProperty> migrationDataProperties = UtilityFunctions.FetchProperties(typeof(MigrationIaasData), filteredData);
            return migrationDataProperties;
        }

        public static MigrationIaasData FilterData(ImportCoreReport coreReportData, Logger.LogHandler logger)
        {
            logger.LogInformation("Retrieving data for workloads suggested for IaaS targets");

            int workloadAspNetWebappsDev = 0;
            int workloadAspNetWebappsProd = 0;
            int workloadServerDev = 0;
            int workloadServerProd = 0;
            int workloadSqlServerDatabaseDev = 0;
            int workloadSqlServerDatabaseProd = 0;
            int azAspNetWebappsIISDev = 0;
            int azAspNetWebappsIISProd = 0;
            int azServerDev = 0;
            int azServerProd = 0;
            int azSqlServerDatabaseDev = 0;
            int azSqlServerDatabaseProd = 0;
            int totalMachinesCount = 0;
            int readyMachinesCount = 0;
            int totalCores = 0;
            double totalMemoryinGB = 0.00;
            double totalStorageinGB = 0.00;
            double aspNetAzureVmCost = 0.00;
            double azureVmCost = 0.00;
            double sqlServerAzureVmCost = 0.00;
            double windowsComputeLicenseCost = 0.00;
            double windowsComputeLicenseCostAhub = 0.00;
            double sqlComputeLicenseCost = 0.00;
            double sqlComputeLicenseCostAhub = 0.00;
            double azureVMCostDevAhub = 0.00;
            double azureVMCostProdAhub = 0.00;
            double costSavingsEsu = 0.00; //Business_Case
            double annualComputeCostEstimate = 0.00;
            double annualComputeCostEstimateRI3year = 0.00;
            double annualComputeCostEstimateAsp = 0.00;
            double annualComputeCostEstimateAhub = 0.00;
            double annualComputeCostEstimateAhubRI3year = 0.00;
            double annualStorageCostEstimate = 0.00;
            Dictionary<string, double> vmTopRecommendationToCostProd = new Dictionary<string, double>();
            Dictionary<string, double> vmTopRecommendationToCostDev = new Dictionary<string, double>();
            Dictionary<string, int> vmSkuRecommendedToCount = new Dictionary<string, int>();
            Dictionary<string, int> vmWarningsToCount = new Dictionary<string, int>();
            int standardHddDrivesCount = 0;
            double standardHddDrivesCost = 0.00;
            int standardSsdDrivesCount = 0;
            double standardSsdDrivesCost = 0.00;
            int premiumDisksCount = 0;
            double premiumDiskCost = 0.00;
            int ultraDrivesCount = 0;
            double ultraDisksCost = 0.00;

             List<string> iaasTransformationSummary = new List<string>
            {
                $"{workloadAspNetWebappsDev} IIS webservers hosting ASP.NET webapps in Dev/Test suggested for migration to {azAspNetWebappsIISDev} Azure VMs(IaaS).",
                $"{workloadAspNetWebappsProd} IIS webservers hosting ASP.NET webapps in Production suggested for migration to {azAspNetWebappsIISProd} Azure VMs(IaaS).",
                $"{workloadServerDev} Servers in Dev/Test suggested for migration to {azServerDev} Azure VMs(IaaS)",
                $"{workloadServerProd} Servers in Production suggested for migration to {azServerProd} Azure VMs(IaaS)",
                $"{workloadSqlServerDatabaseDev} SQL Server Database Engines in Dev/Test suggested for {azSqlServerDatabaseDev} SQL Servers on Azure VM(IaaS",
                $"{workloadSqlServerDatabaseProd} SQL Server Database Engines in Production suggested for {azSqlServerDatabaseProd} SQL Servers on Azure VM(IaaS)",
            };

            azureVmCost -= (aspNetAzureVmCost + sqlServerAzureVmCost);

            List<string> annualComputeCostComparison = new List<string>
            {
                $"Total compute cost for Right-sized IaaS targets without any Azure offer is {annualComputeCostEstimate}",
                $"Total compute cost for Right-sized IaaS targets considering 3Year RI is {annualComputeCostEstimateRI3year}",
                $"Total compute cost for Right-sized IaaS targets considering ASP offer is {annualComputeCostEstimateAsp}",
                $"Total compute cost for Right-sized IaaS targets considering AHUB is {annualComputeCostEstimateAhub}",
                $"Total compute cost for Right-sized IaaS targets considering 3Year RI + AHUB is {annualComputeCostEstimateAhubRI3year}",
            };

            MigrationIaasData migrationData = new MigrationIaasData
            {
                IaasTransformationSummary = JsonConvert.SerializeObject(iaasTransformationSummary),
                TotalMachinesCount = totalMachinesCount,
                ReadyMachinesCount = readyMachinesCount,
                TotalCores = totalCores,
                TotalMemoryinGB = totalMemoryinGB,
                TotalStorageinGB = totalStorageinGB,
                AspNetAzureVMCost = aspNetAzureVmCost,
                AzureVMCost = azureVmCost,
                SqlServerAzureVMCost = sqlServerAzureVmCost,
                WindowsComputeLicenseCost = windowsComputeLicenseCost,
                WindowsComputeLicenseCostAhub = windowsComputeLicenseCostAhub,
                SqlComputeLicenseCost = sqlComputeLicenseCost,
                SqlComputeLicenseCostAhub = sqlComputeLicenseCostAhub,
                AzureVMCostDevAhub = azureVMCostDevAhub,
                AzureVMCostProdAhub = azureVMCostProdAhub,
                CostSavingsEsu = costSavingsEsu,
                AnnualComputeCostComparison = JsonConvert.SerializeObject(annualComputeCostComparison),
                AnnualStorageCost = annualStorageCostEstimate,
                VMTopRecommendationCostProd = JsonConvert.SerializeObject(vmTopRecommendationToCostProd),
                VMTopRecommendationCostDev = JsonConvert.SerializeObject(vmTopRecommendationToCostDev),
                VMSkuRecommendedCount = JsonConvert.SerializeObject(vmSkuRecommendedToCount),
                VMWarnings = JsonConvert.SerializeObject(vmWarningsToCount),
                StandardHddDrivesCount = standardHddDrivesCount,
                StandardHddDrivesCost = standardHddDrivesCost,
                StandardSsdDrivesCount = standardSsdDrivesCount,
                StandardSsdDrivesCost = standardSsdDrivesCost,
                PremiumDisksCount = premiumDisksCount,
                PremiumDiskCost = premiumDiskCost,
                UltraDrivesCount = ultraDrivesCount,
                UltraDisksCost = ultraDisksCost
            };

            logger.LogInformation("Fetched data for workloads suggested for IaaS targets");

            return migrationData;
        }
    }
}
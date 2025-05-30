// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using System.Collections.Generic;
using System.Linq;

using Azure.Migrate.Explore.Excel;
using Azure.Migrate.Explore.Models.CopilotSummary.MigrationSummary;
using Azure.Migrate.Explore.Models;
using Azure.Migrate.Explore.Common;
using Azure.Migrate.Explore.Logger;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Azure.Migrate.Explore.CopilotSummary.FilterExcelData
{
    public static class FilterAvsExcelData
    {
        public static List<MigrationDataProperty> FilterAvsData(ImportCoreReport coreReportData, List<DiscoveryData> discoveredData, List<vCenterHostDiscovery> vCenterHostData, Logger.LogHandler logger)
        {
            AvsData filteredData = FilterData(coreReportData, discoveredData, vCenterHostData, logger);
            List<MigrationDataProperty> migrationDataProperties = UtilityFunctions.FetchProperties(typeof(AvsData), filteredData);
            return migrationDataProperties;
        }

        public static AvsData FilterData(ImportCoreReport coreReportData, List<DiscoveryData> discoveredData, List<vCenterHostDiscovery> vCenterHostData, Logger.LogHandler logger)
        {
            logger.LogInformation("Retrieving data for Azure VMware Solution");

            int totalvCenters = vCenterHostData.FirstOrDefault()?.vCenters ?? 0;
            int totalHosts = vCenterHostData.FirstOrDefault()?.Hosts ?? 0;

            double totalStorageCost = coreReportData.BusinessCaseObj.AzureAvsCost.StorageCost;
            double totalNetworkCost = coreReportData.BusinessCaseObj.AzureAvsCost.NetworkCost;
            double totalITStaffCost = coreReportData.BusinessCaseObj.AzureAvsCost.ITStaffCost;
            double totalFacilitiesCost = coreReportData.BusinessCaseObj.AzureAvsCost.FacilitiesCost;
            double computeAndLicenseCost = coreReportData.BusinessCaseObj.AzureAvsCost.ComputeLicenseCost;
            double estimatedTotalCost = totalStorageCost + totalNetworkCost + totalITStaffCost + totalFacilitiesCost + computeAndLicenseCost;

            double estimatedOnPremAvsCost = coreReportData.BusinessCaseObj.OnPremisesAvsCost.ComputeLicenseCost + coreReportData.BusinessCaseObj.OnPremisesAvsCost.EsuLicenseCost + coreReportData.BusinessCaseObj.OnPremisesAvsCost.StorageCost + coreReportData.BusinessCaseObj.OnPremisesAvsCost.NetworkCost + coreReportData.BusinessCaseObj.OnPremisesAvsCost.ITStaffCost + coreReportData.BusinessCaseObj.OnPremisesAvsCost.FacilitiesCost;

            double estimatedTotalSavings = estimatedOnPremAvsCost - estimatedTotalCost;
            double tco = estimatedTotalCost;

            double price = estimatedTotalSavings / estimatedOnPremAvsCost;
            string savingsPercentage = price <= 0 ? "" : price.ToString("0.00%");

            double windowsServerLicensing = coreReportData.BusinessCaseObj.WindowsServerLicense.ComputeLicenseCost;

            double sqlServerLicensing = coreReportData.BusinessCaseObj.SqlServerLicense.ComputeLicenseCost;


            double esuLicensing = coreReportData.BusinessCaseObj.EsuSavings.ComputeLicenseCost;

            double totalAhubSavings = windowsServerLicensing + sqlServerLicensing;

            double averageCpuUtilizationP95 = 0.0;
            double averageRamUtilizationP95 = 0.0;
            string externalStorage = string.Empty;
            string recommendedNodes = string.Empty;
            double filteredRamInTb = 0.0;

            foreach (var summary in coreReportData.AvsSummaryList)
            {
                if (summary.SizingCriterion == "Pay-as-you-go + RI3Year")
                {
                    averageCpuUtilizationP95 = summary.PredictedCpuUtilizationPercentage;
                    averageRamUtilizationP95 = summary.PredictedMemoryUtilizationPercentage;
                    externalStorage = summary.RecommendedExternalStorage;
                    recommendedNodes = summary.RecommendedNodes;
                    filteredRamInTb = summary.MemoryInTBAvailable;
                }
            }

            double storageInUseGb = discoveredData.Sum(x => x.StorageInUseGB) ?? 0;

            AvsData avsData = new AvsData
            {
                TotalvCenters = totalvCenters,
                TotalHosts = totalHosts,
                AverageAvsCpuUtilizationP95 = averageCpuUtilizationP95,
                AverageAvsRamUtilizationP95 = averageRamUtilizationP95,
                TotalAvsComputeLicenseCost = computeAndLicenseCost,
                TotalAvsStorageCost = totalStorageCost,
                TotalAvsNetworkCost = totalNetworkCost,
                TotalAvsITStaffCost = totalITStaffCost,
                TotalAvsFacilitiesCost = totalFacilitiesCost,
                EstimatedTotalAvsCost = estimatedTotalCost,
                EstimatedTotalSavings = estimatedTotalSavings,
                AvsTCO = tco,
                AvsSavingsPercentage = savingsPercentage,
                AvsExternalStorage = externalStorage,
                RecommendedNodes = recommendedNodes,
                TotalAvsNodeRamInTb = filteredRamInTb,
                TotalWindowsServerLicensing = windowsServerLicensing,
                TotalSqlServerLicensing = sqlServerLicensing,
                TotalEsuSavings = esuLicensing,
                TotalAhubSavingsforAvs = totalAhubSavings,
                VCpuOverSubscription = coreReportData.CorePropertiesObj.VCpuOverSubscription,
                MemoryOverCommit = coreReportData.CorePropertiesObj.MemoryOverCommit,
                DedupeCompression = coreReportData.CorePropertiesObj.DedupeCompression,
                StorageInUseGB = storageInUseGb
            };

            Debug.WriteLine(avsData);
            logger.LogInformation("Fetched data for Azure VMware Solution");

            return avsData;
        }
    }
}

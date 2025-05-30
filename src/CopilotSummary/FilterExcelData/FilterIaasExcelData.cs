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

            foreach (Financial_Summary item in coreReportData.FinancialSummaryList)
            {
                if (item.MigrationStrategy == "Migrate/Rehost to Azure (IaaS)")
                {
                    if (item.Workload == "ASP.NET WebApps on IIS - Dev/Test")
                    {
                        workloadAspNetWebappsDev = item.SourceCount;
                        azAspNetWebappsIISDev = item.TargetCount;
                    }
                    else if (item.Workload == "ASP.NET WebApps on IIS - Prod")
                    {
                        workloadAspNetWebappsProd = item.SourceCount;
                        azAspNetWebappsIISProd = item.TargetCount;
                    }
                    else if (item.Workload == "Servers - Dev/Test")
                    {
                        workloadServerDev = item.SourceCount;
                        azServerDev = item.TargetCount;
                    }
                    else if (item.Workload == "Servers - Prod")
                    {
                        workloadServerProd = item.SourceCount;
                        azServerProd = item.TargetCount;
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
            List<string> iaasTransformationSummary = new List<string>
            {
                $"{workloadAspNetWebappsDev} IIS webservers hosting ASP.NET webapps in Dev/Test suggested for migration to {azAspNetWebappsIISDev} Azure VMs(IaaS).",
                $"{workloadAspNetWebappsProd} IIS webservers hosting ASP.NET webapps in Production suggested for migration to {azAspNetWebappsIISProd} Azure VMs(IaaS).",
                $"{workloadServerDev} Servers in Dev/Test suggested for migration to {azServerDev} Azure VMs(IaaS)",
                $"{workloadServerProd} Servers in Production suggested for migration to {azServerProd} Azure VMs(IaaS)",
                $"{workloadSqlServerDatabaseDev} SQL Server Database Engines in Dev/Test suggested for {azSqlServerDatabaseDev} SQL Servers on Azure VM(IaaS",
                $"{workloadSqlServerDatabaseProd} SQL Server Database Engines in Production suggested for {azSqlServerDatabaseProd} SQL Servers on Azure VM(IaaS)",
            };

            totalMachinesCount = coreReportData.VMIaasServerRehostPerfList.Count() + coreReportData.SqlIaasServerRehostPerfList.Count()
                                + coreReportData.WebappIaasServerRehostPerfList.Count() + coreReportData.SqlIaasInstanceRehostPerfList.Count();

            foreach (VM_IaaS_Server_Rehost_Perf item in coreReportData.VMIaasServerRehostPerfList)
            {
                if (item.AzureVMReadiness == "Ready") readyMachinesCount++;

                totalCores += item.Cores;
                totalMemoryinGB += item.MemoryInMB / 1024;
                totalStorageinGB += item.StorageInGB;

                if (item.Environment == "Prod")
                {
                    azureVmCost += item.MonthlyComputeCostEstimate_AHUB_RI3year * 12;
                    azureVMCostProdAhub += azureVmCost;
                }
                else
                {
                    azureVmCost += item.MonthlyComputeCostEstimate_AHUB * 12;
                    azureVMCostDevAhub += item.MonthlyComputeCostEstimate_AHUB_RI3year * 12;
                }

                annualComputeCostEstimate += item.MonthlyComputeCostEstimate * 12;
                annualComputeCostEstimateRI3year += item.MonthlyComputeCostEstimate_RI3year * 12;
                annualComputeCostEstimateAsp += item.MonthlyComputeCostEstimate_ASP3year * 12;
                annualComputeCostEstimateAhub += item.MonthlyComputeCostEstimate_AHUB * 12;
                annualComputeCostEstimateAhubRI3year += item.MonthlyComputeCostEstimate_AHUB_RI3year * 12;
                annualStorageCostEstimate += item.MonthlyStorageCostEstimate * 12;

                if (item.RecommendedVMSize != "")
                {
                    if (item.Environment == "Prod")
                    {
                        if (!vmTopRecommendationToCostProd.ContainsKey(item.RecommendedVMSize))
                        {
                            vmTopRecommendationToCostProd.Add(item.RecommendedVMSize, 0.00);
                        }
                        vmTopRecommendationToCostProd[item.RecommendedVMSize] += item.MonthlyComputeCostEstimate_AHUB_RI3year * 12;
                    }
                    else
                    {
                        if (!vmTopRecommendationToCostDev.ContainsKey(item.RecommendedVMSize))
                        {
                            vmTopRecommendationToCostDev.Add(item.RecommendedVMSize, 0.00);
                        }
                        vmTopRecommendationToCostDev[item.RecommendedVMSize] += item.MonthlyComputeCostEstimate_AHUB_RI3year * 12;
                    }

                    if (!vmSkuRecommendedToCount.ContainsKey(item.RecommendedVMSize))
                    {
                        vmSkuRecommendedToCount.Add(item.RecommendedVMSize, 0);
                    }
                    vmSkuRecommendedToCount[item.RecommendedVMSize]++;
                }

                if (item.AzureVMReadiness_Warnings != "" && item.AzureVMReadiness_Warnings != "NotApplicable")
                {
                    List<string> itemWarnings = new List<string>(item.AzureVMReadiness_Warnings.Split(';'));
                    foreach (string warning in itemWarnings)
                    {
                        if (warning != "")
                        {
                            if (!vmWarningsToCount.ContainsKey(warning))
                            {
                                vmWarningsToCount.Add(warning, 0);
                            }
                            vmWarningsToCount[warning]++;
                        }
                    }             
                }

                if (item.OperatingSystem.Contains("Windows"))
                {
                    windowsComputeLicenseCost += item.MonthlyComputeCostEstimate_RI3year * 12;
                    windowsComputeLicenseCostAhub += item.MonthlyComputeCostEstimate_AHUB_RI3year * 12;
                }

                standardHddDrivesCount += item.StandardHddDisks;
                standardHddDrivesCost += item.MonthlyStorageCostForStandardHddDisks * 12;
                standardSsdDrivesCount += item.StandardSsdDisks;
                standardSsdDrivesCost += item.MonthlyStorageCostForStandardSsdDisks * 12;
                premiumDisksCount += item.PremiumDisks;
                premiumDiskCost += item.MonthlyStorageCostForPremiumDisks * 12;
                ultraDrivesCount += item.UltraDisks;
                ultraDisksCost += item.MonthlyStorageCostForUltraDisks * 12;
            }

            foreach (SQL_IaaS_Server_Rehost_Perf item in coreReportData.SqlIaasServerRehostPerfList)
            {
                if (item.AzureVMReadiness == "Ready") readyMachinesCount++;

                totalCores += item.Cores;
                totalMemoryinGB += item.MemoryInMB / 1024;
                totalStorageinGB += item.StorageInGB;
                sqlServerAzureVmCost += item.MonthlyComputeCostEstimate_AHUB_RI3year * 12;

                if (item.Environment == "Prod")
                {
                    azureVmCost += item.MonthlyComputeCostEstimate_AHUB_RI3year * 12;
                    azureVMCostProdAhub += item.MonthlyComputeCostEstimate_AHUB_RI3year * 12;
                }
                else
                {
                    azureVmCost += item.MonthlyComputeCostEstimate_AHUB * 12;
                    azureVMCostDevAhub += item.MonthlyComputeCostEstimate_AHUB_RI3year * 12;
                }

                annualComputeCostEstimate += item.MonthlyComputeCostEstimate * 12;
                annualComputeCostEstimateRI3year += item.MonthlyComputeCostEstimate_RI3year * 12;
                annualComputeCostEstimateAsp += item.MonthlyComputeCostEstimate_ASP3year * 12;
                annualComputeCostEstimateAhub += item.MonthlyComputeCostEstimate_AHUB * 12;
                annualComputeCostEstimateAhubRI3year += item.MonthlyComputeCostEstimate_AHUB_RI3year * 12;
                annualStorageCostEstimate += item.MonthlyStorageCostEstimate * 12;

                if (item.RecommendedVMSize != "")
                {
                    if (item.Environment == "Prod")
                    {
                        if (!vmTopRecommendationToCostProd.ContainsKey(item.RecommendedVMSize))
                        {
                            vmTopRecommendationToCostProd.Add(item.RecommendedVMSize, 0.00);
                        }
                        vmTopRecommendationToCostProd[item.RecommendedVMSize] += item.MonthlyComputeCostEstimate_AHUB_RI3year * 12;
                    }
                    else
                    {
                        if (!vmTopRecommendationToCostDev.ContainsKey(item.RecommendedVMSize))
                        {
                            vmTopRecommendationToCostDev.Add(item.RecommendedVMSize, 0.00);
                        }
                        vmTopRecommendationToCostDev[item.RecommendedVMSize] += item.MonthlyComputeCostEstimate_AHUB_RI3year * 12;
                    }

                    if (!vmSkuRecommendedToCount.ContainsKey(item.RecommendedVMSize))
                    {
                        vmSkuRecommendedToCount.Add(item.RecommendedVMSize, 0);
                    }
                    vmSkuRecommendedToCount[item.RecommendedVMSize]++;
                }

                if (item.AzureVMReadiness_Warnings != "" && item.AzureVMReadiness_Warnings != "NotApplicable")
                {
                    List<string> itemWarnings = new List<string>(item.AzureVMReadiness_Warnings.Split(';'));
                    foreach (string warning in itemWarnings)
                    {
                        if (warning != "")
                        {
                            if (!vmWarningsToCount.ContainsKey(warning))
                            {
                                vmWarningsToCount.Add(warning, 0);
                            }
                            vmWarningsToCount[warning]++;
                        }
                    }                    
                }

                sqlComputeLicenseCost += item.MonthlyComputeCostEstimate_RI3year * 12;
                sqlComputeLicenseCostAhub += item.MonthlyComputeCostEstimate_AHUB_RI3year * 12;
                standardHddDrivesCount += item.StandardHddDisks;
                standardHddDrivesCost += item.MonthlyStorageCostForStandardHddDisks * 12;
                standardSsdDrivesCount += item.StandardSsdDisks;
                standardSsdDrivesCost += item.MonthlyStorageCostForStandardSsdDisks * 12;
                premiumDisksCount += item.PremiumDisks;
                premiumDiskCost += item.MonthlyStorageCostForPremiumDisks * 12;
                ultraDrivesCount += item.UltraDisks;
                ultraDisksCost += item.MonthlyStorageCostForUltraDisks * 12;
            }

            foreach (WebApp_IaaS_Server_Rehost_Perf item in coreReportData.WebappIaasServerRehostPerfList)
            {
                if (item.AzureVMReadiness == "Ready") readyMachinesCount++;

                totalCores += item.Cores;
                totalMemoryinGB += item.MemoryInMB / 1024;
                totalStorageinGB += item.StorageInGB;
                aspNetAzureVmCost += item.MonthlyComputeCostEstimate_AHUB_RI3year * 12;

                if (item.Environment == "Prod")
                {
                    azureVmCost += item.MonthlyComputeCostEstimate_AHUB_RI3year * 12;
                    azureVMCostProdAhub += item.MonthlyComputeCostEstimate_AHUB_RI3year * 12;
                }
                else
                {
                    azureVmCost += item.MonthlyComputeCostEstimate_AHUB * 12;
                    azureVMCostDevAhub += item.MonthlyComputeCostEstimate_AHUB_RI3year * 12;
                }

                annualComputeCostEstimate += item.MonthlyComputeCostEstimate * 12;
                annualComputeCostEstimateRI3year += item.MonthlyComputeCostEstimate_RI3year * 12;
                annualComputeCostEstimateAsp += item.MonthlyComputeCostEstimate_ASP3year * 12;
                annualComputeCostEstimateAhub += item.MonthlyComputeCostEstimate_AHUB * 12;
                annualComputeCostEstimateAhubRI3year += item.MonthlyComputeCostEstimate_AHUB_RI3year * 12;
                annualStorageCostEstimate += item.MonthlyStorageCostEstimate * 12;

                if (item.RecommendedVMSize != "")
                {
                    if (item.Environment == "Prod")
                    {
                        if (!vmTopRecommendationToCostProd.ContainsKey(item.RecommendedVMSize))
                        {
                            vmTopRecommendationToCostProd.Add(item.RecommendedVMSize, 0.00);
                        }
                        vmTopRecommendationToCostProd[item.RecommendedVMSize] += item.MonthlyComputeCostEstimate_AHUB_RI3year * 12;
                    }
                    else
                    {
                        if (!vmTopRecommendationToCostDev.ContainsKey(item.RecommendedVMSize))
                        {
                            vmTopRecommendationToCostDev.Add(item.RecommendedVMSize, 0.00);
                        }
                        vmTopRecommendationToCostDev[item.RecommendedVMSize] += item.MonthlyComputeCostEstimate_AHUB_RI3year * 12;
                    }

                    if (!vmSkuRecommendedToCount.ContainsKey(item.RecommendedVMSize))
                    {
                        vmSkuRecommendedToCount.Add(item.RecommendedVMSize, 0);
                    }
                    vmSkuRecommendedToCount[item.RecommendedVMSize]++;
                }

                if (item.AzureVMReadiness_Warnings != "" && item.AzureVMReadiness_Warnings != "NotApplicable")
                {
                    List<string> itemWarnings = new List<string>(item.AzureVMReadiness_Warnings.Split(';'));
                    foreach (string warning in itemWarnings)
                    {
                        if (warning != "")
                        {
                            if (!vmWarningsToCount.ContainsKey(warning))
                            {
                                vmWarningsToCount.Add(warning, 0);
                            }
                            vmWarningsToCount[warning]++;
                        }
                    }
                }

                standardHddDrivesCount += item.StandardHddDisks;
                standardHddDrivesCost += item.MonthlyStorageCostForStandardHddDisks * 12;
                standardSsdDrivesCount += item.StandardSsdDisks;
                standardSsdDrivesCost += item.MonthlyStorageCostForStandardSsdDisks * 12;
                premiumDisksCount += item.PremiumDisks;
                premiumDiskCost += item.MonthlyStorageCostForPremiumDisks * 12;
                ultraDrivesCount += item.UltraDisks;
                ultraDisksCost += item.MonthlyStorageCostForUltraDisks * 12;
            }

            foreach (SQL_IaaS_Instance_Rehost_Perf item in coreReportData.SqlIaasInstanceRehostPerfList)
            {
                if (item.SQLServerOnAzureVMReadiness == "Ready") readyMachinesCount++;

                totalCores += item.VCoresAllocated;
                totalMemoryinGB += item.MemoryInUseInMB / 1024;
                totalStorageinGB += item.TotalDBSizeInMB / 1024;
                sqlServerAzureVmCost += item.MonthlyComputeCostEstimate_AHUB_RI3year * 12;

                if (item.Environment == "Prod")
                {
                    azureVmCost += item.MonthlyComputeCostEstimate_AHUB_RI3year * 12;
                    azureVMCostProdAhub += azureVmCost;
                }
                else
                {
                    azureVmCost += item.MonthlyComputeCostEstimate_AHUB * 12;
                    azureVMCostDevAhub += item.MonthlyComputeCostEstimate_AHUB_RI3year * 12;
                }

                annualComputeCostEstimate += item.MonthlyComputeCostEstimate * 12;
                annualComputeCostEstimateRI3year += item.MonthlyComputeCostEstimate_RI3year * 12;
                annualComputeCostEstimateAsp += item.MonthlyComputeCostEstimate_ASP3year * 12;
                annualComputeCostEstimateAhub += item.MonthlyComputeCostEstimate_AHUB * 12;
                annualComputeCostEstimateAhubRI3year += item.MonthlyComputeCostEstimate_AHUB_RI3year * 12;
                annualStorageCostEstimate += item.MonthlyStorageCostEstimate * 12;

                if (item.SQLServerOnAzureVMConfiguration != "")
                {
                    if (item.Environment == "Prod")
                    {
                        if (!vmTopRecommendationToCostProd.ContainsKey(item.SQLServerOnAzureVMConfiguration))
                        {
                            vmTopRecommendationToCostProd.Add(item.SQLServerOnAzureVMConfiguration, 0.00);
                        }
                        vmTopRecommendationToCostProd[item.SQLServerOnAzureVMConfiguration] += item.MonthlyComputeCostEstimate_AHUB_RI3year * 12;
                    }
                    else
                    {
                        if (!vmTopRecommendationToCostDev.ContainsKey(item.SQLServerOnAzureVMConfiguration))
                        {
                            vmTopRecommendationToCostDev.Add(item.SQLServerOnAzureVMConfiguration, 0.00);
                        }
                        vmTopRecommendationToCostDev[item.SQLServerOnAzureVMConfiguration] += item.MonthlyComputeCostEstimate_AHUB_RI3year * 12;
                    }

                    if (!vmSkuRecommendedToCount.ContainsKey(item.SQLServerOnAzureVMConfiguration))
                    {
                        vmSkuRecommendedToCount.Add(item.SQLServerOnAzureVMConfiguration, 0);
                    }
                    vmSkuRecommendedToCount[item.SQLServerOnAzureVMConfiguration]++;
                }

                if (item.SQLServerOnAzureVMReadiness_Warnings != "" && item.SQLServerOnAzureVMReadiness_Warnings != "NotApplicable")
                {
                    List<string> itemWarnings = new List<string>(item.SQLServerOnAzureVMReadiness_Warnings.Split(';'));
                    foreach (string warning in itemWarnings)
                    {
                        if (warning != "")
                        {
                            if (!vmWarningsToCount.ContainsKey(warning))
                            {
                                vmWarningsToCount.Add(warning, 0);
                            }
                            vmWarningsToCount[warning]++;
                        }
                    }
                }

                sqlComputeLicenseCost += item.MonthlyComputeCostEstimate_RI3year * 12;
                sqlComputeLicenseCostAhub += item.MonthlyComputeCostEstimate_AHUB_RI3year * 12;
                standardHddDrivesCount += item.StandardHddDisks;
                standardHddDrivesCost += item.MonthlyStorageCostForStandardHddDisks * 12;
                standardSsdDrivesCount += item.StandardSsdDisks;
                standardSsdDrivesCost += item.MonthlyStorageCostForStandardSsdDisks * 12;
                premiumDisksCount += item.PremiumDisks;
                premiumDiskCost += item.MonthlyStorageCostForPremiumDisks * 12;
                ultraDrivesCount += item.UltraDisks;
                ultraDisksCost += item.MonthlyStorageCostForUltraDisks * 12;
            }

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
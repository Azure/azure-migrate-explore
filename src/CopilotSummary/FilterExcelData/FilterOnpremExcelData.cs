// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using MathNet.Numerics.Statistics;
using System.Collections.Generic;
using System.Linq;

using Azure.Migrate.Explore.Excel;
using Azure.Migrate.Explore.Common;
using Azure.Migrate.Explore.Models;
using Azure.Migrate.Explore.Models.CopilotSummary.MigrationSummary;

namespace Azure.Migrate.Explore.CopilotSummary.FilterExcelData
{
    public static class FilterOnpremExcelData
    {
        public static List<MigrationDataProperty> FilterOnpremData(List<DiscoveryData> discoveredData, ImportCoreReport coreReportData, Logger.LogHandler logger)
        {
            MigrationOnpremData filteredData = FilterData(discoveredData, coreReportData, logger);
            List<MigrationDataProperty> migrationDataProperties = UtilityFunctions.FetchProperties(typeof(MigrationOnpremData), filteredData);
            return migrationDataProperties;
        }

        public static MigrationOnpremData FilterData(List<DiscoveryData> discoveredData, ImportCoreReport coreReportData, Logger.LogHandler logger)
        {
            logger.LogInformation("Retrieving On-premises data");

            int windowsServerCount = 0;
            int linuxServerCount = 0;
            int unknownServerCount = 0; 
            int prodServerCount = 0;
            int devServerCount = 0;
            int webappServers = 0;
            int otherWorkloads = 0;
            int sqlServers = 0;
            int sqlServersInstances = 0;
            int sqlServerDatabases = 0; // SQL_ALL_SERVERS
            int webAppCount = 0;
            double totalStorageinTB = 0; // SQL_ALL_SERVERS
            double averageCpuUtilizationP95 = 0; // All_VM_Assessment
            double averageRamUtilizationP95 = 0; // All_VM_Assessment
            double estimatedTotalOnpremCost = 0.00;
            double estimatedTotalAzureCost = 0.00;
            double estimatedTotalAzurePaasCost = 0.00;
            double estimatedTotalAzureIaasCost = 0.00;
            double estimatedTotalSavings = 0.00;
            double onpremComputeLicenseCost = 0.00;
            double onpremEsuLicenseCost = 0.00;
            double onpremStorageCost = 0.00;
            double onpremNetworkCost = 0.00;
            double onpremSecurityCost = 0.00;
            double onpremITStaffCost = 0.00;
            double onpremFacilitiesCost = 0.00;
            double totalAzureComputeLicenseCost = 0.00;
            double totalAzureEsuLicenseCost = 0.00;
            double totalAzureStorageCost = 0.00;
            double totalAzureNetworkCost = 0.00;
            double totalAzureSecurityCost = 0.00;
            double totalAzureITStaffCost = 0.00;
            double totalAzureFacilitiesCost = 0.00;
            double computeLicenseSavings = 0.00;
            double esuLicenseSavings = 0.00;
            double storageCostSavings = 0.00;
            double networkCostSavings = 0.00;
            double securityCostSavings = 0.00;
            double itStaffCostSavings = 0.00;
            double facilitiesCostSavings = 0.00;
            double windowsServerLicenseSavingsAhub = 0.00; //All_IaaS_Machines (Perf)
            double sqlServerLicenseSavingsAhub = 0.00; //All_IaaS_Machines (Perf)

            for (int i = 0; i < discoveredData.Count(); i++)
            {
                if (discoveredData[i].EnvironmentType == "Prod")
                    prodServerCount++;
                else if (discoveredData[i].EnvironmentType == "Dev")
                    devServerCount++;

                if (string.IsNullOrWhiteSpace(discoveredData[i].OperatingSystem))
                    unknownServerCount++;
                else
                {
                    if (discoveredData[i].OperatingSystem.Contains("Windows"))
                        windowsServerCount++;
                    else
                        linuxServerCount++;
                }

                if (discoveredData[i].WebAppCount > 0)
                {
                    webappServers++;
                    webAppCount += discoveredData[i].WebAppCount;
                }
                else
                {
                    if (!(discoveredData[i].SqlDiscoveryServerCount > 0 || discoveredData[i].IsSqlServicePresent) && discoveredData[i].OperatingSystem.Contains("Windows"))
                    {
                        otherWorkloads++;
                    }
                }

                if (discoveredData[i].SqlDiscoveryServerCount > 0)
                {
                    sqlServers++;
                    sqlServersInstances += discoveredData[i].SqlDiscoveryServerCount;
                }
            }

            double totalStorageinMB = 0.00;
            foreach (SQL_All_Instances item in coreReportData.SqlAllInstancesList)
            {
                sqlServerDatabases += item.UserDatabases;
                totalStorageinMB += item.TotalDBSizeInMB;
            }
            totalStorageinTB = (totalStorageinMB / 1024.0) / 1024.0;

            List<double> cpuUtilization = new List<double>();
            List<double> memoryUtilization = new List<double>();
            foreach (All_VM_IaaS_Server_Rehost_Perf item in coreReportData.AllVMIaasServerRehostPerfList)
            {
                cpuUtilization.Add(item.CpuUtilizationPercentage);
                memoryUtilization.Add(item.MemoryUtilizationPercentage);
            }
            averageCpuUtilizationP95 = Statistics.Percentile(cpuUtilization, 95);
            averageRamUtilizationP95 = Statistics.Percentile(memoryUtilization, 95);

            Business_Case business_CaseObj = coreReportData.BusinessCaseObj;

            onpremComputeLicenseCost = business_CaseObj.TotalOnPremisesCost.ComputeLicenseCost;
            onpremEsuLicenseCost = business_CaseObj.TotalOnPremisesCost.EsuLicenseCost;
            onpremStorageCost = business_CaseObj.TotalOnPremisesCost.StorageCost;
            onpremNetworkCost = business_CaseObj.TotalOnPremisesCost.NetworkCost;
            onpremSecurityCost = business_CaseObj.TotalOnPremisesCost.SecurityCost;
            onpremITStaffCost = business_CaseObj.TotalOnPremisesCost.ITStaffCost;
            onpremFacilitiesCost = business_CaseObj.TotalOnPremisesCost.FacilitiesCost;
            estimatedTotalOnpremCost = onpremComputeLicenseCost + onpremEsuLicenseCost + onpremStorageCost +
                                       onpremNetworkCost + onpremSecurityCost + onpremITStaffCost +
                                       onpremFacilitiesCost;

            double azureIaasComputeLicenseCost = business_CaseObj.AzureIaaSCost.ComputeLicenseCost;
            double azureIaasEsuLicenseCost = business_CaseObj.AzureIaaSCost.EsuLicenseCost;
            double azureIaasStorageCost = business_CaseObj.AzureIaaSCost.StorageCost;
            double azureIaasNetworkCost = business_CaseObj.AzureIaaSCost.NetworkCost;
            double azureIaasSecurityCost = business_CaseObj.AzureIaaSCost.SecurityCost;
            double azureIaasITStaffCost = business_CaseObj.AzureIaaSCost.ITStaffCost;
            double azureIaasFacilitiesCost = business_CaseObj.AzureIaaSCost.FacilitiesCost;
            estimatedTotalAzureIaasCost = azureIaasComputeLicenseCost + azureIaasEsuLicenseCost + azureIaasStorageCost +
                                          azureIaasNetworkCost + azureIaasSecurityCost + azureIaasITStaffCost +
                                          azureIaasFacilitiesCost;

            double azurePaasComputeLicenseCost = business_CaseObj.AzurePaaSCost.ComputeLicenseCost;
            double azurePaasEsuLicenseCost = business_CaseObj.AzurePaaSCost.EsuLicenseCost;
            double azurePaasStorageCost = business_CaseObj.AzurePaaSCost.StorageCost;
            double azurePaasNetworkCost = business_CaseObj.AzurePaaSCost.NetworkCost;
            double azurePaasSecurityCost = business_CaseObj.AzurePaaSCost.SecurityCost;
            double azurePaasITStaffCost = business_CaseObj.AzurePaaSCost.ITStaffCost;
            double azurePaasFacilitiesCost = business_CaseObj.AzurePaaSCost.FacilitiesCost;
            estimatedTotalAzurePaasCost = azurePaasComputeLicenseCost + azurePaasEsuLicenseCost + azurePaasStorageCost +
                                          azurePaasNetworkCost + azurePaasSecurityCost + azurePaasITStaffCost +
                                          azurePaasFacilitiesCost;

            totalAzureComputeLicenseCost = azureIaasComputeLicenseCost + azurePaasComputeLicenseCost;
            totalAzureEsuLicenseCost = azureIaasEsuLicenseCost + azurePaasEsuLicenseCost;
            totalAzureStorageCost = azureIaasStorageCost + azurePaasStorageCost;
            totalAzureNetworkCost = azureIaasNetworkCost + azurePaasNetworkCost;
            totalAzureSecurityCost = azureIaasSecurityCost + azurePaasSecurityCost;
            totalAzureITStaffCost = azureIaasITStaffCost + azurePaasITStaffCost;
            totalAzureFacilitiesCost = azureIaasFacilitiesCost + azurePaasFacilitiesCost;
            estimatedTotalAzureCost = totalAzureComputeLicenseCost + totalAzureEsuLicenseCost + totalAzureStorageCost +
                                      totalAzureNetworkCost + totalAzureSecurityCost + totalAzureITStaffCost +
                                      totalAzureFacilitiesCost;

            computeLicenseSavings = onpremComputeLicenseCost - totalAzureComputeLicenseCost;
            esuLicenseSavings = onpremEsuLicenseCost - totalAzureEsuLicenseCost;
            storageCostSavings = onpremStorageCost - totalAzureStorageCost;
            networkCostSavings = onpremNetworkCost - totalAzureNetworkCost;
            securityCostSavings = onpremSecurityCost - totalAzureSecurityCost;
            itStaffCostSavings = onpremITStaffCost - totalAzureITStaffCost;
            facilitiesCostSavings = onpremFacilitiesCost - totalAzureFacilitiesCost;
            estimatedTotalSavings = estimatedTotalOnpremCost - estimatedTotalAzureCost;

            double computeCostPAYGAHBRI = 0.00;
            double computeCostPAYGRI = 0.00;
            foreach (VM_IaaS_Server_Rehost_Perf item in coreReportData.VMIaasServerRehostPerfList)
            {
                if (item.OperatingSystem.Contains("Windows"))
                {
                    computeCostPAYGAHBRI += item.MonthlyComputeCostEstimate_AHUB_RI3year;
                    computeCostPAYGRI += item.MonthlyComputeCostEstimate_RI3year;
                }
            }

            foreach (WebApp_IaaS_Server_Rehost_Perf item in coreReportData.WebappIaasServerRehostPerfList)
            {
                computeCostPAYGAHBRI += item.MonthlyComputeCostEstimate_AHUB_RI3year;
                computeCostPAYGRI += item.MonthlyComputeCostEstimate_RI3year;
            }

            windowsServerLicenseSavingsAhub = (computeCostPAYGRI - computeCostPAYGAHBRI) * 12;

            double computeCostsqlMIPaasPAYGAHBRI = 0.00;
            double computeCostsqlMIPaasPAYGRI = 0.00;
            foreach (SQL_MI_PaaS item in coreReportData.SqlMIPaasList)
            {
                computeCostsqlMIPaasPAYGAHBRI += item.MonthlyComputeCostEstimate_AHUB_RI3year;
                computeCostsqlMIPaasPAYGRI += item.MonthlyComputeCostEstimate_RI3year;
            }

            double sqlAhubSavingsMIPaas = (computeCostsqlMIPaasPAYGRI - computeCostsqlMIPaasPAYGAHBRI) * 12;

            double computeCostsqlIaasPaygAhubRI = 0.00;
            double computeCostsqlIaasPaygRI = 0.00;

            foreach (SQL_IaaS_Server_Rehost_Perf item in coreReportData.SqlIaasServerRehostPerfList)
            {
                computeCostsqlIaasPaygAhubRI += item.MonthlyComputeCostEstimate_AHUB_RI3year;
                computeCostsqlIaasPaygRI += item.MonthlyComputeCostEstimate_RI3year;
            }

            foreach (SQL_IaaS_Instance_Rehost_Perf item in coreReportData.SqlIaasInstanceRehostPerfList)
            {
                computeCostsqlIaasPaygAhubRI += item.MonthlyComputeCostEstimate_AHUB_RI3year;
                computeCostsqlIaasPaygRI += item.MonthlyComputeCostEstimate_RI3year;
            }

            double sqlAhubSavingsIaas = (computeCostsqlIaasPaygRI - computeCostsqlIaasPaygAhubRI) * 12;

            sqlServerLicenseSavingsAhub = sqlAhubSavingsMIPaas + sqlAhubSavingsIaas;

            MigrationOnpremData migrationData = new MigrationOnpremData
            {
                AzureRegion = coreReportData.CorePropertiesObj.TargetRegion,
                CurrencySymbol = coreReportData.CorePropertiesObj.Currency,
                TotalServers = discoveredData.Count(),
                WindowsServers = windowsServerCount,
                LinuxServers = linuxServerCount,
                UnknownServers = unknownServerCount,
                ProductionServers = prodServerCount,
                DevServers = devServerCount,
                WebappServers = webappServers,
                OtherWorkloads = otherWorkloads,
                SqlServers = sqlServers,
                SqlServerInstances = sqlServersInstances,
                SqlServerDatabases = sqlServerDatabases,
                WebappCount = webAppCount,
                TotalStorageinTB = totalStorageinTB,
                AverageCpuUtilizationP95 = averageCpuUtilizationP95,
                AverageRamUtilizationP95 = averageRamUtilizationP95,
                EstimatedTotalOnpremCost = estimatedTotalOnpremCost,
                EstimatedTotalAzureCost = estimatedTotalAzureCost,
                EstimatedTotalAzurePaasCost = estimatedTotalAzurePaasCost,
                EstimatedTotalAzureIaasCost = estimatedTotalAzureIaasCost,
                EstimatedTotalSavings = estimatedTotalSavings,
                OnpremComputeLicenseCost = onpremComputeLicenseCost,
                OnpremEsuLicenseCost = onpremEsuLicenseCost,
                OnpremStorageCost = onpremStorageCost,
                OnpremNetworkCost = onpremNetworkCost,
                OnpremSecurityCost = onpremSecurityCost,
                OnpremITStaffCost = onpremITStaffCost,
                OnpremFacilitiesCost = onpremFacilitiesCost,
                TotalAzureComputeLicenseCost = totalAzureComputeLicenseCost,
                TotalAzureEsuLicenseCost = totalAzureEsuLicenseCost,
                TotalAzureStorageCost = totalAzureStorageCost,
                TotalAzureNetworkCost = totalAzureNetworkCost,
                TotalAzureSecurityCost = totalAzureSecurityCost,
                TotalAzureITStaffCost = totalAzureITStaffCost,
                TotalAzureFacilitiesCost = totalAzureFacilitiesCost,
                ComputeLicenseSavings = computeLicenseSavings,
                EsuLicenseSavings = esuLicenseSavings,
                StorageCostSavings = storageCostSavings,
                NetworkCostSavings = networkCostSavings,
                SecurityCostSavings = securityCostSavings,
                ITStaffCostSavings = itStaffCostSavings,
                FacilitiesCostSavings = facilitiesCostSavings,
                WindowsServerLicenseSavingsAhub = windowsServerLicenseSavingsAhub,
                SqlServerLicenseSavingsAhub = sqlServerLicenseSavingsAhub
            };

            logger.LogInformation("Fetched On-premises data");

            return migrationData;
        }
    }    
}
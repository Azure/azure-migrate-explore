// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using System;
using System.Collections.Generic;

using Azure.Migrate.Explore.Common;
using Azure.Migrate.Explore.Excel;
using Azure.Migrate.Explore.Models;
using AzureMigrateExplore.Excel;
using AzureMigrateExplore.Models;

namespace Azure.Migrate.Explore.Assessment.Processor
{
    public class ProcessDatasets
    {
        // Datasets
        private readonly Dictionary<AssessmentInformation, AVSAssessmentPropertiesDataset> AVSAssessmentsData;
        private readonly Dictionary<string, AVSAssessedMachinesDataset> AVSAssessedMachinesData;
        private readonly BusinessCaseDataset BusinessCaseData;
        private readonly Dictionary<string, string> DecommissionedMachinesData;
        private readonly List<InventoryInsights> InventoryInsightsData;
        private readonly List<SoftwareInsights> SoftwareInsightsData;
        private readonly List<SoftwareVulnerabilities> SoftwareVulnerabilitiesData;
        private readonly List<PendingUpdatesServerCounts> PendingUpdatesServerCountsData;

        private AzureAvsCostCalculator AzureAvsCalculator;

        private UserInput UserInputObj;

        public ProcessDatasets
            (
                Dictionary<AssessmentInformation, AVSAssessmentPropertiesDataset> avsAssessmentsData,
                Dictionary<string, AVSAssessedMachinesDataset> avsAssessedMachinesData,
                BusinessCaseDataset businessCaseData, 
                Dictionary<string, string> decommissionedMachinesData,
                List<InventoryInsights> inventoryInsightsData,
                List<SoftwareInsights> softwareInsightsData,
                List<SoftwareVulnerabilities> softwareVulnerabilitiesData,
                List<PendingUpdatesServerCounts> pendingUpdatesServerCountsData,

                UserInput userInputObj
            )
        {
            AVSAssessmentsData = avsAssessmentsData;
            AVSAssessedMachinesData = avsAssessedMachinesData;
            BusinessCaseData = businessCaseData;
            DecommissionedMachinesData = decommissionedMachinesData;
            InventoryInsightsData = inventoryInsightsData;
            SoftwareInsightsData = softwareInsightsData;
            SoftwareVulnerabilitiesData = softwareVulnerabilitiesData;
            PendingUpdatesServerCountsData = pendingUpdatesServerCountsData;
            AzureAvsCalculator = new AzureAvsCostCalculator();
            UserInputObj = userInputObj;
        }

        public void InititateProcessing()
        {
            if (UserInputObj == null)
                throw new Exception("Received null user input object for processing datasets.");

            UserInputObj.LoggerObj.LogInformation("Processing datasets to generate excel models");

            // Core report models
            CoreProperties corePropertiesObj = new CoreProperties();
            Business_Case Business_Case_Data = new Business_Case();
            Cash_Flows Cash_Flows_Data = new Cash_Flows();
            List<AVS_Summary> AVS_Summary_List = new List<AVS_Summary>();
            List<AVS_IaaS_Rehost_Perf> AVS_IaaS_Rehost_Perf_List = new List<AVS_IaaS_Rehost_Perf>();
            List<Decommissioned_Machines> Decommissioned_Machines_List = new List<Decommissioned_Machines>();
            List<YOY_Emissions> YOY_Emissions_List = new List<YOY_Emissions>();
            List<EmissionsDetails> EmissionsDetails_List = new List<EmissionsDetails>();

            // Opportunity report models
            List<SQL_MI_Issues_and_Warnings> SQL_MI_Issues_and_Warnings_List = new List<SQL_MI_Issues_and_Warnings>();
            List<SQL_MI_Opportunity> SQL_MI_Opportunity_List = new List<SQL_MI_Opportunity>();
            List<WebApp_Opportunity> WebApp_Opportunity_List = new List<WebApp_Opportunity>();
            List<VM_Opportunity_Perf> VM_Opportunity_Perf_List = new List<VM_Opportunity_Perf>();
            List<VM_Opportunity_AsOnPrem> VM_Opportunity_AsOnPrem_List = new List<VM_Opportunity_AsOnPrem>();

            // Clash report models
            List<Clash_Report> Clash_Report_List = new List<Clash_Report>();

            // Security and Software Insights report models
            List<InventoryInsights> Inventory_Insights_List = new List<InventoryInsights>();
            List<SoftwareInsights> Software_Insights_List = new List<SoftwareInsights>();
            List<SoftwareVulnerabilities> Software_Vulnerabilities_List = new List<SoftwareVulnerabilities>();
            List<PendingUpdatesServerCounts> PendingUpdates_ServerCounts_List = new List<PendingUpdatesServerCounts>();

            // Core report tabs
            CreateCorePropertiesModel(corePropertiesObj);
            Process_Business_Case_Model(Business_Case_Data);
            Process_Cash_Flows_Model(Cash_Flows_Data);
            Process_AVS_Summary_Model(AVS_Summary_List);
            Process_AVS_IaaS_Rehost_Perf_Model(AVS_IaaS_Rehost_Perf_List);
            Process_Decommissioned_Machines_Model(Decommissioned_Machines_List);
            Process_EmissionsDetails_Model(EmissionsDetails_List);
            Process_YOY_Emissions_Model(YOY_Emissions_List);

            // Security and Software Insights report tabs
            Process_Inventory_Insights_Model(Inventory_Insights_List);
            Process_Software_Insights_Model(Software_Insights_List);
            Process_Software_Vulnerabilities_Model(Software_Vulnerabilities_List);
            Process_PendingUpdates_ServerCounts_Model(PendingUpdates_ServerCounts_List);

            UserInputObj.LoggerObj.LogInformation(90 - UserInputObj.LoggerObj.GetCurrentProgress(), "Completed job for creating excel models");

            UserInputObj.LoggerObj.LogInformation("Generating core report excel sheet");
            ExportCoreReport exportCoreReportObj = new ExportCoreReport
                (
                    corePropertiesObj,
                    Business_Case_Data,
                    Cash_Flows_Data,
                    AVS_Summary_List,
                    AVS_IaaS_Rehost_Perf_List,
                    Decommissioned_Machines_List,
                    EmissionsDetails_List,
                    YOY_Emissions_List
                );
            exportCoreReportObj.GenerateCoreReportExcel();

            UserInputObj.LoggerObj.LogInformation("Generating security and software insights report excel sheet");
            ExportSecurityAndSoftwareInsightReport exportSecurityAndSoftwareInsightReportObj = new ExportSecurityAndSoftwareInsightReport
                (
                    Inventory_Insights_List,
                    Software_Insights_List,
                    Software_Vulnerabilities_List,
                    PendingUpdates_ServerCounts_List
                );
            exportSecurityAndSoftwareInsightReportObj.GenerateSecurityAndSoftwareInsightReportExcel();
        }

        private void CreateCorePropertiesModel(CoreProperties coreProperties)
        {
            UserInputObj.LoggerObj.LogInformation("Creating excel model for assessment properties");
            coreProperties.Workflow = UserInputObj.WorkflowObj.IsExpressWorkflow ? "Express" : "Custom - Assessment";
            coreProperties.BusinessProposal = UserInputObj.BusinessProposal;
            coreProperties.TenantId = UserInputObj.TenantId;
            coreProperties.Subscription = UserInputObj.Subscription.Value;
            coreProperties.ResourceGroupName = UserInputObj.ResourceGroupName.Value;
            coreProperties.TargetRegion = UserInputObj.TargetRegion.Value;
            coreProperties.Currency = UserInputObj.Currency.Key;
            coreProperties.AzureMigrateProjectName = UserInputObj.AzureMigrateProjectName.Value;
            coreProperties.AssessmentSiteName = UserInputObj.AssessmentProjectName;
            coreProperties.AssessmentDuration = UserInputObj.AssessmentDuration.Value;
            coreProperties.OptimizationPreference = UserInputObj.PreferredOptimizationObj.OptimizationPreference.Value;
            coreProperties.AssessSQLServices = UserInputObj.PreferredOptimizationObj.AssessSqlServicesSeparately ? "Yes" : "No";
            coreProperties.VCpuOverSubscription = AvsAssessmentConstants.VCpuOversubscription;
            coreProperties.MemoryOverCommit = AvsAssessmentConstants.MemoryOvercommit;
            coreProperties.DedupeCompression = AvsAssessmentConstants.DedupeCompression;
        }

        private void Process_AVS_Summary_Model(List<AVS_Summary> AVS_Summary_List)
        {
            if (AVSAssessmentsData == null)
                return;
            if (AVSAssessmentsData.Count <= 0)
                return;

            bool isAVSSummarySuccessful = false;
            isAVSSummarySuccessful = Create_AVS_Summary_Model(AVS_Summary_List);

            if (!isAVSSummarySuccessful)
                UserInputObj.LoggerObj.LogWarning("Encountered issue while generating AVS_Summary excel model");
        }

        private bool Create_AVS_Summary_Model(List<AVS_Summary> AVS_Summary_List)
        {
            if (AVSAssessmentsData == null)
            {
                UserInputObj.LoggerObj.LogWarning("Not creating excel model for AVS_Summary as AVS assessments dataset is null");
                return false;
            }

            if (AVSAssessmentsData.Count <= 0)
            {
                UserInputObj.LoggerObj.LogWarning("Not creating excel model for AVS_Summary as AVS assessments dataset is empty");
                return false;
            }

            UserInputObj.LoggerObj.LogInformation("Creating excel model for AVS_Summary ");

            if (AVS_Summary_List == null)
                AVS_Summary_List = new List<AVS_Summary>();

            foreach (var avsAssessmentData in AVSAssessmentsData)
            {
                AVS_Summary obj = new AVS_Summary();

                obj.SubscriptionId = avsAssessmentData.Value.SubscriptionId;
                obj.ResourceGroup = avsAssessmentData.Value.ResourceGroup;
                obj.ProjectName = avsAssessmentData.Value.AssessmentProjectName;
                obj.AssessmentName = avsAssessmentData.Value.AssessmentName;
                obj.SizingCriterion = avsAssessmentData.Value.SizingCriterion;
                obj.AssessmentType = avsAssessmentData.Value.AssessmentType;
                obj.CreatedOn = avsAssessmentData.Value.CreatedOn;
                obj.TotalMachinesAssessed = avsAssessmentData.Value.TotalMachinesAssessed;
                obj.MachinesReady = avsAssessmentData.Value.MachinesReady;
                obj.MachinesReadyWithConditions = avsAssessmentData.Value.MachinesReadyWithConditions;
                obj.MachinesNotReady = avsAssessmentData.Value.MachinesNotReady;
                obj.MachinesReadinessUnknown = avsAssessmentData.Value.MachinesReadinessUnknown;
                obj.TotalRecommendedNumberOfNodes = avsAssessmentData.Value.TotalRecommendedNumberOfNodes;
                obj.NodeTypes = avsAssessmentData.Value.NodeTypes;
                obj.RecommendedNodes = avsAssessmentData.Value.RecommendedNodes;
                obj.RecommendedFttRaidLevels = avsAssessmentData.Value.RecommendedFttRaidLevels;
                obj.RecommendedExternalStorage = avsAssessmentData.Value.RecommendedExternalStorage;
                obj.MonthlyTotalCostEstimate = avsAssessmentData.Value.TotalMonthlyCostEstimate;
                obj.MonthlyAvsExternalStorageCost = avsAssessmentData.Value.TotalExternalStorageCost;
                obj.MonthlyAvsNodeCost = avsAssessmentData.Value.TotalNodeCost;
                obj.MonthlyAvsExternalNetworkCost = avsAssessmentData.Value.TotalExternalNetworkCost;
                obj.PredictedCpuUtilizationPercentage = avsAssessmentData.Value.PredictedCpuUtilizationPercentage;
                obj.PredictedMemoryUtilizationPercentage = avsAssessmentData.Value.PredictedMemoryUtilizationPercentage;
                obj.PredictedStorageUtilizationPercentage = avsAssessmentData.Value.PredictedStorageUtilizationPercentage;
                obj.NumberOfCpuCoresAvailable = (int)Math.Floor(avsAssessmentData.Value.NumberOfCpuCoresAvailable);
                obj.MemoryInTBAvailable = avsAssessmentData.Value.MemoryInTBAvailable;
                obj.StorageInTBAvailable = avsAssessmentData.Value.StorageInTBAvailable;
                obj.NumberOfCpuCoresUsed = (int)Math.Floor(avsAssessmentData.Value.NumberOfCpuCoresUsed);
                obj.MemoryInTBUsed = avsAssessmentData.Value.MemoryInTBUsed;
                obj.StorageInTBUsed = avsAssessmentData.Value.StorageInTBUsed;
                obj.NumberOfCpuCoresFree = (int)Math.Floor(avsAssessmentData.Value.NumberOfCpuCoresFree);
                obj.MemoryInTBFree = avsAssessmentData.Value.MemoryInTBFree;
                obj.StorageInTBFree = avsAssessmentData.Value.StorageInTBFree;
                obj.ConfidenceRating = avsAssessmentData.Value.ConfidenceRating;

                AVS_Summary_List.Add(obj);
            }

            UserInputObj.LoggerObj.LogInformation($"Updated AVS_Summary excel model with data of {AVS_Summary_List.Count} assessments");
            return true;
        }

        private void Process_AVS_IaaS_Rehost_Perf_Model(List<AVS_IaaS_Rehost_Perf> AVS_IaaS_Rehost_Perf_List)
        {
            if (AVSAssessedMachinesData == null)
                return;
            if (AVSAssessedMachinesData.Count <= 0)
                return;

            bool isSuccessful = false;
            isSuccessful = Create_AVS_IaaS_Rehost_Perf_Model(AVS_IaaS_Rehost_Perf_List);

            if (!isSuccessful)
                UserInputObj.LoggerObj.LogWarning("Encountered issue while generating AVS_IaaS_Rehost_Perf excel model");
        }

        private bool Create_AVS_IaaS_Rehost_Perf_Model(List<AVS_IaaS_Rehost_Perf> AVS_IaaS_Rehost_Perf_List)
        {
            if (AVSAssessedMachinesData == null)
            {
                UserInputObj.LoggerObj.LogWarning("Not creating AVS_IaaS_Rehost_Perf excel model as AVS assessed machines dataset is null");
                return false;
            }
            if (AVSAssessedMachinesData.Count <= 0)
            {
                UserInputObj.LoggerObj.LogWarning("Not creating AVS_IaaS_Rehost_Perf excel model as AVS assessed machines dataset is empty");
                return false;
            }

            UserInputObj.LoggerObj.LogInformation("Creating excel model for AVS_IaaS_Rehost_Perf");

            if (AVS_IaaS_Rehost_Perf_List == null)
                AVS_IaaS_Rehost_Perf_List = new List<AVS_IaaS_Rehost_Perf>();

            foreach (var avsAssessedMachine in AVSAssessedMachinesData)
            {
                AVS_IaaS_Rehost_Perf obj = new AVS_IaaS_Rehost_Perf();

                obj.MachineName = avsAssessedMachine.Value.DisplayName;
                obj.AzureVMWareSolutionReadiness = avsAssessedMachine.Value.Suitability;
                obj.AzureVMWareSolutionReadiness_Warnings = avsAssessedMachine.Value.SuitabilityExplanation;
                obj.OperatingSystem = avsAssessedMachine.Value.OperatingSystemName;
                obj.OperatingSystemVersion = avsAssessedMachine.Value.OperatingSystemVersion;
                obj.OperatingSystemArchitecture = avsAssessedMachine.Value.OperatingSystemArchitecture;
                obj.BootType = avsAssessedMachine.Value.BootType;
                obj.Cores = avsAssessedMachine.Value.NumberOfCores;
                obj.MemoryInMB = avsAssessedMachine.Value.MegabytesOfMemory;
                obj.StorageInGB = UtilityFunctions.GetTotalStorage(avsAssessedMachine.Value.Disks);
                obj.StorageInUseInGB = avsAssessedMachine.Value.StorageInUseGB;
                obj.DiskReadInOPS = UtilityFunctions.GetDiskReadInOPS(avsAssessedMachine.Value.Disks);
                obj.DiskWriteInOPS = UtilityFunctions.GetDiskWriteInOPS(avsAssessedMachine.Value.Disks);
                obj.DiskReadInMBPS = UtilityFunctions.GetDiskReadInMBPS(avsAssessedMachine.Value.Disks);
                obj.DiskWriteInMBPS = UtilityFunctions.GetDiskWriteInMBPS(avsAssessedMachine.Value.Disks);
                obj.NetworkAdapters = avsAssessedMachine.Value.NetworkAdapters;

                var macIpKvp = UtilityFunctions.ParseMacIpAddress(avsAssessedMachine.Value.NetworkAdapterList);
                obj.MacAddresses = macIpKvp.Key;
                obj.IpAddresses = macIpKvp.Value;

                obj.NetworkInMBPS = UtilityFunctions.GetNetworkInMBPS(avsAssessedMachine.Value.NetworkAdapterList);
                obj.NetworkOutMBPS = UtilityFunctions.GetNetworkOutMBPS(avsAssessedMachine.Value.NetworkAdapterList);
                obj.DiskNames = UtilityFunctions.GetDiskNames(avsAssessedMachine.Value.Disks);
                obj.MachineId = avsAssessedMachine.Value.DatacenterMachineArmId;

                AVS_IaaS_Rehost_Perf_List.Add(obj);
            }

            UserInputObj.LoggerObj.LogInformation($"Updated AVS_IaaS_Rehost_Perf excel model with data of {AVS_IaaS_Rehost_Perf_List.Count} machines");
            return true;
        }



        private string GetUniquePlanName(string environment, string planName)
        {
            return environment + "_" + planName;
        }

        private void Process_Business_Case_Model(
            Business_Case Business_Case_Data
            )
        {
            UserInputObj.LoggerObj.LogInformation("Creating excel model for Business_Case");

            Business_Case_Data.ApplicationSummaryCostDetails = BusinessCaseData.ApplicationSummaryCostDetails;
            Business_Case_Data.CotsApplicationSummaryCostDetails = BusinessCaseData.CotsApplicationSummaryCostDetails;
            Business_Case_Data.CustomApplicationSummaryCostDetails = BusinessCaseData.CustomApplicationSummaryCostDetails;
            Business_Case_Data.IndependentWorkloadsSummaryCostDetails = BusinessCaseData.IndependentWorkloadsSummaryCostDetails;
            Business_Case_Data.TotalAzureCostDetails = BusinessCaseData.TotalAzureCostDetails;
            Business_Case_Data.TotalOnPremisesCostDetails = BusinessCaseData.TotalOnPremisesCostDetails;
            Business_Case_Data.azureArcEnabledOnPremisesCostDetails = BusinessCaseData.azureArcEnabledOnPremisesCostDetails;
            Business_Case_Data.futureAzureArcEnabledOnPremisesCostDetails = BusinessCaseData.futureAzureArcEnabledOnPremisesCostDetails;
            Business_Case_Data.futureCostDetails = BusinessCaseData.futureCostDetails;
            Business_Case_Data.WindowsAhubSavings = BusinessCaseData.WindowsAhubSavings;
            Business_Case_Data.LinuxAhubSavings = BusinessCaseData.LinuxAhubSavings;
            Business_Case_Data.SqlAhubSavings = BusinessCaseData.SqlAhubSavings;
            Business_Case_Data.SqlAzureCost = BusinessCaseData.SqlAzureCost;
            Business_Case_Data.MachineAzureCost = BusinessCaseData.MachineAzureCost;
            Business_Case_Data.AzureArcEnabledOnPremisesCost = BusinessCaseData.AzureArcEnabledOnPremisesCost;
            Business_Case_Data.FutureCostIncludingAzureArc = BusinessCaseData.FutureCostIncludingAzureArc;
            Business_Case_Data.FutureEsuSavingsFor4YearsIncludingAzureArc = BusinessCaseData.FutureEsuSavingsFor4YearsIncludingAzureArc;
            Business_Case_Data.FutureManagementCostSavingsIncludingAzureArc = BusinessCaseData.FutureManagementCostSavingsIncludingAzureArc;
            Business_Case_Data.FutureSecurityCostSavingsIncludingAzureArc = BusinessCaseData.FutureSecurityCostSavingsIncludingAzureArc;
            Business_Case_Data.AzureArcServicesCost = BusinessCaseData.AzureArcServicesCost;
            Business_Case_Data.FutureAzureArcEnabledOnPremisesCost = BusinessCaseData.FutureAzureArcEnabledOnPremisesCost;
            Business_Case_Data.FutureAzureArcServicesCost = BusinessCaseData.FutureAzureArcServicesCost;

            UserInputObj.LoggerObj.LogInformation("Updated Business_Case excel model");
        }
        private void Process_Cash_Flows_Model(Cash_Flows Cash_Flows_Data)
        {
            UserInputObj.LoggerObj.LogInformation("Creating excel model for Cash_Flows");

            Cash_Flows_Data.TotalYOYCosts.AzureCostYOY = BusinessCaseData.TotalYOYCashFlowsAndEmissions.AzureCostYOY;
            Cash_Flows_Data.TotalYOYCosts.OnPremisesCostYOY = BusinessCaseData.TotalYOYCashFlowsAndEmissions.OnPremisesCostYOY;
            Cash_Flows_Data.TotalYOYCosts.SavingsYOY = BusinessCaseData.TotalYOYCashFlowsAndEmissions.SavingsYOY;

            UserInputObj.LoggerObj.LogInformation("Updated excel model for Cash_Flows");
        }

        private void Process_Decommissioned_Machines_Model(List<Decommissioned_Machines> Decommissioned_Machines_List)
        {
            if (DecommissionedMachinesData == null)
                return;
            if (DecommissionedMachinesData.Count <= 0)
                return;

            bool isSuccessful = false;
            isSuccessful = Create_Decommissioned_Machines_Model(Decommissioned_Machines_List);

            if (!isSuccessful)
                UserInputObj.LoggerObj.LogWarning("Encountered issue while generating excel model for Decommissioned_Machines");
        }

        private bool Create_Decommissioned_Machines_Model(List<Decommissioned_Machines> Decommissioned_Machines_List)
        {
            if (DecommissionedMachinesData == null || DecommissionedMachinesData.Count <= 0)
            {
                UserInputObj.LoggerObj.LogWarning("Empty decommissioned machines data");
                return false;
            }

            if (Decommissioned_Machines_List == null)
                Decommissioned_Machines_List = new List<Decommissioned_Machines>();

            foreach (var kvp in DecommissionedMachinesData)
            {
                Decommissioned_Machines obj = new Decommissioned_Machines();

                obj.MachineName = kvp.Value;
                obj.MachineId = kvp.Key;

                Decommissioned_Machines_List.Add(obj);
            }

            UserInputObj.LoggerObj.LogInformation($"Updated Decommissioned_Machines excel model with data of {Decommissioned_Machines_List.Count} machines");
            return true;
        }

        private void Process_YOY_Emissions_Model(List<YOY_Emissions> YOY_Emissions_List)
        {
            if (BusinessCaseData == null)
                return;
            var yoyEmissionsAzure = new YOY_Emissions
            {
                Source = "Azure",
                Year0 = BusinessCaseData.TotalYOYCashFlowsAndEmissions.AzureEmissionsEstimates[0].Emissions ?? 0,
                Year1 = BusinessCaseData.TotalYOYCashFlowsAndEmissions.AzureEmissionsEstimates[1].Emissions ?? 0,
                Year2 = BusinessCaseData.TotalYOYCashFlowsAndEmissions.AzureEmissionsEstimates[2].Emissions ?? 0,
                Year3 = BusinessCaseData.TotalYOYCashFlowsAndEmissions.AzureEmissionsEstimates[3].Emissions ?? 0
            };
            var yoyEmissionsOnPremises = new YOY_Emissions
            {
                Source = "OnPremises",
                Year0 = BusinessCaseData.TotalYOYCashFlowsAndEmissions.OnPremisesEmissionsEstimates[0].Emissions ?? 0,
                Year1 = BusinessCaseData.TotalYOYCashFlowsAndEmissions.OnPremisesEmissionsEstimates[1].Emissions ?? 0,
                Year2 = BusinessCaseData.TotalYOYCashFlowsAndEmissions.OnPremisesEmissionsEstimates[2].Emissions ?? 0,
                Year3 = BusinessCaseData.TotalYOYCashFlowsAndEmissions.OnPremisesEmissionsEstimates[3].Emissions ?? 0
            };
            YOY_Emissions_List.Add(yoyEmissionsAzure);
            YOY_Emissions_List.Add(yoyEmissionsOnPremises);
        }

        private void Process_EmissionsDetails_Model(List<EmissionsDetails> Emissions_Details_List)
        {
            if (BusinessCaseData == null)
                return;
            if (BusinessCaseData.TotalAzureSustainabilityDetails == null || BusinessCaseData.TotalOnPremisesSustainabilityDetails == null)
                return;
            var azureEmissionsDetails = new EmissionsDetails
            {
                Source = "Azure",
                Scope1Compute = BusinessCaseData.TotalAzureSustainabilityDetails.Scope1.Compute,
                Scope1Storage = BusinessCaseData.TotalAzureSustainabilityDetails.Scope1.Storage,
                Scope2Compute = BusinessCaseData.TotalAzureSustainabilityDetails.Scope2.Compute,
                Scope2Storage = BusinessCaseData.TotalAzureSustainabilityDetails.Scope2.Storage,
                Scope3Compute = BusinessCaseData.TotalAzureSustainabilityDetails.Scope3.Compute,
                Scope3Storage = BusinessCaseData.TotalAzureSustainabilityDetails.Scope3.Storage,
            };
            var onPremisesEmissionsDetails = new EmissionsDetails
            {
                Source = "OnPremises",
                Scope1Compute = BusinessCaseData.TotalOnPremisesSustainabilityDetails.Scope1.Compute,
                Scope1Storage = BusinessCaseData.TotalOnPremisesSustainabilityDetails.Scope1.Storage,
                Scope2Compute = BusinessCaseData.TotalOnPremisesSustainabilityDetails.Scope2.Compute,
                Scope2Storage = BusinessCaseData.TotalOnPremisesSustainabilityDetails.Scope2.Storage,
                Scope3Compute = BusinessCaseData.TotalOnPremisesSustainabilityDetails.Scope3.Compute,
                Scope3Storage = BusinessCaseData.TotalOnPremisesSustainabilityDetails.Scope3.Storage,
            };
            Emissions_Details_List.Add(azureEmissionsDetails);
            Emissions_Details_List.Add(onPremisesEmissionsDetails);
        }

        private void Process_Inventory_Insights_Model(List<InventoryInsights> inventoryInsightsList)
        {
            if (InventoryInsightsData == null || InventoryInsightsData.Count == 0)
                return;

            foreach (var insight in InventoryInsightsData)
            {
                inventoryInsightsList.Add(insight);
            }
        }

        private void Process_Software_Insights_Model(List<SoftwareInsights> softwareInsightsList)
        {
            if (SoftwareInsightsData == null || SoftwareInsightsData.Count == 0)
                return;
            foreach (var insight in SoftwareInsightsData)
            {
                softwareInsightsList.Add(insight);
            }
        }

        private void Process_Software_Vulnerabilities_Model(List<SoftwareVulnerabilities> softwareVulnerabilitiesList)
        {
            if (SoftwareVulnerabilitiesData == null || SoftwareVulnerabilitiesData.Count == 0)
                return;

            foreach (var vulnerability in SoftwareVulnerabilitiesData)
            {
                softwareVulnerabilitiesList.Add(vulnerability);
            }
        }

        private void Process_PendingUpdates_ServerCounts_Model(List<PendingUpdatesServerCounts> pendingUpdatesServerCountsList)
        {
            if (PendingUpdatesServerCountsData == null || PendingUpdatesServerCountsData.Count == 0)
                return;

            foreach (var update in PendingUpdatesServerCountsData)
            {
                pendingUpdatesServerCountsList.Add(update);
            }
        }
    }
}
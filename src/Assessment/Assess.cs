// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Azure.Migrate.Explore.Assessment.Parser;
using Azure.Migrate.Explore.Assessment.Processor;
using Azure.Migrate.Explore.Common;
using Azure.Migrate.Explore.Excel;
using Azure.Migrate.Explore.Factory;
using Azure.Migrate.Explore.HttpRequestHelper;
using Azure.Migrate.Explore.Models;
using Microsoft.UI.Xaml.Input;
using System.Threading.Tasks;
using System.Threading;
using AzureMigrateExplore.Models;

namespace Azure.Migrate.Explore.Assessment
{
    public class Assess
    {
        private UserInput UserInputObj;
        private List<DiscoveryData> DiscoveredData;
        private List<vCenterHostDiscovery> vCenterHostDiscoveries;
        private List<string> ListOfSites;

        public Assess()
        {
            UserInputObj = null;
            DiscoveredData = new List<DiscoveryData>();
        }

        public Assess(UserInput userInputObj)
        {
            UserInputObj = userInputObj;
            DiscoveredData = new List<DiscoveryData>();
            vCenterHostDiscoveries = new List<vCenterHostDiscovery>();
            ListOfSites = new List<string>();
        }

        public Assess(UserInput userInputObj, List<DiscoveryData> discoveredData)
        {
            UserInputObj = userInputObj;
            DiscoveredData = discoveredData;
        }

        public bool BeginAssessment()
        {
            if (UserInputObj == null)
                throw new Exception("User input provided is null.");

            UserInputObj.LoggerObj.LogInformation("Initiating assessment");

            DeletePreviousAssessmentReports();
             string masterSitesUrl = Routes.ProtocolScheme + Routes.AzureManagementApiHostname + Routes.ForwardSlash +
                                    Routes.SubscriptionPath + Routes.ForwardSlash + UserInputObj.Subscription.Key + Routes.ForwardSlash +
                                    Routes.ResourceGroupPath + Routes.ForwardSlash + UserInputObj.ResourceGroupName.Value + Routes.ForwardSlash +
                                    Routes.ProvidersPath + Routes.ForwardSlash + Routes.OffAzureProvidersPath + Routes.ForwardSlash +
                                    Routes.MasterSitesPath + Routes.ForwardSlash + UserInputObj.DiscoverySiteName +
                                    Routes.QueryStringQuestionMark + Routes.QueryParameterApiVersion + Routes.QueryStringEquals + Routes.MasterSiteApiVersion;

            string masterSitesJsonResponse = "";
            try
            {
                masterSitesJsonResponse = new HttpClientHelper().GetHttpRequestJsonStringResponse(masterSitesUrl, UserInputObj).Result;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (AggregateException aeMasterSites)
            {
                string errorMessage = "";
                foreach (var e in aeMasterSites.Flatten().InnerExceptions)
                {
                    if (e is OperationCanceledException)
                        throw e;
                    else
                    {
                        errorMessage = errorMessage + e.Message + " ";
                    }
                }
                UserInputObj.LoggerObj.LogError($"Failed to retrieve master sites: {errorMessage}");
                return false;
            }
            catch (Exception exMasterSitesHttpResponse)
            {
                UserInputObj.LoggerObj.LogError($"Failed to retrieve master sites: {exMasterSitesHttpResponse.Message}");
                return false;
            }

            MasterSitesJSON masterSitesObj = JsonConvert.DeserializeObject<MasterSitesJSON>(masterSitesJsonResponse);
            List<string> ListOfSites = masterSitesObj.Properties.Sites;

            if (DiscoveredData.Count <= 0)
            {
                new ImportDiscoveryReport(UserInputObj.LoggerObj, DiscoveredData, vCenterHostDiscoveries).ImportDiscoveryData();
            }

            new DiscoveryDataValidation().BeginValidation(UserInputObj.LoggerObj, DiscoveredData);
            UserInputObj.LoggerObj.LogInformation($"Total discovered machines: {DiscoveredData.Count}");

            string assessmentSiteMachineListUrl = Routes.ProtocolScheme + Routes.AzureManagementApiHostname + Routes.ForwardSlash +
                                                  Routes.SubscriptionPath + Routes.ForwardSlash + UserInputObj.Subscription.Key + Routes.ForwardSlash +
                                                  Routes.ResourceGroupPath + Routes.ForwardSlash + UserInputObj.ResourceGroupName.Value + Routes.ForwardSlash +
                                                  Routes.ProvidersPath + Routes.ForwardSlash + Routes.MigrateProvidersPath + Routes.ForwardSlash +
                                                  Routes.AssessmentProjectsPath + Routes.ForwardSlash + UserInputObj.AssessmentProjectName + Routes.ForwardSlash +
                                                  Routes.MachinesPath +
                                                  Routes.QueryStringQuestionMark + Routes.QueryParameterApiVersion + Routes.QueryStringEquals + Routes.AssessmentMachineListApiVersion;

            if (UserInputObj.AzureMigrateSourceAppliances.Contains("import"))
                assessmentSiteMachineListUrl += Routes.AssessmentProjectImportFilterPath;

            List<AssessmentSiteMachine> assessmentSiteMachines = new List<AssessmentSiteMachine>();

            while (!string.IsNullOrEmpty(assessmentSiteMachineListUrl))
            {
                if (UserInputObj.CancellationContext.IsCancellationRequested)
                    UtilityFunctions.InitiateCancellation(UserInputObj);

                string assessmentSiteMachinesResponse = "";
                try
                {
                    assessmentSiteMachinesResponse = new HttpClientHelper().GetHttpRequestJsonStringResponse(assessmentSiteMachineListUrl, UserInputObj).Result;
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (AggregateException aeAssessmentSiteMachinesList)
                {
                    string errorMessage = "";
                    foreach (var e in aeAssessmentSiteMachinesList.Flatten().InnerExceptions)
                    {
                        if (e is OperationCanceledException)
                            throw e;
                        else
                        {
                            errorMessage = errorMessage + e.Message + " ";
                        }
                    }
                    UserInputObj.LoggerObj.LogError($"Failed to retrieve machine data from assessment site: {errorMessage}");
                    return false;
                }
                catch (Exception exAssessmentSiteMachinesList)
                {
                    UserInputObj.LoggerObj.LogError($"Failed to retrieve machine data from assessment site: {exAssessmentSiteMachinesList.Message}");
                    return false;
                }

                AssessmentSiteMachinesListJSON assessmentSiteMachinesObj = JsonConvert.DeserializeObject<AssessmentSiteMachinesListJSON>(assessmentSiteMachinesResponse);

                assessmentSiteMachineListUrl = assessmentSiteMachinesObj.NextLink;

                foreach (var value in assessmentSiteMachinesObj.Values)
                {
                    AssessmentSiteMachine obj = new AssessmentSiteMachine
                    {
                        DisplayName = value.Properties.DisplayName,
                        AssessmentId = value.Id?.ToLower(),
                        DiscoveryMachineArmId = value.Properties.DiscoveryMachineArmId?.ToLower(),
                        SqlInstancesCount = value.Properties.SqlInstances.Count,
                        WebApplicationsCount = value.Properties.WebApplications.Count
                    };

                    assessmentSiteMachines.Add(obj);
                }
            }

            UserInputObj.LoggerObj.LogInformation(3, $"Retrieved data for {assessmentSiteMachines.Count} assessment site machine"); // IsExpressWorkflow ? 25 : 5 % complete

            Dictionary<string, string> AssessmentIdToDiscoveryIdLookup = new Dictionary<string, string>();

            // Independent
            Dictionary<string, List<AssessmentSiteMachine>> AzureVM = new Dictionary<string, List<AssessmentSiteMachine>>();
            Dictionary<string, List<AssessmentSiteMachine>> AzureSql = new Dictionary<string, List<AssessmentSiteMachine>>();
            Dictionary<string, List<AssessmentSiteMachine>> AzureWebApp = new Dictionary<string, List<AssessmentSiteMachine>>();
            HashSet<string> AzureWebApp_IaaS = new HashSet<string>(); // For migrate to all IaaS we do not need web app assessments 
            List<AssessmentSiteMachine> AzureVMWareSolution = new List<AssessmentSiteMachine>();

            // Dependent
            HashSet<string> SqlServicesVM = new HashSet<string>();
            HashSet<string> GeneralVM = new HashSet<string>(); // Machines without sql, webapp or sql services

            HashSet<string> discoveryMachineArmIdSet = GetDiscoveredMachineIDsSet();
            Dictionary<string, string> DecommissionedMachinesData = new Dictionary<string, string>();

            foreach (var assessmentSiteMachine in assessmentSiteMachines)
            {
                if (string.IsNullOrEmpty(assessmentSiteMachine.AssessmentId))
                    continue;

                if (string.IsNullOrEmpty(assessmentSiteMachine.DiscoveryMachineArmId))
                    continue;

                if (!discoveryMachineArmIdSet.Contains(assessmentSiteMachine.DiscoveryMachineArmId) && IsMachineDiscoveredBySelectedSourceAppliance(assessmentSiteMachine.DiscoveryMachineArmId))
                {
                    if (!DecommissionedMachinesData.ContainsKey(assessmentSiteMachine.DiscoveryMachineArmId))
                        DecommissionedMachinesData.Add(assessmentSiteMachine.DiscoveryMachineArmId, assessmentSiteMachine.DisplayName);
                }

                foreach (var discoverySiteMachine in DiscoveredData)
                {
                    if (string.IsNullOrEmpty(discoverySiteMachine.MachineId))
                        continue;

                    if (!discoverySiteMachine.MachineId.Equals(assessmentSiteMachine.DiscoveryMachineArmId))
                        continue;

                    bool addMachineToGeneralVM = true;

                    if (!AssessmentIdToDiscoveryIdLookup.ContainsKey(assessmentSiteMachine.AssessmentId))
                        AssessmentIdToDiscoveryIdLookup.Add(assessmentSiteMachine.AssessmentId, discoverySiteMachine.MachineId);

                    if (UserInputObj.BusinessProposal == BusinessProposal.Comprehensive.ToString())
                    {
                        if (!AzureVM.ContainsKey(discoverySiteMachine.EnvironmentType))
                            AzureVM.Add(discoverySiteMachine.EnvironmentType, new List<AssessmentSiteMachine>());
                        AzureVM[discoverySiteMachine.EnvironmentType].Add(assessmentSiteMachine);
                    }

                    if (UserInputObj.BusinessProposal == BusinessProposal.Comprehensive.ToString() &&
                        assessmentSiteMachine.SqlInstancesCount > 0 &&
                        discoverySiteMachine.SqlDiscoveryServerCount > 0)
                    {
                        addMachineToGeneralVM = false;

                        if (!AzureSql.ContainsKey(discoverySiteMachine.EnvironmentType))
                            AzureSql.Add(discoverySiteMachine.EnvironmentType, new List<AssessmentSiteMachine>());
                        AzureSql[discoverySiteMachine.EnvironmentType].Add(assessmentSiteMachine);
                    }

                    if (UserInputObj.BusinessProposal == BusinessProposal.Comprehensive.ToString() &&
                        assessmentSiteMachine.WebApplicationsCount > 0 &&
                        discoverySiteMachine.WebAppCount > 0)
                    {
                        addMachineToGeneralVM = false;

                        if (UserInputObj.PreferredOptimizationObj.OptimizationPreference.Value.Equals("Modernize"))
                        {
                            if (!AzureWebApp.ContainsKey(discoverySiteMachine.EnvironmentType))
                                AzureWebApp.Add(discoverySiteMachine.EnvironmentType, new List<AssessmentSiteMachine>());
                            AzureWebApp[discoverySiteMachine.EnvironmentType].Add(assessmentSiteMachine);
                        }
                        else // migrate to all IaaS
                        {
                            if (!AzureWebApp_IaaS.Contains(discoverySiteMachine.MachineId))
                                AzureWebApp_IaaS.Add(discoverySiteMachine.MachineId);
                        }
                    }

                    if (UserInputObj.BusinessProposal == BusinessProposal.Comprehensive.ToString() &&
                        discoverySiteMachine.IsSqlServicePresent &&
                        UserInputObj.PreferredOptimizationObj.AssessSqlServicesSeparately)
                    {
                        addMachineToGeneralVM = false;

                        if (!SqlServicesVM.Contains(discoverySiteMachine.MachineId))
                            SqlServicesVM.Add(discoverySiteMachine.MachineId);
                    }

                    if (discoverySiteMachine.MachineId.Contains("vmwaresites") || discoverySiteMachine.MachineId.Contains("importsites"))
                    {
                        AzureVMWareSolution.Add(assessmentSiteMachine);
                    }

                    if (UserInputObj.BusinessProposal == BusinessProposal.Comprehensive.ToString() &&
                        addMachineToGeneralVM)
                    {
                        if (!GeneralVM.Contains(discoverySiteMachine.MachineId))
                            GeneralVM.Add(discoverySiteMachine.MachineId);
                    }

                    // No other machines ahead need to be checked
                    break;
                }
            }

            string RandomSessionId = new Random().Next(0, 100000).ToString("D5");
            UserInputObj.LoggerObj.LogInformation($"ID for this session: {RandomSessionId}");

            // Collect all discovery machine ARM IDs for scoped business case
            List<string> scopedMachineIds = assessmentSiteMachines
                .Where(machine => !string.IsNullOrEmpty(machine.DiscoveryMachineArmId))
                .Select(machine => machine.DiscoveryMachineArmId!)
                .Distinct()
                .ToList();

            UserInputObj.LoggerObj.LogInformation($"Creating scoped business case with {scopedMachineIds.Count} machines");

            BusinessCaseInformation bizCaseObj = new BusinessCaseSettingsFactory().GetBusinessCaseSettings(UserInputObj, RandomSessionId, scopedMachineIds);
            KeyValuePair<BusinessCaseInformation, AssessmentPollResponse> bizCaseCompletionResultKvp = new KeyValuePair<BusinessCaseInformation, AssessmentPollResponse>(bizCaseObj, AssessmentPollResponse.NotCreated);
            try
            {
                bizCaseCompletionResultKvp = new BusinessCaseBuilder(bizCaseObj).BuildBusinessCase(UserInputObj);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (AggregateException aeBuildBizCase)
            {
                string errorMessage = "";
                foreach (var e in aeBuildBizCase.Flatten().InnerExceptions)
                {
                    if (e is OperationCanceledException)
                        throw e;
                    else
                    {
                        errorMessage = errorMessage + e.Message + " ";
                    }
                }
                throw new Exception(errorMessage);
            }
            catch (Exception)
            {
                throw;
            }

            UserInputObj.LoggerObj.LogInformation($"Business case {bizCaseCompletionResultKvp.Key.BusinessCaseName} is in {bizCaseCompletionResultKvp.Value.ToString()} state");

            UserInputObj.LoggerObj.LogInformation($"General VM count: {GeneralVM.Count}");

            UserInputObj.LoggerObj.LogInformation($"Machines with SQL services: {SqlServicesVM.Count}");

            HttpClientHelper clientHelper = new HttpClientHelper();

            var siteConditions = ListOfSites != null && ListOfSites.Count > 0
                ? string.Join(" or ", ListOfSites.Select(site =>
                    $"id contains '{site.Replace("'", "''")}'"))
                : null;

            var argQuery = siteConditions != null
                ? $"migrateresources | where {siteConditions}"
                : $"migrateresources | where id has '/subscriptions/{UserInputObj.Subscription.Key}/resourceGroups/{UserInputObj.ResourceGroupName.Value}/'";            
            List<string> resolvedScopes = clientHelper.ResolveScopeAsync(UserInputObj, argQuery.ToString()).Result;
            string assessmentProjectArmId = $"/subscriptions/{UserInputObj.Subscription.Key}/resourceGroups/{UserInputObj.ResourceGroupName.Value}/providers/Microsoft.Migrate/assessmentProjects/{UserInputObj.AssessmentProjectName}";

            UserInputObj.LoggerObj.LogInformation($"ARM overrides: azureLocation='{UserInputObj.TargetRegion.Key}', currency='{UserInputObj.Currency.Key}', performanceTimeRange='{UserInputObj.AssessmentDuration.Key}'");

            Dictionary<string, string> argDict = new Dictionary<string, string>
            {
                {"azureLocation", UserInputObj.TargetRegion.Key},
                {"currency", UserInputObj.Currency.Key},
                {"performanceTimeRange", UserInputObj.AssessmentDuration.Key},
            };
            var deployResult = clientHelper.DeployAssessmentArmTemplateAsync(
                UserInputObj,
                UserInputObj.Subscription.Key,
                UserInputObj.ResourceGroupName.Value,
                assessmentProjectArmId,
                $"AME-{RandomSessionId}",
                argQuery.ToString(),
                resolvedScopes,
                argDict).Result;

            var assessmentInfo = new AssessmentInformation(
                $"AME-{RandomSessionId}",
                AssessmentType.HeterogeneousAssessment,
                AssessmentTag.PerformanceBased,
                ""
            );

            var reportHandler = new HeterogeneousReportHandler();
            bool isCompleted = reportHandler.WaitForHeterogeneousAssessmentCompletion(UserInputObj, assessmentInfo);

            if (isCompleted)
            {
                reportHandler.GenerateAndDownloadHeterogeneousReportAsync(UserInputObj, assessmentInfo).Wait();
            }
            else
            {
                UserInputObj.LoggerObj.LogError("Heterogeneous assessment did not complete successfully. Skipping report generation.");
            }

            UserInputObj.LoggerObj.LogInformation(65 - UserInputObj.LoggerObj.GetCurrentProgress(), $"Completed assessment creation job"); // 65 % complete

           Dictionary<AssessmentInformation, AssessmentPollResponse> AVSAssessmentStatusMap = new Dictionary<AssessmentInformation, AssessmentPollResponse>();

            if (AVSAssessmentStatusMap.Count > 0)
            {
                const int AvsMaxPollAttempts = 40;
                const int AvsPollDelayMs = 60_000;
                var avsPollHelper = new HttpClientHelper();

                foreach (var assessment in AVSAssessmentStatusMap.Keys.ToList())
                {
                    AssessmentPollResponse currentStatus = AVSAssessmentStatusMap[assessment];

                    for (int attempt = 1; attempt <= AvsMaxPollAttempts; attempt++)
                    {
                        if (UserInputObj.CancellationContext.IsCancellationRequested)
                            UtilityFunctions.InitiateCancellation(UserInputObj);

                        UserInputObj.LoggerObj.LogInformation($"Polling AVS assessment {assessment.AssessmentName} (Attempt {attempt}/{AvsMaxPollAttempts})...");

                        try
                        {
                            currentStatus = avsPollHelper.PollAssessment(UserInputObj, assessment).Result;
                        }
                        catch (AggregateException aggregateEx)
                        {
                            var flattened = aggregateEx.Flatten();
                            foreach (var inner in flattened.InnerExceptions)
                            {
                                if (inner is OperationCanceledException)
                                    throw inner;
                                UserInputObj.LoggerObj.LogWarning($"AVS assessment {assessment.AssessmentName} polling error: {inner.Message}");
                            }
                            currentStatus = AssessmentPollResponse.Error;
                        }
                        catch (OperationCanceledException)
                        {
                            throw;
                        }
                        catch (Exception ex)
                        {
                            UserInputObj.LoggerObj.LogWarning($"AVS assessment {assessment.AssessmentName} polling error: {ex.Message}");
                            currentStatus = AssessmentPollResponse.Error;
                        }

                        AVSAssessmentStatusMap[assessment] = currentStatus;

                        if (currentStatus == AssessmentPollResponse.Completed)
                        {
                            UserInputObj.LoggerObj.LogInformation($"AVS assessment {assessment.AssessmentName} completed successfully.");
                            break;
                        }

                        if (currentStatus == AssessmentPollResponse.OutDated ||
                            currentStatus == AssessmentPollResponse.Invalid ||
                            currentStatus == AssessmentPollResponse.Error)
                        {
                            UserInputObj.LoggerObj.LogWarning($"AVS assessment {assessment.AssessmentName} reached terminal state {currentStatus}.");
                            break;
                        }

                        if (attempt == AvsMaxPollAttempts)
                        {
                            UserInputObj.LoggerObj.LogWarning($"AVS assessment {assessment.AssessmentName} did not complete within the polling window.");
                            break;
                        }

                        Thread.Sleep(AvsPollDelayMs);
                    }
                }
            }

            if (AVSAssessmentStatusMap.Count > 0)
                UserInputObj.LoggerObj.LogInformation($"Total AVS Assessments: {AVSAssessmentStatusMap.Count}");

            
            Dictionary<AssessmentInformation, AVSAssessmentPropertiesDataset> AVSAssessmentsData = new Dictionary<AssessmentInformation, AVSAssessmentPropertiesDataset>();
            Dictionary<string, AVSAssessedMachinesDataset> AVSAssessedMachinesData = new Dictionary<string, AVSAssessedMachinesDataset>();
            if (AVSAssessmentStatusMap.Count > 0)
            {
                ParseAVSAssessments(AVSAssessmentsData, AVSAssessedMachinesData, AVSAssessmentStatusMap);
            }

            BusinessCaseDataset BusinessCaseData = new BusinessCaseDataset();
            if (bizCaseCompletionResultKvp.Value == AssessmentPollResponse.Completed)
            {
                ParseBusinessCase(bizCaseCompletionResultKvp, BusinessCaseData);
            }
            List<InventoryInsights> InventoryInsightsData = new List<InventoryInsights>();
            List<SoftwareInsights> SoftwareInsightsData = new List<SoftwareInsights>();
            List<SoftwareVulnerabilities> SoftwareVulnerabilitiesData = new List<SoftwareVulnerabilities>();
            List<PendingUpdatesServerCounts> PendingUpdatesServerCountsData = new List<PendingUpdatesServerCounts>();

            // Fetch inventory insights data using ARG queries
            try
            {
                UserInputObj.LoggerObj.LogInformation("Starting inventory insights data collection");

                // Extract machine IDs from discovered data only
                List<string> machineIds = new List<string>();

                // Add machine IDs from discovered data
                foreach (var discoveryData in DiscoveredData)
                {
                    if (!string.IsNullOrEmpty(discoveryData.MachineId))
                    {
                        machineIds.Add(discoveryData.MachineId.ToLower());
                    }
                }

                // Remove duplicates
                machineIds = machineIds.Distinct().ToList();

                UserInputObj.LoggerObj.LogInformation($"Found {machineIds.Count} unique machine IDs for inventory insights collection");

                if (machineIds.Count > 0)
                {
                    // Prepare subscriptions array
                    string[] subscriptions = { UserInputObj.Subscription.Key };

                    // Extract site IDs from machine IDs for inventory insights
                    List<string> siteIds = new List<string>();
                    foreach (var machineId in machineIds)
                    {
                        // Machine IDs typically look like: /subscriptions/{sub}/resourceGroups/{rg}/providers/Microsoft.OffAzure/{siteType}/machines/{machineId}
                        // We need to extract the site part for the filter
                        int machineIndex = machineId.IndexOf("/machines/", StringComparison.OrdinalIgnoreCase);
                        if (machineIndex > 0)
                        {
                            string siteId = machineId.Substring(0, machineIndex);
                            siteIds.Add(siteId);
                        }
                    }

                    // Remove duplicates and create site filter
                    siteIds = siteIds.Distinct().ToList();
                    
                    if (siteIds.Count == 0)
                    {
                        UserInputObj.LoggerObj.LogWarning("No valid site types found in machine IDs");
                        return true;
                    }

                    // Fetch inventory insights data
                    UserInputObj.LoggerObj.LogInformation("Fetching inventory insights data from ARG");
                    UserInputObj.LoggerObj.LogInformation($"Site IDs for filter: {string.Join(", ", siteIds)}");
                    try
                    {
                        InventoryInsightsData = AzureMigrateExplore.Assessment.ARGQueryBuilder.GetInventoryInsightsDataAsync(
                            UserInputObj, subscriptions, siteIds).Result;
                        UserInputObj.LoggerObj.LogInformation($"Retrieved {InventoryInsightsData.Count} inventory insights records");
                    }
                    catch (Exception exInventory)
                    {
                        UserInputObj.LoggerObj.LogError($"Failed to fetch inventory insights data: {exInventory.Message}");
                        if (exInventory.InnerException != null)
                            UserInputObj.LoggerObj.LogError($"Inner exception: {exInventory.InnerException.Message}");
                    }

                    // Fetch software insights data
                    UserInputObj.LoggerObj.LogInformation("Fetching software insights data from ARG");
                    try
                    {
                        SoftwareInsightsData = AzureMigrateExplore.Assessment.ARGQueryBuilder.GetSoftwareAnalysisData(
                            UserInputObj, subscriptions, machineIds).Result;
                        UserInputObj.LoggerObj.LogInformation($"Retrieved {SoftwareInsightsData.Count} software insights records");
                    }
                    catch (Exception exSoftware)
                    {
                        UserInputObj.LoggerObj.LogWarning($"Failed to fetch software insights data: {exSoftware.Message}");
                    }

                    // Fetch software vulnerabilities data
                    UserInputObj.LoggerObj.LogInformation("Fetching software vulnerabilities data from ARG");
                    try
                    {
                        SoftwareVulnerabilitiesData = AzureMigrateExplore.Assessment.ARGQueryBuilder.GetSoftwareVulnerabilitiesData(
                            UserInputObj, subscriptions, siteIds, machineIds).Result;
                        UserInputObj.LoggerObj.LogInformation($"Retrieved {SoftwareVulnerabilitiesData.Count} software vulnerabilities records");
                    }
                    catch (Exception exVulnerabilities)
                    {
                        UserInputObj.LoggerObj.LogWarning($"Failed to fetch software vulnerabilities data: {exVulnerabilities.Message}");
                    }

                    // Fetch pending updates data
                    UserInputObj.LoggerObj.LogInformation("Fetching pending updates data from ARG");
                    try
                    {
                        PendingUpdatesServerCountsData = AzureMigrateExplore.Assessment.ARGQueryBuilder.GetPendingUpdatesServerCountDataAsync(
                            UserInputObj, subscriptions, siteIds).Result;
                        UserInputObj.LoggerObj.LogInformation($"Retrieved {PendingUpdatesServerCountsData.Count} pending updates records");
                    }
                    catch (Exception exVulnerabilities)
                    {
                        UserInputObj.LoggerObj.LogWarning($"Failed to fetch pending updates counts data: {exVulnerabilities.Message}");
                    }
                }
                else
                {
                    UserInputObj.LoggerObj.LogWarning("No machine IDs available for inventory insights collection");
                }

                UserInputObj.LoggerObj.LogInformation("Completed inventory insights data collection");
            }
            catch (Exception exInventoryInsights)
            {
                UserInputObj.LoggerObj.LogError($"Error during inventory insights data collection: {exInventoryInsights.Message}");
                // Continue processing with empty lists rather than failing the entire assessment
            }

            ProcessDatasets processorObj = new ProcessDatasets
                (
                    AVSAssessmentsData,
                    AVSAssessedMachinesData,
                    BusinessCaseData,
                    DecommissionedMachinesData,
                    InventoryInsightsData,
                    SoftwareInsightsData,
                    SoftwareVulnerabilitiesData,
                    PendingUpdatesServerCountsData,
                    UserInputObj
                );
            processorObj.InititateProcessing();

            return true;
        }

        private void ParseBusinessCase(KeyValuePair<BusinessCaseInformation, AssessmentPollResponse> bizCaseCompletionResultKvp, BusinessCaseDataset BusinessCaseData)
        {
            UserInputObj.LoggerObj.LogInformation("Initiating parsing for business case");
            try
            {
                new BusinessCaseParser(bizCaseCompletionResultKvp).ParseBusinessCase(UserInputObj, BusinessCaseData);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (AggregateException aeBizCaseParse)
            {
                string errorMessage = "";
                foreach (var e in aeBizCaseParse.Flatten().InnerExceptions)
                {
                    if (e is OperationCanceledException)
                        throw e;
                    else
                    {
                        errorMessage = errorMessage + e.Message + " ";
                    }
                }
                UserInputObj.LoggerObj.LogError($"Business case parsing error : {errorMessage}");
            }
            catch (Exception exBizCaseParse)
            {
                UserInputObj.LoggerObj.LogError($"Business case parsing error {exBizCaseParse.Message}");
            }

            UserInputObj.LoggerObj.LogInformation("Business case parsing job completed");
        }

        private void ParseAVSAssessments(Dictionary<AssessmentInformation, AVSAssessmentPropertiesDataset> AVSAssessmentsData, Dictionary<string, AVSAssessedMachinesDataset> AVSAssessedMachinesData, Dictionary<AssessmentInformation, AssessmentPollResponse> AVSAssessmentStatusMap)
        {
            UserInputObj.LoggerObj.LogInformation("Initiating parsing for AVS assessments");
            try
            {
                new AVSAssessmentParser(AVSAssessmentStatusMap).ParseAVSAssessment(AVSAssessmentsData, AVSAssessedMachinesData, UserInputObj);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (AggregateException aeAVSAssessmentParse)
            {
                string errorMessage = "";
                foreach (var e in aeAVSAssessmentParse.Flatten().InnerExceptions)
                {
                    if (e is OperationCanceledException)
                        throw e;
                    else
                    {
                        errorMessage = errorMessage + e.Message + " ";
                    }
                }
                UserInputObj.LoggerObj.LogError($"AVS assessment parsing error : {errorMessage}");
            }
            catch (Exception exAVSAssessmentParse)
            {
                UserInputObj.LoggerObj.LogError($"AVS assessment parsing error {exAVSAssessmentParse.Message}");
            }

            UserInputObj.LoggerObj.LogInformation(75 - UserInputObj.LoggerObj.GetCurrentProgress(), "AVS assessment parsing job completed"); // 75 % Complete
        }

        #region Deletion
        private void DeletePreviousAssessmentReports()
        {
            UserInputObj.LoggerObj.LogInformation("Deleting previous assessment reports, if any");
            DeletePreviousCoreReport();
            DeletePreviousOpportunityReport();
            DeletePreviousClashReport();
        }

        private void DeletePreviousCoreReport()
        {
            UserInputObj.LoggerObj.LogInformation("Deleting previous core report");
            var directory = UtilityFunctions.GetReportsDirectory();
            var file = Path.Combine(directory, "AzureMigrate_Assessment_Core_Report.xlsx");

            if (!Directory.Exists(directory))
            {
                UserInputObj.LoggerObj.LogInformation("No core report found");
                return;
            }

            if (!File.Exists(file))
            {
                UserInputObj.LoggerObj.LogInformation("No core report file found");
                return;
            }

            UserInputObj.LoggerObj.LogInformation("Core report found, please ensure the file is closed otherwise deleting it won't be possible and process will terminate");
            try
            {
                File.Delete(file);
                UserInputObj.LoggerObj.LogInformation("Core report file deleted successfully");
            }
            catch (IOException ex)
            {
                UserInputObj.LoggerObj.LogError($"Failed to delete core report file: {ex.Message}");
                throw;
            }
        }

        private void DeletePreviousOpportunityReport()
        {
            UserInputObj.LoggerObj.LogInformation("Deleting previous opportunity report");
            var directory = UtilityFunctions.GetReportsDirectory();
            var file = Path.Combine(directory, "AzureMigrate_Assessment_Opportunity_Report.xlsx");
            if (!Directory.Exists(directory))
            {
                UserInputObj.LoggerObj.LogInformation("No opportunity report found");
                return;
            }

            if (!File.Exists(file))
            {
                UserInputObj.LoggerObj.LogInformation("No opportunity report file found");
                return;
            }

            UserInputObj.LoggerObj.LogInformation("Opportunity report found, please ensure the file is closed otherwise deleting it won't be possible and process will terminate");
            try
            {
                File.Delete(file);
                UserInputObj.LoggerObj.LogInformation("Opportunity report file deleted successfully");
            }
            catch (IOException ex)
            {
                UserInputObj.LoggerObj.LogError($"Failed to delete opportunity report file: {ex.Message}");
                throw;
            }
        }

        private void DeletePreviousClashReport()
        {
            UserInputObj.LoggerObj.LogInformation("Deleting previous clash report");
            var directory = UtilityFunctions.GetReportsDirectory();
            var file = Path.Combine(directory, "AzureMigrate_Assessment_Clash_Report.xlsx");
            if (!Directory.Exists(directory))
            {
                UserInputObj.LoggerObj.LogInformation("No clash report found");
                return;
            }

            if (!File.Exists(file))
            {
                UserInputObj.LoggerObj.LogInformation("No clash report file found");
                return;
            }

            UserInputObj.LoggerObj.LogInformation("Clash report found, please ensure the file is closed otherwise deleting it won't be possible and process will terminate");
            try
            {
                File.Delete(file);
                UserInputObj.LoggerObj.LogInformation("Clash report file deleted successfully");
            }
            catch (IOException ex)
            {
                UserInputObj.LoggerObj.LogError($"Failed to delete clash report file: {ex.Message}");
                throw;
            }
        }
        #endregion

        #region Utilities
        private HashSet<string> GetDiscoveredMachineIDsSet()
        {
            HashSet<string> result = new HashSet<string>();

            foreach (var discoveredMachine in DiscoveredData)
            {
                if (!result.Contains(discoveredMachine.MachineId))
                    result.Add(discoveredMachine.MachineId);
            }

            return result;
        }

        private bool IsMachineDiscoveredBySelectedSourceAppliance(string discoveryArmId)
        {
            if (string.IsNullOrEmpty(discoveryArmId))
                return false;
            if (UserInputObj.AzureMigrateSourceAppliances == null || UserInputObj.AzureMigrateSourceAppliances.Count <= 0)
                return false;

            bool getVmware = UserInputObj.AzureMigrateSourceAppliances.Contains("vmware");
            bool getHyperv = UserInputObj.AzureMigrateSourceAppliances.Contains("hyperv");
            bool getPhysical = UserInputObj.AzureMigrateSourceAppliances.Contains("physical");
            bool getImport = UserInputObj.AzureMigrateSourceAppliances.Contains("import");

            bool isVmwareSite = discoveryArmId.Contains("vmwaresites");
            bool isHypervSite = discoveryArmId.Contains("hypervsites");
            bool isServerSite = discoveryArmId.Contains("serversites");
            bool isImportSite = discoveryArmId.Contains("importsites");

            if ((getVmware && isVmwareSite) ||
                (getHyperv && isHypervSite) ||
                (getPhysical && isServerSite) ||
                (getImport && isImportSite))
                return true;

            return false;
        }
        #endregion
    }
}
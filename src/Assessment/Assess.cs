// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

using Azure.Migrate.Explore.Assessment.Parser;
using Azure.Migrate.Explore.Assessment.Processor;
using Azure.Migrate.Explore.Common;
using Azure.Migrate.Explore.Excel;
using Azure.Migrate.Explore.Factory;
using Azure.Migrate.Explore.HttpRequestHelper;
using Azure.Migrate.Explore.Models;

namespace Azure.Migrate.Explore.Assessment
{
    public class Assess
    {
        private UserInput UserInputObj;
        private List<DiscoveryData> DiscoveredData;
        private List<vCenterHostDiscovery> vCenterHostDiscoveries;

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

            Dictionary<string, List<string>> GroupMachinesMap = new Dictionary<string, List<string>>();
            List<string> CreatedGroups = new List<string>();

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

            BusinessCaseInformation bizCaseObj = new BusinessCaseSettingsFactory().GetBusinessCaseSettings(UserInputObj, RandomSessionId);
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

            foreach (var kvp in AzureVM)
            {
                UserInputObj.LoggerObj.LogInformation($"Total {kvp.Key} environment machines: {kvp.Value.Count}");
                GroupMachinesMap.Add($"AzureVM-{kvp.Key}-AME-{RandomSessionId}", ObtainAssessmentMachineIdList(kvp.Value));
            }

            if (AzureVMWareSolution.Count > 0)
            {
                UserInputObj.LoggerObj.LogInformation($"Machines for Azure VMWare solution: {AzureVMWareSolution.Count}");
                GroupMachinesMap.Add($"Azure-VMWareSolution-AME-{RandomSessionId}", ObtainAssessmentMachineIdList(AzureVMWareSolution));
            }

            foreach (var kvp in AzureSql)
            {
                UserInputObj.LoggerObj.LogInformation($"{kvp.Key} environment SQL machines: {kvp.Value.Count}");
                GroupMachinesMap.Add($"SQL-{kvp.Key}-AME-{RandomSessionId}", ObtainAssessmentMachineIdList(kvp.Value));
            }

            foreach (var kvp in AzureWebApp)
            {
                UserInputObj.LoggerObj.LogInformation($"{kvp.Key} environment machines with web applications: {kvp.Value.Count}");
                GroupMachinesMap.Add($"WebApp-{kvp.Key}-AME-{RandomSessionId}", ObtainAssessmentMachineIdList(kvp.Value));
            }

            UserInputObj.LoggerObj.LogInformation($"General VM count: {GeneralVM.Count}");

            UserInputObj.LoggerObj.LogInformation($"Machines with SQL services: {SqlServicesVM.Count}");

            Dictionary<string, GroupPollResponse> GroupStatusMap = new Dictionary<string, GroupPollResponse>();

            foreach (var kvp in GroupMachinesMap)
            {
                if (UserInputObj.CancellationContext.IsCancellationRequested)
                    UtilityFunctions.InitiateCancellation(UserInputObj);

                bool isGroupCreationComplete = false;
                try
                {
                    GroupStatusMap.Add(kvp.Key, GroupPollResponse.Invalid);
                    isGroupCreationComplete = new HttpClientHelper().CreateGroup(UserInputObj, kvp, GroupStatusMap).Result;
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (AggregateException aeCreateGroup)
                {
                    string errorMessage = "";
                    foreach (var e in aeCreateGroup.Flatten().InnerExceptions)
                    {
                        if (e is OperationCanceledException)
                            throw e;
                        else
                        {
                            errorMessage = errorMessage + e.Message + " ";
                        }
                    }
                    UserInputObj.LoggerObj.LogWarning($"Group {kvp.Key} creation failed: {errorMessage}");
                    isGroupCreationComplete = false;
                }
                catch (Exception ex)
                {
                    UserInputObj.LoggerObj.LogWarning($"Group {kvp.Key} creation failed: {ex.Message}");
                    isGroupCreationComplete = false;
                }

                if (!isGroupCreationComplete)
                {
                    UserInputObj.LoggerObj.LogWarning($"Group {kvp.Key} could not be created, skipping corresponding assessments");
                    continue;
                }

                UserInputObj.LoggerObj.LogInformation($"Group {kvp.Key} created successfully");
                CreatedGroups.Add(kvp.Key);
            }

            if (CreatedGroups.Count <= 0)
            {
                UserInputObj.LoggerObj.LogError("No groups created, terminating process");
                return false;
            }

            UserInputObj.LoggerObj.LogInformation(2, $"Created groups: {CreatedGroups.Count}"); // IsExpressWorkflow ? 27 : 7 % complete

            int completedGroups = 0;
            int invalidGroups = 0;
            foreach (var kvp in GroupStatusMap)
            {
                if (kvp.Value == GroupPollResponse.Completed)
                    completedGroups += 1;
                else if (kvp.Value == GroupPollResponse.Invalid)
                    invalidGroups += 1;
            }

            if (completedGroups <= 0)
            {
                UserInputObj.LoggerObj.LogError("No groups in completed state, terminating process");
                return false;
            }
            if (invalidGroups > 0)
                UserInputObj.LoggerObj.LogWarning($"Invalid groups: {invalidGroups}");

            UserInputObj.LoggerObj.LogInformation(3, $"Completed groups: {completedGroups}"); // IsExpressWorkflow ? 30 : 10 % complete

            List<AssessmentInformation> AllAssessments = new List<AssessmentInformation>();

            foreach (KeyValuePair<string, GroupPollResponse> group in GroupStatusMap)
            {
                if (group.Value != GroupPollResponse.Completed)
                    continue;
                if (string.IsNullOrEmpty(group.Key))
                    continue;
                try
                {
                    if (group.Key.Contains("AzureVM"))
                        AllAssessments.AddRange(new AzureVMAssessmentSettingsFactory().GetAzureVMAssessmentSettings(UserInputObj, group.Key));

                    else if (group.Key.Contains("Azure-VMWareSolution"))
                        AllAssessments.AddRange(new AzureVMWareSolutionAssessmentSettingsFactory().GetAzureVMWareSolutionAssessmentSettings(UserInputObj, group.Key));

                    else if (group.Key.Contains("SQL"))
                        AllAssessments.AddRange(new AzureSQLAssessmentSettingsFactory().GetAzureSQLAssessmentSettings(UserInputObj, group.Key));

                    else if (group.Key.Contains("WebApp"))
                        AllAssessments.AddRange(new AzureWebAppAssessmentSettingsFactory().GetAzureWebAppAssessmentSettings(UserInputObj, group.Key));
                }
                catch (Exception exAssessmentFactory)
                {
                    UserInputObj.LoggerObj.LogError($"Retrieval from assessment factory failed: {exAssessmentFactory.Message}");
                }
            }

            if (AllAssessments.Count <= 0)
            {
                UserInputObj.LoggerObj.LogError("Factories returned no assessment settings, terminating process");
                return false;
            }

            UserInputObj.LoggerObj.LogInformation($"Total assessments to be created: {AllAssessments.Count}");

            UserInputObj.LoggerObj.LogInformation("Sorting assessments based on creation priority, assessments which take the most time, will be created first");
            AllAssessments.Sort(CompareAssessmentCreationPriority);

            UserInputObj.LoggerObj.LogInformation($"Initiating Assessment creation");

            Dictionary<AssessmentInformation, AssessmentPollResponse> AssessmentStatusMap = new Dictionary<AssessmentInformation, AssessmentPollResponse>();
            try
            {
                AssessmentStatusMap = new BatchAssessments(AllAssessments).CreateAssessmentsInBatch(UserInputObj);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (AggregateException aeBatchAssessment)
            {
                string errorMessage = "";
                foreach (var e in aeBatchAssessment.Flatten().InnerExceptions)
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

            UserInputObj.LoggerObj.LogInformation($"Retrieved status information for {AssessmentStatusMap.Count} assessments");

            int completedAssessmentsCount = 0;
            int invalidAssessmentsCount = 0;
            int outdatedAssessmentsCount = 0;
            foreach (var kvp in AssessmentStatusMap)
            {
                if (kvp.Value == AssessmentPollResponse.Completed)
                    completedAssessmentsCount += 1;

                else if (kvp.Value == AssessmentPollResponse.Invalid)
                    invalidAssessmentsCount += 1;

                else if (kvp.Value == AssessmentPollResponse.OutDated)
                    outdatedAssessmentsCount += 1;
            }
            if (completedAssessmentsCount <= 0 && outdatedAssessmentsCount <= 0)
            {
                UserInputObj.LoggerObj.LogError($"No assessments in completed or outdated state");
                return false;
            }
            UserInputObj.LoggerObj.LogInformation($"Completed assessments: {completedAssessmentsCount}");
            if (outdatedAssessmentsCount > 0)
                UserInputObj.LoggerObj.LogWarning($"Out-dated assessments: {outdatedAssessmentsCount}");
            if (invalidAssessmentsCount > 0)
                UserInputObj.LoggerObj.LogError($"Invalid assessments: {invalidAssessmentsCount}");

            UserInputObj.LoggerObj.LogInformation(65 - UserInputObj.LoggerObj.GetCurrentProgress(), $"Completed assessment creation job"); // 65 % complete

            Dictionary<AssessmentInformation, AssessmentPollResponse> AzureVMAssessmentStatusMap = new Dictionary<AssessmentInformation, AssessmentPollResponse>();
            Dictionary<AssessmentInformation, AssessmentPollResponse> AzureSQLAssessmentStatusMap = new Dictionary<AssessmentInformation, AssessmentPollResponse>();
            Dictionary<AssessmentInformation, AssessmentPollResponse> AVSAssessmentStatusMap = new Dictionary<AssessmentInformation, AssessmentPollResponse>();
            Dictionary<AssessmentInformation, AssessmentPollResponse> AzureAppServiceWebAppAssessmentStatusMap = new Dictionary<AssessmentInformation, AssessmentPollResponse>();

            UserInputObj.LoggerObj.LogInformation("Sorting different assessment types to create datasets");

            foreach (var kvp in AssessmentStatusMap)
            {
                if (kvp.Key.AssessmentType == AssessmentType.MachineAssessment)
                {
                    if (!AzureVMAssessmentStatusMap.ContainsKey(kvp.Key))
                        AzureVMAssessmentStatusMap.Add(kvp.Key, kvp.Value);
                }

                else if (kvp.Key.AssessmentType == AssessmentType.SQLAssessment)
                {
                    if (!AzureSQLAssessmentStatusMap.ContainsKey(kvp.Key))
                        AzureSQLAssessmentStatusMap.Add(kvp.Key, kvp.Value);
                }

                else if (kvp.Key.AssessmentType == AssessmentType.AVSAssessment)
                {
                    if (!AVSAssessmentStatusMap.ContainsKey(kvp.Key))
                        AVSAssessmentStatusMap.Add(kvp.Key, kvp.Value);
                }

                else if (kvp.Key.AssessmentType == AssessmentType.WebAppAssessment)
                {
                    if (!AzureAppServiceWebAppAssessmentStatusMap.ContainsKey(kvp.Key))
                        AzureAppServiceWebAppAssessmentStatusMap.Add(kvp.Key, kvp.Value);
                }
            }

            if (AzureVMAssessmentStatusMap.Count > 0)
                UserInputObj.LoggerObj.LogInformation($"Total Azure VM Assessments: {AzureVMAssessmentStatusMap.Count}");
            if (AzureSQLAssessmentStatusMap.Count > 0)
                UserInputObj.LoggerObj.LogInformation($"Total Azure SQL Assessments: {AzureSQLAssessmentStatusMap.Count}");
            if (AVSAssessmentStatusMap.Count > 0)
                UserInputObj.LoggerObj.LogInformation($"Total AVS Assessments: {AVSAssessmentStatusMap.Count}");
            if (AzureAppServiceWebAppAssessmentStatusMap.Count > 0)
                UserInputObj.LoggerObj.LogInformation($"Total Web Application Assessments: {AzureAppServiceWebAppAssessmentStatusMap.Count}");

            Dictionary<string, AzureVMPerformanceBasedDataset> AzureVMPerformanceBasedMachinesData = new Dictionary<string, AzureVMPerformanceBasedDataset>();
            Dictionary<string, AzureVMAsOnPremDataset> AzureVMAsOnPremMachinesData = new Dictionary<string, AzureVMAsOnPremDataset>();
            if (AzureVMAssessmentStatusMap.Count > 0)
            {
                ParseAzureVMAssessments(AzureVMPerformanceBasedMachinesData, AzureVMAsOnPremMachinesData, AzureVMAssessmentStatusMap);
            }

            Dictionary<AssessmentInformation, AVSAssessmentPropertiesDataset> AVSAssessmentsData = new Dictionary<AssessmentInformation, AVSAssessmentPropertiesDataset>();
            Dictionary<string, AVSAssessedMachinesDataset> AVSAssessedMachinesData = new Dictionary<string, AVSAssessedMachinesDataset>();
            if (AVSAssessmentStatusMap.Count > 0)
            {
                ParseAVSAssessments(AVSAssessmentsData, AVSAssessedMachinesData, AVSAssessmentStatusMap);
            }

            Dictionary<string, AzureWebAppDataset> AzureWebAppData = new Dictionary<string, AzureWebAppDataset>();
            if (AzureAppServiceWebAppAssessmentStatusMap.Count > 0)
            {
                ParseAzureWebAppAssessments(AzureWebAppData, AzureAppServiceWebAppAssessmentStatusMap);
            }

            Dictionary<string, AzureSQLInstanceDataset> AzureSQLInstancesData = new Dictionary<string, AzureSQLInstanceDataset>();
            Dictionary<string, AzureSQLMachineDataset> AzureSQLMachinesData = new Dictionary<string, AzureSQLMachineDataset>();
            if (AzureSQLAssessmentStatusMap.Count > 0)
            {
                ParseAzureSQLAssessedInstances(AzureSQLInstancesData, AzureSQLAssessmentStatusMap);
                ParseAzureSQLAssessedMachines(AzureSQLMachinesData, AzureSQLAssessmentStatusMap);
            }

            BusinessCaseDataset BusinessCaseData = new BusinessCaseDataset();
            if (bizCaseCompletionResultKvp.Value == AssessmentPollResponse.Completed)
            {
                ParseBusinessCase(bizCaseCompletionResultKvp, BusinessCaseData);
            }

            ProcessDatasets processorObj = new ProcessDatasets
                (
                    AssessmentIdToDiscoveryIdLookup,
                    AzureWebApp_IaaS,
                    SqlServicesVM,
                    GeneralVM,
                    AzureVMPerformanceBasedMachinesData,
                    AzureVMAsOnPremMachinesData,
                    AVSAssessmentsData,
                    AVSAssessedMachinesData,
                    AzureWebAppData,
                    AzureSQLInstancesData,
                    AzureSQLMachinesData,
                    BusinessCaseData,
                    DecommissionedMachinesData,
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

        private void ParseAzureSQLAssessedMachines(Dictionary<string, AzureSQLMachineDataset> AzureSQLMachinesData, Dictionary<AssessmentInformation, AssessmentPollResponse> AzureSQLAssessmentStatusMap)
        {
            UserInputObj.LoggerObj.LogInformation("Initiating parsing for Azure SQL assessed machines");

            try
            {
                new AzureSQLAssessedMachinesParser(AzureSQLAssessmentStatusMap).ParseAssessedSQLMachines(AzureSQLMachinesData, UserInputObj);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (AggregateException aeSqlMachinesParse)
            {
                string errorMessage = "";
                foreach (var e in aeSqlMachinesParse.Flatten().InnerExceptions)
                {
                    if (e is OperationCanceledException)
                        throw e;
                    else
                    {
                        errorMessage = errorMessage + e.Message + " ";
                    }
                }
                UserInputObj.LoggerObj.LogError($"Azure SQL assessed machines parsing error : {errorMessage}");
            }
            catch (Exception exSqlAssessmentParse)
            {
                UserInputObj.LoggerObj.LogError($"Azure SQL assessed machines parsing error {exSqlAssessmentParse.Message}");
            }

            UserInputObj.LoggerObj.LogInformation(85 - UserInputObj.LoggerObj.GetCurrentProgress(), "Azure SQL assessed machines parsing job completed"); // 85 % complete
        }

        private void ParseAzureSQLAssessedInstances(Dictionary<string, AzureSQLInstanceDataset> AzureSQLInstancesData, Dictionary<AssessmentInformation, AssessmentPollResponse> AzureSQLAssessmentStatusMap)
        {
            UserInputObj.LoggerObj.LogInformation("Initiating parsing for Azure SQL assessed instances");
            try
            {
                new AzureSQLAssessedInstancesParser(AzureSQLAssessmentStatusMap).ParseAssessedSQLInstances(AzureSQLInstancesData, UserInputObj);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (AggregateException aeSqlInstancesParse)
            {
                string errorMessage = "";
                foreach (var e in aeSqlInstancesParse.Flatten().InnerExceptions)
                {
                    if (e is OperationCanceledException)
                        throw e;
                    else
                    {
                        errorMessage = errorMessage + e.Message + " ";
                    }
                }
                UserInputObj.LoggerObj.LogError($"Azure SQL assessed instances parsing error : {errorMessage}");
            }
            catch (Exception exSqlAssessmentParse)
            {
                UserInputObj.LoggerObj.LogError($"Azure SQL assessed instances parsing error {exSqlAssessmentParse.Message}");
            }

            UserInputObj.LoggerObj.LogInformation(83 - UserInputObj.LoggerObj.GetCurrentProgress(), "Azure SQL assessed instances parsing job completed"); // 83 %  complete
        }

        private void ParseAzureWebAppAssessments(Dictionary<string, AzureWebAppDataset> AzureWebAppData, Dictionary<AssessmentInformation, AssessmentPollResponse> AzureWebAppAssessmentStatusMap)
        {
            UserInputObj.LoggerObj.LogInformation("Initiating parsing for Azure Web App assessments");
            try
            {
                new AzureWebAppParser(AzureWebAppAssessmentStatusMap).ParseWebAppAssessments(AzureWebAppData, UserInputObj);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (AggregateException aeWebAppAssessmentParse)
            {
                string errorMessage = "";
                foreach (var e in aeWebAppAssessmentParse.Flatten().InnerExceptions)
                {
                    if (e is OperationCanceledException)
                        throw e;
                    else
                    {
                        errorMessage = errorMessage + e.Message + " ";
                    }
                }
                UserInputObj.LoggerObj.LogError($"Azure web app assessment parsing error : {errorMessage}");
            }
            catch (Exception exWebAppAssessmentParse)
            {
                UserInputObj.LoggerObj.LogError($"Azure web app assessment parsing error {exWebAppAssessmentParse.Message}");
            }

            UserInputObj.LoggerObj.LogInformation(80 - UserInputObj.LoggerObj.GetCurrentProgress(), "Azure Web App assessment parsing job completed"); // 80 % complete
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

        private void ParseAzureVMAssessments(Dictionary<string, AzureVMPerformanceBasedDataset> AzureVMPerformanceBasedMachinesData, Dictionary<string, AzureVMAsOnPremDataset> AzureVMAsOnPremMachinesData, Dictionary<AssessmentInformation, AssessmentPollResponse> AzureVMAssessmentStatusMap)
        {
            UserInputObj.LoggerObj.LogInformation("Initiating parsing for AzureVM assessments");
            try
            {
                new AzureVMAssessmentParser(AzureVMAssessmentStatusMap).ParseVMAssessments(AzureVMPerformanceBasedMachinesData, AzureVMAsOnPremMachinesData, UserInputObj);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (AggregateException aeAzureVMAssessmentParse)
            {
                string errorMessage = "";
                foreach (var e in aeAzureVMAssessmentParse.Flatten().InnerExceptions)
                {
                    if (e is OperationCanceledException)
                        throw e;
                    else
                    {
                        errorMessage = errorMessage + e.Message + " ";
                    }
                }
                UserInputObj.LoggerObj.LogError($"Azure VM assessment parsing error : {errorMessage}");
            }
            catch (Exception exAzureVMAssessmentParse)
            {
                UserInputObj.LoggerObj.LogError($"Azure VM assessment parsing error {exAzureVMAssessmentParse.Message}");
            }

            UserInputObj.LoggerObj.LogInformation(70 - UserInputObj.LoggerObj.GetCurrentProgress(), "Azure VM assessment parsing job completed"); // 70 % Complete
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
            var directory = CoreReportConstants.CoreReportDirectory;
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
            var directory = OpportunityReportConstants.OpportunityReportDirectory;
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
            var directory = ClashReportConstants.ClashReportDirectory;
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
        private List<string> ObtainAssessmentMachineIdList(List<AssessmentSiteMachine> assessmentSiteMachines)
        {
            List<string> result = new List<string>();
            foreach (var assessmentSiteMachine in assessmentSiteMachines)
                result.Add(assessmentSiteMachine.AssessmentId);

            return result;
        }

        private static int CompareAssessmentCreationPriority(AssessmentInformation a, AssessmentInformation b)
        {
            if (object.ReferenceEquals(a, b))
                return 0;
            if (a == null)
                return -1;
            if (b == null)
                return 1;

            return a.AssessmentCreationPriority.CompareTo(b.AssessmentCreationPriority);
        }

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
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

using Azure.Migrate.Export.Models;
using Azure.Migrate.Export.HttpRequestHelper;
using Azure.Migrate.Export.Common;

namespace Azure.Migrate.Export.Assessment.Parser
{
    public class AzureWebAppParser
    {
        private readonly Dictionary<AssessmentInformation, AssessmentPollResponse> AzureWebAppAssessmentStatusMap;
        public AzureWebAppParser(Dictionary<AssessmentInformation, AssessmentPollResponse> azureWebAppAssessmentStatusMap)
        {
            AzureWebAppAssessmentStatusMap = azureWebAppAssessmentStatusMap;
        }

        public void ParseWebAppAssessments(Dictionary<string, AzureWebAppDataset> AzureWebAppData, UserInput userInputObj)
        {
            if (userInputObj == null)
                throw new Exception("Received null user input object.");
            
            if (AzureWebAppAssessmentStatusMap == null)
            {
                userInputObj.LoggerObj.LogError("Azure web app asessment status map is null, terminating parsing");
                return;
            }

            if (AzureWebAppAssessmentStatusMap.Count <= 0)
            {
                userInputObj.LoggerObj.LogError("Azure web app assessment status map is empty, terminating parsing");
                return;
            }

            foreach (var kvp in AzureWebAppAssessmentStatusMap)
            {
                if (!UtilityFunctions.IsAssessmentCompleted(kvp))
                {
                    userInputObj.LoggerObj.LogWarning($"Skipping parsing assessment {kvp.Key.AssessmentName} as it is in {new EnumDescriptionHelper().GetEnumDescription(kvp.Value)} state");
                    continue;
                }

                userInputObj.LoggerObj.LogInformation($"Parsing Azure Web App assessment {kvp.Key.AssessmentName}");

                string url = Routes.ProtocolScheme + Routes.AzureManagementApiHostname + Routes.ForwardSlash +
                             Routes.SubscriptionPath + Routes.ForwardSlash + userInputObj.Subscription.Key + Routes.ForwardSlash +
                             Routes.ResourceGroupPath + Routes.ForwardSlash + userInputObj.ResourceGroupName.Value + Routes.ForwardSlash +
                             Routes.ProvidersPath + Routes.ForwardSlash + Routes.MigrateProvidersPath + Routes.ForwardSlash +
                             Routes.AssessmentProjectsPath + Routes.ForwardSlash + userInputObj.AssessmentProjectName + Routes.ForwardSlash +
                             Routes.GroupsPath + Routes.ForwardSlash + kvp.Key.GroupName + Routes.ForwardSlash +
                             new EnumDescriptionHelper().GetEnumDescription(kvp.Key.AssessmentType) + Routes.ForwardSlash + kvp.Key.AssessmentName +
                             Routes.QueryStringQuestionMark +
                             Routes.QueryParameterApiVersion + Routes.QueryStringEquals + Routes.AssessmentMachineListApiVersion;
                
                string assessmentPropertiesResponse = "";
                try
                {
                    assessmentPropertiesResponse = new HttpClientHelper().GetHttpRequestJsonStringResponse(url, userInputObj).Result;
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (AggregateException aeWebAppAssessmentProperty)
                {
                    string errorMessage = "";
                    foreach (var e in aeWebAppAssessmentProperty.Flatten().InnerExceptions)
                    {
                        if (e is OperationCanceledException)
                            throw e;
                        else
                        {
                            errorMessage = errorMessage + e.Message + " ";
                        }
                    }
                    userInputObj.LoggerObj.LogError($"Failed to retrieve Azure web app assessment properties for assessment: {errorMessage}");
                    url = "";
                    continue;
                }
                catch (Exception exWebAppProperties)
                {
                    userInputObj.LoggerObj.LogError($"Failed to retrieve Azure web app assessment properties for assessment: {exWebAppProperties.Message}");
                    url = "";
                    continue;
                }

                AzureAppServiceWebAppPropertiesJSON webAppPropertiesObj = JsonConvert.DeserializeObject<AzureAppServiceWebAppPropertiesJSON>(assessmentPropertiesResponse);
                double monthlyCostEstimate = webAppPropertiesObj.Properties.MonthlyCost;
                double monthlySecurityCostEstimate = webAppPropertiesObj.Properties.MonthlySecurityCost;

                url = Routes.ProtocolScheme + Routes.AzureManagementApiHostname + Routes.ForwardSlash +
                      Routes.SubscriptionPath + Routes.ForwardSlash + userInputObj.Subscription.Key + Routes.ForwardSlash +
                      Routes.ResourceGroupPath + Routes.ForwardSlash + userInputObj.ResourceGroupName.Value + Routes.ForwardSlash +
                      Routes.ProvidersPath + Routes.ForwardSlash + Routes.MigrateProvidersPath + Routes.ForwardSlash +
                      Routes.AssessmentProjectsPath + Routes.ForwardSlash + userInputObj.AssessmentProjectName + Routes.ForwardSlash +
                      Routes.GroupsPath + Routes.ForwardSlash + kvp.Key.GroupName + Routes.ForwardSlash +
                      new EnumDescriptionHelper().GetEnumDescription(kvp.Key.AssessmentType) + Routes.ForwardSlash + kvp.Key.AssessmentName + Routes.ForwardSlash +
                      Routes.AzureAppServiceAssessedWebAppsPath +
                      Routes.QueryStringQuestionMark +
                      Routes.QueryParameterApiVersion + Routes.QueryStringEquals + Routes.AssessmentMachineListApiVersion;

                while (!string.IsNullOrEmpty(url))
                {
                    if (userInputObj.CancellationContext.IsCancellationRequested)
                        UtilityFunctions.InitiateCancellation(userInputObj);
                    
                    string assessedWebAppsResponse = "";
                    try
                    {
                        assessedWebAppsResponse = new HttpClientHelper().GetHttpRequestJsonStringResponse(url, userInputObj).Result;
                    }
                    catch (OperationCanceledException)
                    {
                        throw;
                    }
                    catch (AggregateException aeAssessedWebApps)
                    {
                        string errorMessage = "";
                        foreach (var e in aeAssessedWebApps.Flatten().InnerExceptions)
                        {
                            if (e is OperationCanceledException)
                                throw e;
                            else
                            {
                                errorMessage = errorMessage + e.Message + " ";
                            }
                        }
                        userInputObj.LoggerObj.LogError($"Failed to retrieve web app data from assessment: {errorMessage}");
                        url = "";
                        continue;
                    }
                    catch (Exception exAssessedWebApps)
                    {
                        userInputObj.LoggerObj.LogError($"Failed to retrieve web app data from assessment: {exAssessedWebApps.Message}");
                        url = "";
                        continue;
                    }

                    AzureAppServiceAssessedWebAppsJSON obj = JsonConvert.DeserializeObject<AzureAppServiceAssessedWebAppsJSON>(assessedWebAppsResponse);
                    url = obj.NextLink;

                    foreach (var value in obj.Values)
                    {
                        string key = value.Properties.DiscoveredWebAppId?.ToLower();
                        UpdateAssessedWebAppDataset(AzureWebAppData, value, key, kvp.Key);

                        AzureWebAppData[key].MonthlySecurityCost = monthlySecurityCostEstimate;

                        if (kvp.Key.AssessmentTag == AssessmentTag.PerformanceBased)
                            AzureWebAppData[key].EstimatedComputeCost = monthlyCostEstimate;
                        else if (kvp.Key.AssessmentTag == AssessmentTag.PerformanceBased_RI3year)
                            AzureWebAppData[key].EstimatedComputeCost_RI3year = monthlyCostEstimate;
                        else if (kvp.Key.AssessmentTag == AssessmentTag.PerformanceBased_ASP3year)
                            AzureWebAppData[key].EstimatedComputeCost_ASP3year = monthlyCostEstimate;
                    }
                }
            }
        }

        private void UpdateAssessedWebAppDataset(Dictionary<string, AzureWebAppDataset> AzureWebAppData, AzureAppServiceAssessedWebAppValue value, string key, AssessmentInformation assessmentInfo)
        {
            if (AzureWebAppData.ContainsKey(key))
                return;
            
            AzureWebAppData.Add(key, new AzureWebAppDataset());

            AzureWebAppData[key].MachineName = value.Properties.MachineName;
            AzureWebAppData[key].DiscoveredMachineId = value.Properties.DiscoveredMachineId?.ToLower();
            AzureWebAppData[key].DiscoveredWebAppId = value.Properties.DiscoveredWebAppId;
            AzureWebAppData[key].WebAppName = value.Properties.WebAppName;

            if (assessmentInfo.AssessmentName.Contains("Dev"))
                AzureWebAppData[key].Environment = "Dev";
            else if (assessmentInfo.AssessmentName.Contains("Prod"))
                AzureWebAppData[key].Environment = "Prod";
            
            var assessmentResult = value.Properties.TargetSpecificResult != null && value.Properties.TargetSpecificResult.Count > 0
                ? value.Properties.TargetSpecificResult.Values.FirstOrDefault()?.AssessmentResult
                : null;
            AzureWebAppData[key].Suitability = (Suitabilities)(assessmentResult?.Suitability);
            
            var migrationIssues = new List<AzureAppServiceAssessedWebAppMigrationIssueInfo>();
            if (value.Properties.TargetSpecificResult != null)
            {
                foreach (var tsr in value.Properties.TargetSpecificResult.Values)
                {
                    if (tsr.MigrationIssues != null)
                        migrationIssues.AddRange(tsr.MigrationIssues);
                }
            }
            AzureWebAppData[key].MigrationIssues = GetAssessedMigrationIssueList(migrationIssues);
            AzureWebAppData[key].AppServicePlanName = assessmentResult?.AppServicePlanName;
            AzureWebAppData[key].WebAppSkuName = assessmentResult?.WebAppSkuName;
            AzureWebAppData[key].GroupName = assessmentInfo.GroupName;
        }

        #region Utilities
        private List<AssessedMigrationIssue> GetAssessedMigrationIssueList(List<AzureAppServiceAssessedWebAppMigrationIssueInfo> migrationIssues)
        {
            List<AssessedMigrationIssue> migrationIssueList = new List<AssessedMigrationIssue>();
            
            foreach (var migrationIssue in migrationIssues)
            {
                AssessedMigrationIssue obj = new AssessedMigrationIssue();
                obj.IssueId = migrationIssue.IssueId;
                obj.IssueCategory = migrationIssue.IssueCategory;
                obj.IssueDescriptionList = migrationIssue.IssueDescriptionList;

                migrationIssueList.Add(obj);
            }

            return migrationIssueList;
        }
        #endregion
    }
}
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

using Azure.Migrate.Explore.Common;
using Azure.Migrate.Explore.HttpRequestHelper;
using Azure.Migrate.Explore.Models;

namespace Azure.Migrate.Explore.Assessment.Parser
{
    public class BusinessCaseParser
    {
        private readonly KeyValuePair<BusinessCaseInformation, AssessmentPollResponse> BizCaseCompletionResult;

        public BusinessCaseParser(KeyValuePair<BusinessCaseInformation, AssessmentPollResponse> bizCaseCompletionResult)
        {
            BizCaseCompletionResult = bizCaseCompletionResult;
        }

        public void ParseBusinessCase(UserInput userInputObj, BusinessCaseDataset BusinessCaseData)
        {
            if (userInputObj == null)
                throw new Exception("Received null user input object");

            string commonUrl = Routes.ProtocolScheme + Routes.AzureManagementApiHostname + Routes.ForwardSlash +
                               Routes.SubscriptionPath + Routes.ForwardSlash + userInputObj.Subscription.Key + Routes.ForwardSlash +
                               Routes.ResourceGroupPath + Routes.ForwardSlash + userInputObj.ResourceGroupName.Value + Routes.ForwardSlash +
                               Routes.ProvidersPath + Routes.ForwardSlash + Routes.MigrateProvidersPath + Routes.ForwardSlash +
                               Routes.AssessmentProjectsPath + Routes.ForwardSlash + userInputObj.AssessmentProjectName + Routes.ForwardSlash +
                               Routes.BusinessCasesPath + Routes.ForwardSlash + BizCaseCompletionResult.Key.BusinessCaseName + Routes.ForwardSlash +
                               "{BusinessCaseSummariesPath}" + Routes.QueryStringQuestionMark +
                               Routes.QueryParameterApiVersion + Routes.QueryStringEquals + Routes.BusinessCaseApiVersion;

            string overviewSummariesUrl = commonUrl.Replace("{BusinessCaseSummariesPath}", Routes.BusinessCaseOverviewSummariesPath + Routes.ForwardSlash + Routes.DefaultPath);
            BusinessCaseOverviewSummaryJSON bizCaseOverviewSummariesJsonObj = ParseBusinessCaseOverviewSummaries(overviewSummariesUrl, userInputObj);

            UpdateBusinessCaseDataset(bizCaseOverviewSummariesJsonObj,
                                      BusinessCaseData,
                                      userInputObj);
        }

        private void UpdateBusinessCaseDataset(BusinessCaseOverviewSummaryJSON bizCaseOverviewSummariesJsonObj, BusinessCaseDataset BusinessCaseData, UserInput userInputObj)
        {
            if (bizCaseOverviewSummariesJsonObj == null)
            {
                userInputObj.LoggerObj.LogWarning("Business case information not parsed successfully, dataset may not be complete");
                return;
            }

            BusinessCaseData.ApplicationSummaryCostDetails = ConvertFromBusinessCaseCostDetailsJSON(bizCaseOverviewSummariesJsonObj.Properties.ApplicationSummary);
            BusinessCaseData.CotsApplicationSummaryCostDetails = ConvertFromBusinessCaseCostDetailsJSON(bizCaseOverviewSummariesJsonObj.Properties.CotsApplicationSummary);
            BusinessCaseData.CustomApplicationSummaryCostDetails = ConvertFromBusinessCaseCostDetailsJSON(bizCaseOverviewSummariesJsonObj.Properties.CustomApplicationSummary);
            BusinessCaseData.IndependentWorkloadsSummaryCostDetails = ConvertFromBusinessCaseCostDetailsJSON(bizCaseOverviewSummariesJsonObj.Properties.IndependentWorkloadsSummary);
            BusinessCaseData.TotalAzureCostDetails = ConvertFromBusinessCaseCostDetailsBreakupJSON(bizCaseOverviewSummariesJsonObj.Properties.TotalAzureCostDetails);
            BusinessCaseData.TotalOnPremisesCostDetails = ConvertFromBusinessCaseCostDetailsBreakupJSON(bizCaseOverviewSummariesJsonObj.Properties.TotalOnPremisesCostDetails);
            BusinessCaseData.azureArcEnabledOnPremisesCostDetails = ConvertFromBusinessCaseCostDetailsBreakupJSON(bizCaseOverviewSummariesJsonObj.Properties.AzureArcEnabledOnPremisesCostDetails);
            BusinessCaseData.futureAzureArcEnabledOnPremisesCostDetails = ConvertFromBusinessCaseCostDetailsBreakupJSON(bizCaseOverviewSummariesJsonObj.Properties.FutureAzureArcEnabledOnPremisesCostDetails);
            BusinessCaseData.futureCostDetails = ConvertFromBusinessCaseCostDetailsBreakupJSON(bizCaseOverviewSummariesJsonObj.Properties.FutureCostDetails);
            BusinessCaseData.WindowsAhubSavings = bizCaseOverviewSummariesJsonObj.Properties.WindowsAHUBSavings ?? 0;
            BusinessCaseData.LinuxAhubSavings = bizCaseOverviewSummariesJsonObj.Properties.LinuxAHUBSavings ?? 0;
            BusinessCaseData.SqlAhubSavings = bizCaseOverviewSummariesJsonObj.Properties.SqlAHUBSavings ?? 0;
            BusinessCaseData.SqlAzureCost = bizCaseOverviewSummariesJsonObj.Properties.SqlAzureCost ?? 0;
            BusinessCaseData.MachineAzureCost = bizCaseOverviewSummariesJsonObj.Properties.MachineAzureCost ?? 0;
            BusinessCaseData.AzureArcEnabledOnPremisesCost = bizCaseOverviewSummariesJsonObj.Properties.AzureArcEnabledOnPremisesCost ?? 0;
            BusinessCaseData.FutureCostIncludingAzureArc = bizCaseOverviewSummariesJsonObj.Properties.FutureCostIncludingAzureArc ?? 0;
            BusinessCaseData.FutureEsuSavingsFor4YearsIncludingAzureArc = bizCaseOverviewSummariesJsonObj.Properties.FutureEsuSavingsFor4YearsIncludingAzureArc ?? 0;
            BusinessCaseData.FutureManagementCostSavingsIncludingAzureArc = bizCaseOverviewSummariesJsonObj.Properties.FutureManagementCostSavingsIncludingAzureArc ?? 0;
            BusinessCaseData.FutureSecurityCostSavingsIncludingAzureArc = bizCaseOverviewSummariesJsonObj.Properties.FutureSecurityCostSavingsIncludingAzureArc ?? 0;
            BusinessCaseData.AzureArcServicesCost = bizCaseOverviewSummariesJsonObj.Properties.AzureArcServicesCost ?? 0;
            BusinessCaseData.FutureAzureArcEnabledOnPremisesCost = bizCaseOverviewSummariesJsonObj.Properties.FutureAzureArcEnabledOnPremisesCost ?? 0;
            BusinessCaseData.FutureAzureArcServicesCost = bizCaseOverviewSummariesJsonObj.Properties.FutureAzureArcServicesCost ?? 0;
            BusinessCaseData.TotalYOYCashFlowsAndEmissions = bizCaseOverviewSummariesJsonObj.Properties.YearOnYearEstimates;
            BusinessCaseData.TotalAzureSustainabilityDetails = bizCaseOverviewSummariesJsonObj.Properties.TotalAzureSustainabilityDetails;
            BusinessCaseData.TotalOnPremisesSustainabilityDetails = bizCaseOverviewSummariesJsonObj.Properties.TotalOnPremisesSustainabilityDetails;
        }

        private BusinessCaseDatasetCostDetails ConvertFromBusinessCaseCostDetailsJSON(BusinessCaseCostDetailsJSON bizCaseCostDetailsJsonObj)
        {
            if (bizCaseCostDetailsJsonObj == null)
                return new BusinessCaseDatasetCostDetails();

            BusinessCaseDatasetCostDetails bizCaseDatasetCostDetails = new BusinessCaseDatasetCostDetails
            {
                AzureCost = ConvertFromBusinessCaseCostDetailsBreakupJSON(bizCaseCostDetailsJsonObj.AzureCost),
                OnPremCost = ConvertFromBusinessCaseCostDetailsBreakupJSON(bizCaseCostDetailsJsonObj.OnPremCost)
            };

            return bizCaseDatasetCostDetails;
        }

        private BusinessCaseDatasetCostDetailsBreakup ConvertFromBusinessCaseCostDetailsBreakupJSON(BusinessCaseCostDetailsBreakupJSON bizCaseCostDetailsBreakupJsonObj)
        {
            if (bizCaseCostDetailsBreakupJsonObj == null)
                return new BusinessCaseDatasetCostDetailsBreakup();

            BusinessCaseDatasetCostDetailsBreakup bizCaseDatasetCostDetailsBreakup = new BusinessCaseDatasetCostDetailsBreakup
            {
                TotalCost = bizCaseCostDetailsBreakupJsonObj.TotalCost ?? 0,
                StorageCost = bizCaseCostDetailsBreakupJsonObj.StorageCost ?? 0,
                ComputeCost = bizCaseCostDetailsBreakupJsonObj.ComputeCost ?? 0,
                ITLaborCost = bizCaseCostDetailsBreakupJsonObj.ITLaborCost ?? 0,
                NetworkCost = bizCaseCostDetailsBreakupJsonObj.NetworkCost ?? 0,
                AhubSavings = bizCaseCostDetailsBreakupJsonObj.AHUBSavings ?? 0,
                EsuSavings = bizCaseCostDetailsBreakupJsonObj.ESUSavings ?? 0,
                SecurityCost = bizCaseCostDetailsBreakupJsonObj.SecurityCost ?? 0,
                FacilitiesCost = bizCaseCostDetailsBreakupJsonObj.FacilitiesCost ?? 0,
                ManagementCost = bizCaseCostDetailsBreakupJsonObj.ManagementCostDetails != null ?
                                 bizCaseCostDetailsBreakupJsonObj.ManagementCostDetails.ManagementCost ?? 0 : 0,
                LicenseCost = bizCaseCostDetailsBreakupJsonObj.LicenseCostDetails != null ?
                              bizCaseCostDetailsBreakupJsonObj.LicenseCostDetails.LicenseCost ?? 0 : 0,
                LinuxAhubSavings = bizCaseCostDetailsBreakupJsonObj.LinuxAHUBSavings ?? 0
            };

            return bizCaseDatasetCostDetailsBreakup;
        }

        private BusinessCaseOverviewSummaryJSON ParseBusinessCaseOverviewSummaries(string url, UserInput userInputObj)
        {
            string response = GetJsonResponse(url, userInputObj);

            if (string.IsNullOrEmpty(response))
            {
                userInputObj.LoggerObj.LogWarning($"Received empty response for business case url: {url}");
                return null;
            }

            return JsonConvert.DeserializeObject<BusinessCaseOverviewSummaryJSON>(response);
        }

        private string GetJsonResponse(string url, UserInput userInputObj, bool isPost = false)
        {
            string response = "";
            try
            {
                response = new HttpClientHelper().GetHttpRequestJsonStringResponse(url, userInputObj, isPost).Result;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (AggregateException aeBusinessCase)
            {
                string errorMessage = "";
                foreach (var e in aeBusinessCase.Flatten().InnerExceptions)
                {
                    if (e is OperationCanceledException)
                        throw e;
                    else
                    {
                        errorMessage = errorMessage + e.Message + " ";
                    }
                }
                userInputObj.LoggerObj.LogWarning($"Failed parsing business case url {url}: {errorMessage}");
            }
            catch (Exception exBusinessCase)
            {
                userInputObj.LoggerObj.LogWarning($"Failed parsing business case url {url}: {exBusinessCase.Message}");
            }

            return response;
        }
    }
}
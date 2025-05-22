using Newtonsoft.Json;
using System;
using System.Collections.Generic;

using Azure.Migrate.Export.Common;
using Azure.Migrate.Export.HttpRequestHelper;
using Azure.Migrate.Export.Models;

namespace Azure.Migrate.Export.Assessment.Parser
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

            BusinessCaseIaaSSummaryJSON bizCaseIaasSummariesJsonObj = null;
            BusinessCasePaaSSummaryJSON bizCasePaasSummariesJsonObj = null;
            BusinessCaseAvsSummaryJSON bizCaseAvsSummariesJsonObj = null;

            string compareSummaryUrl = commonUrl.Replace("{BusinessCaseSummariesPath}", Routes.BusinessCaseCompareSummariesPath);
            BusinessCaseCompareSummaryJSON bizCaseCompareSummaryJsonObj = ParseBusinessCaseCompareSummary(compareSummaryUrl, userInputObj);

            string overviewSummariesUrl = commonUrl.Replace("{BusinessCaseSummariesPath}", Routes.BusinessCaseOverviewSummariesPath + Routes.ForwardSlash + Routes.DefaultPath);
            BusinessCaseOverviewSummaryJSON bizCaseOverviewSummariesJsonObj = ParseBusinessCaseOverviewSummaries(overviewSummariesUrl, userInputObj);

            if (userInputObj.BusinessProposal == BusinessProposal.AVS.ToString())
            {
                string avsSummariesUrl = commonUrl.Replace("{BusinessCaseSummariesPath}", Routes.BusinessCaseAvsSummariesPath + Routes.ForwardSlash + Routes.DefaultPath);
                bizCaseAvsSummariesJsonObj = ParseBusinessCaseAvsSummaries(avsSummariesUrl, userInputObj);
            }
            else
            {
                string iaasSummariesUrl = commonUrl.Replace("{BusinessCaseSummariesPath}", Routes.BusinessCaseIaasSummariesPath + Routes.ForwardSlash + Routes.DefaultPath);
                bizCaseIaasSummariesJsonObj = ParseBusinessCaseIaasSummaries(iaasSummariesUrl, userInputObj);

                string paasSummariesUrl = commonUrl.Replace("{BusinessCaseSummariesPath}", Routes.BusinessCasePaasSummariesPath + Routes.ForwardSlash + Routes.DefaultPath);
                bizCasePaasSummariesJsonObj = ParseBusinessCasePaasSummaries(paasSummariesUrl, userInputObj);
            }            

            UpdateBusinessCaseDataset(bizCaseCompareSummaryJsonObj,
                                      bizCaseOverviewSummariesJsonObj,
                                      bizCaseIaasSummariesJsonObj,
                                      bizCasePaasSummariesJsonObj,
                                      bizCaseAvsSummariesJsonObj,
                                      BusinessCaseData,
                                      userInputObj);
        }

        private void UpdateBusinessCaseDataset(BusinessCaseCompareSummaryJSON bizCaseCompareSummaryJsonObj,
                                               BusinessCaseOverviewSummaryJSON bizCaseOverviewSummariesJsonObj,
                                               BusinessCaseIaaSSummaryJSON bizCaseIaaSSummariesJsonObj,
                                               BusinessCasePaaSSummaryJSON bizCasePaaSSummariesJsonObj,
                                               BusinessCaseAvsSummaryJSON bizCaseAvsSummariesJsonObj,
                                               BusinessCaseDataset BusinessCaseData,
                                               UserInput userInputObj)
        {
            if ((bizCaseCompareSummaryJsonObj == null || bizCaseOverviewSummariesJsonObj == null) ||
                (userInputObj.BusinessProposal == BusinessProposal.AVS.ToString() && bizCaseAvsSummariesJsonObj == null) ||
                (userInputObj.BusinessProposal != BusinessProposal.AVS.ToString() && (bizCasePaaSSummariesJsonObj == null || bizCaseIaaSSummariesJsonObj == null)))
            {
                userInputObj.LoggerObj.LogWarning("Business case information not parsed successfully, dataset may not be complete");
                return;
            }

            BusinessCaseData.OnPremIaaSCostDetails.ComputeLicenseCost = bizCaseCompareSummaryJsonObj.OnPremisesIaaSCostDetails?.ComputeCost ?? 0.00;
            BusinessCaseData.OnPremIaaSCostDetails.EsuLicenseCost = bizCaseCompareSummaryJsonObj.OnPremisesIaaSCostDetails?.ESUSavings ?? 0.00;
            BusinessCaseData.OnPremIaaSCostDetails.StorageCost = bizCaseCompareSummaryJsonObj.OnPremisesIaaSCostDetails?.StorageCost ?? 0.00;
            BusinessCaseData.OnPremIaaSCostDetails.NetworkCost = bizCaseCompareSummaryJsonObj.OnPremisesIaaSCostDetails?.NetworkCost ?? 0.00;
            BusinessCaseData.OnPremIaaSCostDetails.SecurityCost = bizCaseCompareSummaryJsonObj.OnPremisesIaaSCostDetails?.SecurityCost ?? 0.00;
            BusinessCaseData.OnPremIaaSCostDetails.ITStaffCost = bizCaseCompareSummaryJsonObj.OnPremisesIaaSCostDetails?.ITLaborCost ?? 0.00;
            BusinessCaseData.OnPremIaaSCostDetails.FacilitiesCost = bizCaseCompareSummaryJsonObj.OnPremisesIaaSCostDetails?.FacilitiesCost ?? 0.00;

            BusinessCaseData.OnPremPaaSCostDetails.ComputeLicenseCost = bizCaseCompareSummaryJsonObj.OnPremisesPaaSCostDetails?.ComputeCost ?? 0.00;
            BusinessCaseData.OnPremPaaSCostDetails.EsuLicenseCost = bizCaseCompareSummaryJsonObj.OnPremisesPaaSCostDetails?.ESUSavings ?? 0.00;
            BusinessCaseData.OnPremPaaSCostDetails.StorageCost = bizCaseCompareSummaryJsonObj.OnPremisesPaaSCostDetails?.StorageCost ?? 0.00;
            BusinessCaseData.OnPremPaaSCostDetails.NetworkCost = bizCaseCompareSummaryJsonObj.OnPremisesPaaSCostDetails?.NetworkCost ?? 0.00;
            BusinessCaseData.OnPremPaaSCostDetails.SecurityCost = bizCaseCompareSummaryJsonObj.OnPremisesPaaSCostDetails?.SecurityCost ?? 0.00;
            BusinessCaseData.OnPremPaaSCostDetails.ITStaffCost = bizCaseCompareSummaryJsonObj.OnPremisesPaaSCostDetails?.ITLaborCost ?? 0.00;
            BusinessCaseData.OnPremPaaSCostDetails.FacilitiesCost = bizCaseCompareSummaryJsonObj.OnPremisesPaaSCostDetails?.FacilitiesCost ?? 0.00;

            BusinessCaseData.OnPremAvsCostDetails.ComputeLicenseCost = bizCaseCompareSummaryJsonObj.OnPremisesAvsCostDetails?.ComputeCost ?? 0.00;
            BusinessCaseData.OnPremAvsCostDetails.EsuLicenseCost = bizCaseCompareSummaryJsonObj.OnPremisesAvsCostDetails?.ESUSavings ?? 0.00;
            BusinessCaseData.OnPremAvsCostDetails.StorageCost = bizCaseCompareSummaryJsonObj.OnPremisesAvsCostDetails?.StorageCost ?? 0.00;
            BusinessCaseData.OnPremAvsCostDetails.NetworkCost = bizCaseCompareSummaryJsonObj.OnPremisesAvsCostDetails?.NetworkCost ?? 0.00;
            BusinessCaseData.OnPremAvsCostDetails.SecurityCost = bizCaseCompareSummaryJsonObj.OnPremisesAvsCostDetails?.SecurityCost ?? 0.00;
            BusinessCaseData.OnPremAvsCostDetails.ITStaffCost = bizCaseCompareSummaryJsonObj.OnPremisesAvsCostDetails?.ITLaborCost ?? 0.00;
            BusinessCaseData.OnPremAvsCostDetails.FacilitiesCost = bizCaseCompareSummaryJsonObj.OnPremisesAvsCostDetails?.FacilitiesCost ?? 0.00;

            BusinessCaseData.AzureIaaSCostDetails.ITStaffCost = bizCaseCompareSummaryJsonObj.AzureIaaSCostDetails?.ITLaborCost ?? 0.00;
            BusinessCaseData.AzurePaaSCostDetails.ITStaffCost = bizCaseCompareSummaryJsonObj.AzurePaaSCostDetails?.ITLaborCost ?? 0.00;

            BusinessCaseData.AzureAvsCostDetails.ComputeLicenseCost = bizCaseCompareSummaryJsonObj.AzureAvsCostDetails?.ComputeCost ?? 0.00;
            BusinessCaseData.AzureAvsCostDetails.EsuLicenseCost = bizCaseCompareSummaryJsonObj.AzureAvsCostDetails?.ESUSavings ?? 0.00;
            BusinessCaseData.AzureAvsCostDetails.StorageCost = bizCaseCompareSummaryJsonObj.AzureAvsCostDetails?.StorageCost ?? 0.00;
            BusinessCaseData.AzureAvsCostDetails.NetworkCost = bizCaseCompareSummaryJsonObj.AzureAvsCostDetails?.NetworkCost ?? 0.00;
            BusinessCaseData.AzureAvsCostDetails.SecurityCost = bizCaseCompareSummaryJsonObj.AzureAvsCostDetails?.SecurityCost ?? 0.00;
            BusinessCaseData.AzureAvsCostDetails.ITStaffCost = bizCaseCompareSummaryJsonObj.AzureAvsCostDetails?.ITLaborCost ?? 0.00;

            BusinessCaseData.WindowsServerLicense.ComputeLicenseCost = bizCaseOverviewSummariesJsonObj.Properties?.WindowsAhubSavings ?? 0.00;
            BusinessCaseData.SqlServerLicense.ComputeLicenseCost = bizCaseOverviewSummariesJsonObj.Properties?.SqlAhubSavings ?? 0.00;
            BusinessCaseData.EsuSavings.ComputeLicenseCost = bizCaseOverviewSummariesJsonObj.Properties?.EsuSavingsFor4years ?? 0.00;

            BusinessCaseData.TotalYOYCashFlowsAndEmissions = bizCaseOverviewSummariesJsonObj.Properties.YearOnYearEstimates;
            BusinessCaseData.TotalAzureSustainabilityDetails = bizCaseOverviewSummariesJsonObj.Properties.TotalAzureSustainabilityDetails;
            BusinessCaseData.TotalOnPremisesSustainabilityDetails = bizCaseOverviewSummariesJsonObj.Properties.TotalOnPremisesSustainabilityDetails;
            if (userInputObj.BusinessProposal == BusinessProposal.AVS.ToString())
                BusinessCaseData.AvsYOYCashFlows = bizCaseAvsSummariesJsonObj.Properties.AzureAvsSummary.YearOnYearEstimates;
            else
                BusinessCaseData.IaaSYOYCashFlows = bizCaseIaaSSummariesJsonObj.Properties.AzureIaaSSummary.YearOnYearEstimates;
        }

        private BusinessCasePaaSSummaryJSON ParseBusinessCasePaasSummaries(string url, UserInput userInputObj)
        {
            string response = GetJsonResponse(url, userInputObj);

            if (string.IsNullOrEmpty(response))
            {
                userInputObj.LoggerObj.LogWarning($"Received empty response for business case url: {url}");
                return null;
            }

            return JsonConvert.DeserializeObject<BusinessCasePaaSSummaryJSON>(response);
        }

        private BusinessCaseIaaSSummaryJSON ParseBusinessCaseIaasSummaries(string url, UserInput userInputObj)
        {
            string response = GetJsonResponse(url, userInputObj);

            if (string.IsNullOrEmpty(response))
            {
                userInputObj.LoggerObj.LogWarning($"Received empty response for business case url: {url}");
                return null;
            }

            return JsonConvert.DeserializeObject<BusinessCaseIaaSSummaryJSON>(response);
        }

        private BusinessCaseAvsSummaryJSON ParseBusinessCaseAvsSummaries(string url, UserInput userInputObj)
        {
            string response = GetJsonResponse(url, userInputObj);

            if (string.IsNullOrEmpty(response))
            {
                userInputObj.LoggerObj.LogWarning($"Received empty response for business case url: {url}");
                return null;
            }

            return JsonConvert.DeserializeObject<BusinessCaseAvsSummaryJSON>(response);
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

        private BusinessCaseCompareSummaryJSON ParseBusinessCaseCompareSummary(string url, UserInput userInputObj)
        {
            string response = GetJsonResponse(url, userInputObj, true);

            if (string.IsNullOrEmpty(response))
            {
                userInputObj.LoggerObj.LogWarning($"Received empty response for business case url: {url}");
                return null;
            }

            return JsonConvert.DeserializeObject<BusinessCaseCompareSummaryJSON>(response);
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
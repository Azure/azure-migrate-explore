// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using Microsoft.Identity.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Azure.Migrate.Explore.Authentication;
using Azure.Migrate.Explore.Common;
using Azure.Migrate.Explore.Models;

namespace Azure.Migrate.Explore.HttpRequestHelper
{
    public class HttpClientHelper
    {
        int NumberOfTries;

        public HttpClientHelper()
        {
            NumberOfTries = 0;
        }

        #region Project details
        public async Task<JToken> GetProjectDetailsHttpJsonResponse(string Url)
        {
            HttpResponseMessage response = await GetProjectDetailsHttpResponse(Url);

            if (response == null)
                throw new Exception("Could not obtain a HTTP response.");

            else if (!response.IsSuccessStatusCode)
            {
                string responseContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Error getting HTTP response: {response.StatusCode}: {responseContent}");
            }

            string responseJson = await response.Content.ReadAsStringAsync();
            dynamic responseObject = JsonConvert.DeserializeObject(responseJson);
            return JToken.Parse(responseObject.ToString());
        }

        private async Task<HttpResponseMessage> GetProjectDetailsHttpResponse(string Url)
        {
            NumberOfTries++;
            AuthenticationResult authResult = null;
            HttpResponseMessage response;
            bool isException = false;
            try
            {
                authResult = await AzureAuthenticationHandler.RetrieveAuthenticationToken();
            }
            catch (Exception exCacheTokenRetrieval)
            {
                throw new Exception($"Cached token retrieval failed: {exCacheTokenRetrieval.Message} Please re-login");
            }

            try
            {
                HttpClient httpClient = new HttpClient()
                {
                    Timeout = TimeSpan.FromSeconds(45),
                };

                Uri baseAddress = new Uri(Url);
                string clientRequestId = Guid.NewGuid().ToString();

                httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + authResult.AccessToken);
                httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
                httpClient.DefaultRequestHeaders.Add("x-ms-client-request-id", clientRequestId + "-azmigexp");

                response = await httpClient.GetAsync(baseAddress);
            }
            catch (Exception exProjectDetailsHttpRequest)
            {
                isException = true;
                if (NumberOfTries < HttpUtilities.MaxProjectDetailsRetries && HttpUtilities.IsRetryNeeded(null, exProjectDetailsHttpRequest))
                {
                    Thread.Sleep(10000);
                    response = await GetProjectDetailsHttpResponse(Url);
                }
                else
                    throw;
            }

            if (!isException)
            {
                if ((response == null || !response.IsSuccessStatusCode) && HttpUtilities.IsRetryNeeded(response, null) && NumberOfTries < HttpUtilities.MaxProjectDetailsRetries)
                {
                    Thread.Sleep(10000);
                    response = await GetProjectDetailsHttpResponse(Url);
                }
            }

            return response;
        }
        #endregion

        #region General information
        public async Task<string> GetHttpRequestJsonStringResponse(string Url, UserInput userInputObj, bool isPost = false)
        {
            HttpResponseMessage response = await GetHttpResponse(Url, userInputObj, isPost);

            if (response == null)
                throw new Exception($"Could not obtain a HTTP GET response for url {Url}.");

            else if (!response.IsSuccessStatusCode)
            {
                string responseContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"HTTP GET response for url {Url} failure: {response.StatusCode}: {responseContent}");
            }

            return await response.Content.ReadAsStringAsync();
        }

        private async Task<HttpResponseMessage> GetHttpResponse(string Url, UserInput userInputObj, bool isPost = false)
        {
            if (userInputObj.CancellationContext.IsCancellationRequested)
                UtilityFunctions.InitiateCancellation(userInputObj);

            NumberOfTries++;
            AuthenticationResult authResult = null;
            HttpResponseMessage response;
            bool isException = false;
            try
            {
                authResult = await AzureAuthenticationHandler.RetrieveAuthenticationToken();
            }
            catch (Exception exCacheTokenRetrieval)
            {
                throw new Exception($"Cached token retrieval failed: {exCacheTokenRetrieval.Message} Please re-login");
            }

            string workflow = userInputObj.WorkflowObj.IsExpressWorkflow ? "express" : "custom";

            try
            {
                HttpClient httpClient = new HttpClient()
                {
                    Timeout = TimeSpan.FromSeconds(60),
                };

                Uri baseAddress = new Uri(Url);
                string clientRequestId = Guid.NewGuid().ToString();

                httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + authResult.AccessToken);
                httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
                httpClient.DefaultRequestHeaders.Add("x-ms-client-request-id", clientRequestId + "-" + workflow + "-azmigexp");

                if (isPost)
                {
                    string blankContent = "";
                    byte[] buffer = Encoding.UTF8.GetBytes(blankContent);
                    ByteArrayContent byteContent = new ByteArrayContent(buffer);
                    byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    response = await httpClient.PostAsync(baseAddress, byteContent);
                }
                else
                    response = await httpClient.GetAsync(baseAddress);
            }
            catch (Exception exGeneralGetHttpRequest)
            {
                isException = true;
                if (NumberOfTries < HttpUtilities.MaxInformationDataRetries && HttpUtilities.IsRetryNeeded(null, exGeneralGetHttpRequest))
                {
                    userInputObj.LoggerObj.LogWarning($"HTTP GET request to url: {Url} error: {exGeneralGetHttpRequest.Message} Will try again after 1 minute");
                    Thread.Sleep(60000);
                    response = await GetHttpResponse(Url, userInputObj);
                }
                else
                    throw;
            }

            if (!isException)
            {
                if ((response == null || !response.IsSuccessStatusCode) && HttpUtilities.IsRetryNeeded(response, null) && NumberOfTries < HttpUtilities.MaxInformationDataRetries)
                {
                    userInputObj.LoggerObj.LogWarning($"HTTP GET request to url {Url} failed: {response.StatusCode}: {response.Content} Will try again after 1 minute");
                    Thread.Sleep(60000);
                    response = await GetHttpResponse(Url, userInputObj);
                }
            }

            return response;
        }
        #endregion

        #region Group creation and updation
        public async Task<bool> CreateGroup(UserInput userInputObj, KeyValuePair<string, List<string>> groupInformation, Dictionary<string, GroupPollResponse> GroupStatusMap)
        {
            userInputObj.LoggerObj.LogInformation($"Creating container for group {groupInformation.Key}");
            HttpResponseMessage createResponse = await SendGroupCreationRequest(userInputObj, groupInformation);

            if (createResponse == null)
                throw new Exception($"Could not obtain a HTTP response for group {groupInformation.Key} creation.");

            else if (!createResponse.IsSuccessStatusCode)
            {
                string createResponseContent = await createResponse.Content.ReadAsStringAsync();
                throw new Exception($"HTTP create group response for {groupInformation.Key} was not successful: {createResponse.StatusCode}: {createResponseContent}");
            }

            else if (createResponse.StatusCode != HttpStatusCode.Created)
                throw new Exception($"Received response: {createResponse.StatusCode} for group {groupInformation.Key} is not as expected: {HttpStatusCode.Created}");

            userInputObj.LoggerObj.LogInformation($"Group {groupInformation.Key} container created");

            userInputObj.LoggerObj.LogInformation($"Updating group {groupInformation.Key} with {groupInformation.Value.Count} machines");

            GroupPollResponse pollResponse = GroupPollResponse.Invalid;
            List<List<string>> updatedGroupMachines = UpdateTo5kMachinesPerRequest(userInputObj, groupInformation);
            if (updatedGroupMachines.Count > 1)
            {
                userInputObj.LoggerObj.LogInformation($"Group {groupInformation.Key} has more than 5000 machines, will be updated in {updatedGroupMachines.Count} batches");
            }

            int index = 0;
            foreach (var updatedMachine in updatedGroupMachines)
            {
                // Reset global variable
                NumberOfTries = 0;
                KeyValuePair<string, List<string>> groupInformationPerRequest = new KeyValuePair<string, List<string>>(groupInformation.Key, updatedMachine);

                if (updatedGroupMachines.Count > 1)
                {
                    userInputObj.LoggerObj.LogInformation($"Updating group {groupInformationPerRequest.Key} Batch {++index} with {updatedMachine.Count} machines");
                }

                HttpResponseMessage updateResponse = await SendMachineUpdationInGroupRequest(userInputObj, groupInformationPerRequest);

                if (updateResponse == null)
                    throw new Exception($"Could not obtain a HTTP response for machine updation in group {groupInformationPerRequest.Key}");
                else if (!updateResponse.IsSuccessStatusCode)
                {
                    string updateResponseContent = await updateResponse.Content.ReadAsStringAsync();
                    throw new Exception($"HTTP update machines in group {groupInformationPerRequest.Key} response was not successful: {updateResponse.StatusCode}: {updateResponseContent}");
                }
                else if (updateResponse.StatusCode != HttpStatusCode.OK)
                    throw new Exception($"Received response: {updateResponse.StatusCode} for update machines in group {groupInformationPerRequest.Key} is not as expected: {HttpStatusCode.OK}");

                pollResponse = WaitForGroupUpdateCompletion(userInputObj, groupInformation.Key);
                if (pollResponse != GroupPollResponse.Completed)
                {
                    userInputObj.LoggerObj.LogError($"Failed to update the group {groupInformation.Key} with machine batch {index - 1}, process will be terminated.");
                    return false;
                }
            }

            GroupStatusMap[groupInformation.Key] = pollResponse;
            userInputObj.LoggerObj.LogInformation($"Updated group {groupInformation.Key} with machines");

            return true;
        }

        private async Task<HttpResponseMessage> SendGroupCreationRequest(UserInput userInputObj, KeyValuePair<string, List<string>> groupInformation)
        {
            if (userInputObj.CancellationContext.IsCancellationRequested)
                UtilityFunctions.InitiateCancellation(userInputObj);

            NumberOfTries++;
            AuthenticationResult authResult = null;
            HttpResponseMessage response;
            bool isException = false;
            try
            {
                authResult = await AzureAuthenticationHandler.RetrieveAuthenticationToken();
            }
            catch (Exception exCacheTokenRetrieval)
            {
                throw new Exception($"Cached token retrieval failed: {exCacheTokenRetrieval.Message} Please re-login");
            }

            string workflow = userInputObj.WorkflowObj.IsExpressWorkflow ? "express" : "custom";

            try
            {
                HttpClient httpClient = new HttpClient()
                {
                    Timeout = TimeSpan.FromSeconds(60),
                };

                string url = Routes.ProtocolScheme + Routes.AzureManagementApiHostname + Routes.ForwardSlash +
                             Routes.SubscriptionPath + Routes.ForwardSlash + userInputObj.Subscription.Key + Routes.ForwardSlash +
                             Routes.ResourceGroupPath + Routes.ForwardSlash + userInputObj.ResourceGroupName.Value + Routes.ForwardSlash +
                             Routes.ProvidersPath + Routes.ForwardSlash + Routes.MigrateProvidersPath + Routes.ForwardSlash +
                             Routes.AssessmentProjectsPath + Routes.ForwardSlash + userInputObj.AssessmentProjectName + Routes.ForwardSlash +
                             Routes.GroupsPath + Routes.ForwardSlash + groupInformation.Key +
                             Routes.QueryStringQuestionMark +
                             Routes.QueryParameterApiVersion + Routes.QueryStringEquals + Routes.CreateGroupApiVersion;
                Uri baseAddress = new Uri(url);

                string clientRequestId = Guid.NewGuid().ToString();
                httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + authResult.AccessToken);
                httpClient.DefaultRequestHeaders.Add("x-ms-client-request-id", clientRequestId + "-" + workflow + "-azmigexp");

                CreateGroupBodyJSON createGroupJsonObj = new CreateGroupBodyJSON();

                if (userInputObj.AzureMigrateSourceAppliances.Contains("import"))
                    createGroupJsonObj.Properties.GroupType = "Import";

                string createGroupJsonBody = JsonConvert.SerializeObject(createGroupJsonObj);
                byte[] buffer = Encoding.UTF8.GetBytes(createGroupJsonBody);
                ByteArrayContent byteContent = new ByteArrayContent(buffer);
                byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                response = await httpClient.PutAsync(baseAddress, byteContent);
            }
            catch (Exception exCreateGroup)
            {
                isException = true;
                if (NumberOfTries < HttpUtilities.MaxInformationDataRetries && HttpUtilities.IsRetryNeeded(null, exCreateGroup))
                {
                    userInputObj.LoggerObj.LogWarning($"HTTP PUT create group {groupInformation.Key} request error: {exCreateGroup.Message} Will try again after 1 minute");
                    Thread.Sleep(60000);
                    response = await SendGroupCreationRequest(userInputObj, groupInformation);
                }
                else
                    throw;
            }

            if (!isException)
            {
                if ((response == null || !response.IsSuccessStatusCode) && HttpUtilities.IsRetryNeeded(response, null) && NumberOfTries < HttpUtilities.MaxInformationDataRetries)
                {
                    userInputObj.LoggerObj.LogWarning($"HTTP PUT create group {groupInformation.Key} request failed: {response.StatusCode}: {response.Content} Will try again after 1 minute");
                    Thread.Sleep(60000);
                    response = await SendGroupCreationRequest(userInputObj, groupInformation);
                }
            }

            return response;
        }

        private async Task<HttpResponseMessage> SendMachineUpdationInGroupRequest(UserInput userInputObj, KeyValuePair<string, List<string>> groupInformation)
        {
            if (userInputObj.CancellationContext.IsCancellationRequested)
                UtilityFunctions.InitiateCancellation(userInputObj);

            NumberOfTries++;
            AuthenticationResult authResult = null;
            HttpResponseMessage response;
            bool isException = false;
            try
            {
                authResult = await AzureAuthenticationHandler.RetrieveAuthenticationToken();
            }
            catch (Exception exCacheTokenRetrieval)
            {
                throw new Exception($"Cached token retrieval failed: {exCacheTokenRetrieval.Message} Please re-login");
            }

            string workflow = userInputObj.WorkflowObj.IsExpressWorkflow ? "express" : "custom";

            try
            {
                HttpClient httpClient = new HttpClient()
                {
                    Timeout = TimeSpan.FromSeconds(60),
                };

                string url = Routes.ProtocolScheme + Routes.AzureManagementApiHostname + Routes.ForwardSlash +
                             Routes.SubscriptionPath + Routes.ForwardSlash + userInputObj.Subscription.Key + Routes.ForwardSlash +
                             Routes.ResourceGroupPath + Routes.ForwardSlash + userInputObj.ResourceGroupName.Value + Routes.ForwardSlash +
                             Routes.ProvidersPath + Routes.ForwardSlash + Routes.MigrateProvidersPath + Routes.ForwardSlash +
                             Routes.AssessmentProjectsPath + Routes.ForwardSlash + userInputObj.AssessmentProjectName + Routes.ForwardSlash +
                             Routes.GroupsPath + Routes.ForwardSlash + groupInformation.Key + Routes.ForwardSlash + Routes.UpdateMachinesPath +
                             Routes.QueryStringQuestionMark +
                             Routes.QueryParameterApiVersion + Routes.QueryStringEquals + Routes.CreateGroupApiVersion;
                Uri baseAddress = new Uri(url);

                string clientRequestId = Guid.NewGuid().ToString();
                httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + authResult.AccessToken);
                httpClient.DefaultRequestHeaders.Add("x-ms-client-request-id", clientRequestId + "-" + workflow + "-azmigexp");

                UpdateMachinesInGroupBodyJSON updateMachinesInGroupObj = new UpdateMachinesInGroupBodyJSON();
                updateMachinesInGroupObj.Properties.Machines.AddRange(groupInformation.Value);
                string updateMachinesInGroupJsonBody = JsonConvert.SerializeObject(updateMachinesInGroupObj);
                byte[] buffer = Encoding.UTF8.GetBytes(updateMachinesInGroupJsonBody);
                ByteArrayContent byteContent = new ByteArrayContent(buffer);
                byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                response = await httpClient.PostAsync(baseAddress, byteContent);
            }
            catch (Exception exGroupUpdate)
            {
                isException = true;
                if (NumberOfTries < HttpUtilities.MaxInformationDataRetries && HttpUtilities.IsRetryNeeded(null, exGroupUpdate))
                {
                    userInputObj.LoggerObj.LogWarning($"HTTP POST update machines in group {groupInformation.Key} request error: {exGroupUpdate.Message} Will try again after 1 minute");
                    Thread.Sleep(60000);
                    response = await SendMachineUpdationInGroupRequest(userInputObj, groupInformation);
                }
                else
                    throw;
            }

            if (!isException)
            {
                if ((response == null || !response.IsSuccessStatusCode) && HttpUtilities.IsRetryNeeded(response, null) && NumberOfTries < HttpUtilities.MaxInformationDataRetries)
                {
                    userInputObj.LoggerObj.LogWarning($"HTTP POST update machines in group {groupInformation.Key} request failed: {response.StatusCode}: {response.Content} Will try again after 1 minute");
                    Thread.Sleep(60000);
                    response = await SendMachineUpdationInGroupRequest(userInputObj, groupInformation);
                }
            }

            return response;
        }
        #endregion

        #region Group polling
        public async Task<GroupPollResponse> PollGroup(UserInput userInputObj, string groupName)
        {
            if (userInputObj.CancellationContext.IsCancellationRequested)
                UtilityFunctions.InitiateCancellation(userInputObj);

            userInputObj.LoggerObj.LogInformation($"Polling group {groupName} for status");

            AuthenticationResult authResult = null;
            HttpResponseMessage response;

            try
            {
                authResult = await AzureAuthenticationHandler.RetrieveAuthenticationToken();
            }
            catch (Exception exCacheTokenRetrieval)
            {
                throw new Exception($"Cached token retrieval failed: {exCacheTokenRetrieval.Message} Please re-login");
            }

            string workflow = userInputObj.WorkflowObj.IsExpressWorkflow ? "express" : "custom";

            try
            {
                HttpClient httpClient = new HttpClient()
                {
                    Timeout = TimeSpan.FromSeconds(60),
                };

                string url = Routes.ProtocolScheme + Routes.AzureManagementApiHostname + Routes.ForwardSlash +
                             Routes.SubscriptionPath + Routes.ForwardSlash + userInputObj.Subscription.Key + Routes.ForwardSlash +
                             Routes.ResourceGroupPath + Routes.ForwardSlash + userInputObj.ResourceGroupName.Value + Routes.ForwardSlash +
                             Routes.ProvidersPath + Routes.ForwardSlash + Routes.MigrateProvidersPath + Routes.ForwardSlash +
                             Routes.AssessmentProjectsPath + Routes.ForwardSlash + userInputObj.AssessmentProjectName + Routes.ForwardSlash +
                             Routes.GroupsPath + Routes.ForwardSlash + groupName +
                             Routes.QueryStringQuestionMark +
                             Routes.QueryParameterApiVersion + Routes.QueryStringEquals + Routes.CreateGroupApiVersion;

                Uri baseAddress = new Uri(url);

                string clientRequestId = Guid.NewGuid().ToString();
                httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + authResult.AccessToken);
                httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
                httpClient.DefaultRequestHeaders.Add("x-ms-client-request-id", clientRequestId + "-" + workflow + "-azmigexp");

                response = await httpClient.GetAsync(baseAddress);

                if (response == null || !response.IsSuccessStatusCode)
                {
                    userInputObj.LoggerObj.LogWarning($"Group {groupName} polling response was not success. Status: {response?.StatusCode} Content: {response?.Content}");
                    if (!HttpUtilities.IsRetryNeeded(response, null))
                        return GroupPollResponse.Error;

                    return GroupPollResponse.NotCompleted;
                }

                string responseContent = await response.Content.ReadAsStringAsync();
                GroupInformationJSON groupInformationObj = JsonConvert.DeserializeObject<GroupInformationJSON>(responseContent);

                if (groupInformationObj.Properties.GroupStatus.Equals("Completed"))
                {
                    userInputObj.LoggerObj.LogInformation($"Group {groupName} completed");
                    return GroupPollResponse.Completed;
                }
                else if (groupInformationObj.Properties.GroupStatus.Equals("Invalid"))
                {
                    userInputObj.LoggerObj.LogWarning($"Group {groupName} is in invalid state, corresponding assessments will be skipped");
                    return GroupPollResponse.Invalid;
                }

                userInputObj.LoggerObj.LogInformation($"Group {groupName} status is {groupInformationObj.Properties.GroupStatus}");
                return GroupPollResponse.NotCompleted;
            }
            catch (Exception exPollGroup)
            {
                userInputObj.LoggerObj.LogWarning($"Group {groupName} polling error: {exPollGroup.Message}");

                if (!HttpUtilities.IsRetryNeeded(null, exPollGroup))
                    return GroupPollResponse.Error;
            }

            return GroupPollResponse.NotCompleted;
        }
        #endregion

        #region Assessment creation
        public async Task<bool> CreateAssessment(UserInput userInputObj, AssessmentInformation assessmentInfo)
        {
            userInputObj.LoggerObj.LogInformation($"Creating assessment {assessmentInfo.AssessmentName} in group {assessmentInfo.GroupName}");
            HttpResponseMessage createResponse = await SendAssessmentCreationRequest(userInputObj, assessmentInfo);

            if (createResponse == null)
                throw new Exception($"Could not obtain a HTTP response for assessment {assessmentInfo.AssessmentName}.");

            else if (!createResponse.IsSuccessStatusCode)
            {
                string createResponseContent = await createResponse.Content.ReadAsStringAsync();
                throw new Exception($"HTTP create assessment {assessmentInfo.AssessmentName} response was not successful: {createResponse.StatusCode}: {createResponseContent}");
            }

            else if (createResponse.StatusCode != HttpStatusCode.Created)
                throw new Exception($"Received response for assessment {assessmentInfo.AssessmentName}: {createResponse.StatusCode} is not as expected: {HttpStatusCode.Created}");

            userInputObj.LoggerObj.LogInformation($"Assessment {assessmentInfo.AssessmentName} created in group {assessmentInfo.GroupName}");

            return true;
        }

        private async Task<HttpResponseMessage> SendAssessmentCreationRequest(UserInput userInputObj, AssessmentInformation assessmentInfo)
        {
            if (userInputObj.CancellationContext.IsCancellationRequested)
                UtilityFunctions.InitiateCancellation(userInputObj);

            NumberOfTries++;
            AuthenticationResult authResult = null;
            HttpResponseMessage response;
            bool isException = false;
            try
            {
                authResult = await AzureAuthenticationHandler.RetrieveAuthenticationToken();
            }
            catch (Exception exCacheTokenRetrieval)
            {
                throw new Exception($"Cached token retrieval failed: {exCacheTokenRetrieval.Message} Please re-login");
            }

            string workflow = userInputObj.WorkflowObj.IsExpressWorkflow ? "express" : "custom";

            try
            {
                HttpClient httpClient = new HttpClient()
                {
                    Timeout = TimeSpan.FromSeconds(60),
                };

                string apiVersion = Routes.AssessmentMachineListApiVersion;
                if (assessmentInfo.AssessmentType == AssessmentType.AVSAssessment)
                    apiVersion = Routes.AvsAssessmentApiVersion;

                string url = Routes.ProtocolScheme + Routes.AzureManagementApiHostname + Routes.ForwardSlash +
                             Routes.SubscriptionPath + Routes.ForwardSlash + userInputObj.Subscription.Key + Routes.ForwardSlash +
                             Routes.ResourceGroupPath + Routes.ForwardSlash + userInputObj.ResourceGroupName.Value + Routes.ForwardSlash +
                             Routes.ProvidersPath + Routes.ForwardSlash + Routes.MigrateProvidersPath + Routes.ForwardSlash +
                             Routes.AssessmentProjectsPath + Routes.ForwardSlash + userInputObj.AssessmentProjectName + Routes.ForwardSlash +
                             Routes.GroupsPath + Routes.ForwardSlash + assessmentInfo.GroupName + Routes.ForwardSlash +
                             new EnumDescriptionHelper().GetEnumDescription(assessmentInfo.AssessmentType) + Routes.ForwardSlash + assessmentInfo.AssessmentName +
                             Routes.QueryStringQuestionMark +
                             Routes.QueryParameterApiVersion + Routes.QueryStringEquals + apiVersion;
                Uri baseAddress = new Uri(url);
                string clientRequestId = Guid.NewGuid().ToString();
                httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + authResult.AccessToken);
                httpClient.DefaultRequestHeaders.Add("x-ms-client-request-id", clientRequestId + "-" + workflow + "-azmigexp");

                string assessmentSetting = assessmentInfo.AssessmentSettings;
                byte[] buffer = Encoding.UTF8.GetBytes(assessmentSetting);
                ByteArrayContent byteContent = new ByteArrayContent(buffer);
                byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                response = await httpClient.PutAsync(baseAddress, byteContent);
            }
            catch (Exception exCreateAssessment)
            {
                isException = true;
                if (NumberOfTries < HttpUtilities.MaxInformationDataRetries && HttpUtilities.IsRetryNeeded(null, exCreateAssessment))
                {
                    userInputObj.LoggerObj.LogWarning($"HTTP PUT create assessment {assessmentInfo.AssessmentName} request error: {exCreateAssessment.Message} Will try again after 1 minute");
                    Thread.Sleep(60000);
                    response = await SendAssessmentCreationRequest(userInputObj, assessmentInfo);
                }
                else
                    throw;
            }

            if (!isException)
            {
                if ((response == null || !response.IsSuccessStatusCode) && HttpUtilities.IsRetryNeeded(response, null) && NumberOfTries < HttpUtilities.MaxInformationDataRetries)
                {
                    userInputObj.LoggerObj.LogWarning($"HTTP PUT create assessment {assessmentInfo.AssessmentName} request failed: {response.StatusCode}: {response.Content} Will try again after 1 minute");
                    Thread.Sleep(60000);
                    response = await SendAssessmentCreationRequest(userInputObj, assessmentInfo);
                }
            }

            return response;
        }
        #endregion

        #region Assessment Polling
        public async Task<AssessmentPollResponse> PollAssessment(UserInput userInputObj, AssessmentInformation assessmentInfo)
        {
            if (userInputObj.CancellationContext.IsCancellationRequested)
                UtilityFunctions.InitiateCancellation(userInputObj);

            userInputObj.LoggerObj.LogInformation($"Polling assessment {assessmentInfo.AssessmentName} for status");

            AuthenticationResult authResult = null;
            HttpResponseMessage response;

            try
            {
                authResult = await AzureAuthenticationHandler.RetrieveAuthenticationToken();
            }
            catch (Exception exCacheTokenRetrieval)
            {
                throw new Exception($"Cached token retrieval failed: {exCacheTokenRetrieval.Message} Please re-login");
            }

            string workflow = userInputObj.WorkflowObj.IsExpressWorkflow ? "express" : "custom";

            try
            {
                HttpClient httpClient = new HttpClient()
                {
                    Timeout = TimeSpan.FromSeconds(60),
                };

                string url = Routes.ProtocolScheme + Routes.AzureManagementApiHostname + Routes.ForwardSlash +
                             Routes.SubscriptionPath + Routes.ForwardSlash + userInputObj.Subscription.Key + Routes.ForwardSlash +
                             Routes.ResourceGroupPath + Routes.ForwardSlash + userInputObj.ResourceGroupName.Value + Routes.ForwardSlash +
                             Routes.ProvidersPath + Routes.ForwardSlash + Routes.MigrateProvidersPath + Routes.ForwardSlash +
                             Routes.AssessmentProjectsPath + Routes.ForwardSlash + userInputObj.AssessmentProjectName + Routes.ForwardSlash +
                             Routes.GroupsPath + Routes.ForwardSlash + assessmentInfo.GroupName + Routes.ForwardSlash +
                             new EnumDescriptionHelper().GetEnumDescription(assessmentInfo.AssessmentType) + Routes.ForwardSlash + assessmentInfo.AssessmentName +
                             Routes.QueryStringQuestionMark +
                             Routes.QueryParameterApiVersion + Routes.QueryStringEquals + Routes.AssessmentMachineListApiVersion;
                Uri baseAddress = new Uri(url);
                string clientRequestId = Guid.NewGuid().ToString();
                httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + authResult.AccessToken);
                httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
                httpClient.DefaultRequestHeaders.Add("x-ms-client-request-id", clientRequestId + "-" + workflow + "-azmigexp");

                response = await httpClient.GetAsync(baseAddress);

                if (response == null || !response.IsSuccessStatusCode)
                {
                    userInputObj.LoggerObj.LogWarning($"Assessment {assessmentInfo.AssessmentName} polling response was not success. Status: {response?.StatusCode} Content: {response?.Content}");
                    if (!HttpUtilities.IsRetryNeeded(response, null))
                        return AssessmentPollResponse.Error;

                    return AssessmentPollResponse.NotCompleted;
                }

                string responseContent = await response.Content.ReadAsStringAsync();
                AssessmentInformationJSON assessmentInformationObj = JsonConvert.DeserializeObject<AssessmentInformationJSON>(responseContent);

                if (assessmentInformationObj.Properties.Status.Equals("Completed"))
                {
                    userInputObj.LoggerObj.LogInformation($"Assessment {assessmentInfo.AssessmentName} completed");
                    return AssessmentPollResponse.Completed;
                }
                else if (assessmentInformationObj.Properties.Status.Equals("OutDated"))
                {
                    userInputObj.LoggerObj.LogWarning($"Assessment {assessmentInfo.AssessmentName} became out-dated during computation");
                    return AssessmentPollResponse.OutDated;
                }
                else if (assessmentInformationObj.Properties.Status.Equals("Invalid"))
                {
                    userInputObj.LoggerObj.LogError($"Assessment {assessmentInfo.AssessmentName} is invalid, corresponding datapoints will contain default values");
                    return AssessmentPollResponse.Invalid;
                }

                userInputObj.LoggerObj.LogInformation($"Assessment {assessmentInfo.AssessmentName} status is {assessmentInformationObj.Properties.Status}");

                return AssessmentPollResponse.NotCompleted;
            }
            catch (Exception exPollAssessment)
            {
                userInputObj.LoggerObj.LogWarning($"Assessment {assessmentInfo.AssessmentName} polling error: {exPollAssessment.Message}");

                if (!HttpUtilities.IsRetryNeeded(null, exPollAssessment))
                    return AssessmentPollResponse.Error;
            }

            return AssessmentPollResponse.NotCompleted;
        }
        #endregion

        #region Business case creation
        public async Task<bool> CreateBusinessCase(UserInput userInputObj, BusinessCaseInformation businessCaseInfo)
        {
            userInputObj.LoggerObj.LogInformation($"Creating business case {businessCaseInfo.BusinessCaseName}");
            HttpResponseMessage createResponse = await SendBusinessCaseCreationRequest(userInputObj, businessCaseInfo);

            if (createResponse == null)
                throw new Exception($"Could not obtain a HTTP response for business case {businessCaseInfo.BusinessCaseName}.");
            else if (!createResponse.IsSuccessStatusCode)
            {
                string createResponseContent = await createResponse.Content.ReadAsStringAsync();
                throw new Exception($"HTTP create business case {businessCaseInfo.BusinessCaseName} response was not successful: {createResponse.StatusCode}: {createResponseContent}");
            }
            else if (createResponse.StatusCode != HttpStatusCode.OK)
                throw new Exception($"Received response for business case {businessCaseInfo.BusinessCaseName}: {createResponse.StatusCode} is not as expected: {HttpStatusCode.Created}");

            userInputObj.LoggerObj.LogInformation($"Business case {businessCaseInfo.BusinessCaseName} created");

            return true;
        }

        private async Task<HttpResponseMessage> SendBusinessCaseCreationRequest(UserInput userInputObj, BusinessCaseInformation businessCaseInfo)
        {
            if (userInputObj.CancellationContext.IsCancellationRequested)
                UtilityFunctions.InitiateCancellation(userInputObj);

            NumberOfTries++;
            AuthenticationResult authResult = null;
            HttpResponseMessage response;
            bool isException = false;
            try
            {
                authResult = await AzureAuthenticationHandler.RetrieveAuthenticationToken();
            }
            catch (Exception exCacheTokenRetrieval)
            {
                throw new Exception($"Cached token retrieval failed: {exCacheTokenRetrieval.Message} Please re-login");
            }

            string workflow = userInputObj.WorkflowObj.IsExpressWorkflow ? "express" : "custom";

            try
            {
                HttpClient httpClient = new HttpClient()
                {
                    Timeout = TimeSpan.FromSeconds(60),
                };

                string url = Routes.ProtocolScheme + Routes.AzureManagementApiHostname + Routes.ForwardSlash +
                             Routes.SubscriptionPath + Routes.ForwardSlash + userInputObj.Subscription.Key + Routes.ForwardSlash +
                             Routes.ResourceGroupPath + Routes.ForwardSlash + userInputObj.ResourceGroupName.Value + Routes.ForwardSlash +
                             Routes.ProvidersPath + Routes.ForwardSlash + Routes.MigrateProvidersPath + Routes.ForwardSlash +
                             Routes.AssessmentProjectsPath + Routes.ForwardSlash + userInputObj.AssessmentProjectName + Routes.ForwardSlash +
                             Routes.BusinessCasesPath + Routes.ForwardSlash + businessCaseInfo.BusinessCaseName + Routes.QueryStringQuestionMark +
                             Routes.QueryParameterApiVersion + Routes.QueryStringEquals + Routes.BusinessCaseApiVersion;
                Uri baseAddress = new Uri(url);
                string clientRequestId = Guid.NewGuid().ToString();
                httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + authResult.AccessToken);
                httpClient.DefaultRequestHeaders.Add("x-ms-client-request-id", clientRequestId + "-" + workflow + "-azmigexp");

                string businessCaseSettings = businessCaseInfo.BusinessCaseSettings;
                byte[] buffer = Encoding.UTF8.GetBytes(businessCaseSettings);
                ByteArrayContent byteContent = new ByteArrayContent(buffer);
                byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                response = await httpClient.PutAsync(baseAddress, byteContent);
            }
            catch (Exception exCreateBizCase)
            {
                isException = true;
                if (NumberOfTries < HttpUtilities.MaxInformationDataRetries && HttpUtilities.IsRetryNeeded(null, exCreateBizCase))
                {
                    userInputObj.LoggerObj.LogWarning($"HTTP PUT business case {businessCaseInfo.BusinessCaseName} request error: {exCreateBizCase.Message} Will try again after 1 minute");
                    Thread.Sleep(60000);
                    response = await SendBusinessCaseCreationRequest(userInputObj, businessCaseInfo);
                }
                else
                    throw;
            }

            if (!isException)
            {
                if ((response == null || !response.IsSuccessStatusCode) && HttpUtilities.IsRetryNeeded(response, null) && NumberOfTries < HttpUtilities.MaxInformationDataRetries)
                {
                    userInputObj.LoggerObj.LogWarning($"HTTP PUT business case {businessCaseInfo.BusinessCaseName} request failed: {response.StatusCode}: {response.Content} Will try again after 1 minute");
                    Thread.Sleep(60000);
                    response = await SendBusinessCaseCreationRequest(userInputObj, businessCaseInfo);
                }
            }

            return response;
        }
        #endregion

        #region Business case polling
        public async Task<AssessmentPollResponse> PollBusinessCase(UserInput userInputObj, BusinessCaseInformation businessCaseInfo)
        {
            if (userInputObj.CancellationContext.IsCancellationRequested)
                UtilityFunctions.InitiateCancellation(userInputObj);

            userInputObj.LoggerObj.LogInformation($"Polling business case {businessCaseInfo.BusinessCaseName} for status");

            AuthenticationResult authResult = null;
            HttpResponseMessage response;

            try
            {
                authResult = await AzureAuthenticationHandler.RetrieveAuthenticationToken();
            }
            catch (Exception exCacheTokenRetrieval)
            {
                throw new Exception($"Cached token retrieval failed: {exCacheTokenRetrieval.Message} Please re-login");
            }

            string workflow = userInputObj.WorkflowObj.IsExpressWorkflow ? "express" : "custom";

            try
            {
                HttpClient httpClient = new HttpClient()
                {
                    Timeout = TimeSpan.FromSeconds(60),
                };

                string url = Routes.ProtocolScheme + Routes.AzureManagementApiHostname + Routes.ForwardSlash +
                             Routes.SubscriptionPath + Routes.ForwardSlash + userInputObj.Subscription.Key + Routes.ForwardSlash +
                             Routes.ResourceGroupPath + Routes.ForwardSlash + userInputObj.ResourceGroupName.Value + Routes.ForwardSlash +
                             Routes.ProvidersPath + Routes.ForwardSlash + Routes.MigrateProvidersPath + Routes.ForwardSlash +
                             Routes.AssessmentProjectsPath + Routes.ForwardSlash + userInputObj.AssessmentProjectName + Routes.ForwardSlash +
                             Routes.BusinessCasesPath + Routes.ForwardSlash + businessCaseInfo.BusinessCaseName + Routes.QueryStringQuestionMark +
                             Routes.QueryParameterApiVersion + Routes.QueryStringEquals + Routes.BusinessCaseApiVersion;
                Uri baseAddress = new Uri(url);
                string clientRequestId = Guid.NewGuid().ToString();
                httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + authResult.AccessToken);
                httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
                httpClient.DefaultRequestHeaders.Add("x-ms-client-request-id", clientRequestId + "-" + workflow + "-azmigexp");

                response = await httpClient.GetAsync(baseAddress);

                if (response == null || !response.IsSuccessStatusCode)
                {
                    userInputObj.LoggerObj.LogWarning($"Business case {businessCaseInfo.BusinessCaseName} polling response was not success. Status: {response?.StatusCode} Content: {response?.Content}");
                    if (!HttpUtilities.IsRetryNeeded(response, null))
                        return AssessmentPollResponse.Error;

                    return AssessmentPollResponse.NotCompleted;
                }

                string responseContent = await response.Content.ReadAsStringAsync();
                BusinessCaseInformationJSON businessCaseInformationObj = JsonConvert.DeserializeObject<BusinessCaseInformationJSON>(responseContent);

                if (businessCaseInformationObj.Properties.State.Equals("Completed"))
                {
                    userInputObj.LoggerObj.LogInformation($"Business case {businessCaseInfo.BusinessCaseName} completed");
                    return AssessmentPollResponse.Completed;
                }
                else if (businessCaseInformationObj.Properties.State.Equals("OutDated"))
                {
                    userInputObj.LoggerObj.LogWarning($"Business case {businessCaseInfo.BusinessCaseName} became out-dated during computation");
                    return AssessmentPollResponse.OutDated;
                }
                else if (businessCaseInformationObj.Properties.State.Equals("Invalid"))
                {
                    userInputObj.LoggerObj.LogError($"Business case {businessCaseInfo.BusinessCaseName} is invalid, corresponding datapoints will contain default values");
                    return AssessmentPollResponse.Invalid;
                }

                userInputObj.LoggerObj.LogInformation($"Business case {businessCaseInfo.BusinessCaseName} status is {businessCaseInformationObj.Properties.State}");

                return AssessmentPollResponse.NotCompleted;
            }
            catch (Exception exPollBizCase)
            {
                userInputObj.LoggerObj.LogWarning($"Business case {businessCaseInfo.BusinessCaseName} polling error: {exPollBizCase.Message}");

                if (!HttpUtilities.IsRetryNeeded(null, exPollBizCase))
                    return AssessmentPollResponse.Error;
            }

            return AssessmentPollResponse.NotCompleted;
        }
        #endregion

        #region Utilities
        private List<List<string>> UpdateTo5kMachinesPerRequest(UserInput userInputObj, KeyValuePair<string, List<string>> groupInformation)
        {
            List<List<string>> updatedGroupMachines = new List<List<string>>();

            if (groupInformation.Value.Count > 5000)
            {
                int startIndex = 0;
                for (int i = 0; i < Math.Ceiling((double)((double)groupInformation.Value.Count / 5000)); i++)
                {
                    int endIndex = Math.Min(startIndex + 4999, groupInformation.Value.Count - 1);
                    List<string> newList = groupInformation.Value.GetRange(startIndex, endIndex - startIndex + 1);
                    startIndex = endIndex + 1;
                    updatedGroupMachines.Add(newList);
                }
            }
            else
            {
                updatedGroupMachines.Add(groupInformation.Value);
            }
            return updatedGroupMachines;
        }

        private GroupPollResponse WaitForGroupUpdateCompletion(UserInput userInputObj, string groupName, int numberOfPollTries = 0)
        {
            GroupPollResponse pollResult = GroupPollResponse.Invalid;
            while (numberOfPollTries < 50) // limit to prevent infinite loop on non-retryable failures
            {
                bool isNonRetriableResponse = false;
                try
                {
                    pollResult = new HttpClientHelper().PollGroup(userInputObj, groupName).Result;

                    if (pollResult == GroupPollResponse.Completed)
                    {
                        return pollResult;
                    }
                    else if (pollResult == GroupPollResponse.Error)
                    {
                        userInputObj.LoggerObj.LogWarning($"Polling for group {groupName} resulted in a non-retryable error");
                        isNonRetriableResponse = true;
                    }
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (AggregateException aePollGroup)
                {
                    string errorMessage = "";
                    foreach (var e in aePollGroup.Flatten().InnerExceptions)
                    {
                        if (e is OperationCanceledException)
                            throw e;
                        else
                        {
                            errorMessage = errorMessage + e.Message + " ";
                        }
                    }
                    userInputObj.LoggerObj.LogWarning($"Group {groupName} polling failed: {errorMessage}");
                    isNonRetriableResponse = !HttpUtilities.IsRetryableException(aePollGroup);
                }
                catch (Exception ex)
                {
                    userInputObj.LoggerObj.LogWarning($"Group {groupName} polling failed: {ex.Message}");
                    isNonRetriableResponse = !HttpUtilities.IsRetryableException(ex);
                }

                if (isNonRetriableResponse)
                    numberOfPollTries += 1;

                Thread.Sleep(10000);
            }
            return pollResult;
        }
        #endregion
    }
}
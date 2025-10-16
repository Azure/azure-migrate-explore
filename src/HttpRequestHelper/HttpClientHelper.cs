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
using System.Text.Json;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
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

        public async Task<HttpResponseMessage> GetHttpResponseForARGQuery(UserInput userInputObj, string bodyContent)
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

            try
            {
                HttpClient httpClient = new HttpClient()
                {
                    Timeout = TimeSpan.FromSeconds(60),
                };

                Uri baseAddress = new Uri(Routes.ArgUri);
                string clientRequestId = Guid.NewGuid().ToString();

                httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + authResult.AccessToken);
                httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
                httpClient.DefaultRequestHeaders.Add("x-ms-client-request-id", clientRequestId + "-" + "-azmigexp");

                byte[] buffer = Encoding.UTF8.GetBytes(bodyContent);
                ByteArrayContent byteContent = new ByteArrayContent(buffer);
                byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                response = await httpClient.PostAsync(baseAddress, byteContent);
            }
            catch (Exception exGeneralGetHttpRequest)
            {
                isException = true;
                if (NumberOfTries < HttpUtilities.MaxInformationDataRetries && HttpUtilities.IsRetryNeeded(null, exGeneralGetHttpRequest))
                {
                    userInputObj.LoggerObj.LogWarning($"HTTP POST request to url: {Routes.ArgUri} error: {exGeneralGetHttpRequest.Message} Will try again after 1 minute");
                    Thread.Sleep(60000);
                    response = await GetHttpResponseForARGQuery(userInputObj, bodyContent);
                }
                else
                    throw;
            }

            if (!isException)
            {
                if ((response == null || !response.IsSuccessStatusCode) && HttpUtilities.IsRetryNeeded(response, null) && NumberOfTries < HttpUtilities.MaxInformationDataRetries)
                {
                    userInputObj.LoggerObj.LogWarning($"HTTP POST request to url {Routes.ArgUri} failed: {response.StatusCode}: {response.Content} Will try again after 1 minute");
                    Thread.Sleep(60000);
                    response = await GetHttpResponseForARGQuery(userInputObj, bodyContent);
                }
            }
            return response;
        }
        #endregion

        #region Resolve Scope
        public async Task<List<string>> ResolveScopeAsync(UserInput userInputObj, string resourceGraphQuery)
        {
            userInputObj.LoggerObj.LogInformation("Resolving assessment scope to determine assessment resource types");

            if (userInputObj.CancellationContext.IsCancellationRequested)
                UtilityFunctions.InitiateCancellation(userInputObj);

            AuthenticationResult authResult;
            try
            {
                authResult = await AzureAuthenticationHandler.RetrieveAuthenticationToken();
            }
            catch (Exception ex)
            {
                throw new Exception($"Token retrieval failed: {ex.Message}. Please re-login.");
            }

            int numberOfTries = 0;
            HttpResponseMessage response = null;

            try
            {
                numberOfTries++;
                HttpClient httpClient = new HttpClient() { Timeout = TimeSpan.FromSeconds(60) };

                string url = Routes.ProtocolScheme + Routes.AzureManagementApiHostname + Routes.ForwardSlash +
                            Routes.SubscriptionPath + Routes.ForwardSlash + userInputObj.Subscription.Key + Routes.ForwardSlash +
                            Routes.ResourceGroupPath + Routes.ForwardSlash + userInputObj.ResourceGroupName.Value + Routes.ForwardSlash +
                            Routes.ProvidersPath + Routes.ForwardSlash + Routes.MigrateProvidersPath + Routes.ForwardSlash +
                            Routes.AssessmentProjectsPath + Routes.ForwardSlash + userInputObj.AssessmentProjectName + Routes.ForwardSlash +
                            "resolvescope" + Routes.QueryStringQuestionMark +
                            Routes.QueryParameterApiVersion + Routes.QueryStringEquals + Routes.ResolveScopeApiVersion;

                string requestBody = "{\"argQueries\": [\"" + resourceGraphQuery + "\"]}";

                string clientRequestId = Guid.NewGuid().ToString();
                httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + authResult.AccessToken);
                httpClient.DefaultRequestHeaders.Add("x-ms-client-request-id", clientRequestId + "-resolveScope-azmigexp");

                byte[] buffer = Encoding.UTF8.GetBytes(requestBody);
                ByteArrayContent byteContent = new ByteArrayContent(buffer);
                byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                response = await httpClient.PostAsync(url, byteContent);

                if (!response.IsSuccessStatusCode)
                {
                    string error = await response.Content.ReadAsStringAsync();
                    throw new Exception($"ResolveScope request failed: {response.StatusCode} - {error}");
                }

                string responseContent = await response.Content.ReadAsStringAsync();
                using JsonDocument json = JsonDocument.Parse(responseContent);

                var types = new List<string>();
                if (json.RootElement.TryGetProperty("assessmentArmResourceTypes", out JsonElement arr))
                {
                    foreach (JsonElement elem in arr.EnumerateArray())
                    {
                        string? resourceType = elem.GetString();
                        if (!string.IsNullOrWhiteSpace(resourceType))
                        {
                            types.Add(resourceType.ToLowerInvariant());
                        }
                    }
                }

                const string heterogeneousType = "microsoft.migrate/assessmentprojects/heterogeneousassessments";
                if (!types.Contains(heterogeneousType))
                {
                    types.Add(heterogeneousType);
                    userInputObj.LoggerObj.LogInformation("Resolve scope did not include heterogeneous assessments; adding default scope type.");
                }

                userInputObj.LoggerObj.LogInformation($"Resolve scope returned {types.Count} resource types: {string.Join(", ", types)}");
                return types;
            }
            catch (Exception ex)
            {
                if (numberOfTries < HttpUtilities.MaxInformationDataRetries)
                {
                    userInputObj.LoggerObj.LogWarning($"ResolveScope failed: {ex.Message}. Retrying in 1 minute...");
                    Thread.Sleep(60000);
                    return await ResolveScopeAsync(userInputObj, resourceGraphQuery);
                }
                throw;
            }
        }
        #endregion

        #region Deploy Assessment ARM Template

        public async Task<bool> DeployAssessmentArmTemplateAsync(
            UserInput userInputObj,
            string subscriptionId,
            string resourceGroupName,
            string assessmentProjectId,
            string assessmentName,
            string resourceGraphQuery,
            List<string> allowedAssessmentResourceTypes,
            Dictionary<string, object> assessmentSettings)
        {
            userInputObj.LoggerObj.LogInformation($"Deploying assessment ARM template for {assessmentName}");

            // Load ARM Template JSON
            string templatePath = GetAssessmentTemplatePath();

            if (!File.Exists(templatePath))
                throw new FileNotFoundException($"ARM template not found at: {templatePath}", templatePath);

            string fullTemplateJson = await File.ReadAllTextAsync(templatePath);
            JObject templateData = JObject.Parse(fullTemplateJson);

            // Extract assessment project name from ARM ID
            string assessmentProjectName = assessmentProjectId.Split('/').Last();

            // Inject azureResourceGraphQuery variable
            if (templateData["variables"] == null)
                templateData["variables"] = new JObject();

            templateData["variables"]["azureResourceGraphQuery"] = resourceGraphQuery;

            // Filter allowed resources
            JArray originalResources = (JArray)templateData["resources"];
            HashSet<string> allowedTypes = allowedAssessmentResourceTypes
                .Select(x => x.ToLowerInvariant())
                .ToHashSet();

            List<JObject> keptResources = new List<JObject>();
            foreach (JObject res in originalResources)
            {
                string type = res.Value<string>("type")?.ToLowerInvariant() ?? "";
                if (allowedTypes.Contains(type))
                    keptResources.Add(res);
                else
                    userInputObj.LoggerObj.LogInformation($"Excluding resource of type {type} not in allowed assessment scope");
            }

            // Track kept IDs for dependency pruning
            HashSet<string> keptIds = keptResources
                .Select(r => r.Value<string>("id"))
                .Where(id => !string.IsNullOrEmpty(id))
                .ToHashSet();

            foreach (var res in keptResources)
            {
                // Fix dependsOn
                if (res["dependsOn"] != null)
                {
                    JArray depends = (JArray)res["dependsOn"];
                    JArray filtered = new JArray(depends.Where(d => keptIds.Contains(d.ToString())));
                    res["dependsOn"] = filtered;
                }

                // Handle webAppCompoundAssessments
                if (res.Value<string>("type")?.ToLowerInvariant().EndsWith("/webappcompoundassessments") == true)
                {
                    JObject props = (JObject)res["properties"];
                    if (props?["targetAssessmentArmIds"] is JObject targetIds)
                    {
                        JObject pruned = new JObject();
                        foreach (var kvp in targetIds)
                        {
                            if (keptIds.Contains(kvp.Value.ToString()))
                                pruned[kvp.Key] = kvp.Value;
                        }
                        props["targetAssessmentArmIds"] = pruned;
                    }
                }

                // Handle heterogeneousAssessments
                if (res.Value<string>("type")?.ToLowerInvariant().EndsWith("/heterogeneousassessments") == true)
                {
                    JObject props = (JObject)res["properties"];
                    if (props?["assessmentArmIds"] is JArray armIds)
                    {
                        JArray prunedList = new JArray(armIds.Where(a => keptIds.Contains(a.ToString())));
                        props["assessmentArmIds"] = prunedList;
                    }
                }
            }

            // Replace filtered resources in template
            templateData["resources"] = new JArray(keptResources);
            userInputObj.LoggerObj.LogInformation($"Filtered ARM template resources from {originalResources.Count} to {keptResources.Count}");

            // Build parameters
            JObject baseParameters = new JObject
            {
                ["assessmentProjectName"] = new JObject { ["value"] = assessmentProjectName },
                ["assessmentProjectId"] = new JObject { ["value"] = assessmentProjectId },
                ["assessmentName"] = new JObject { ["value"] = assessmentName }
            };

            // Apply user-provided overrides
            foreach (var kvp in assessmentSettings)
            {
                baseParameters[kvp.Key] = new JObject { ["value"] = JToken.FromObject(kvp.Value) };
            }

            // Construct deployment payload
            JObject deploymentPayload = new JObject
            {
                ["properties"] = new JObject
                {
                    ["template"] = templateData,
                    ["parameters"] = baseParameters,
                    ["mode"] = "Incremental"
                }
            };

            string deploymentName = $"assessment-{assessmentName}-{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";
            string url = $"https://management.azure.com/subscriptions/{subscriptionId}/resourceGroups/{resourceGroupName}/providers/Microsoft.Resources/deployments/{deploymentName}?api-version=2021-04-01";

            // Send the PUT request like your other methods
            HttpResponseMessage response = await SendArmDeploymentRequest(userInputObj, url, deploymentPayload.ToString());

            if (response == null)
                throw new Exception($"No response for ARM deployment {assessmentName}");

            else if (!response.IsSuccessStatusCode)
            {
                string content = await response.Content.ReadAsStringAsync();
                throw new Exception($"HTTP ARM deployment {assessmentName} failed: {response.StatusCode}: {content}");
            }

            userInputObj.LoggerObj.LogInformation($"Successfully initiated ARM deployment for assessment {assessmentName}");
            return true;
        }


        /// <summary>
        /// Sends the ARM deployment PUT request (retry logic included)
        /// </summary>
        private async Task<HttpResponseMessage> SendArmDeploymentRequest(UserInput userInputObj, string url, string requestBody)
        {
            if (userInputObj.CancellationContext.IsCancellationRequested)
                UtilityFunctions.InitiateCancellation(userInputObj);

            NumberOfTries++;
            HttpResponseMessage response;
            bool isException = false;

            AuthenticationResult authResult;
            try
            {
                authResult = await AzureAuthenticationHandler.RetrieveAuthenticationToken();
            }
            catch (Exception ex)
            {
                throw new Exception($"Token retrieval failed: {ex.Message}");
            }

            try
            {
                using HttpClient httpClient = new HttpClient
                {
                    Timeout = TimeSpan.FromSeconds(120)
                };

                httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", authResult.AccessToken);
                httpClient.DefaultRequestHeaders.Add("x-ms-client-request-id", Guid.NewGuid().ToString() + "-arm-deploy");

                ByteArrayContent byteContent = new ByteArrayContent(Encoding.UTF8.GetBytes(requestBody));
                byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                response = await httpClient.PutAsync(url, byteContent);
            }
            catch (Exception ex)
            {
                isException = true;
                if (NumberOfTries < HttpUtilities.MaxInformationDataRetries && HttpUtilities.IsRetryNeeded(null, ex))
                {
                    userInputObj.LoggerObj.LogWarning($"HTTP PUT ARM deployment request failed: {ex.Message} Retrying in 60 seconds...");
                    Thread.Sleep(60000);
                    response = await SendArmDeploymentRequest(userInputObj, url, requestBody);
                }
                else throw;
            }

            if (!isException)
            {
                if ((response == null || !response.IsSuccessStatusCode) && HttpUtilities.IsRetryNeeded(response, null) && NumberOfTries < HttpUtilities.MaxInformationDataRetries)
                {
                    userInputObj.LoggerObj.LogWarning($"HTTP PUT ARM deployment failed: {response?.StatusCode}. Retrying in 60 seconds...");
                    Thread.Sleep(60000);
                    response = await SendArmDeploymentRequest(userInputObj, url, requestBody);
                }
            }

            return response;
        }

        #endregion

        #region Assessment creation
        public async Task<bool> CreateAssessment(UserInput userInputObj, AssessmentInformation assessmentInfo)
        {
            userInputObj.LoggerObj.LogInformation($"Creating assessment {assessmentInfo.AssessmentName}");
            HttpResponseMessage createResponse = await SendAssessmentCreationRequest(userInputObj, assessmentInfo);

            if (createResponse == null)
                throw new Exception($"Could not obtain a HTTP response for assessment {assessmentInfo.AssessmentName}.");

            else if (!createResponse.IsSuccessStatusCode)
            {
                string createResponseContent = await createResponse.Content.ReadAsStringAsync();
                throw new Exception($"HTTP create assessment {assessmentInfo.AssessmentName} response was not successful: {createResponse.StatusCode}: {createResponseContent}");
            }

            else if (createResponse.StatusCode != HttpStatusCode.Created && createResponse.StatusCode != HttpStatusCode.OK)
                throw new Exception($"Received response for assessment {assessmentInfo.AssessmentName}: {createResponse.StatusCode} is not as expected: {HttpStatusCode.Created} or {HttpStatusCode.OK}");

            userInputObj.LoggerObj.LogInformation($"Assessment {assessmentInfo.AssessmentName} created");

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

                string url = Routes.ProtocolScheme + Routes.AzureManagementApiHostname + Routes.ForwardSlash +
                             Routes.SubscriptionPath + Routes.ForwardSlash + userInputObj.Subscription.Key + Routes.ForwardSlash +
                             Routes.ResourceGroupPath + Routes.ForwardSlash + userInputObj.ResourceGroupName.Value + Routes.ForwardSlash +
                             Routes.ProvidersPath + Routes.ForwardSlash + Routes.MigrateProvidersPath + Routes.ForwardSlash +
                             Routes.AssessmentProjectsPath + Routes.ForwardSlash + userInputObj.AssessmentProjectName + Routes.ForwardSlash +
                             new EnumDescriptionHelper().GetEnumDescription(assessmentInfo.AssessmentType) + Routes.ForwardSlash + assessmentInfo.AssessmentName +
                             Routes.QueryStringQuestionMark +
                             Routes.QueryParameterApiVersion + Routes.QueryStringEquals + Routes.AssessmentApiVersion;
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
                             new EnumDescriptionHelper().GetEnumDescription(assessmentInfo.AssessmentType) + Routes.ForwardSlash + assessmentInfo.AssessmentName +
                             Routes.QueryStringQuestionMark +
                             Routes.QueryParameterApiVersion + Routes.QueryStringEquals + Routes.AssessmentApiVersion;

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
                if (assessmentInfo.AssessmentType == AssessmentType.AVSAssessment)
                {
                    AvsAssessmentInformationJSON avsAssessmentInformationObj = JsonConvert.DeserializeObject<AvsAssessmentInformationJSON>(responseContent);
                    if (avsAssessmentInformationObj.Properties.Details.Status.Equals("Completed"))
                    {
                        userInputObj.LoggerObj.LogInformation($"Assessment {assessmentInfo.AssessmentName} completed");
                        return AssessmentPollResponse.Completed;
                    }
                    else if (avsAssessmentInformationObj.Properties.Details.Status.Equals("OutDated"))
                    {
                        userInputObj.LoggerObj.LogWarning($"Assessment {assessmentInfo.AssessmentName} became out-dated during computation");
                        return AssessmentPollResponse.OutDated;
                    }
                    else if (avsAssessmentInformationObj.Properties.Details.Status.Equals("Invalid"))
                    {
                        userInputObj.LoggerObj.LogError($"Assessment {assessmentInfo.AssessmentName} is invalid, corresponding datapoints will contain default values");
                        return AssessmentPollResponse.Invalid;
                    }
                    userInputObj.LoggerObj.LogInformation($"Assessment {assessmentInfo.AssessmentName} status is {avsAssessmentInformationObj.Properties.Details.Status}");
                }
                else
                {
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
                }

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

        private static string GetAssessmentTemplatePath([CallerFilePath] string callerFilePath = "")
        {
            string? directory = Path.GetDirectoryName(callerFilePath);
            if (string.IsNullOrEmpty(directory))
                throw new InvalidOperationException("Could not determine source directory for ARM template lookup.");

            return Path.Combine(directory, "AssessmentArmTemplate.json");
        }
    }
}
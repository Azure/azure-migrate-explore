using System;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Migrate.Explore.Common;
using Azure.Migrate.Explore.HttpRequestHelper;
using Azure.Migrate.Explore.Models;
using Microsoft.Identity.Client;
using Azure.Migrate.Explore.Authentication;

namespace Azure.Migrate.Explore.Assessment
{
    public class HeterogeneousReportHandler
    {
        private static readonly HttpClient httpClient = new HttpClient();
        private const int MaxPollAttempts = 40;
        private const int PollIntervalMs = 60_000; // 1 minute

        public bool WaitForHeterogeneousAssessmentCompletion(UserInput userInputObj, AssessmentInformation assessmentInfo)
        {
            const int MaxPollAttempts = 40; // ~40 minutes
            const int PollIntervalMs = 120_000; // 1 minute

            if (userInputObj == null || assessmentInfo == null)
                throw new ArgumentNullException("Invalid input to polling method");

            userInputObj.LoggerObj.LogInformation($"Waiting for heterogeneous assessment '{assessmentInfo.AssessmentName}' to complete...");

            string statusUrl =
                $"{Routes.ProtocolScheme}{Routes.AzureManagementApiHostname}/subscriptions/{userInputObj.Subscription.Key}/resourceGroups/{userInputObj.ResourceGroupName.Value}/providers/Microsoft.Migrate/assessmentProjects/{userInputObj.AssessmentProjectName}/HeterogeneousAssessments/{assessmentInfo.AssessmentName}?api-version=2024-03-03-preview";

            try
            {
                string bearerToken = AzureAuthenticationHandler.RetrieveAuthenticationToken().Result.AccessToken;

                for (int attempt = 1; attempt <= MaxPollAttempts; attempt++)
                {
                    userInputObj.LoggerObj.LogInformation($"Polling assessment status (Attempt {attempt}/{MaxPollAttempts})...");

                    var request = new HttpRequestMessage(HttpMethod.Get, statusUrl);
                    request.Headers.Add("Authorization", $"Bearer {bearerToken}");

                    var response = httpClient.Send(request); // blocking call
                    var content = response.Content.ReadAsStringAsync().Result;

                    if (response.IsSuccessStatusCode && IsAssessmentCompleted(content))
                    {
                        userInputObj.LoggerObj.LogInformation("Heterogeneous assessment completed successfully.");
                        return true;
                    }

                    System.Threading.Thread.Sleep(PollIntervalMs);
                }

                userInputObj.LoggerObj.LogWarning("Assessment did not complete within the polling window.");
                return false;
            }
            catch (Exception ex)
            {
                userInputObj.LoggerObj.LogError($"Error while polling assessment completion: {ex.Message}");
                return false;
            }
        }

        private bool IsAssessmentCompleted(string jsonResponse)
        {
            try
            {
                using (var doc = JsonDocument.Parse(jsonResponse))
                {
                    if (doc.RootElement.TryGetProperty("properties", out var props))
                    {
                        if (props.TryGetProperty("status", out var statusProp))
                        {
                            string status = statusProp.GetString()?.ToLowerInvariant();
                            return status == "completed" || status == "succeeded";
                        }
                    }
                }
            }
            catch
            {
                // ignore malformed JSON
            }
            return false;
        }

        public async Task GenerateAndDownloadHeterogeneousReportAsync(UserInput userInputObj, AssessmentInformation assessmentInfo)
        {
            // Sanity checks
            if (assessmentInfo == null)
                throw new ArgumentNullException(nameof(assessmentInfo));

            if (userInputObj == null)
                throw new ArgumentNullException(nameof(userInputObj));

            userInputObj.LoggerObj.LogInformation($"Starting report generation for {assessmentInfo.AssessmentName}");

            // 🔹 1. Get authentication token
            AuthenticationResult authResult;
            try
            {
                authResult = await AzureAuthenticationHandler.RetrieveAuthenticationToken();
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get Azure token: {ex.Message}");
            }

            // 🔹 2. Construct the URLs dynamically
            string basePath =
                $"{Routes.ProtocolScheme}{Routes.AzureManagementApiHostname}/subscriptions/{userInputObj.Subscription.Key}/resourceGroups/{userInputObj.ResourceGroupName.Value}/providers/Microsoft.Migrate/assessmentProjects/{userInputObj.AssessmentProjectName}/HeterogeneousAssessments/{assessmentInfo.AssessmentName}";

            string generateReportUrl = $"{basePath}/generateReport?api-version=2024-03-03-preview";
            string downloadUrl = $"{basePath}/downloadUrl?api-version=2024-03-03-preview";

            // 🔹 3. Trigger report generation
            var generateResponse = await TriggerReportGeneration(generateReportUrl, authResult.AccessToken, userInputObj);
            if (!generateResponse)
            {
                userInputObj.LoggerObj.LogError($"Failed to trigger report generation for {assessmentInfo.AssessmentName}");
                return;
            }

            // 🔹 4. Poll until report is ready
            bool reportReady = await PollUntilReportReady(generateReportUrl, authResult.AccessToken, userInputObj);

            if (!reportReady)
            {
                userInputObj.LoggerObj.LogError($"Report for {assessmentInfo.AssessmentName} did not complete in time.");
                return;
            }

            // 🔹 5. Download the report
            await DownloadReport(downloadUrl, authResult.AccessToken, assessmentInfo, userInputObj);
        }

        private async Task<bool> TriggerReportGeneration(string url, string bearerToken, UserInput userInputObj)
        {
            try
            {
                userInputObj.LoggerObj.LogInformation($"POST {url}");

                var request = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json")
                };
                request.Headers.Add("Authorization", $"Bearer {bearerToken}");

                var response = await httpClient.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    userInputObj.LoggerObj.LogInformation("Report generation initiated successfully.");
                    return true;
                }

                userInputObj.LoggerObj.LogWarning($"Report generation initiation failed. Status: {response.StatusCode}");
                return false;
            }
            catch (Exception ex)
            {
                userInputObj.LoggerObj.LogWarning($"Error initiating report: {ex.Message}");
                return false;
            }
        }

        private async Task<bool> PollUntilReportReady(string url, string bearerToken, UserInput userInputObj)
        {
            for (int attempt = 1; attempt <= MaxPollAttempts; attempt++)
            {
                userInputObj.LoggerObj.LogInformation($"Polling report status (Attempt {attempt}/{MaxPollAttempts})...");

                try
                {
                    var req = new HttpRequestMessage(HttpMethod.Get, url);
                    req.Headers.Add("Authorization", $"Bearer {bearerToken}");

                    var resp = await httpClient.SendAsync(req);
                    var content = await resp.Content.ReadAsStringAsync();

                    if (resp.IsSuccessStatusCode && IsReportComplete(content))
                    {
                        userInputObj.LoggerObj.LogInformation("Report generation completed successfully!");
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    userInputObj.LoggerObj.LogWarning($"Polling error: {ex.Message}");
                }

                await Task.Delay(PollIntervalMs);
            }

            return false;
        }

        private async Task DownloadReport(string downloadUrl, string bearerToken, AssessmentInformation assessmentInfo, UserInput userInputObj)
        {
            userInputObj.LoggerObj.LogInformation("Fetching download URL...");

            string reportDownloadLink = null;
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, downloadUrl);
                request.Headers.Add("Authorization", $"Bearer {bearerToken}");

                var response = await httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();

                using (var doc = JsonDocument.Parse(content))
                {
                    if (doc.RootElement.TryGetProperty("downloadUrl", out var urlElement))
                        reportDownloadLink = urlElement.GetString();
                }

                if (string.IsNullOrEmpty(reportDownloadLink))
                {
                    userInputObj.LoggerObj.LogError("Could not find valid download URL in API response.");
                    return;
                }
            }
            catch (Exception ex)
            {
                userInputObj.LoggerObj.LogError($"Failed fetching download URL: {ex.Message}");
                return;
            }

            // 🔹 Download the actual file
            userInputObj.LoggerObj.LogInformation($"Downloading report from {reportDownloadLink}");

            try
            {
                var fileName = Path.Combine(AppContext.BaseDirectory, $"{assessmentInfo.AssessmentName}_Report_{DateTime.UtcNow:yyyyMMdd_HHmmss}.zip");
                var fileBytes = await httpClient.GetByteArrayAsync(reportDownloadLink);
                await File.WriteAllBytesAsync(fileName, fileBytes);

                userInputObj.LoggerObj.LogInformation($"Report saved at: {fileName}");
            }
            catch (Exception ex)
            {
                userInputObj.LoggerObj.LogError($"Error downloading report: {ex.Message}");
            }
        }

        private bool IsReportComplete(string jsonResponse)
        {
            try
            {
                using (var doc = JsonDocument.Parse(jsonResponse))
                {
                    if (doc.RootElement.TryGetProperty("status", out var statusProp))
                    {
                        string status = statusProp.GetString()?.ToLowerInvariant();
                        return status == "completed" || status == "succeeded";
                    }
                }
            }
            catch
            {
                // ignore malformed json
            }
            return false;
        }
    }
}

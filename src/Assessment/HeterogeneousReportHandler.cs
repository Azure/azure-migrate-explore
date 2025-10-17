using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Text;
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
                            string? status = statusProp.GetString()?.ToLowerInvariant();
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
            bool generationTriggered = await TriggerReportGeneration(generateReportUrl, authResult.AccessToken, userInputObj);
            if (!generationTriggered)
            {
                userInputObj.LoggerObj.LogError($"Failed to trigger report generation for {assessmentInfo.AssessmentName}");
                return;
            }

            // 🔹 4. Poll until the report download succeeds
            bool reportDownloaded = await PollUntilReportReady(downloadUrl, authResult.AccessToken, assessmentInfo, userInputObj);

            if (!reportDownloaded)
            {
                userInputObj.LoggerObj.LogError($"Report for {assessmentInfo.AssessmentName} did not complete in time.");
            }
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

        private async Task<bool> PollUntilReportReady(string url, string bearerToken, AssessmentInformation assessmentInfo, UserInput userInputObj)
        {
            for (int attempt = 1; attempt <= MaxPollAttempts; attempt++)
            {
                userInputObj.LoggerObj.LogInformation($"Attempting to download report (Attempt {attempt}/{MaxPollAttempts})...");

                bool downloadSucceeded = await DownloadReport(url, bearerToken, assessmentInfo, userInputObj);
                if (downloadSucceeded)
                {
                    return true;
                }

                await Task.Delay(PollIntervalMs);
            }

            return false;
        }

        private async Task<bool> DownloadReport(string downloadUrl, string bearerToken, AssessmentInformation assessmentInfo, UserInput userInputObj)
        {
            userInputObj.LoggerObj.LogInformation("Fetching download URL...");

            string? reportDownloadLink = null;
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, downloadUrl);
                request.Headers.Add("Authorization", $"Bearer {bearerToken}");
                request.Content = new StringContent("{}", Encoding.UTF8, "application/json");

                var response = await httpClient.SendAsync(request);
                if (response.StatusCode == HttpStatusCode.NotFound ||
                    response.StatusCode == HttpStatusCode.Accepted ||
                    response.StatusCode == HttpStatusCode.BadRequest)
                {
                    userInputObj.LoggerObj.LogInformation($"Report is not ready yet. Service returned {response.StatusCode}.");
                    return false;
                }

                if (!response.IsSuccessStatusCode)
                {
                    string error = await response.Content.ReadAsStringAsync();
                    userInputObj.LoggerObj.LogError($"Failed fetching download URL: {response.StatusCode} - {error}");
                    return false;
                }

                if (response.Content == null)
                {
                    userInputObj.LoggerObj.LogInformation("Download URL response had no content.");
                    return false;
                }

                var content = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(content))
                {
                    userInputObj.LoggerObj.LogInformation("Download URL response body empty.");
                    return false;
                }

                string trimmedContent = content.Trim();

                reportDownloadLink = ExtractDownloadLink(trimmedContent, userInputObj);

                if (string.IsNullOrWhiteSpace(reportDownloadLink))
                {
                    userInputObj.LoggerObj.LogInformation("Download URL not yet available in response.");
                    return false;
                }
            }
            catch (HttpRequestException ex)
            {
                userInputObj.LoggerObj.LogInformation($"Download URL request failed: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                userInputObj.LoggerObj.LogError($"Failed fetching download URL: {ex.Message}");
                return false;
            }

            // 🔹 Download the actual file
            string downloadUri = reportDownloadLink!;
            userInputObj.LoggerObj.LogInformation($"Downloading report from {downloadUri}");

            try
            {
                string reportsDirectory = UtilityFunctions.GetReportsDirectory();
                if (string.IsNullOrWhiteSpace(reportsDirectory))
                {
                    reportsDirectory = Path.Combine(AppContext.BaseDirectory, "Project Reports");
                    userInputObj.LoggerObj.LogInformation($"Reports directory not set. Using default path: {reportsDirectory}");
                }

                reportsDirectory = reportsDirectory.Trim();
                Directory.CreateDirectory(reportsDirectory);

                var fileName = Path.Combine(reportsDirectory, $"{assessmentInfo.AssessmentName}_Report_{DateTime.UtcNow:yyyyMMdd_HHmmss}.zip");
                var fileBytes = await httpClient.GetByteArrayAsync(downloadUri);
                await File.WriteAllBytesAsync(fileName, fileBytes);
                userInputObj.LoggerObj.LogInformation($"Report saved at: {fileName}");

                try
                {
                    ZipFile.ExtractToDirectory(fileName, reportsDirectory, overwriteFiles: true);
                    userInputObj.LoggerObj.LogInformation($"Report extracted to: {reportsDirectory}");
                }
                catch (Exception ex)
                {
                    userInputObj.LoggerObj.LogError($"Error extracting report archive: {ex.Message}");
                    return false;
                }

                return true;
            }
            catch (HttpRequestException ex)
            {
                userInputObj.LoggerObj.LogInformation($"Report content not ready yet: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                userInputObj.LoggerObj.LogError($"Error downloading report: {ex.Message}");
                return false;
            }
        }

        private static string? ExtractDownloadLink(string rawContent, UserInput userInputObj)
        {
            if (string.IsNullOrWhiteSpace(rawContent))
            {
                return null;
            }

            // 1. JSON object with downloadUrl property
            if (rawContent.StartsWith("{") && rawContent.EndsWith("}"))
            {
                try
                {
                    using var doc = JsonDocument.Parse(rawContent);
                    if (doc.RootElement.TryGetProperty("downloadUrl", out var urlElement))
                    {
                        var urlCandidate = urlElement.GetString();
                        if (!string.IsNullOrWhiteSpace(urlCandidate))
                        {
                            return urlCandidate;
                        }
                    }
                }
                catch (JsonException ex)
                {
                    userInputObj.LoggerObj.LogWarning($"Unable to parse download response object: {ex.Message}");
                }
            }

            string candidateContent = rawContent;

            // 2. JSON string payload (e.g. "https://...")
            try
            {
                var parsedString = JsonSerializer.Deserialize<string>(rawContent);
                if (!string.IsNullOrWhiteSpace(parsedString))
                {
                    candidateContent = parsedString;
                }
            }
            catch (JsonException)
            {
                // ignore; content might already be plain text
            }

            string candidate = candidateContent.Trim();

            if (candidate.StartsWith("\\\"") && candidate.EndsWith("\\\"") && candidate.Length >= 4)
            {
                candidate = candidate.Substring(2, candidate.Length - 4);
            }

            if (candidate.StartsWith("\"") && candidate.EndsWith("\""))
            {
                candidate = candidate.Trim('"');
            }

            candidate = candidate.Replace("\\\"", "\"");

            candidate = candidate.Trim();

            return string.IsNullOrWhiteSpace(candidate) ? null : candidate;
        }
    }
}

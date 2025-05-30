using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Identity.Client;
using System.Threading.Tasks;
using AzureMigrateExplore;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Diagnostics;
using System.IO.IsolatedStorage;
using System.IO;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Azure.Migrate.Explore.Authentication
{
    public static class AzureAuthenticationHandler
    {
        private static readonly List<string> scopes = new List<string>()
                {
                    "https://management.azure.com/.default"
                };

        private static string accessToken;
        private static AuthenticationResult _mockAuthResult;

        // Check if the test access token file is present and validates the token
        public static async Task<bool> IsTestFilePresent()
        {
            string testFilePath = Path.Combine(AppContext.BaseDirectory, "testAccessToken.txt");

            //if file is present, read the accessToken from it
            if (File.Exists(testFilePath))
            {
                accessToken = File.ReadAllText(testFilePath).Trim();

                //decode the token to check if it is valid and match the appId, objectId and tenantId
                var handler = new JsonWebTokenHandler();
                var token = handler.ReadJsonWebToken(accessToken);

                // Check if the token is expired
                if (token.ValidTo < DateTime.UtcNow)
                {
                    throw new Exception("Access token is expired.");
                }

                // Extract claims from the token
                var appId = token.Claims.FirstOrDefault(c => c.Type == "appid")?.Value;
                var objectId = token.Claims.FirstOrDefault(c => c.Type == "oid")?.Value;
                var tenantId = token.Claims.FirstOrDefault(c => c.Type == "tid")?.Value;

                // Validate the appId, objectId, and tenantId
                var expectedAppId = "#{Test_MI_AppId}#";
                var expectedObjectId = "#{Test_MI_ObjectId}#";
                var expectedTenantId = "#{Test_MI_TenantId}#";

                if (appId != expectedAppId || objectId != expectedObjectId || tenantId != expectedTenantId)
                {
                    throw new Exception("Invalid access token.");
                }

                return true;
            }

            return false;
        }

        public static async Task<AuthenticationResult> CommonLogin()
        {
            if (await IsTestFilePresent())
            {
                // If the test access token file is present, use the mock authentication result
                _mockAuthResult = CreateMockAuthResult();

                return _mockAuthResult;
            }
            return await Login();
        }

        private static AuthenticationResult CreateMockAuthResult()
        {
            // Create a minimal mock auth result for testing
            return new AuthenticationResult(
                accessToken: accessToken,
                isExtendedLifeTimeToken: false,
                uniqueId: "mock-unique-id",
                expiresOn: DateTimeOffset.Now.AddHours(1),
                extendedExpiresOn: DateTimeOffset.Now.AddHours(2),
                tenantId: "mock-tenant-id",
                account: null,
                idToken: null,
                scopes: new string[] { "https://management.azure.com/.default" },
                tokenType: "Bearer",
                correlationId: Guid.NewGuid()
            );
        }

        public static async Task<AuthenticationResult> TenantLogin(string tenantID)
        {
            ProjectDetails.InitializeTenantAuthentication(tenantID);
            return await Login();
        }

        public static async Task<AuthenticationResult> Login()
        {
            AuthenticationResult authResult = null;
            var accounts = await ProjectDetails.PublicClientApp.GetAccountsAsync();
            var firstAccount = accounts.FirstOrDefault();

            try
            {
                authResult = await ProjectDetails.clientApp.AcquireTokenSilent(scopes, firstAccount)
                                                          .ExecuteAsync();
            }
            catch (MsalUiRequiredException)
            {
                // A MsalUiRequiredException happened on AcquireTokenSilent.
                // This indicates you need to call AcquireTokenInteractive to acquire a token
                try
                {
                    SystemWebViewOptions systemWebViewOptions = new SystemWebViewOptions();

                    authResult = await ProjectDetails.clientApp.AcquireTokenInteractive(scopes)
                                                              .ExecuteAsync();
                }
                catch (MsalException exInteractiveLogin)
                {
                    throw new Exception($"Error during interactive login: {exInteractiveLogin.Message}");
                }
            }
            catch (Exception exLogin)
            {
                throw new Exception($"Error during login: {exLogin.Message}");
            }
            accessToken = authResult.AccessToken;
#if DEBUG
            Console.WriteLine(authResult.AccessToken);
            Console.WriteLine(authResult.Account.Username);
#endif
            return authResult;
        }

        public static async Task<PubSubResponse> GetPubSubUrlAndSessionId()
        {
            if (string.IsNullOrEmpty(accessToken))
            {
                throw new InvalidOperationException("Access token is not available. Please login first.");
            }

            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var requestBody = new
                {
                    accessToken = accessToken
                };

                var content = new StringContent(JsonConvert.SerializeObject(requestBody), System.Text.Encoding.UTF8, "application/json");

                var authUrl = "#{AuthURL}#";
                var sessionGenerateURL = $"{authUrl}/session/generate";
                var response = new HttpResponseMessage();
                try
                {
                    response = await httpClient.PostAsync(sessionGenerateURL, content);
                }
                catch (Exception ex) {
                    throw new Exception($"Error calling API: {ex.Message}");
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<PubSubResponse>(responseContent);

                return result;
            }
        }

        public class PubSubResponse
        {
            [JsonProperty("pubSubEndpoint")]
            public string PubSubEndpoint { get; set; }

            [JsonProperty("id")]
            public string Id { get; set; }
        }

        public static async Task Logout()
        {
            var accounts = (await ProjectDetails.PublicClientApp.GetAccountsAsync()).ToList();
            while (accounts.Any())
            {
                try
                {
                    await ProjectDetails.PublicClientApp.RemoveAsync(accounts.First());
                    accounts = (await ProjectDetails.PublicClientApp.GetAccountsAsync()).ToList();
                }
                catch (MsalException ex)
                {
                    throw new Exception($"Error during user logout: {ex.Message}");
                }
            }
        }

        public static async Task<AuthenticationResult> RetrieveAuthenticationToken()
        {
            AuthenticationResult authResult = null;
            var accounts = await ProjectDetails.PublicClientApp.GetAccountsAsync();
            var firstAccount = accounts.FirstOrDefault();

            try
            {
                // AcquireTokenSilent - Retrieves token from the encrypted cache. It auto-refreshes token based on AuthenticationResult.ExpiresOn
                authResult = await ProjectDetails.PublicClientApp.AcquireTokenSilent(scopes, firstAccount)
                                                          .ExecuteAsync();
            }
            catch (Exception exRetrieveAuthenticationToken)
            {
                throw new Exception($"Error during cached token retrieval: {exRetrieveAuthenticationToken.Message}");
            }
            return authResult;
        }
    }
}
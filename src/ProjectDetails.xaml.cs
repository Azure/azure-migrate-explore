// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Identity.Client;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Azure.Migrate.Explore.Authentication;
using Azure.Migrate.Explore.Common;
using Azure.Migrate.Explore.HttpRequestHelper;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using Microsoft.UI;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace AzureMigrateExplore
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ProjectDetails : Page
    {
        private MainWindow mainObj;
        private string DiscoverySiteName = null;
        private string AssessmentProjectName = null;
        public static string PowerShellClientId = "1950a258-227b-4e31-a9cf-717495945fc2";
        public static string CommonAuthorityEndpoint = "https://login.microsoftonline.com/common/oauth2/authorize";
        public static string TenantAuthorityEndpoint = "https://login.microsoftonline.com/_tenantID/oauth2/authorize";
        public static IPublicClientApplication clientApp;

        public ProjectDetails(MainWindow obj)
        {
            this.InitializeComponent();
            mainObj = obj;
            InitializeCommonAuthentication();
            //BeginAuthentication();
        }

        public static IPublicClientApplication PublicClientApp => clientApp;

        public static void InitializeCommonAuthentication()
        {

            clientApp = PublicClientApplicationBuilder.Create(PowerShellClientId).WithDefaultRedirectUri()
                                                      .WithAuthority(new Uri(CommonAuthorityEndpoint))
                                                      .Build();
            TokenCacheHelper.EnableSerialization(clientApp.UserTokenCache);
        }

        public static void InitializeTenantAuthentication(string tenantID)
        {
            string finalAuthorityEndpoint = TenantAuthorityEndpoint.Replace("_tenantID", tenantID);
            clientApp = PublicClientApplicationBuilder.Create(PowerShellClientId).WithDefaultRedirectUri()
                                                      .WithAuthority(new Uri(finalAuthorityEndpoint))
                                                      .Build();
            TokenCacheHelper.EnableSerialization(clientApp.UserTokenCache);
        }

        public async Task BeginAuthentication()
        {
            await InitializeAuthentication();
        }

        #region Authentication
        private void TenantIdEntry_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Enable the Confirm button if the TenantIdEntry is not empty
            ConfirmTenantIdChangeButton.IsEnabled = !string.IsNullOrWhiteSpace(TenantIdEntry.Text);

            // Update the TenantIdInfoLabel with the current text
            //TenantIdInfoLabel.Text = TenantIdEntry.Text;
        }

        public async Task InitializeAuthentication()
        {
            AuthenticationResult authResult = null;
            try
            {
                authResult = await AzureAuthenticationHandler.CommonLogin();
            }
            catch (Exception exCommonLogin)
            {
                await DisplayAlert("Error", $"Login failed. Please close the application and re-try: {exCommonLogin.Message}", "OK");
                mainObj.CloseForm();
            }

            TenantIdEntry.Text = authResult.TenantId;
            ConfirmTenantIdChangeButton.IsEnabled = false;
            TenantIdEntry.IsReadOnly = false;

            await InitializeSubscriptionPicker();
        }
        #endregion

        private async Task<bool> DisplayAlert(string title, string content, string primaryButtonText, string secondaryButtonText = null)
        {
            var dialog = new ContentDialog
            {
                Title = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Children =
                    {
                        new FontIcon
                        {
                            Glyph = "\uE946",
                            Margin = new Thickness(0, 0, 10, 0),
                            Foreground = new SolidColorBrush(Colors.DeepSkyBlue)
                        },
                        new TextBlock { Text = title, VerticalAlignment = VerticalAlignment.Center }
                    }
                },
                Content = content,
                PrimaryButtonText = primaryButtonText,
                CloseButtonText = secondaryButtonText,
                XamlRoot = this.Content.XamlRoot
            };

            var result = await dialog.ShowAsync();
            return result == ContentDialogResult.Primary;
        }

        #region Initialization
        private async Task InitializeSubscriptionPicker()
        {
            SubscriptionPicker.Items.Clear();
            SubscriptionPicker.IsEnabled = false;
            List<KeyValuePair<string, string>> Subscriptions = new List<KeyValuePair<string, string>>();

            //if test file is present, mock subscription will be shown
            if(await AzureAuthenticationHandler.IsTestFilePresent())
            {
                Subscriptions.Add(new KeyValuePair<string, string>("mock-subscription-id", "Mock Subscription - mock-subscription-id"));
                SubscriptionPicker.ItemsSource = Subscriptions;
                SubscriptionPicker.SelectedValuePath = "Key";
                SubscriptionPicker.DisplayMemberPath = "Value";
                SubscriptionPicker.IsEnabled = true;
                return;
            }

            HttpClientHelper httpClientHelperObj = new HttpClientHelper();

            string nextLink = Routes.ProtocolScheme + Routes.AzureManagementApiHostname + Routes.ForwardSlash +
                              Routes.SubscriptionPath +
                              Routes.QueryStringQuestionMark + Routes.QueryParameterApiVersion + Routes.QueryStringEquals + Routes.ProjectDetailsApiVersion;

            try
            {
                while (!string.IsNullOrEmpty(nextLink))
                {
                    List<KeyValuePair<string, string>> partialSubscriptionsList = new List<KeyValuePair<string, string>>();

                    JToken jsonTokenResponse = await httpClientHelperObj.GetProjectDetailsHttpJsonResponse(nextLink);
                    if (jsonTokenResponse.HasValues)
                    {
                        foreach (var x in jsonTokenResponse["value"])
                            partialSubscriptionsList.Add(new KeyValuePair<string, string>(x["subscriptionId"].Value<string>(), x["displayName"].Value<string>() + " - " + x["subscriptionId"].Value<string>()));
                        Subscriptions.AddRange(partialSubscriptionsList);

                        if (jsonTokenResponse["nextLink"] != null)
                            nextLink = jsonTokenResponse["nextLink"].Value<string>();
                        else
                            nextLink = null;
                    }
                }
            }
            catch (Exception exSubscriptions)
            {
                await DisplayAlert("Error", $"Could not retrieve Subscriptions data: {exSubscriptions.Message} Please re-login.", "OK");
                mainObj.CloseForm();
            }

            try
            {
                Subscriptions.Sort(CompareValue);
                
                if (Subscriptions.Count <= 0)
                {
                    SubscriptionInfoLabel.Text = "No Subscriptions found. Please change the Tenant ID.";
                    SubscriptionPicker.IsEnabled = false;
                    ResourceGroupPicker.IsEnabled = false;
                    AzureMigrateProjectPicker.IsEnabled = false;
                }
                else
                {
                    SubscriptionPicker.IsEnabled = true;
                }
                SubscriptionPicker.ItemsSource = Subscriptions;
                SubscriptionPicker.SelectedValuePath = "Key";
                SubscriptionPicker.DisplayMemberPath = "Value";
            }
            catch (Exception exSubscriptionDataHandling)
            {
                await DisplayAlert("Error", $"Error handling subscription data: {exSubscriptionDataHandling.Message} Please log an issue.", "OK");
                mainObj.CloseForm();
            }
        }
        #endregion

        private async Task InitializeResourceGroupNameComboBox()
        {
            //if test file is present, mock resource group will be shown
            if(AzureAuthenticationHandler.IsTestFilePresent().Result)
            {
                ResourceGroupPicker.ItemsSource = new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("mock-resource-group-id", "Mock Resource Group - mock-resource-group-id") };
                ResourceGroupPicker.SelectedValuePath = "Key";
                ResourceGroupPicker.DisplayMemberPath = "Value";
                ResourceGroupPicker.IsEnabled = true;
                return;
            }

            KeyValuePair<string, string> selectedSubscription = GetSelectedSubscription();
            if (string.IsNullOrEmpty(selectedSubscription.Key) || string.IsNullOrEmpty(selectedSubscription.Value))
            {
                await DisplayAlert("Error", "Empty Subscription ID or name. Please select Subscription again.", "OK");
                return;
            }
            ResourceGroupNameInfoLabel.Text = "";
            ResourceGroupPicker.IsEnabled = false;
            ResourceGroupPicker.Text = "Loading...";

            List<KeyValuePair<string, string>> ResourceGroups = new List<KeyValuePair<string, string>>();

            HttpClientHelper httpClientHelperObj = new HttpClientHelper();

            string nextLink = Routes.ProtocolScheme + Routes.AzureManagementApiHostname + Routes.ForwardSlash +
                              Routes.SubscriptionPath + Routes.ForwardSlash + selectedSubscription.Key + Routes.ForwardSlash +
                              Routes.ResourceGroupPath +
                              Routes.QueryStringQuestionMark + Routes.QueryParameterApiVersion + Routes.QueryStringEquals + Routes.ProjectDetailsApiVersion;

            try
            {
                while (!string.IsNullOrEmpty(nextLink))
                {
                    List<KeyValuePair<string, string>> partialResourceGroupList = new List<KeyValuePair<string, string>>();

                    JToken jsonTokenResponse = await httpClientHelperObj.GetProjectDetailsHttpJsonResponse(nextLink);
                    if (jsonTokenResponse.HasValues)
                    {
                        foreach (var x in jsonTokenResponse["value"] ?? Enumerable.Empty<JToken>())
                        {
                            var id = x["id"]?.Value<string>();
                            var name = x["name"]?.Value<string>();
                            if (id != null && name != null)
                            {
                                partialResourceGroupList.Add(new KeyValuePair<string, string>(id, name));
                            }
                        }
                        ResourceGroups.AddRange(partialResourceGroupList);

                        nextLink = jsonTokenResponse["nextLink"]?.Value<string>();
                    }
                    else
                    {
                        nextLink = null;
                    }
                }
            }
            catch (Exception exResourceGroups)
            {
                await DisplayAlert("Error", $"Could not retrieve Resource Group data: {exResourceGroups.Message} Please re-login.", "OK");
                mainObj.CloseForm();
            }

            try
            {
                ResourceGroups.Sort(CompareValue);
                if (ResourceGroups.Count <= 0)
                    ResourceGroupNameInfoLabel.Text = "No Resource Groups found. Please select a different Subscription ID";
                ResourceGroupPicker.ItemsSource = ResourceGroups;
                ResourceGroupPicker.SelectedValuePath = "Key";
                ResourceGroupPicker.DisplayMemberPath = "Value";
            }
            catch (Exception exResourceGroupDataHandling)
            {
                await DisplayAlert("Error", $"Error handling Resource Group data: {exResourceGroupDataHandling.Message} Please log an issue.", "OK");
                mainObj.CloseForm();
            }

            ResourceGroupPicker.IsEnabled = true;
            if (ResourceGroups.Count > 0)
                ResourceGroupPicker.Text = "Please select Resource Group";
        }

        private async Task InitializeAzureMigrateProjectNameComboBox()
        {
            //if test file is present, mock migrate project will be shown
            if(AzureAuthenticationHandler.IsTestFilePresent().Result)
            {
                AzureMigrateProjectPicker.ItemsSource = new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("mock-migrate-project-id", "Mock Migrate Project - mock-migrate-project-id") };
                AzureMigrateProjectPicker.SelectedValuePath = "Key";
                AzureMigrateProjectPicker.DisplayMemberPath = "Value";
                AzureMigrateProjectPicker.IsEnabled = true;
                return;
            }
            
            KeyValuePair<string, string> selectedSubscription = GetSelectedSubscription();
            KeyValuePair<string, string> selectedResourceGroup = GetSelectedResourceGroupName();

            if (string.IsNullOrEmpty(selectedSubscription.Key) || string.IsNullOrEmpty(selectedSubscription.Value))
            {
                await DisplayAlert("Error", "Empty Subscription ID or name. Please Select Subscription again.", "OK");
                return;
            }

            if (string.IsNullOrEmpty(selectedResourceGroup.Key) || string.IsNullOrEmpty(selectedResourceGroup.Value))
            {
                await DisplayAlert("Error", "Empty Resource Group ID or name. Please select Resource Group again.", "OK");
                return;
            }

            AzureMigrateProjectNameInfoLabel.Text = "";
            AzureMigrateProjectPicker.Text = "Loading...";
            AzureMigrateProjectPicker.IsEnabled = false;

            List<KeyValuePair<string, string>> AzureMigrateProjects = new List<KeyValuePair<string, string>>();

            HttpClientHelper httpClientHelperObj = new HttpClientHelper();

            string nextLink = Routes.ProtocolScheme + Routes.AzureManagementApiHostname + Routes.ForwardSlash +
                              Routes.SubscriptionPath + Routes.ForwardSlash + selectedSubscription.Key + Routes.ForwardSlash +
                              Routes.ResourceGroupPath + Routes.ForwardSlash + selectedResourceGroup.Value + Routes.ForwardSlash +
                              Routes.ProvidersPath + Routes.ForwardSlash + Routes.MigrateProvidersPath + Routes.ForwardSlash +
                              Routes.MigrateProjectsPath +
                              Routes.QueryStringQuestionMark + Routes.QueryParameterApiVersion + Routes.QueryStringEquals + Routes.ProjectDetailsApiVersion;

            try
            {
                while (!string.IsNullOrEmpty(nextLink))
                {
                    List<KeyValuePair<string, string>> partialAzureMigrateProjectList = new List<KeyValuePair<string, string>>();

                    JToken jsonTokenResponse = await httpClientHelperObj.GetProjectDetailsHttpJsonResponse(nextLink);
                    if (jsonTokenResponse.HasValues)
                    {
                        foreach (var x in jsonTokenResponse["value"] ?? Enumerable.Empty<JToken>())
                        {
                            var id = x["id"]?.Value<string>();
                            var name = x["name"]?.Value<string>();
                            if (id != null && name != null)
                            {
                                partialAzureMigrateProjectList.Add(new KeyValuePair<string, string>(id, name));
                            }
                        }
                        AzureMigrateProjects.AddRange(partialAzureMigrateProjectList);

                        nextLink = jsonTokenResponse["nextLink"]?.Value<string>();
                    }
                    else
                    {
                        nextLink = null;
                    }
                }
            }
            catch (Exception exAzureMigrateProjects)
            {
                await DisplayAlert("Error", $"Could not retrieve Azure Migrate projects data: {exAzureMigrateProjects.Message} Please re-login.", "OK");
                mainObj.CloseForm();
            }

            try
            {
                AzureMigrateProjects.Sort(CompareValue);
                if (AzureMigrateProjects.Count <= 0)
                    AzureMigrateProjectNameInfoLabel.Text = "No Migrate projects found. Please select a different Resource group.";
                AzureMigrateProjectPicker.ItemsSource = AzureMigrateProjects;
                AzureMigrateProjectPicker.SelectedValuePath = "Key";
                AzureMigrateProjectPicker.DisplayMemberPath = "Value";

            }
            catch (Exception exAzureMigrateProjectDataHandling)
            {
                await DisplayAlert("Error", $"Error handling Migrate project data: {exAzureMigrateProjectDataHandling.Message} Please log an issue.", "OK");
                mainObj.CloseForm();
            }

            AzureMigrateProjectPicker.IsEnabled = true;
            if (AzureMigrateProjects.Count > 0)
                AzureMigrateProjectPicker.Text = "Please select Migrate Project";
        }

        private async Task InitializeDiscoverySiteName()
        {
            KeyValuePair<string, string> selectedSubscription = GetSelectedSubscription();
            KeyValuePair<string, string> selectedResourceGroup = GetSelectedResourceGroupName();
            KeyValuePair<string, string> selectedAzureMigrateProject = GetSelectedAzureMigrateProject();

            if (string.IsNullOrEmpty(selectedSubscription.Key) || string.IsNullOrEmpty(selectedSubscription.Value))
            {
                await DisplayAlert("Error", "Empty Subscription ID or name. Please Select Subscription again.", "OK");
                return;
            }

            if (string.IsNullOrEmpty(selectedResourceGroup.Key) || string.IsNullOrEmpty(selectedResourceGroup.Value))
            {
                await DisplayAlert("Error", "Empty Resource Group ID or name. Please select Resource Group again.", "OK");
                return;
            }

            if (string.IsNullOrEmpty(selectedAzureMigrateProject.Key) || string.IsNullOrEmpty(selectedAzureMigrateProject.Value))
            {
                await DisplayAlert("Error", "Empty Azure Migrate project ID or name. Please select Migrate project again.", "OK");
                return;
            }

            SiteDiscoveryStatusInfoLabel.Text = "Fetching inventory data...";
            HttpClientHelper httpClientHelperObj = new HttpClientHelper();

            string url = Routes.ProtocolScheme + Routes.AzureManagementApiHostname + Routes.ForwardSlash +
                         Routes.SubscriptionPath + Routes.ForwardSlash + selectedSubscription.Key + Routes.ForwardSlash +
                         Routes.ResourceGroupPath + Routes.ForwardSlash + selectedResourceGroup.Value + Routes.ForwardSlash +
                         Routes.ProvidersPath + Routes.ForwardSlash + Routes.MigrateProvidersPath + Routes.ForwardSlash +
                         Routes.MigrateProjectsPath + Routes.ForwardSlash + selectedAzureMigrateProject.Value + Routes.ForwardSlash +
                         Routes.SolutionsPath + Routes.ForwardSlash + Routes.ServerDiscoveryPath +
                         Routes.QueryStringQuestionMark + Routes.QueryParameterApiVersion + Routes.QueryStringEquals + Routes.ProjectDetailsApiVersion;

            try
            {
                JToken jsonTokenResponse = await httpClientHelperObj.GetProjectDetailsHttpJsonResponse(url);
                if (jsonTokenResponse.HasValues)
                {
                    string masterSiteId = jsonTokenResponse["properties"]?["details"]?["extendedDetails"]?["masterSiteId"]?.Value<string>();
                    if (!string.IsNullOrEmpty(masterSiteId))
                    {
                        var masterSiteIdContents = masterSiteId.Split('/').ToList();
                        if (masterSiteIdContents.Count > 0)
                        {
                            DiscoverySiteName = masterSiteIdContents.Last();
                            var sitesUrl = Routes.ProtocolScheme + Routes.AzureManagementApiHostname + masterSiteId +
                                       Routes.QueryStringQuestionMark + Routes.QueryParameterApiVersion + Routes.QueryStringEquals +
                                       Routes.MasterSiteApiVersion;
                            var responseSites = (await httpClientHelperObj.GetProjectDetailsHttpJsonResponse(sitesUrl))["properties"]["sites"];
                            bool hasVmware = responseSites.Any(site => site.ToString().ToLower().Contains("vmwaresites"));
                            bool hasHyperV = responseSites.Any(site => site.ToString().ToLower().Contains("hypervsites"));
                            bool hasPhysical = responseSites.Any(site => site.ToString().ToLower().Contains("serversites"));
                            bool hasImport = responseSites.Any(site => site.ToString().ToLower().Contains("importsites"));
                            mainObj.SetHasApplianceInventory(hasVmware || hasHyperV || hasPhysical);
                            mainObj.SetHasImportInventory(hasImport);
                            if (!hasVmware && !hasHyperV && !hasPhysical && !hasImport)
                            {
                                await DisplayAlert("Error", "No discovery data was found. Discover your on-premises inventory to proceed ahead", "OK");
                                mainObj.CloseForm();
                            }
                        }
                        else
                        {
                            throw new Exception("masterSiteId response in json does not contain enough elements");
                        }
                    }
                }
            }
            catch (Exception exDiscoverySiteName)
            {
                await DisplayAlert("Error", $"Could not obtain discovery site name: {exDiscoverySiteName.Message} Please re-login.", "OK");
                mainObj.CloseForm();
            }
        }

        private async Task InitializeAssessmentProjectName()
        {
            KeyValuePair<string, string> selectedSubscription = GetSelectedSubscription();
            KeyValuePair<string, string> selectedResourceGroup = GetSelectedResourceGroupName();
            KeyValuePair<string, string> selectedAzureMigrateProject = GetSelectedAzureMigrateProject();

            if (string.IsNullOrEmpty(selectedSubscription.Key) || string.IsNullOrEmpty(selectedSubscription.Value))
            {
                await DisplayAlert("Error", "Empty Subscription ID or name. Please Select Subscription again.", "OK");
                return;
            }

            if (string.IsNullOrEmpty(selectedResourceGroup.Key) || string.IsNullOrEmpty(selectedResourceGroup.Value))
            {
                await DisplayAlert("Error", "Empty Resource Group ID or name. Please select Resource Group again.", "OK");
                return;
            }

            if (string.IsNullOrEmpty(selectedAzureMigrateProject.Key) || string.IsNullOrEmpty(selectedAzureMigrateProject.Value))
            {
                await DisplayAlert("Error", "Empty Azure Migrate project ID or name. Please select Migrate project again.", "OK");
                return;
            }

            HttpClientHelper httpClientHelperObj = new HttpClientHelper();

            string url = Routes.ProtocolScheme + Routes.AzureManagementApiHostname + Routes.ForwardSlash +
                         Routes.SubscriptionPath + Routes.ForwardSlash + selectedSubscription.Key + Routes.ForwardSlash +
                         Routes.ResourceGroupPath + Routes.ForwardSlash + selectedResourceGroup.Value + Routes.ForwardSlash +
                         Routes.ProvidersPath + Routes.ForwardSlash + Routes.MigrateProvidersPath + Routes.ForwardSlash +
                         Routes.MigrateProjectsPath + Routes.ForwardSlash + selectedAzureMigrateProject.Value + Routes.ForwardSlash +
                         Routes.SolutionsPath + Routes.ForwardSlash + Routes.ServerAssessmentPath +
                         Routes.QueryStringQuestionMark + Routes.QueryParameterApiVersion + Routes.QueryStringEquals + Routes.ProjectDetailsApiVersion;

            try
            {
                JToken jsonTokenResponse = await httpClientHelperObj.GetProjectDetailsHttpJsonResponse(url);
                if (jsonTokenResponse.HasValues)
                {
                    string projectId = jsonTokenResponse["properties"]?["details"]?["extendedDetails"]?["projectId"]?.Value<string>();
                    if (!string.IsNullOrEmpty(projectId))
                    {
                        var projectIdContents = projectId.Split('/').ToList();
                        if (projectIdContents.Count > 0)
                        {
                            AssessmentProjectName = projectIdContents.Last();
                            SiteDiscoveryStatusInfoLabel.Text = "Inventory data fetched successfully";
                        }
                        else
                        {
                            throw new Exception("projectId response in json does not contain enough elements");
                        }
                    }
                }
            }
            catch (Exception exAssessmentProjectName)
            {
                await DisplayAlert("Error", $"Could not obtain assessment project name: {exAssessmentProjectName.Message} Please re-login.", "OK");
                mainObj.CloseForm();
            }
        }

        #region Utilities
        private static int CompareKey(KeyValuePair<string, string> a, KeyValuePair<string, string> b)
        {
            return a.Key.CompareTo(b.Key);
        }

        private static int CompareValue(KeyValuePair<string, string> a, KeyValuePair<string, string> b)
        {
            return a.Value.CompareTo(b.Value);
        }
        #endregion

        #region Tenant ID Change Handler
        private async void TenantIdTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            AuthenticationResult authResult = null;
            try
            {
                authResult = await AzureAuthenticationHandler.RetrieveAuthenticationToken();
            }
            catch (Exception exRetrieveAzureAuthenticationToken)
            {
                await DisplayAlert("Error", $"Failed to retrieve authentication token. Please close the application and re-login: {exRetrieveAzureAuthenticationToken.Message}", "OK");
                return;
            }

            if (string.IsNullOrEmpty(TenantIdEntry.Text) || !ValidateTenantIdTextBox())
            {
                ConfirmTenantIdChangeButton.IsEnabled = false;
                return;
            }

            ConfirmTenantIdChangeButton.IsEnabled = true;
            //TenantIdInfoLabel.Text = TenantIdEntry.Text;

        }

        private async void ConfirmTenantIdChangeButton_Click(object sender, RoutedEventArgs e)
        {
            AuthenticationResult authResult = null;

            //TenantIdInfoLabel.Text = "";

            SubscriptionPicker.SelectedItem = null;
            SubscriptionPicker.ItemsSource = null;
            SubscriptionInfoLabel.Text = "";

            ResourceGroupPicker.SelectedItem = null;
            ResourceGroupPicker.ItemsSource = null;
            ResourceGroupNameInfoLabel.Text = "";

            AzureMigrateProjectPicker.SelectedItem = null;
            AzureMigrateProjectPicker.ItemsSource = null;
            AzureMigrateProjectNameInfoLabel.Text = "";

            DiscoverySiteName = null;
            SiteDiscoveryStatusInfoLabel.Text = "";

            AssessmentProjectName = null;

            mainObj.MakeProjectDetailsActionButtonEnableDecision();
            mainObj.MakeProjectDetailsTabButtonEnableDecision();

            try
            {
                await AzureAuthenticationHandler.Logout();
            }
            catch (Exception exLogout)
            {
                await DisplayAlert("Error", $"Logout failed: {exLogout.Message} Please re-login.", "OK");
                mainObj.CloseForm();
            }

            try
            {
                authResult = await AzureAuthenticationHandler.TenantLogin(TenantIdEntry.Text);
            }
            catch (Exception exTenantLogin)
            {
                await DisplayAlert("Error", $"Tenant login failed: {exTenantLogin.Message} Please restart application", "OK");
                mainObj.CloseForm();
            }

            if (!authResult.TenantId.Equals(TenantIdEntry.Text))
                TenantIdEntry.Text = "Entered and logged-in Tenant ID do not match";

            ConfirmTenantIdChangeButton.IsEnabled = false;

            await InitializeSubscriptionPicker();
        }
        #endregion

        private async void SubscriptionPicker_SelectedIndexChanged(object sender, RoutedEventArgs e)
        {
            ResourceGroupPicker.SelectedItem = null;
            ResourceGroupPicker.ItemsSource = null;
            ResourceGroupNameInfoLabel.Text = "";

            AzureMigrateProjectPicker.SelectedItem = null;
            AzureMigrateProjectPicker.ItemsSource = null;
            AzureMigrateProjectNameInfoLabel.Text = "";

            DiscoverySiteName = null;
            SiteDiscoveryStatusInfoLabel.Text = "";

            AssessmentProjectName = null;

            mainObj.MakeProjectDetailsActionButtonEnableDecision();
            mainObj.MakeProjectDetailsTabButtonEnableDecision();

            await InitializeResourceGroupNameComboBox();
        }

        private async void ResourceGroupPicker_SelectedIndexChanged(object sender, RoutedEventArgs e)
        {
            AzureMigrateProjectPicker.SelectedItem = null;
            AzureMigrateProjectPicker.ItemsSource = null;
            AzureMigrateProjectNameInfoLabel.Text = "";

            DiscoverySiteName = null;
            SiteDiscoveryStatusInfoLabel.Text = "";

            AssessmentProjectName = null;

            mainObj.MakeProjectDetailsActionButtonEnableDecision();
            mainObj.MakeProjectDetailsTabButtonEnableDecision();

            await InitializeAzureMigrateProjectNameComboBox();
        }

        private async void AzureMigrateProjectPicker_SelectedIndexChanged(object sender, RoutedEventArgs e)
        {
            DiscoverySiteName = null;
            SiteDiscoveryStatusInfoLabel.Text = "";

            AssessmentProjectName = null;

            mainObj.MakeProjectDetailsActionButtonEnableDecision();
            mainObj.MakeProjectDetailsTabButtonEnableDecision();

            await InitializeDiscoverySiteName();
            await InitializeAssessmentProjectName();

            mainObj.MakeProjectDetailsActionButtonEnableDecision();
            mainObj.MakeProjectDetailsTabButtonEnableDecision();
        }

        #region Validation
        public bool ValidateProjectDetails()
        {
            if (!ValidateTenantIdTextBox())
                return false;

            if (!ValidateSubscriptionComboBox())
                return false;

            if (!ValidateResourceGroupNameComboBox())
                return false;

            if (!ValidateAzureMigrateProjectNameComboBox())
                return false;

            if (!ValidateDiscoverySiteName() && !ValidateAssessmentProjectName())
                return false;

            return true;
        }

        private bool ValidateTenantIdTextBox()
        {
            if (string.IsNullOrEmpty(TenantIdEntry.Text))
                return false;

            else if (!Regex.IsMatch(TenantIdEntry.Text, @"^(\{){0,1}[0-9a-fA-F]{8}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{12}(\}){0,1}$"))
                return false;

            return true;
        }

        private bool ValidateSubscriptionComboBox()
        {
            if (SubscriptionPicker.SelectedItem == null)
                return false;

            else if (!Regex.IsMatch(((KeyValuePair<string, string>)SubscriptionPicker.SelectedItem).Key, @"^(\{){0,1}[0-9a-fA-F]{8}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{12}(\}){0,1}$"))
                return false;

            return true;
        }

        private bool ValidateResourceGroupNameComboBox()
        {
            if (ResourceGroupPicker.SelectedItem == null)
                return false;

            return true;
        }

        private bool ValidateAzureMigrateProjectNameComboBox()
        {
            if (AzureMigrateProjectPicker.SelectedItem == null)
                return false;

            return true;
        }

        private bool ValidateDiscoverySiteName()
        {
            if (string.IsNullOrEmpty(DiscoverySiteName))
                return false;

            return true;
        }

        private bool ValidateAssessmentProjectName()
        {
            if (string.IsNullOrEmpty(AssessmentProjectName))
                return false;

            return true;
        }
        #endregion

        #region Getter Methods
        public string GetTenantId()
        {
            return TenantIdEntry.Text;
        }

        public KeyValuePair<string, string> GetSelectedSubscription()
        {
            KeyValuePair<string, string> empty = new KeyValuePair<string, string>("", "");
            if (SubscriptionPicker.SelectedItem == null)
                return empty;

            return (KeyValuePair<string, string>)SubscriptionPicker.SelectedItem;
        }

        public KeyValuePair<string, string> GetSelectedResourceGroupName()
        {
            KeyValuePair<string, string> empty = new KeyValuePair<string, string>("", "");
            if (ResourceGroupPicker.SelectedItem == null)
                return empty;

            return (KeyValuePair<string, string>)ResourceGroupPicker.SelectedItem;
        }

        public KeyValuePair<string, string> GetSelectedAzureMigrateProject()
        {
            KeyValuePair<string, string> empty = new KeyValuePair<string, string>("", "");
            if (AzureMigrateProjectPicker.SelectedItem == null)
                return empty;

            return (KeyValuePair<string, string>)AzureMigrateProjectPicker.SelectedItem;
        }

        public string GetDiscoverySiteName()
        {
            return DiscoverySiteName;
        }

        public string GetAssessmentProjectName()
        {
            return AssessmentProjectName;
        }
        #endregion

        #region Event Handlers

        //private async void ConfirmTenantIdChangeButton_Click(object sender, RoutedEventArgs e)
        //{
        //    // Handle button click event
        //}

        //private async void SubscriptionPicker_SelectedIndexChanged(object sender, RoutedEventArgs e)
        //{
        //    // Handle subscription picker selection change
        //}

        //private async void ResourceGroupPicker_SelectedIndexChanged(object sender, RoutedEventArgs e)
        //{
        //    // Handle resource group picker selection change
        //}

        //private async void AzureMigrateProjectPicker_SelectedIndexChanged(object sender, RoutedEventArgs e)
        //{
        //    // Handle Azure Migrate project picker selection change
        //}

        private void OnBackButtonClicked(object sender, RoutedEventArgs e)
        {
            // Handle back button click
        }

        private void OnNextButtonClicked(object sender, RoutedEventArgs e)
        {
            // Handle next button click
        }

        #endregion
    }
}

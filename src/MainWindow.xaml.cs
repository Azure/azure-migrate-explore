// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.Storage;
using Azure.Migrate.Explore.Authentication;
using Azure.Migrate.Explore.Common;
using Azure.Migrate.Explore.Models;
using Azure.Migrate.Explore.Models.CopilotSummary;
using Azure.Migrate.Explore.CopilotSummary;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using AzureMigrateExplore;
using Windows.System;
using System.Diagnostics;
using System.Reflection;
using Microsoft.UI.Xaml.Input;
using Azure.Migrate.Explore.Summary;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Azure.Messaging.WebPubSub;
using Azure.Messaging.WebPubSub.Clients;
using static AzureMigrateExplore.ChatPage;
using Azure.Migrate.Explore.Excel;
using Azure.Migrate.Explore.Logger;
using Azure.Migrate.Explore.Models.CopilotSummary.CopilotClientContract;
using Windows.Media.Protection.PlayReady;
using Windows.UI.Notifications;
using Microsoft.Windows.AppNotifications.Builder;
using Microsoft.Windows.AppNotifications;
using DocumentFormat.OpenXml.Math;
using System.Reactive.Concurrency;
using System.Linq;

namespace AzureMigrateExplore
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Page
    {
        public ProjectDetails ProjectDetailsObj;
        public Configuration ConfigurationObj;
        private AssessmentSettings AssessmentSettingsObj;
        //private TrackProgress TrackProgressFormObj;
        private CopilotQuestionnaire CopilotQuestionnaireFormObj;
        private Welcome WelcomeObj;
        private ChatPage ChatPageObj;
        private NavigationViewItem? CurrentButtonTab;
        private WebPubSubClient _webPubSubClient;
        private bool HasImportInventory = false;
        private string selectedDirectory = string.Empty;
        private bool HasApplianceInventory = false;

        private DispatcherTimer _keepAliveTimer;

        private DispatcherTimer summaryGenerationTimer;

        private Stopwatch stopwatch = new Stopwatch();

        public MainWindow()
        {
            this.InitializeComponent();
            try
            {
                WelcomeLogin();
                WelcomeObj = new Welcome();
                WelcomeObj.LoginButtonClicked += Welcome_LoginButtonClicked;
                InitializeConfiguration();
                InitializeAssessmentSettings();
                InitializeCopilotQuestionnaire();
                InitializeChatPage();

                DisableOverlayGrid();
                HandleTabChange(WelcomeObj, WelcomeTabButton);

                ChatPageTabButton.IsEnabled = false;

                var VersionLabel = GetVersion();
                NavView.PaneTitle = "Azure Migrate Explore\n" + VersionLabel;
            }
            catch (Exception ex)
            {
                DisplayAlert("Initialization Error", $"An error occurred during initialization: {ex.Message}", "OK");
                CloseForm();
            }
        }
        private string GetVersion()
        {
            var version = Assembly.GetExecutingAssembly()
                                  .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                                  ?.InformationalVersion;
            return version ?? "v1.0.0";
        }
        private async void Welcome_LoginButtonClicked(object? sender, EventArgs e)
        {
            SigningInDialog.ShowAsync();
            InitializeProjectDetails();
            await ProjectDetailsObj.BeginAuthentication();
            SigningInDialog.Hide();
            
            // Ask user if they want a new instance or use existing
            ContentDialog instanceTypeDialog = new ContentDialog
            {
                Title = "Azure Migrate Explore",
                Content = "Would you like to create new reports or use existing ones?",
                PrimaryButtonText = "Use Existing",
                SecondaryButtonText = "Create New",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.Content.XamlRoot
            };
            
            var instanceChoice = await instanceTypeDialog.ShowAsync();
            // If user wants to use existing instance, show directory selection
            var directoriesList = getCustomerDirectories();
            
            // If the user cancels, return to welcome screen
            if (instanceChoice == ContentDialogResult.None)
            {
                return;
            }
            
            // If user wants to create a new instance
            if (instanceChoice == ContentDialogResult.Secondary)
            {
                bool validNameProvided = false;
                string instanceName = "";
                
                while (!validNameProvided)
                {
                    // Ask for instance name
                    TextBox nameInputBox = new TextBox 
                    { 
                        PlaceholderText = "Please enter a new name for this instance",
                        Text = instanceName // Keep previous input if re-prompting
                    };
                    
                    ContentDialog instanceNameDialog = new ContentDialog
                    {
                        Title = "Enter Instance Name",
                        Content = nameInputBox,
                        PrimaryButtonText = "OK",
                        SecondaryButtonText = "Cancel",
                        DefaultButton = ContentDialogButton.Primary,
                        XamlRoot = this.Content.XamlRoot
                    };

                    var instanceNameResult = await instanceNameDialog.ShowAsync();
                    
                    if (instanceNameResult == ContentDialogResult.Primary)
                    {
                        instanceName = nameInputBox.Text.Trim();
                        
                        if (string.IsNullOrWhiteSpace(instanceName))
                        {
                            ContentDialog errorDialog = new ContentDialog
                            {
                                Title = "Error",
                                Content = "Instance name cannot be empty.",
                                PrimaryButtonText = "OK",
                                XamlRoot = this.Content.XamlRoot
                            };
                            await errorDialog.ShowAsync();
                            // Continue loop to prompt again
                        }
                        else
                        {
                            // Check if this name already exists in directoriesList
                            bool nameExists = false;
                            foreach (var dir in directoriesList)
                            {
                                string folderName = Path.GetFileName(dir);
                                if (string.Equals(folderName, instanceName, StringComparison.OrdinalIgnoreCase))
                                {
                                    nameExists = true;
                                    break;
                                }
                            }
                            
                            if (nameExists)
                            {
                                ContentDialog duplicateDialog = new ContentDialog
                                {
                                    Title = "Name Already Exists",
                                    Content = "An instance with this name already exists. Please choose a different name.",
                                    PrimaryButtonText = "OK",
                                    XamlRoot = this.Content.XamlRoot
                                };
                                await duplicateDialog.ShowAsync();
                                // Continue loop to prompt again
                            }
                            else
                            {
                                // Valid new name provided
                                validNameProvided = true;
                            }
                        }
                    }
                    else
                    {
                        // User cancelled the name dialog
                        return; // Exit without proceeding
                    }
                }

                var newInstanceDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Project Reports\\", instanceName);

                // Set the instance directory as the selected directory
                this.selectedDirectory = newInstanceDirectory;
                UtilityFunctions.SetSelectedDirectory(newInstanceDirectory);
                
                // Continue with your existing flow
                HandleTabChange(ProjectDetailsObj, ProjectDetailsTabButton);
                WelcomeTabButton.Visibility = Visibility.Collapsed;
                NavView.Visibility = Visibility.Visible;
                return;
            }
            
            // Display directories in a dropdown for user selection
            if (directoriesList.Count > 0)
            {
                ContentDialog directoryDialog = new ContentDialog
                {
                    Title = "Select Customer Directory",
                    Content = new StackPanel(),
                    PrimaryButtonText = "Select",
                    CloseButtonText = "Cancel",
                    DefaultButton = ContentDialogButton.Primary,
                    XamlRoot = this.Content.XamlRoot
                };
                
                ComboBox directoriesComboBox = new ComboBox
                {
                    PlaceholderText = "Choose a directory",
                    Width = 300,
                    Margin = new Thickness(0, 10, 0, 0)
                };
                
                // Add directory names to the dropdown
                foreach (var dir in directoriesList)
                {
                    directoriesComboBox.Items.Add(Path.GetFileName(dir));
                }
                
                // Add the dropdown to the dialog's content
                var panel = directoryDialog.Content as StackPanel;
                panel.Children.Add(new TextBlock 
                { 
                    Text = "Please select a customer directory to continue:", 
                    Margin = new Thickness(0, 0, 0, 10) 
                });
                panel.Children.Add(directoriesComboBox);
                
                // Show the dialog and get the result
                var result = await directoryDialog.ShowAsync();

                // Process the selection
                if (result == ContentDialogResult.Primary && directoriesComboBox.SelectedItem != null)
                {
                    // Use the selected directory
                    string selectedDirName = directoriesComboBox.SelectedItem.ToString();
                    string selectedDirPath = directoriesList.First(d => Path.GetFileName(d) == selectedDirName);

                    // Set the selected directory as your working directory
                    this.selectedDirectory = selectedDirPath;
                    UtilityFunctions.SetSelectedDirectory(selectedDirPath);

                    bool hasDiscoveryReport = File.Exists(Path.Combine(selectedDirPath, "AzureMigrate_Discovery_Report.xlsx"));
                    bool hasAllFourReports = hasDiscoveryReport &&
                                            File.Exists(Path.Combine(selectedDirPath, "AzureMigrate_Assessment_Core_Report.xlsx")) &&
                                            File.Exists(Path.Combine(selectedDirPath, "AzureMigrate_Assessment_Clash_Report.xlsx")) &&
                                            File.Exists(Path.Combine(selectedDirPath, "AzureMigrate_Assessment_Opportunity_Report.xlsx"));
                    
                    // Navigate based on available reports
                    if (hasAllFourReports)
                    {
                        // If all four reports exist, go to Copilot Questionnaire
                        HandleTabChange(CopilotQuestionnaireFormObj, CopilotQuestionnaireTabButton);
                        DisableAssessmentSettingsTabButton();
                        DisableConfigurationTabButton();
                    }
                    else
                    {
                        // If only discovery report exists, go to project details to go through custom assessment flow
                        HandleTabChange(ProjectDetailsObj, ProjectDetailsTabButton);
                    }
                }
                else
                {
                    // User canceled, go to default view
                    HandleTabChange(ProjectDetailsObj, ProjectDetailsTabButton);
                }
            }
            else
            {
                // No directories found
                await DisplayAlert("No Directories", "No customer directories were found. Creating a new setup.", "OK");
                HandleTabChange(ProjectDetailsObj, ProjectDetailsTabButton);
            }
            
            WelcomeTabButton.Visibility = Visibility.Collapsed;
            NavView.Visibility = Visibility.Visible;
        }       
        public List<string> getCustomerDirectories()
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory + "Project Reports\\";
            var allSubdirectories = Directory.GetDirectories(baseDirectory).ToList();
            var validDirectories = new List<string>();

            foreach (var dir in allSubdirectories)
            {
                // Check if directory has the discovery report
                bool hasDiscoveryReport = File.Exists(Path.Combine(dir, "AzureMigrate_Discovery_Report.xlsx"));

                // Check if directory has all four reports
                bool hasAllFourReports = hasDiscoveryReport &&
                                        File.Exists(Path.Combine(dir, "AzureMigrate_Assessment_Core_Report.xlsx")) &&
                                        File.Exists(Path.Combine(dir, "AzureMigrate_Assessment_Clash_Report.xlsx")) &&
                                        File.Exists(Path.Combine(dir, "AzureMigrate_Assessment_Opportunity_Report.xlsx"));

                // Add directory to valid directories if it has just the discovery report or all four reports
                if (hasDiscoveryReport || hasAllFourReports)
                {
                    validDirectories.Add(dir);
                }
            }

            return validDirectories;
        }

        private void WelcomeLogin()
        {
            NavView.Visibility = Visibility.Collapsed;
            BackButton.Visibility = Visibility.Collapsed;
            NextButton.Visibility = Visibility.Collapsed;
            SubmitButton.Visibility = Visibility.Collapsed;
            StopButton.Visibility = Visibility.Collapsed;
            RetryButton.Visibility = Visibility.Collapsed;
            AssessButton.Visibility = Visibility.Collapsed;
            GenerateSummaryButton.Visibility = Visibility.Collapsed;
        }

        private void NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            var selectedItem = args.SelectedItem as NavigationViewItem;
            if (selectedItem != null)
            {
                switch (selectedItem.Tag)
                {
                    case "ProjectDetails":
                        HandleTabChange(ProjectDetailsObj, ProjectDetailsTabButton);
                        break;
                    case "Configuration":
                        HandleTabChange(ConfigurationObj, ConfigurationTabButton);
                        break;
                    case "AssessmentSettings":
                        AssessmentSettingsObj.Initialize();
                        HandleTabChange(AssessmentSettingsObj, AssessmentSettingsTabButton);
                        break;
                    case "CopilotQuestionnaire":
                        HandleTabChange(CopilotQuestionnaireFormObj, CopilotQuestionnaireTabButton);
                        break;
                    case "ChatPage":
                        HandleTabChange(ChatPageObj, ChatPageTabButton);
                        break;
                    case "Close":
                        CloseForm();
                        break;
                }
            }
        }

        private void InitializeConfiguration()
        {
            ConfigurationObj = new Configuration(this, AssessmentSettingsObj);
            ConfigurationObj.SetDefaultConfigurationValues();
        }

        private void InitializeProjectDetails()
        {
            ProjectDetailsObj = new ProjectDetails(this);
        }

        private void InitializeAssessmentSettings()
        {
            AssessmentSettingsObj = new AssessmentSettings(this);
            AssessmentSettingsObj.Initialize();
        }

        private void InitializeCopilotQuestionnaire()
        {
            CopilotQuestionnaireFormObj = new CopilotQuestionnaire(this);
            CopilotQuestionnaireFormObj.SetDefaultConfigurationValues();
            //GlobalConnection.SessionId = Guid.NewGuid().ToString()
        }

        private void InitializeChatPage()
        
        {
            ChatPageObj = new ChatPage(this, CopilotQuestionnaireFormObj);
        }

        public void EnableOverlayGrid()
        {
            OverlayGrid.Visibility = Visibility.Visible;
        }

        public void DisableOverlayGrid()
        {
            OverlayGrid.Visibility = Visibility.Collapsed;
        }

        #region Action Button click

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentButtonTab == ProjectDetailsTabButton)
            {
                ConfigurationObj.SetDefaultConfigurationValues();
                HandleTabChange(ConfigurationObj, ConfigurationTabButton);
            }
            else if (CurrentButtonTab == ConfigurationTabButton)
            {
                HandleTabChange(AssessmentSettingsObj, AssessmentSettingsTabButton);
                AssessmentSettingsObj.InitializeTargetRegionPicker();
            }
            else if (CurrentButtonTab == AssessmentSettingsTabButton)
            {
                HandleTabChange(CopilotQuestionnaireFormObj, CopilotQuestionnaireTabButton);
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentButtonTab == ConfigurationTabButton)
                HandleTabChange(ProjectDetailsObj, ProjectDetailsTabButton);
            else if (CurrentButtonTab == AssessmentSettingsTabButton)
                HandleTabChange(ConfigurationObj, ConfigurationTabButton);
            else if (CurrentButtonTab == ChatPageTabButton)
                HandleTabChange(CopilotQuestionnaireFormObj, CopilotQuestionnaireTabButton);
        }

        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {

            // Retrieve project details
            string tenantId = ProjectDetailsObj.GetTenantId();
            KeyValuePair<string, string> subscription = ProjectDetailsObj.GetSelectedSubscription();
            KeyValuePair<string, string> resourceGroup = ProjectDetailsObj.GetSelectedResourceGroupName();
            KeyValuePair<string, string> azureMigrateProject = ProjectDetailsObj.GetSelectedAzureMigrateProject();
            string discoverySiteName = ProjectDetailsObj.GetDiscoverySiteName();
            string assessmentProjectName = ProjectDetailsObj.GetAssessmentProjectName();

            // Retrieve configuration
            List<string> azureMigrateSourceAppliances = ConfigurationObj.GetAzureMigrateSourceAppliances();
            bool isExpressWorkflow = ConfigurationObj.IsExpressWorkflowSelected();
            string businessProposal = ConfigurationObj.GetBusinessProposal();
            string module = ConfigurationObj.GetModule();

            // Retrieve assessment settings
            KeyValuePair<string, string> targetRegion = AssessmentSettingsObj.GetTargetRegion();
            KeyValuePair<string, string> currency = AssessmentSettingsObj.GetCurrency();
            KeyValuePair<string, string> assessmentDuration = AssessmentSettingsObj.GetAssessmentDuration();
            KeyValuePair<string, string> optimizationPreference = AssessmentSettingsObj.GetSelectedOptimizationPreference();
            bool assessSqlServicesSeparately = AssessmentSettingsObj.IsAssessSqlServicesSeparatelyChecked();

            UserInput userInputObj = new UserInput(
                                                    tenantId, subscription, resourceGroup, azureMigrateProject, discoverySiteName, assessmentProjectName,
                                                    azureMigrateSourceAppliances, isExpressWorkflow, businessProposal, module,
                                                    targetRegion, currency, assessmentDuration, optimizationPreference, assessSqlServicesSeparately
                                                  );

            AssessmentSettingsObj.DisableAssessmentQuestionnaire();
            HandleTabChange(AssessmentSettingsObj, AssessmentSettingsTabButton);
            AssessmentSettingsObj.BeginProcess(userInputObj);
        }

        private async void StopButton_Click(object sender, RoutedEventArgs e)
        {
            if (AssessmentSettingsObj.IsProcessCancellationRequested())
            {
                await DisplayAlert("Azure Migrate Explore", "Process cancellation request has been received.\nPlease wait until we terminate all processes.", "OK");
                return;
            }

            bool result = await DisplayAlert("Azure Migrate Explore", "Process has not finished, are you sure you want to stop? Please be patient as the process will stop at the next stable state once you select 'Yes'.", "Yes", "No");
            if (!result)
                return;

            AssessmentSettingsObj.CancelProcess();   
        }

        private void RetryButton_Click(object sender, RoutedEventArgs e)
        {
            HandleTabChange(ProjectDetailsObj, ProjectDetailsTabButton);
        }

        private void AssessButton_Click(object sender, RoutedEventArgs e)
        {
            ConfigurationObj.SetModule("Assessment");
            HandleTabChange(AssessmentSettingsObj, AssessmentSettingsTabButton);
        }

        private bool IsFileOpen(string filePath)
        {
            try
            {
                using (FileStream stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    stream.Close();
                }
            }
            catch (IOException)
            {
                return true;
            }
            return false;
        }

        private async void GenerateSummaryButton_Click(object sender, RoutedEventArgs e)
        {
            EnableOverlayGrid();
            // Clear the chat content
            ChatPageObj.ClearChatContent();

            ChatPageTabButton.IsEnabled = true;
            HandleTabChange(ChatPageObj, ChatPageTabButton);

            var pubSubResponse = await AzureAuthenticationHandler.GetPubSubUrlAndSessionId();
            GlobalConnection.PubSubUrl = pubSubResponse.PubSubEndpoint;
            GlobalConnection.SessionId = pubSubResponse.Id;

            string reportsDirectory = selectedDirectory;
            string[] reportFiles = new string[]
            {
                Path.Combine(reportsDirectory, "AzureMigrate_Assessment_Core_Report.xlsx"),
                Path.Combine(reportsDirectory, "AzureMigrate_Assessment_Clash_Report.xlsx"),
                Path.Combine(reportsDirectory, "AzureMigrate_Discovery_Report.xlsx"),
                Path.Combine(reportsDirectory, "AzureMigrate_Assessment_Opportunity_Report.xlsx")
            };

            foreach (var filePath in reportFiles)
            {
                if (IsFileOpen(filePath))
                {
                    await DisplayAlert("Files Open", "Please close all the Azure Migrate data files (.xlsx) before generating the summary.", "OK");
                    return;
                }
            }

            string customerName = CopilotQuestionnaireFormObj.GetCustomerName();
            string customerIndustry = CopilotQuestionnaireFormObj.GetCustomerIndustry();
            string motivation = CopilotQuestionnaireFormObj.GetMotivation();
            string datacenterLocation = CopilotQuestionnaireFormObj.GetDatacenterLocation();
            string aiOpportunities = CopilotQuestionnaireFormObj.GetAiOpportunities();
            string otherDetails = CopilotQuestionnaireFormObj.GetOtherDetails();
            bool aiAgreementCheckboxState = CopilotQuestionnaireFormObj.GetAIAgreementCheckboxState();
            LogHandler logger = new LogHandler();
            ImportCoreReport coreReportData = SummaryGenerationHelper.FetchCoreReportExcelData(logger);

            string optimizationPreference = coreReportData.CorePropertiesObj.OptimizationPreference;
            var tenantId = coreReportData.CorePropertiesObj.TenantId;
            var subscriptionId = coreReportData.CorePropertiesObj.Subscription;

            CopilotInput copilotInputObj = new CopilotInput(customerName, customerIndustry, motivation, datacenterLocation, aiOpportunities, otherDetails, optimizationPreference);

            logger.LogInformation("SessionId: " + GlobalConnection.SessionId);
            logger.LogInformation(JsonConvert.SerializeObject(copilotInputObj));
            logger.LogInformation("AIAgreementCheckbox: " + aiAgreementCheckboxState);

            List<string> migrationData = SummaryGenerationHelper.GetSummaryInsights(copilotInputObj.LoggerObj);

            StartTimer();

            GlobalConnection.UserAction = UserAction.GenerateSummary.ToString();
            var userQueryObject = new UserQuery
            {
                UserAction = GlobalConnection.UserAction,
                Payload = JsonConvert.SerializeObject(copilotInputObj),
                AIAgreementCheckboxState = aiAgreementCheckboxState
            };

            var payloadContent = new QueryInput
            {
                UserQuery = userQueryObject,
                MigrationData = JsonConvert.SerializeObject(migrationData),
                TenantId = tenantId,
                SubscriptionId = subscriptionId,
            };

            if (_webPubSubClient == null)
            {
                _webPubSubClient = new WebPubSubClient(new Uri(GlobalConnection.PubSubUrl));

                _webPubSubClient.Connected += async (WebPubSubConnectedEventArgs e) =>
                {
                    GlobalConnection.ConnectionId = e.ConnectionId;
                    Debug.WriteLine($"Connected with ConnectionId: {e.ConnectionId}");

                    var messageBodyObject = new
                    {
                        SessionId = GlobalConnection.SessionId,
                        ClientConnectionId = GlobalConnection.ConnectionId,
                        ServiceIdentifier = "azuremigrate-ameservice",
                        ServiceVersion = "v1",
                        Payload = new
                        {
                            Content = JsonConvert.SerializeObject(payloadContent)
                        }
                    };

                    string messageBody = JsonConvert.SerializeObject(
                        messageBodyObject,
                        new JsonSerializerSettings
                        {
                            ContractResolver = new CamelCasePropertyNamesContractResolver()
                        }
                    );

                    try
                    {
                        // Send custom event to server
                        await _webPubSubClient.SendEventAsync("message", BinaryData.FromString(messageBody), WebPubSubDataType.Text);
                        Debug.WriteLine("Message sent to WebPubSub successfully.");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error sending message to WebPubSub: {ex.Message}");
                        _ = DispatcherQueue.TryEnqueue(async () =>
                        {
                            await DisplayAlert("Error", $"{ex.Message} + Please click OK to close the app and retry.", "OK", "Cancel");
                        });
                        throw new Exception(ex.Message);
                    }
                };

                try
                {
                    await _webPubSubClient.StartAsync();
                    await ChatPageObj.ReceiveMessagesFromPubSub(_webPubSubClient);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error starting WebPubSub client: {ex.Message}");
                    throw;
                }
            }
            else
            {
                var messageBodyObject = new
                {
                    SessionId = GlobalConnection.SessionId,
                    ClientConnectionId = GlobalConnection.ConnectionId,
                    ServiceIdentifier = "azuremigrate-ameservice",
                    ServiceVersion = "v1",
                    Payload = new
                    {
                        Content = JsonConvert.SerializeObject(payloadContent)
                    }
                };

                string messageBody = JsonConvert.SerializeObject(
                    messageBodyObject,
                    new JsonSerializerSettings
                    {
                        ContractResolver = new CamelCasePropertyNamesContractResolver()
                    }
                );

                try
                {
                    // Send custom event to server
                    await _webPubSubClient.SendEventAsync("message", BinaryData.FromString(messageBody), WebPubSubDataType.Text);
                    Debug.WriteLine("Message sent to WebPubSub successfully.");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error sending message to WebPubSub: {ex.Message}");
                    // Handle the error as needed
                    _ = DispatcherQueue.TryEnqueue(async () =>
                    {
                        await DisplayAlert("Error", $"{ex.Message} + Please click OK to close the app and retry.", "OK", "Cancel");
                    });
                    throw;
                }
            }
        }

        public async Task ReGenerateSummary(string sectionName)
        {
            string customerName = CopilotQuestionnaireFormObj.GetCustomerName();
            string customerIndustry = CopilotQuestionnaireFormObj.GetCustomerIndustry();
            string motivation = CopilotQuestionnaireFormObj.GetMotivation();
            string datacenterLocation = CopilotQuestionnaireFormObj.GetDatacenterLocation();
            string aiOpportunities = CopilotQuestionnaireFormObj.GetAiOpportunities();
            string otherDetails = CopilotQuestionnaireFormObj.GetOtherDetails();
            bool aiAgreementCheckboxState = CopilotQuestionnaireFormObj.GetAIAgreementCheckboxState();
            
            LogHandler logger = new LogHandler();
            ImportCoreReport coreReportData = SummaryGenerationHelper.FetchCoreReportExcelData(logger);

            string optimizationPreference = coreReportData.CorePropertiesObj.OptimizationPreference;
            var tenantId = coreReportData.CorePropertiesObj.TenantId;
            var subscriptionId = coreReportData.CorePropertiesObj.Subscription;

            CopilotInput copilotInputObj = new CopilotInput(customerName, customerIndustry, motivation, datacenterLocation, aiOpportunities, otherDetails, optimizationPreference);

            List<string> migrationData = SummaryGenerationHelper.GetSummaryInsights(copilotInputObj.LoggerObj);

            StartTimer();

            GlobalConnection.UserAction = UserAction.UpdateSummary.ToString();
            var userQueryObject = new UserQuery
            {
                UserAction = GlobalConnection.UserAction,
                SectionName = sectionName,
                Payload = JsonConvert.SerializeObject(copilotInputObj),
                AIAgreementCheckboxState = aiAgreementCheckboxState
            };

            var payloadContent = new QueryInput
            {
                UserQuery = userQueryObject,
                MigrationData = JsonConvert.SerializeObject(migrationData),
                TenantId = tenantId,
                SubscriptionId = subscriptionId
            };

            var messageBodyObject = new
            {
                SessionId = GlobalConnection.SessionId,
                ClientConnectionId = GlobalConnection.ConnectionId,
                ServiceIdentifier = "azuremigrate-ameservice",
                ServiceVersion = "v1",
                Payload = new
                {
                    Content = JsonConvert.SerializeObject(payloadContent)
                }
            };

            string messageBody = JsonConvert.SerializeObject(
                messageBodyObject,
                new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                }
            );

            try
            {
                // Send custom event to server
                await _webPubSubClient.SendEventAsync("message", BinaryData.FromString(messageBody), WebPubSubDataType.Text);
                Debug.WriteLine("Message sent to WebPubSub successfully.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error sending message to WebPubSub: {ex.Message}");
                await DisplayAlert("Error", $"{ex.Message} + Please click OK to close the app and retry.", "OK", "Cancel");
                throw;
            }
        }

        public void StartTimer()
        {
            stopwatch.Reset();
            stopwatch.Start();
            Debug.WriteLine("Timer started");

            summaryGenerationTimer = new DispatcherTimer();
            summaryGenerationTimer.Interval = TimeSpan.FromMinutes(1);
            summaryGenerationTimer.Tick += SummaryGenerationTimer_Tick;
            summaryGenerationTimer.Start();
        }

        public void StopTimer()
        {
            stopwatch.Stop();
            summaryGenerationTimer.Stop();
        }

        private void SummaryGenerationTimer_Tick(object sender, object e)
        {
            if (stopwatch.Elapsed.TotalMinutes >= 10)
            {
                Debug.WriteLine($"Elapsed time: {stopwatch.Elapsed.TotalMinutes} minutes");
                stopwatch.Stop();
                summaryGenerationTimer.Stop();
                Debug.WriteLine("Timer stopped");
                DisplayAlert("Error", "An error occurred while processing your request. Please click OK to close the app and retry.", "OK", "Cancel");
            }
        }

        #endregion

        #region Tab Button Click

        public void HandleTabChange(Page formObjToActivate, NavigationViewItem nextButtonTab)
        {
            if (CurrentButtonTab != null)
                CurrentButtonTab.Background = new SolidColorBrush(Colors.White);

            DecideVisibleActionButtons(nextButtonTab);
            DecideEnabledActionButtons(nextButtonTab);
            DecideVisibleTabButtons();
            DecideEnabledTabButtons(nextButtonTab);
            if (nextButtonTab != null)
            {
                nextButtonTab.Background = new SolidColorBrush(ColorHelper.FromArgb(255, 120, 190, 255));
                CurrentButtonTab = nextButtonTab;
            }

            OpenChildForm(formObjToActivate);

            NavView.SelectedItem = nextButtonTab;
        }

        private async void OpenChildForm(Page formObjToActivate)
        {
            if (formObjToActivate == null)
            {
                await DisplayAlert("Error", "Failed to load the form. Please re-login", "OK");
                CloseForm();
                return;
            }

            contentFrame.Content = formObjToActivate;
        }


        #endregion

        private void EditCustomerDetailsButton_Click(object sender, RoutedEventArgs e)
        {
            HandleTabChange(CopilotQuestionnaireFormObj, CopilotQuestionnaireTabButton);
        }

        #region Form Closing

        public async Task CloseForm()
        {
            var previousTab = CurrentButtonTab; // Store the current tab
            bool result = await DisplayAlert("Sign Out", "Do you want to sign out and exit?", "Yes", "No");
            if (result)
            {
                await AzureAuthenticationHandler.Logout();
                await DisplayAlert("Azure Migrate Explore", "You have been signed out successfully.", "OK");

                this.InitializeComponent();
                try
                {
                    WelcomeLogin();
                    WelcomeObj = new Welcome();
                    WelcomeObj.LoginButtonClicked += Welcome_LoginButtonClicked;
                    InitializeConfiguration();
                    InitializeAssessmentSettings();
                    InitializeCopilotQuestionnaire();
                    InitializeChatPage();

                    DisableOverlayGrid();
                    HandleTabChange(WelcomeObj, WelcomeTabButton);

                    ChatPageTabButton.IsEnabled = false;

                    var VersionLabel = GetVersion();
                    NavView.PaneTitle = "Azure Migrate Explore\n" + VersionLabel;
                }
                catch (Exception ex)
                {
                    DisplayAlert("Initialization Error", $"An error occurred during initialization: {ex.Message}", "OK");
                    Application.Current.Exit();
                }
            }
            else
            {
                // Restore the previous tab
                if (previousTab != null)
                {
                    NavView.SelectedItem = previousTab;
                }
            }
        }

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
            if(result == ContentDialogResult.Primary)
            {
                return result == ContentDialogResult.Primary;
            }
            return result == ContentDialogResult.Secondary;
        }

        private async void AzureMigrateExploreMainForm_FormClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (AssessmentSettingsObj.IsBackGroundWorkerRunning())
            {
                bool resultTrackProgress = await DisplayAlert("Azure Migrate Explore", "Process has not finished, are you sure you want to close application?", "Yes", "No");
                if (!resultTrackProgress)
                {
                    e.Cancel = true;
                    return;
                }

                AssessmentSettingsObj.CancelProcess();
            }

            try
            {
                await AzureAuthenticationHandler.Logout();
            }
            catch (Exception exLogout)
            {
                await DisplayAlert("Error", $"Failed to logout. Please delete the cached token file. Error: {exLogout.Message}", "OK");
            }
        }


        #endregion

        #region Action Button Handler

        private void DecideVisibleActionButtons(NavigationViewItem nextButtonTab)
        {
            if (nextButtonTab == ProjectDetailsTabButton)
            {
                HideBackButton();
                ShowNextButton();
                HideStopButton();
                HideAssessButton();
                HideSubmitButton();
                HideRetryButton();

                HideGenerateSummaryButton();
            }
            else if (nextButtonTab == ConfigurationTabButton)
            {
                ShowBackButton();
                HideStopButton();
                HideAssessButton();

                if (ConfigurationObj.DisplaySubmitButton())
                {
                    ShowSubmitButton();
                    HideNextButton();
                }
                else
                {
                    ShowNextButton();
                    HideSubmitButton();
                }

                HideRetryButton();

                HideGenerateSummaryButton();
            }
            else if (nextButtonTab == AssessmentSettingsTabButton)
            {
                ShowBackButton();
                HideRetryButton();
                HideAssessButton();

                ShowSubmitButton();
                HideStopButton();
                HideNextButton();

                HideGenerateSummaryButton();
            }
            else if (nextButtonTab == CopilotQuestionnaireTabButton)
            {
                HideRetryButton();
                ShowBackButton();
                HideAssessButton();
                HideStopButton();
                HideNextButton();
                HideSubmitButton();

                ShowGenerateSummaryButton();
            }
            else if (nextButtonTab == CopilotChatTabButton)
            {
                HideRetryButton();
                HideNextButton();
                HideAssessButton();

                HideBackButton();
                HideSubmitButton();

                HideGenerateSummaryButton();
                HideStopButton();
            }
            else if (nextButtonTab == ChatPageTabButton)
            {
                HideRetryButton();
                HideNextButton();
                HideAssessButton();
                HideSubmitButton();
                HideGenerateSummaryButton();
                ShowBackButton();
            }
        }

        private void DecideEnabledActionButtons(NavigationViewItem nextButtonTab)
        {
            if (nextButtonTab == ProjectDetailsTabButton)
            {
                MakeProjectDetailsActionButtonEnableDecision();
            }
            else if (nextButtonTab == ConfigurationTabButton)
            {
                MakeConfigurationActionButtonsEnabledDecision();
            }
            else if (nextButtonTab == AssessmentSettingsTabButton)
            {
                MakeAssessmentSettingsActionButtonsEnabledDecision();
            }
            //else if (nextButtonTab == TrackProgressTabButton)
            //{
            //    MakeTrackProgressActionButtonsEnabledDecisions();
            //}
            else if (nextButtonTab == CopilotQuestionnaireTabButton)
            {
                MakeCopilotQuestionnaireActionButtonsEnabledDecisions();
            }
            else if (nextButtonTab == CopilotChatTabButton)
            {
                MakeCopilotChatActionButtonsEnabledDecisions();
            }
        }

        #endregion
        #region Tab Button Handler

        public void DecideVisibleTabButtons()
        {
            ShowProjectDetailsTabButton();
            ShowConfigurationTabButton();
            if (ConfigurationObj.DisplaySubmitButton())
                DisableAssessmentSettingsTabButton();
            else
                ShowAssessmentSettingsTabButton();
            //ShowTrackProgressTabButton();
            ShowCopilotQuestionnaireTabButton();
            if (UtilityFunctions.CheckCopilotQuestionnaireTabVisibility())
                EnableCopilotQuestionnaireTabButton();
            else
                DisableCopilotQuestionnaireTabButton();
            HideCopilotChatTabButton();
        }

        private void DecideEnabledTabButtons(NavigationViewItem nextButtonTab)
        {
            if (nextButtonTab == WelcomeTabButton)
            {
                WelcomeLogin();
            }
            else if (nextButtonTab == ProjectDetailsTabButton)
            {
                MakeProjectDetailsTabButtonEnableDecision();
            }
            else if (nextButtonTab == ConfigurationTabButton)
            {
                MakeConfigurationTabButtonEnableDecisions();
            }
            else if (nextButtonTab == AssessmentSettingsTabButton)
            {
                MakeAssessmentSettingsTabButtonEnableDecisions();
            }
            //else if (nextButtonTab == TrackProgressTabButton)
            //{
            //    MakeTrackProgressTabButtonEnableDecisions();
            //}
            else if (nextButtonTab == CopilotQuestionnaireTabButton)
            {
                MakeCopilotQuestionnaireTabButtonEnableDecisions();
            }
            else if (nextButtonTab == CopilotChatTabButton)
            {
                MakeCopilotChatTabButtonEnableDecisions();
            }
            else if (nextButtonTab == ChatPageTabButton)
            {
                NextButton.Visibility = Visibility.Collapsed;
                BackButton.Visibility = Visibility.Visible;
            }
        }

        #endregion

        #region Form Closing

        #endregion


        #region Tab Button Click

        private void ProjectDetailsTabButton_Click(object sender, RoutedEventArgs e)
        {
            HandleTabChange(ProjectDetailsObj, ProjectDetailsTabButton);
        }

        private void ConfigurationTabButton_Click(object sender, RoutedEventArgs e)
        {
            HandleTabChange(ConfigurationObj, ConfigurationTabButton);
        }

        private void AssessmentSettingsTabButton_Click(object sender, RoutedEventArgs e)
        {
            HandleTabChange(AssessmentSettingsObj, AssessmentSettingsTabButton);
        }

        private void CopilotQuestionnaireTabButton_Click(object sender, RoutedEventArgs e)
        {
            HandleTabChange(CopilotQuestionnaireFormObj, CopilotQuestionnaireTabButton);
        }

        #endregion

        #region Enable/Disable/Hide/Show Action Buttons

        public void SwitchToCopilotQuestionnaireTab()
        {
            HandleTabChange(CopilotQuestionnaireFormObj, CopilotQuestionnaireTabButton);
        }

        public void EnableNextButton()
        {
            NextButton.IsEnabled = true;
        }

        public void DisableNextButton()
        {
            NextButton.IsEnabled = false;
        }

        public void EnableSubmitButton()
        {
            SubmitButton.IsEnabled = true;
        }

        public void DisableSubmitButton()
        {
            SubmitButton.IsEnabled = false;
        }

        public void EnableBackButton()
        {
            BackButton.IsEnabled = true;
        }

        public void DisableBackButton()
        {
            BackButton.IsEnabled = false;
        }

        public void EnableStopButton()
        {
            StopButton.IsEnabled = true;
        }

        public void DisableStopButton()
        {
            StopButton.IsEnabled = false;
        }

        public void EnableRetryButton()
        {
            RetryButton.IsEnabled = true;
        }

        public void DisableRetryButton()
        {
            RetryButton.IsEnabled = false;
        }

        public void EnableAssessButton()
        {
            AssessButton.IsEnabled = true;
        }

        public void DisableAssessButton()
        {
            AssessButton.IsEnabled = false;
        }

        public void HideAssessButton()
        {
            AssessButton.Visibility = Visibility.Collapsed;
        }

        public void ShowAssessButton()
        {
            AssessButton.Visibility = Visibility.Visible;
        }

        public void HideNextButton()
        {
            NextButton.Visibility = Visibility.Collapsed;
        }

        public void ShowNextButton()
        {
            NextButton.Visibility = Visibility.Visible;
        }

        public void HideSubmitButton()
        {
            SubmitButton.Visibility = Visibility.Collapsed;
        }

        public void ShowSubmitButton()
        {
            SubmitButton.Visibility = Visibility.Visible;
        }

        public void ShowBackButton()
        {
            BackButton.Visibility = Visibility.Visible;
        }

        public void HideBackButton()
        {
            BackButton.Visibility = Visibility.Collapsed;
        }

        public void ShowRetryButton()
        {
            RetryButton.Visibility = Visibility.Visible;
        }

        public void HideRetryButton()
        {
            RetryButton.Visibility = Visibility.Collapsed;
        }

        public void ShowStopButton()
        {
            StopButton.Visibility = Visibility.Visible;
        }

        public void HideStopButton()
        {
            StopButton.Visibility = Visibility.Collapsed;
        }

        public void EnableGenerateSummaryButton()
        {
            GenerateSummaryButton.IsEnabled = true;
        }

        public void DisableGenerateSummaryButton()
        {
            GenerateSummaryButton.IsEnabled = false;
        }

        public void ShowGenerateSummaryButton()
        {
            GenerateSummaryButton.Visibility = Visibility.Visible;
        }

        public void HideGenerateSummaryButton()
        {
            GenerateSummaryButton.Visibility = Visibility.Collapsed;
        }

        #endregion

        #region Enable/Disable/Hide/Show Tab Buttons

        public void EnableProjectDetailsTabButton()
        {
            ProjectDetailsTabButton.IsEnabled = true;
        }

        public void DisableProjectDetailsTabButton()
        {
            ProjectDetailsTabButton.IsEnabled = false;
        }

        public void EnableConfigurationTabButton()
        {
            ConfigurationTabButton.IsEnabled = true;
        }

        public void DisableConfigurationTabButton()
        {
            ConfigurationTabButton.IsEnabled = false;
        }

        public void EnableAssessmentSettingsTabButton()
        {
            AssessmentSettingsTabButton.IsEnabled = true;
        }

        public void DisableAssessmentSettingsTabButton()
        {
            AssessmentSettingsTabButton.IsEnabled = false;
        }

        public void EnableCopilotQuestionnaireTabButton()
        {
            CopilotQuestionnaireTabButton.IsEnabled = true;
        }

        public void DisableCopilotQuestionnaireTabButton()
        {
            CopilotQuestionnaireTabButton.IsEnabled = false;
        }

        public void EnableCopilotChatTabButton()
        {
            CopilotChatTabButton.IsEnabled = true;
        }

        public void DisableCopilotChatTabButton()
        {
            CopilotChatTabButton.IsEnabled = false;
        }

        public void ShowProjectDetailsTabButton()
        {
            ProjectDetailsTabButton.Visibility = Visibility.Visible;
        }

        public void HideProjectDetailsTabButton()
        {
            ProjectDetailsTabButton.Visibility = Visibility.Collapsed;
        }

        public void ShowConfigurationTabButton()
        {
            ConfigurationTabButton.Visibility = Visibility.Visible;
        }

        public void HideConfigurationTabButton()
        {
            ConfigurationTabButton.Visibility = Visibility.Collapsed;
        }

        public void ShowAssessmentSettingsTabButton()
        {
            AssessmentSettingsTabButton.Visibility = Visibility.Visible;
        }

        public void HideAssessmentSettingsTabButton()
        {
            AssessmentSettingsTabButton.Visibility = Visibility.Collapsed;
        }

        public void ShowCopilotQuestionnaireTabButton()
        {
            CopilotQuestionnaireTabButton.Visibility = Visibility.Visible;
        }

        public void HideCopilotQuestionnaireTabButton()
        {
            CopilotQuestionnaireTabButton.Visibility = Visibility.Collapsed;
        }

        public void ShowCopilotChatTabButton()
        {
            CopilotChatTabButton.Visibility = Visibility.Visible;
        }

        public void HideCopilotChatTabButton()
        {
            CopilotChatTabButton.Visibility = Visibility.Collapsed;
        }


        #endregion

        #region Tab specific action button Enable/Disable/Hide/Show decision makers

        public void MakeProjectDetailsActionButtonEnableDecision()
        {
            HideBackButton();
            HideRetryButton();
            HideAssessButton();

            if (ProjectDetailsObj.ValidateProjectDetails())
            {
                ShowNextButton();
                EnableNextButton();

                HideSubmitButton();
                HideStopButton();
            }
            else
            {
                ShowNextButton();
                DisableNextButton();

                HideSubmitButton();
                HideStopButton();
            }
        }

        public void MakeConfigurationActionButtonsEnabledDecision()
        {
            ShowBackButton();
            EnableBackButton();
            HideRetryButton();
            HideAssessButton();

            if (ConfigurationObj.ValidateConfiguration())
            {
                if (ConfigurationObj.DisplaySubmitButton())
                {
                    ShowSubmitButton();
                    if (!AssessmentSettingsObj.IsBackGroundWorkerRunning())
                        EnableSubmitButton();
                    else
                        DisableSubmitButton();

                    HideNextButton();
                    HideStopButton();
                }
                else
                {
                    ShowNextButton();
                    EnableNextButton();

                    HideStopButton();
                    HideSubmitButton();
                }
            }
            else
            {
                if (ConfigurationObj.DisplaySubmitButton())
                {
                    ShowSubmitButton();
                    DisableSubmitButton();

                    HideStopButton();
                    HideNextButton();
                }
                else
                {
                    ShowNextButton();
                    DisableNextButton();

                    HideStopButton();
                    HideSubmitButton();
                }
            }
        }

        public void MakeAssessmentSettingsActionButtonsEnabledDecision()
        {
            ShowBackButton();
            EnableBackButton();
            HideRetryButton();
            HideAssessButton();

            ShowSubmitButton();
            HideNextButton();
            HideStopButton();

            if (AssessmentSettingsObj.ValidateAssessmentSettings() && !AssessmentSettingsObj.IsBackGroundWorkerRunning())
                EnableSubmitButton();
            else
                DisableSubmitButton();
        }

        public void MakeTrackProgressActionButtonsEnabledDecisions(bool showAndEnableAssessButton = false, bool showAndEnableRetryButton = false, bool enableNextButton = false)
        {
            HideStopButton();
            HideNextButton();
            HideSubmitButton();
            HideRetryButton();
            HideBackButton();
            HideAssessButton();

            if (AssessmentSettingsObj.DisplayActionButtonDecision() == 1)
            {
                ShowAssessButton();
                EnableAssessButton();

                DisableRetryButton();
                HideRetryButton();

                DisableAssessmentSettingsTabButton();
            }
            else if (AssessmentSettingsObj.DisplayActionButtonDecision() == 2)
            {
                ShowRetryButton();
                EnableRetryButton();
                ShowBackButton();
                EnableBackButton();

                DisableAssessButton();
                HideAssessButton();
            }
            else if (AssessmentSettingsObj.DisplayActionButtonDecision() == 3)
            {
                DisableRetryButton();
                HideRetryButton();

                DisableAssessButton();
                HideAssessButton();
            }

            if (AssessmentSettingsObj.IsBackGroundWorkerRunning())
            {
                ShowStopButton();
                EnableStopButton();
            }
            else
            {
                DisableStopButton();
                HideStopButton();
            }

            if (showAndEnableAssessButton)
            {
                ShowAssessButton();
                EnableAssessButton();
            }
            else if (showAndEnableRetryButton)
            {
                ShowRetryButton();
                EnableRetryButton();
                ShowBackButton();
                EnableBackButton();
            }
        }

        public void MakeCopilotQuestionnaireActionButtonsEnabledDecisions()
        {
            HideRetryButton();
            HideAssessButton();
            HideBackButton();
            HideStopButton();
            HideSubmitButton();
            HideNextButton();

            ShowGenerateSummaryButton();
            if (UtilityFunctions.CheckCopilotQuestionnaireTabVisibility() && CopilotQuestionnaireFormObj.ValidateQuestionnaireDetails())
            {
                EnableGenerateSummaryButton();
            }
            //else
            //{
            //    DisableGenerateSummaryButton();
            //}
        }

        public void MakeCopilotChatActionButtonsEnabledDecisions()
        {
            HideRetryButton();
            HideAssessButton();
            HideNextButton();
            HideSubmitButton();
            HideBackButton();
            HideGenerateSummaryButton();
            HideStopButton();
            DisableStopButton();
        }

        #endregion

        #region Tab specific tab button Enable/Disable/Hide/Show decision makers

        public void MakeProjectDetailsTabButtonEnableDecision()
        {
            if (UtilityFunctions.CheckCopilotQuestionnaireTabVisibility())
            {
                ShowCopilotQuestionnaireTabButton();
                EnableCopilotQuestionnaireTabButton();
                HideCopilotChatTabButton();
                DisableCopilotChatTabButton();
            }
            else
            {
                ShowCopilotQuestionnaireTabButton();
                DisableCopilotQuestionnaireTabButton();
                HideCopilotChatTabButton();
                DisableCopilotChatTabButton();
            }

            bool displaySubmitButton = ConfigurationObj.DisplaySubmitButton();
            if (ProjectDetailsObj == null || !ProjectDetailsObj.ValidateProjectDetails())
            {
                DisableConfigurationTabButton();
                if (displaySubmitButton)
                    DisableAssessmentSettingsTabButton();
                else
                    ShowAssessmentSettingsTabButton();
                DisableAssessmentSettingsTabButton();
                return;
            }

            EnableConfigurationTabButton();

            if (ConfigurationObj == null || !ConfigurationObj.ValidateConfiguration())
            {
                if (displaySubmitButton)
                    DisableAssessmentSettingsTabButton();
                else
                    ShowAssessmentSettingsTabButton();
                DisableAssessmentSettingsTabButton();
                return;
            }

            if (displaySubmitButton)
            {
                DisableAssessmentSettingsTabButton();
                return;
            }

            //ShowAssessmentSettingsTabButton();
            //EnableAssessmentSettingsTabButton();

            if (AssessmentSettingsObj == null || !AssessmentSettingsObj.ValidateAssessmentSettings())
            {
                return;
            }

            if (AssessmentSettingsObj.IsBackGroundWorkerRunning())
            {
                DisableCopilotQuestionnaireTabButton();
            }
        }

        public void MakeConfigurationTabButtonEnableDecisions()
        {
            if (UtilityFunctions.CheckCopilotQuestionnaireTabVisibility())
            {
                ShowCopilotQuestionnaireTabButton();
                EnableCopilotQuestionnaireTabButton();
                HideCopilotChatTabButton();
                DisableCopilotChatTabButton();
            }
            else
            {
                ShowCopilotQuestionnaireTabButton();
                DisableCopilotQuestionnaireTabButton();
                HideCopilotChatTabButton();
                DisableCopilotChatTabButton();
            }

            ShowAssessmentSettingsTabButton();
            EnableAssessmentSettingsTabButton();

            if (AssessmentSettingsObj == null || !AssessmentSettingsObj.ValidateAssessmentSettings())
            {
                return;
            }

            if (AssessmentSettingsObj.IsBackGroundWorkerRunning())
            {
                DisableCopilotQuestionnaireTabButton();
            }
        }

        public void MakeAssessmentSettingsTabButtonEnableDecisions()
        {
            if (AssessmentSettingsObj.IsBackGroundWorkerRunning())
            {
                DisableCopilotQuestionnaireTabButton();
            }
            else if (UtilityFunctions.CheckCopilotQuestionnaireTabVisibility())
            {
                ShowCopilotQuestionnaireTabButton();
                EnableCopilotQuestionnaireTabButton();
                HideCopilotChatTabButton();
                DisableCopilotChatTabButton();
            }
            else
            {
                ShowCopilotQuestionnaireTabButton();
                DisableCopilotQuestionnaireTabButton();
                HideCopilotChatTabButton();
                DisableCopilotChatTabButton();
            }

            if (AssessmentSettingsObj == null || !AssessmentSettingsObj.ValidateAssessmentSettings())
            {
                return;
            }
        }

        public void MakeTrackProgressTabButtonEnableDecisions()
        {
            // Previous state must be retained for AME, enabled tab buttons do not change.
            if (AssessmentSettingsObj.IsBackGroundWorkerRunning())
            {
                DisableCopilotQuestionnaireTabButton();
                DisableAssessmentSettingsTabButton();
                DisableConfigurationTabButton();
                DisableProjectDetailsTabButton();
            }
            else if (UtilityFunctions.CheckCopilotQuestionnaireTabVisibility())
            {
                ShowCopilotQuestionnaireTabButton();
                EnableCopilotQuestionnaireTabButton();
                HideCopilotChatTabButton();
                DisableCopilotChatTabButton();
            }
            else
            {
                ShowCopilotQuestionnaireTabButton();
                DisableCopilotQuestionnaireTabButton();
                HideCopilotChatTabButton();
                DisableCopilotChatTabButton();
            }
        }

        public void MakeCopilotQuestionnaireTabButtonEnableDecisions()
        {
            HideCopilotChatTabButton();
            DisableCopilotChatTabButton();
        }

        public void MakeCopilotChatTabButtonEnableDecisions()
        {
            // Previous state must be retained, enabled tab buttons do not change.
        }

        #endregion

        #region Utilities
        public bool IsAvsBusinessProposalSelected()
        {
            return (ConfigurationObj.GetBusinessProposal() == BusinessProposal.AVS.ToString());
        }

        private void PrivacyDocButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var uri = new Uri("https://aka.ms/privacy");
            var success = Windows.System.Launcher.LaunchUriAsync(uri);
        }

        private void HelperDocButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var uri = new Uri("https://aka.ms/AMECopilotDocs");
            var success = Windows.System.Launcher.LaunchUriAsync(uri);
        }

        private void BottomDockCloseButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            CloseForm();
        }

        public void SetHasImportInventory(bool hasImportInventory)
        {
            HasImportInventory = hasImportInventory;
        }

        public void SetHasApplianceInventory(bool hasApplianceInventory)
        {
            HasApplianceInventory = hasApplianceInventory;
        }
        public bool GetHasImportInventory()
        {
            return HasImportInventory;
        }

        public bool GetHasApplianceInventory()
        {
            return HasApplianceInventory;
        }

        //public void DisableOptimizationPreferenceComboBox()
        //{
        //    if (AssessmentSettingsObj == null)
        //        return;
        //    AssessmentSettingsObj.InitializeCurrencyPicker();
        //    AssessmentSettingsObj.InitializeTargetRegionPicker();
        //    AssessmentSettingsObj.DisableOptimizationPreferenceComboBox();
        //}

        //public void EnableOptimizationPreferenceComboBox()
        //{
        //    if (AssessmentSettingsObj == null)
        //        return;
        //    AssessmentSettingsObj.InitializeCurrencyPicker();
        //    AssessmentSettingsObj.InitializeTargetRegionPicker();
        //    AssessmentSettingsObj.EnableOptimizationPreferenceComboBox();
        //}

        //public void DisableAssessmentDurationComboBox()
        //{
        //    if (AssessmentSettingsObj == null)
        //        return;
        //    AssessmentSettingsObj.InitializeCurrencyPicker();
        //    AssessmentSettingsObj.InitializeTargetRegionPicker();
        //    AssessmentSettingsObj.DisableAssessmentDurationComboBox();
        //}

        //public void EnableAssessmentDurationComboBox()
        //{
        //    if (AssessmentSettingsObj == null)
        //        return;
        //    AssessmentSettingsObj.InitializeCurrencyPicker();
        //    AssessmentSettingsObj.InitializeTargetRegionPicker();
        //    AssessmentSettingsObj.EnableAssessmentDurationComboBox();
        //}
        #endregion
    }
}

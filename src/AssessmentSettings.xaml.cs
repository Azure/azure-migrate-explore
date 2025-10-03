// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Azure.Migrate.Explore.Processor;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI;
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
using Azure.Migrate.Explore.Common;
using Azure.Migrate.Explore.Models;
using System.ComponentModel;
using Azure.Migrate.Explore.Logger;
using Azure.Migrate.Explore.Discovery;
using Azure.Migrate.Explore.Factory;
using Azure.Migrate.Explore.Assessment;
using static Azure.Migrate.Explore.Discovery.Discover;
using Microsoft.Windows.AppNotifications.Builder;
using Microsoft.Windows.AppNotifications;
using Microsoft.IdentityModel.Tokens;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace AzureMigrateExplore
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AssessmentSettings : Page
    {
        private readonly MainWindow mainObj;
        private BackgroundWorker azureMigrateExploreBackgroundWorker;
        private UserInput UserInputObj;

        public AssessmentSettings(MainWindow obj)
        {
            this.InitializeComponent();
            mainObj = obj;
            Initialize();
            InitializeBackgroundWorker();
        }
        #region Initialization
        public void Initialize()
        {
            InitializeCurrencyPicker();
            InitializeAssessmentDurationPicker();
            InitializeTargetRegionPicker();
            //InitializeOptimizationPreference(BusinessProposal.Comprehensive);
        }

        public void InitializeTargetRegionPicker()
        {
            List<KeyValuePair<string, string>> location;

            if (mainObj.IsAvsBusinessProposalSelected())
            {
                location = new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>("australiaeast", "Australia East"),
                    new KeyValuePair<string, string>("canadacentral", "Canada Central"),
                    new KeyValuePair<string, string>("centralindia", "Central India"),
                    new KeyValuePair<string, string>("centralus", "Central US"),
                    new KeyValuePair<string, string>("eastasia", "East Asia"),
                    new KeyValuePair<string, string>("eastus", "East US"),
                    new KeyValuePair<string, string>("eastus2", "East US 2"),
                    new KeyValuePair<string, string>("germanywestcentral", "Germany West Central"),
                    new KeyValuePair<string, string>("italynorth", "Italy North"),
                    new KeyValuePair<string, string>("northcentralus", "North Central US"),
                    new KeyValuePair<string, string>("qatarcentral", "Qatar Central"),
                    new KeyValuePair<string, string>("southcentralus", "South Central US"),
                    new KeyValuePair<string, string>("southeastasia", "Southeast Asia"),
                    new KeyValuePair<string, string>("switzerlandnorth", "Switzerland North"),
                    new KeyValuePair<string, string>("switzerlandwest", "Switzerland South"),
                    new KeyValuePair<string, string>("uaenorth", "UAE North"),
                    new KeyValuePair<string, string>("uksouth", "UK South"),
                    new KeyValuePair<string, string>("westeurope", "West Europe"),
                    new KeyValuePair<string, string>("westus", "West US"),
                    new KeyValuePair<string, string>("westus2", "West US 2")
                };
            }
            else
            {
                location = new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("australiaeast", "Australia East"),
            new KeyValuePair<string, string>("australiasoutheast", "Australia Southeast"),
            new KeyValuePair<string, string>("brazilsouth", "Brazil South"),
            new KeyValuePair<string, string>("canadacentral", "Canada Central"),
            new KeyValuePair<string, string>("centralus", "Central US"),
            new KeyValuePair<string, string>("eastasia", "East Asia"),
            new KeyValuePair<string, string>("eastus", "East US"),
            new KeyValuePair<string, string>("eastus2", "East US 2"),
            new KeyValuePair<string, string>("francecentral", "France Central"),
            new KeyValuePair<string, string>("germanywestcentral", "Germany West Central"),
            new KeyValuePair<string, string>("japaneast", "Japan East"),
            new KeyValuePair<string, string>("japanwest", "Japan West"),
            new KeyValuePair<string, string>("northeurope", "North Europe"),
            new KeyValuePair<string, string>("southafricanorth", "South Africa North"),
            new KeyValuePair<string, string>("southcentralus", "South Central US"),
            new KeyValuePair<string, string>("southeastasia", "Southeast Asia"),
            new KeyValuePair<string, string>("swedencentral", "Sweden Central"),
            new KeyValuePair<string, string>("uaenorth", "UAE North"),
            new KeyValuePair<string, string>("uksouth", "UK South"),
            new KeyValuePair<string, string>("ukwest", "UK West"),
            new KeyValuePair<string, string>("westeurope", "West Europe"),
            new KeyValuePair<string, string>("westus2", "West US 2")
        };
            }

            TargetRegionPicker.ItemsSource = location;
            TargetRegionPicker.SelectedValuePath = "Key";
            TargetRegionPicker.DisplayMemberPath = "Value";
            TargetRegionPicker.SelectedItem = null;
            DecideAssessmentDurationComboBox();
            DecideOptimizationPreferenceComboBox();
        }

        public void InitializeCurrencyPicker()
        {
            List<KeyValuePair<string, string>> currency = new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("USD", "United States – Dollar ($) USD"),
            new KeyValuePair<string, string>("AUD", "Australia – Dollar ($) AUD"),
            new KeyValuePair<string, string>("BRL", "Brazil – Real (R$) BRL"),
            new KeyValuePair<string, string>("CAD", "Canada – Dollar ($) CAD"),
            new KeyValuePair<string, string>("DKK", "Denmark – Krone (kr) DKK"),
            new KeyValuePair<string, string>("EUR", "Euro Zone – Euro (€) EUR"),
            new KeyValuePair<string, string>("INR", "India – Rupee (₹) INR"),
            new KeyValuePair<string, string>("JPY", "Japan – Yen (¥) JPY"),
            new KeyValuePair<string, string>("KRW", "Korea – Won (₩) KRW"),
            new KeyValuePair<string, string>("NZD", "New Zealand – Dollar ($) NZD"),
            new KeyValuePair<string, string>("NOK", "Norway – Krone (kr) NOK"),
            new KeyValuePair<string, string>("RUB", "Russia – Ruble (руб) RUB"),
            new KeyValuePair<string, string>("SEK", "Sweden – Krona (kr) SEK"),
            new KeyValuePair<string, string>("CHF", "Switzerland – Franc (chf) CHF"),
            new KeyValuePair<string, string>("TWD", "Taiwan – Dollar (NT$) TWD"),
            new KeyValuePair<string, string>("GBP", "United Kingdom – Pound (£) GBP")
        };

            CurrencyPicker.ItemsSource = currency;
            CurrencyPicker.SelectedValuePath = "Key";
            CurrencyPicker.DisplayMemberPath = "Value";
            CurrencyPicker.SelectedItem = new KeyValuePair<string, string>("USD", "United States – Dollar ($) USD");
        }

        private void InitializeAssessmentDurationPicker()
        {
            List<KeyValuePair<string, string>> assessmentDurations = new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("day", "Day"),
            new KeyValuePair<string, string>("week", "Week"),
            new KeyValuePair<string, string>("month", "Month")
        };

            AssessmentDurationPicker.ItemsSource = assessmentDurations;
            AssessmentDurationPicker.SelectedValuePath = "Key";
            AssessmentDurationPicker.DisplayMemberPath = "Value";
            AssessmentDurationPicker.SelectedItem = new KeyValuePair<string, string>("week", "Week");
        }

        public void InitializeOptimizationPreference(BusinessProposal businessProposal)
        {
            List<KeyValuePair<string, string>> optimizationPreferences = new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("Modernize", "Modernize"),
            new KeyValuePair<string, string>("MinimizetimewithAzureVM", "Minimize time with Azure VM")
        };

            if (businessProposal == BusinessProposal.AVS)
            {
                var kvp = new KeyValuePair<string, string>("MigrateToAvs", "Migrate to AVS");
                optimizationPreferences.Add(kvp);

                MigrationStrategyPicker.ItemsSource = optimizationPreferences;
                MigrationStrategyPicker.SelectedValuePath = "Key";
                MigrationStrategyPicker.DisplayMemberPath = "Value";
                MigrationStrategyPicker.SelectedItem = kvp;
            }
            else
            {
                MigrationStrategyPicker.ItemsSource = optimizationPreferences;
                MigrationStrategyPicker.SelectedValuePath = "Key";
                MigrationStrategyPicker.DisplayMemberPath = "Value";
                MigrationStrategyPicker.SelectedItem = new KeyValuePair<string, string>("Modernize", "Modernize");
            }

            AssessSqlServicesSeparatelyLabel.Visibility = Visibility.Collapsed;
        }
        #endregion

        #region Validation
        public bool ValidateAssessmentSettings()
        {
            if (!ValidateTargetRegion())
                return false;
            if (!ValidateCurrency())
                return false;
            if (!ValidateAssessmentDuration())
                return false;
            if (!ValidateOptimizationPreference())
                return false;

            return true;
        }

        private bool ValidateTargetRegion()
        {
            if (TargetRegionPicker.SelectedItem == null)
                return false;
            return true;
        }

        private bool ValidateCurrency()
        {
            if (CurrencyPicker.SelectedItem == null)
                return false;
            return true;
        }

        private bool ValidateAssessmentDuration()
        {
            if (AssessmentDurationPicker.SelectedItem == null)
                return false;

            return true;
        }

        private bool ValidateOptimizationPreference()
        {
            if (MigrationStrategyPicker.SelectedItem == null)
                return false;

            /*
            else if (((KeyValuePair<string, string>)OptimizationPreferencePicker.SelectedItem).Key == "ModernizeToPaaS" &&
                     ((KeyValuePair<string, string>)OptimizationPreferencePicker.SelectedItem).Value == "Modernize to PaaS (PaaS preferred)" &&
                     !AssessSqlServicesSeparatelyLabel.IsVisible)
            {
                return false;
            }
            */

            return true;
        }
        #endregion

        #region Picker Selection Changed
        private void OptimizationPreferencePicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            KeyValuePair<string, string> selectedOptimizationPreference = GetSelectedOptimizationPreference();
            if (!string.IsNullOrEmpty(selectedOptimizationPreference.Value) && !string.IsNullOrEmpty(selectedOptimizationPreference.Key)
                && selectedOptimizationPreference.Value == "Modernize")
                AssessSqlServicesSeparatelyLabel.Visibility = Visibility.Visible;
            else
                AssessSqlServicesSeparatelyLabel.Visibility = Visibility.Collapsed;

            mainObj.MakeAssessmentSettingsActionButtonsEnabledDecision();
            mainObj.MakeAssessmentSettingsTabButtonEnableDecisions();
        }

        private async void TargetRegionPicker_SelectedIndexChanged(object sender, RoutedEventArgs e)
        {
            mainObj.EnableSubmitButton();
            mainObj.MakeAssessmentSettingsActionButtonsEnabledDecision();
            mainObj.MakeAssessmentSettingsTabButtonEnableDecisions();
        }

        private void CurrencyPicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            mainObj.MakeAssessmentSettingsActionButtonsEnabledDecision();
            mainObj.MakeAssessmentSettingsTabButtonEnableDecisions();
        }

        private void AssessmentDurationPicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            mainObj.MakeAssessmentSettingsActionButtonsEnabledDecision();
            mainObj.MakeAssessmentSettingsTabButtonEnableDecisions();
        }
        #endregion

        #region Getter Methods
        public KeyValuePair<string, string> GetTargetRegion()
        {
            KeyValuePair<string, string> empty = new KeyValuePair<string, string>("", "");
            if (TargetRegionPicker.SelectedItem == null)
                return empty;

            return (KeyValuePair<string, string>)TargetRegionPicker.SelectedItem;
        }

        public KeyValuePair<string, string> GetCurrency()
        {
            KeyValuePair<string, string> empty = new KeyValuePair<string, string>("", "");
            if (CurrencyPicker.SelectedItem == null)
                return empty;

            return (KeyValuePair<string, string>)CurrencyPicker.SelectedItem;
        }

        public KeyValuePair<string, string> GetAssessmentDuration()
        {
            KeyValuePair<string, string> empty = new KeyValuePair<string, string>("", "");
            if (AssessmentDurationPicker.SelectedItem == null)
                return empty;

            return (KeyValuePair<string, string>)AssessmentDurationPicker.SelectedItem;
        }

        public KeyValuePair<string, string> GetSelectedOptimizationPreference()
        {
            KeyValuePair<string, string> empty = new KeyValuePair<string, string>("", "");
            if (MigrationStrategyPicker.SelectedItem == null)
                return empty;

            return (KeyValuePair<string, string>)MigrationStrategyPicker.SelectedItem;
        }

        public bool IsAssessSqlServicesSeparatelyChecked()
        {
            return AssessSqlServicesSeparatelyCheckBox.IsChecked ?? false;
        }
        #endregion

        public void DecideAssessmentDurationComboBox()
        {
            if (mainObj.ConfigurationObj.IsQuickAVSButtonClicked())
                AssessmentDurationPicker.IsEnabled = false;
            else
                AssessmentDurationPicker.IsEnabled = true;

            return;
        }

        public void DecideOptimizationPreferenceComboBox()
        {
            if (mainObj.ConfigurationObj.IsQuickAVSButtonClicked())
            {
                InitializeOptimizationPreference(BusinessProposal.AVS);
                MigrationStrategyPicker.IsEnabled = false;
            }
            else
            {
                InitializeOptimizationPreference(BusinessProposal.Comprehensive);
                MigrationStrategyPicker.IsEnabled = true;
            }

            return;
        }

        #region Initialization
        public void InitializeBackgroundWorker()
        {
            azureMigrateExploreBackgroundWorker = new BackgroundWorker
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };
            azureMigrateExploreBackgroundWorker.DoWork += new DoWorkEventHandler(azureMigrateExploreBackgroundWorker_DoWork);
            azureMigrateExploreBackgroundWorker.ProgressChanged += new ProgressChangedEventHandler(azureMigrateExploreBackgroundWorker_ProgressChanged);
            azureMigrateExploreBackgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(azureMigrateExploreBackgroundWorker_RunWorkerCompleted);
        }
        #endregion

        #region Background worker
        private void azureMigrateExploreBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            UserInputObj.LoggerObj.LogInformation(1, "Initiating process"); // 1 % complete

            Process processObj = new Process();
            processObj.Initiate(UserInputObj);

            // Wait for worker to finalize its logs
            //Thread.Sleep(10000);
        }

        private void azureMigrateExploreBackgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            ProgressBar.Value = e.ProgressPercentage;
            ProgressRing.IsActive = true;
            AppendTextBox("cls");
            AppendTextBox(e.UserState.ToString());

            if (UserInputObj.WorkflowObj.IsExpressWorkflow)
            {
                string messageReceived = e.UserState.ToString();
                if (!string.IsNullOrEmpty(messageReceived) && messageReceived.Contains(LoggerConstants.DiscoveryCompletionConstantMessage))
                {
                    //ProcessLogsTextBox.Text = "Discovery has been completed and assessment is in progress \n";
                }
            }
        }

        private async void azureMigrateExploreBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            string processInfoMessage = "";

            if (UserInputObj.CancellationContext.IsCancellationRequested)
            {
                processInfoMessage = "The process has been terminated.";
                ProgressRing.IsActive = false;
                ProgressBar.Value = 0;
                mainObj.EnableProjectDetailsTabButton();
                mainObj.EnableConfigurationTabButton();
                mainObj.EnableAssessmentSettingsTabButton();
                mainObj.ShowBackButton();
                mainObj.EnableBackButton();
                EnableAssessmentQuestionnaire();
            }
            else if (e.Error != null)
            {
                processInfoMessage = "An error has terminated the process. You can find the errors in the log file, resolve and retry. If the problem persists, write to us at am3copilotcrew@microsoft.com";
                ProgressRing.IsActive = false;
            }
            else
            {
                ProgressBar.Value = 100;
                ProgressRing.IsActive = false;
                int discoveredMachinesCount = DiscoveredMachinesCount.DiscoveredDataCount;
                if (discoveredMachinesCount == 0)
                {
                    ProgressBar.Value = 0;
                    processInfoMessage = "No machines were discovered for the selected project and discovery source. Please try again with a different project or configuration.";

                    // Send notification to Windows notification center
                    SendToast("Assessment Completed", "Azure Migrate data analysis complete!");
                }
                else if (UserInputObj.WorkflowObj.IsExpressWorkflow) // This means express assessment has completed
                {
                    // Send notification to Windows notification center
                    SendToast("Assessment Completed", "Azure Migrate data analysis complete!");
                    string folder= UtilityFunctions.GetReportsDirectory();
                    string lastFolderName = folder.Split('\\', StringSplitOptions.RemoveEmptyEntries).Last();

                    if(UserInputObj.BusinessProposal.Equals(BusinessProposal.AVS.ToString())){
                        processInfoMessage = $"Assessment has been completed. You can find the excel reports in the {lastFolderName} folder at {UtilityFunctions.GetReportsDirectory()}.\n\nAfter reviewing the reports you can open AVS_Migration PowerBI template at {Directory.GetCurrentDirectory()}, and export PowerBI as a PowerPoint or PDF presentation.";
                    } 
                    else {
                        processInfoMessage = $"Assessment has been completed. You can find the excel reports in the {lastFolderName} folder at {UtilityFunctions.GetReportsDirectory()}.\n\nAfter reviewing the reports you can open Azure_Migration_and_Modernization PowerBI template at {Directory.GetCurrentDirectory()}, and export PowerBI as a PowerPoint or PDF presentation.";
                    }
                }
                else if (!UserInputObj.WorkflowObj.IsExpressWorkflow && !string.IsNullOrEmpty(UserInputObj.WorkflowObj.Module))
                {
                    // Send notification to Windows notification center
                    SendToast("Assessment Completed", "Azure Migrate data analysis complete!");

                    if (UserInputObj.WorkflowObj.Module.Equals("Discovery")) // Custom Discovery completed
                        processInfoMessage = $"\"Discovered_VMs\" report has been generated at {UtilityFunctions.GetReportsDirectory()}.\nYou can now do the required customizations in the report by specifying the environment, moving servers out of scope by deleting rows in the report, and then run assessment on the customized discovery scope.";
                    else if (UserInputObj.WorkflowObj.Module.Equals("Assessment"))
                    { // Custom Assessment completed
                        string folder = UtilityFunctions.GetReportsDirectory();
                        string lastFolderName = folder.Split('\\', StringSplitOptions.RemoveEmptyEntries).Last();
                        if (UserInputObj.BusinessProposal.Equals(BusinessProposal.AVS.ToString()))
                        {
                            processInfoMessage = $"Assessment has been completed. You can find the excel reports in the {lastFolderName} folder at {UtilityFunctions.GetReportsDirectory()}.\n\nAfter reviewing the reports you can open AVS_Migration PowerBI template at {Directory.GetCurrentDirectory()}, and export PowerBI as a PowerPoint or PDF presentation.";
                        }
                        else
                        {
                            processInfoMessage = $"Assessment has been completed. You can find the excel reports in the {lastFolderName} folder at {UtilityFunctions.GetReportsDirectory()}.\n\nAfter reviewing the reports you can open Azure_Migration_and_Modernization PowerBI template at {Directory.GetCurrentDirectory()}, and export PowerBI as a PowerPoint or PDF presentation.";
                        }
                    }
                    else
                    { 
                        processInfoMessage = "Unable to generate informational message regarding process completion, please review the log file.";
                    }
                }
                else
                {
                    // Send notification to Windows notification center
                    SendToast("Assessment Completed", "Azure Migrate data analysis complete!");
                    processInfoMessage = "Unable to generate informational message regarding process completion, please review the log file.";
                }
            }

            DiscoveredMachinesCount.DiscoveredDataCount = 0;

            // Show the processInfoMessage in a ContentDialog
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
                        }, // Information icon
                        new TextBlock { Text = "Discovery Process Information", VerticalAlignment = VerticalAlignment.Center }
                    }
                },
                Content = processInfoMessage,
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };

            await dialog.ShowAsync();

            if (ProgressBar.Value >= 100 && !UserInputObj.WorkflowObj.IsExpressWorkflow && !string.IsNullOrEmpty(UserInputObj.WorkflowObj.Module) && UserInputObj.WorkflowObj.Module.Equals("Discovery"))
            {
                //mainObj.MakeTrackProgressActionButtonsEnabledDecisions(true, false, false);
                mainObj.ShowNextButton();
                mainObj.EnableNextButton();
                mainObj.EnableProjectDetailsTabButton();
                mainObj.EnableConfigurationTabButton();
                mainObj.EnableAssessmentSettingsTabButton();
                mainObj.ShowBackButton();
                mainObj.EnableBackButton();
                EnableAssessmentQuestionnaire();
            }
            else if (ProgressBar.Value >= 100 && !UserInputObj.WorkflowObj.IsExpressWorkflow && !string.IsNullOrEmpty(UserInputObj.WorkflowObj.Module) && UserInputObj.WorkflowObj.Module.Equals("Assessment"))
            {
                mainObj.MakeTrackProgressActionButtonsEnabledDecisions(false, false, true);
                mainObj.ShowNextButton();
                mainObj.EnableNextButton();
                mainObj.ShowProjectDetailsTabButton();
                mainObj.EnableConfigurationTabButton();
                mainObj.EnableAssessmentSettingsTabButton();
                mainObj.ShowBackButton();
                mainObj.EnableBackButton();
                EnableAssessmentQuestionnaire();
            }
            else if (ProgressBar.Value >= 100 && UserInputObj.WorkflowObj.IsExpressWorkflow)
            {
                mainObj.MakeTrackProgressActionButtonsEnabledDecisions(false, false, true);
                mainObj.ShowNextButton();
                mainObj.EnableNextButton();
                mainObj.EnableProjectDetailsTabButton();
                mainObj.EnableConfigurationTabButton();
                mainObj.EnableAssessmentSettingsTabButton();
                mainObj.ShowBackButton();
                mainObj.EnableBackButton();
                EnableAssessmentQuestionnaire();
            }
            else if (ProgressBar.Value < 100)
            {
                mainObj.MakeTrackProgressActionButtonsEnabledDecisions(false, true, false);
                mainObj.HideNextButton();
                mainObj.ShowRetryButton();
                mainObj.EnableRetryButton();
                mainObj.EnableProjectDetailsTabButton();
                mainObj.EnableConfigurationTabButton();
                mainObj.EnableAssessmentSettingsTabButton();
                mainObj.ShowBackButton();
                mainObj.EnableBackButton();
                EnableAssessmentQuestionnaire();
            }
            else
            {
                mainObj.MakeTrackProgressActionButtonsEnabledDecisions();
                mainObj.EnableProjectDetailsTabButton();
                mainObj.EnableConfigurationTabButton();

                mainObj.ShowBackButton();
                mainObj.EnableBackButton();
                EnableAssessmentQuestionnaire();
            }

            mainObj.MakeTrackProgressTabButtonEnableDecisions();
        }

        public static bool SendToast(string title, string message)
        {
            var appNotification = new AppNotificationBuilder()
                .AddText(title)
                .AddText(message)
                .BuildNotification();

            AppNotificationManager.Default.Show(appNotification);

            return appNotification.Id != 0; // return true (indicating success) if the toast was sent (if it has an Id)
        }

        public void DisableAssessmentQuestionnaire()
        {
            TargetRegionPicker.IsEnabled = false;
            CurrencyPicker.IsEnabled = false;
            AssessmentDurationPicker.IsEnabled = false;
            MigrationStrategyPicker.IsEnabled = false;
        }

        public void EnableAssessmentQuestionnaire()
        {
            TargetRegionPicker.IsEnabled = true;
            CurrencyPicker.IsEnabled = true;
            AssessmentDurationPicker.IsEnabled = true;
            MigrationStrategyPicker.IsEnabled = true;
        }

        #endregion

        #region Process initiation
        public void BeginProcess(UserInput userInputObj)
        {
            // Reset progress & previous logs in UI
            ProgressBar.Value = 0;
            //ProcessLogsTextBox.Text = string.Empty;
            AppendTextBox("cls");

            UserInputObj = userInputObj;
            UserInputObj.LoggerObj.ReportProgress += new EventHandler<LogEventHandler>(TrackProgress_ProgressHandler);

            try
            {
                if (!azureMigrateExploreBackgroundWorker.IsBusy)
                    azureMigrateExploreBackgroundWorker.RunWorkerAsync();
            }
            catch (Exception exRunWorkerAsync)
            {
                UserInputObj.LoggerObj.LogError($"Could not run worker: {exRunWorkerAsync.Message}");
            }

            mainObj.MakeTrackProgressActionButtonsEnabledDecisions();
            mainObj.MakeTrackProgressTabButtonEnableDecisions();
        }
        #endregion

        #region Process cancellation
        public void CancelProcess()
        {
            azureMigrateExploreBackgroundWorker.CancelAsync();
            UserInputObj.CancellationContext.Cancel();
            //ProcessLogsTextBox.Text = "The process is terminating.";
        }

        public bool IsProcessCancellationRequested()
        {
            return UserInputObj.CancellationContext.IsCancellationRequested;
        }
        private void TrackProgress_ProgressHandler(object sender, LogEventHandler e)
        {
            try
            {
                if(e.Percentage >= 100)
                {
                    DateTime now = DateTime.UtcNow;
                    string currentTimeStamp = now.ToString("yyyy-MM-dd-HH:mm:ss");
                    string message = currentTimeStamp + LoggerConstants.LogTypeMessageSeparator + LoggerConstants.InformationLogTypePrefix + LoggerConstants.LogTypeMessageSeparator;
                    AppendTextBox($"{message} Process Completed.");
                    return;
                }

                azureMigrateExploreBackgroundWorker.ReportProgress(e.Percentage, e.Message);
            }
            catch (Exception exReportProgress)
            {
                // Write the first message in UI logs
                AppendTextBox($"{e.Message}");
                AppendTextBox($"{UtilityFunctions.PrependErrorLogType()}Could not report progress: {exReportProgress.Message}");
            }
        }
        #endregion

        #region Process logs text box
        private void AppendTextBox(string value)
        {
            _ = DispatcherQueue.TryEnqueue(() =>
            {
                if (value == "cls")
                {
                    //ProcessLogsTextBox.Text = string.Empty;
                }
                else
                {
                    value = value + Environment.NewLine;
                    var color = value.Contains(LoggerConstants.InformationLogTypePrefix) ? new SolidColorBrush(Colors.Black) :
                                value.Contains(LoggerConstants.WarningLogTypePrefix) ? new SolidColorBrush(Colors.Yellow) :
                                value.Contains(LoggerConstants.ErrorLogTypePrefix) ? new SolidColorBrush(Colors.Red) :
                                value.Contains(LoggerConstants.DebugLogTypePrefix) ? new SolidColorBrush(Colors.Cyan) : new SolidColorBrush(Colors.White);

                    //ProcessLogsTextBox.Text += value;
                    ProcessLogsTextBlock.Text = value;
                }
            });
        }

        //private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        //{
        //    var grid = (Grid)VisualTreeHelper.GetChild(ProcessLogsTextBox, 0);
        //    for (var i = 0; i <= VisualTreeHelper.GetChildrenCount(grid) - 1; i++)
        //    {
        //        object obj = VisualTreeHelper.GetChild(grid, i);
        //        if (!(obj is ScrollViewer)) continue;
        //        ((ScrollViewer)obj).ChangeView(0.0f, ((ScrollViewer)obj).ExtentHeight, 1.0f);
        //        break;
        //    }
        //}

        #endregion

        #region Background worker running state
        public bool IsBackGroundWorkerRunning()
        {
            return azureMigrateExploreBackgroundWorker.IsBusy;
        }
        #endregion

        #region Decision makers
        public int DisplayActionButtonDecision()
        {
            if (IsBackGroundWorkerRunning())
                return 3;

            if (ProgressBar.Value < 1.0)
                return 2;

            if (!UserInputObj.WorkflowObj.IsExpressWorkflow && !string.IsNullOrEmpty(UserInputObj.WorkflowObj.Module) && UserInputObj.WorkflowObj.Module.Equals("Discovery"))
                return 1;

            return 3;
        }
        #endregion

    }
}

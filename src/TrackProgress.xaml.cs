// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using Azure.Migrate.Explore.Common;
using Azure.Migrate.Explore.Logger;
using Azure.Migrate.Explore.Models;
using Azure.Migrate.Explore.Processor;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using Windows.Foundation;
using Microsoft.UI;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace AzureMigrateExplore
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class TrackProgress : Page
    {
        private MainWindow mainObj;
        private BackgroundWorker azureMigrateExploreBackgroundWorker;
        private UserInput UserInputObj;
        public TrackProgress(MainWindow obj)
        {
            this.InitializeComponent();
            mainObj = obj;
            InitializeBackgroundWorker();
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
            ProgressBar.Value= e.ProgressPercentage / 100.0;
            AppendTextBox(e.UserState.ToString());

            if (UserInputObj.WorkflowObj.IsExpressWorkflow)
            {
                string messageReceived = e.UserState.ToString();
                if (!string.IsNullOrEmpty(messageReceived) && messageReceived.Contains(LoggerConstants.DiscoveryCompletionConstantMessage))
                {
                    ProcessLogsTextBox.Text = "Discovery has been completed and assessment is in progress \n";
                }
            }
        }

        private void azureMigrateExploreBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            string processInfoMessage = "";

            if (UserInputObj.CancellationContext.IsCancellationRequested)
            {
                processInfoMessage = "The process has been terminated.";
            }
            else if (e.Error != null)
            {
                processInfoMessage = "An error has terminated the process. You can find the errors in the log file, resolve and retry. If the problem persists, write to us at am3copilotcrew@microsoft.com";
            }
            else
            {
                ProgressBar.Value = 1.0;
                if (UserInputObj.WorkflowObj.IsExpressWorkflow) // This means express assessment has completed
                    processInfoMessage = $"Assessment has been completed. You can find the Core-Report and Opportunity-Report folders at {Directory.GetCurrentDirectory()}.\nAfter reviewing the reports you can open Azure_Migration_and_Modernization PowerBI template, and export PowerBI as a PowerPoint or PDF presentation.";
                else if (!UserInputObj.WorkflowObj.IsExpressWorkflow && !string.IsNullOrEmpty(UserInputObj.WorkflowObj.Module))
                {
                    if (UserInputObj.WorkflowObj.Module.Equals("Discovery")) // Custom Discovery completed
                        processInfoMessage = $"\"Discovered_VMs\" report has been generated at {Directory.GetCurrentDirectory()}\\Discovery-Report.\nYou can now do the required customizations in the report by specifying the environment, moving servers out of scope by deleting rows in the report, and then run assessment on the customized discovery scope.";
                    else if (UserInputObj.WorkflowObj.Module.Equals("Assessment")) // Custom Assessment completed
                        processInfoMessage = $"Assessment has been completed. You can find the Core-Report and Opportunity-Report folders at {Directory.GetCurrentDirectory()}.\nAfter reviewing the reports you can open Azure_Migration_and_Modernization PowerBI template, and export PowerBI as a PowerPoint or PDF presentation.";
                    else
                        processInfoMessage = "Unable to generate informational message regarding process completion, please review the log file.";
                }
                else
                    processInfoMessage = "Unable to generate informational message regarding process completion, please review the log file.";
            }
            ProcessLogsTextBox.Text = processInfoMessage;

            if (ProgressBar.Value >= 1.0 && !UserInputObj.WorkflowObj.IsExpressWorkflow && !string.IsNullOrEmpty(UserInputObj.WorkflowObj.Module) && UserInputObj.WorkflowObj.Module.Equals("Discovery"))
            {
                mainObj.MakeTrackProgressActionButtonsEnabledDecisions(true, false, false);
            }
            else if (ProgressBar.Value >= 1.0 && !UserInputObj.WorkflowObj.IsExpressWorkflow && !string.IsNullOrEmpty(UserInputObj.WorkflowObj.Module) && UserInputObj.WorkflowObj.Module.Equals("Assessment"))
            {
                mainObj.MakeTrackProgressActionButtonsEnabledDecisions(false, false, true);
            }
            else if (ProgressBar.Value >= 1.0 && UserInputObj.WorkflowObj.IsExpressWorkflow)
            {
                mainObj.MakeTrackProgressActionButtonsEnabledDecisions(false, false, true);
            }
            else if (ProgressBar.Value < 1.0)
            {
                mainObj.MakeTrackProgressActionButtonsEnabledDecisions(false, true, false);
            }
            else
            {
                mainObj.MakeTrackProgressActionButtonsEnabledDecisions();
            }

            mainObj.MakeTrackProgressTabButtonEnableDecisions();
        }

        #endregion

        #region Process initiation
        public void BeginProcess(UserInput userInputObj)
        {
            // Reset progress & previous logs in UI
            ProgressBar.Value = 0;
            ProcessLogsTextBox.Text = string.Empty;
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
            ProcessLogsTextBox.Text = "The process is terminating.";
        }

        public bool IsProcessCancellationRequested()
        {
            return UserInputObj.CancellationContext.IsCancellationRequested;
        }
        private void TrackProgress_ProgressHandler(object sender, LogEventHandler e)
        {
            try
            {
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
                    ProcessLogsTextBox.Text = string.Empty;
                }
                else
                {
                    value = value + Environment.NewLine;
                    var color = value.Contains(LoggerConstants.InformationLogTypePrefix) ? new SolidColorBrush(Colors.Black) :
                                value.Contains(LoggerConstants.WarningLogTypePrefix) ? new SolidColorBrush(Colors.Yellow) :
                                value.Contains(LoggerConstants.ErrorLogTypePrefix) ? new SolidColorBrush(Colors.Red) :
                                value.Contains(LoggerConstants.DebugLogTypePrefix) ? new SolidColorBrush(Colors.Cyan) : new SolidColorBrush(Colors.White);

                    ProcessLogsTextBox.Text += value;
                }
            });
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var grid = (Grid)VisualTreeHelper.GetChild(ProcessLogsTextBox, 0);
            for (var i = 0; i <= VisualTreeHelper.GetChildrenCount(grid) - 1; i++)
            {
                object obj = VisualTreeHelper.GetChild(grid, i);
                if (!(obj is ScrollViewer)) continue;
                ((ScrollViewer)obj).ChangeView(0.0f, ((ScrollViewer)obj).ExtentHeight, 1.0f);
                break;
            }
        }

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

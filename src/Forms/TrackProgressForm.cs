using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using Azure.Migrate.Export.Common;
using Azure.Migrate.Export.Logger;
using Azure.Migrate.Export.Models;
using Azure.Migrate.Export.Processor;

namespace Azure.Migrate.Export.Forms
{
    public partial class TrackProgressForm : Form
    {
        private AzureMigrateExportMainForm mainFormObj;
        private UserInput UserInputObj;

        public TrackProgressForm(AzureMigrateExportMainForm obj)
        {
            InitializeComponent();
            mainFormObj = obj;
            InitializeBackgroundWorker();
        }

        #region Initialization
        public void InitializeBackgroundWorker()
        {
            AzureMigrateExportBackGroundWorker.DoWork += AzureMigrateExportBackGroundWorker_DoWork;
            AzureMigrateExportBackGroundWorker.WorkerReportsProgress = true;
            AzureMigrateExportBackGroundWorker.ProgressChanged += AzureMigrateExportBackGroundWorker_ProgressChanged;
            AzureMigrateExportBackGroundWorker.WorkerSupportsCancellation = true;
            AzureMigrateExportBackGroundWorker.RunWorkerCompleted += AzureMigrateExportBackGroundWorker_RunWorkerCompleted;
        }
        #endregion

        #region Background worker
        private void AzureMigrateExportBackGroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            UserInputObj.LoggerObj.LogInformation(1, "Initiating process"); // 1 % complete

            Process processObj = new Process();
            processObj.Initiate(UserInputObj);

            // Wait for worker to finalize its logs
            Thread.Sleep(10000);
        }

        private void AzureMigrateExportBackGroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            ProgressBar.Value = e.ProgressPercentage;
            AppendTextBox(e.UserState.ToString());

            if (UserInputObj.WorkflowObj.IsExpressWorkflow)
            {
                string messageReceived = e.UserState.ToString();
                if (!string.IsNullOrEmpty(messageReceived) && messageReceived.Contains(LoggerConstants.DiscoveryCompletionConstantMessage))
                {
                    ProcessInfoTextBox.Clear();
                    ProcessInfoTextBox.Text = "Discovery has been completed and assessment is in progress";
                }
            }
        }

        private void AzureMigrateExportBackGroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            string processInfoMessage = "";

            if (UserInputObj.CancellationContext.IsCancellationRequested)
                processInfoMessage = "The process has been terminated.";
            else if (ProgressBar.Value >= 100)
            {
                if (UserInputObj.WorkflowObj.IsExpressWorkflow) // This means express assessment has completed
                    if (UserInputObj.BusinessProposal.Equals(BusinessProposal.AVS.ToString()))
                    {
                        processInfoMessage = $"Assessment has been completed. You can find the Core-Report and Opportunity-Report folders at {Directory.GetCurrentDirectory()}.\nAfter reviewing the reports you can open AVS_Migration PowerBI template, and export PowerBI as a PowerPoint or PDF presentation.";
                    }
                    else
                    {
                        processInfoMessage = $"Assessment has been completed. You can find the Core-Report and Opportunity-Report folders at {Directory.GetCurrentDirectory()}.\n After reviewing the reports you can open Azure_Migration_and_Modernization PowerBI template, and export PowerBI as a PowerPoint or PDF presentation.";
                    }
                else if (!UserInputObj.WorkflowObj.IsExpressWorkflow && !string.IsNullOrEmpty(UserInputObj.WorkflowObj.Module))
                {
                    if (UserInputObj.WorkflowObj.Module.Equals("Discovery")) // Custom Discovery completed
                        processInfoMessage = $"\"Discovered_VMs\" report has been generated at {Directory.GetCurrentDirectory()}\\Discovery-Report.\nYou can now do the required customizations in the report by specifying the environment, moving servers out of scope by deleting rows in the report, and then run assessment on the customized discovery scope.";
                    else if (UserInputObj.WorkflowObj.Module.Equals("Assessment")) // Custom Assessment completed
                        if (UserInputObj.BusinessProposal.Equals(BusinessProposal.AVS.ToString()))
                        {
                            processInfoMessage = $"Assessment has been completed. You can find the Core-Report and Opportunity-Report folders at {Directory.GetCurrentDirectory()}.\nAfter reviewing the reports you can open AVS_Migration PowerBI template, and export PowerBI as a PowerPoint or PDF presentation.";
                        }
                        else
                        {
                            processInfoMessage = $"Assessment has been completed. You can find the Core-Report and Opportunity-Report folders at {Directory.GetCurrentDirectory()}.\n After reviewing the reports you can open Azure_Migration_and_Modernization PowerBI template, and export PowerBI as a PowerPoint or PDF presentation.";
                        }
                    else
                        processInfoMessage = "Unable to generate informational message regarding process completion, please review the log file.";
                }
                else
                    processInfoMessage = "Unable to generate informational message regarding process completion, please review the log file.";
            }
            else if (ProgressBar.Value < 100)
                processInfoMessage = "An error has terminated the process. You can find the errors in the log file, resolve and retry. If the problem persists, write to us @ amesupport@microsoft.com";
            else
                processInfoMessage = "Unable to generate informational message regarding process completion, please review the log file.";

            ProcessInfoTextBox.Clear();
            ProcessInfoTextBox.Text = processInfoMessage;

            if (ProgressBar.Value >= 100 && !UserInputObj.WorkflowObj.IsExpressWorkflow && !string.IsNullOrEmpty(UserInputObj.WorkflowObj.Module) && UserInputObj.WorkflowObj.Module.Equals("Discovery"))
            {
                mainFormObj.MakeTrackProgressActionButtonsEnabledDecisions(true, false);
            }
            else if (ProgressBar.Value < 100)
            {
                mainFormObj.MakeTrackProgressActionButtonsEnabledDecisions(false, true);
            }
            else 
            {
                mainFormObj.MakeTrackProgressActionButtonsEnabledDecisions();
            }
            mainFormObj.MakeTrackProgressTabButtonEnableDecisions();
        }
        #endregion

        #region Process initiation
        public void BeginProcess(UserInput userInputObj)
        {
            // Reset progress & previous logs in UI
            ProgressBar.Value = 0;
            ProcessInfoTextBox.Clear();
            AppendTextBox("cls");

            UserInputObj = userInputObj;
            UserInputObj.LoggerObj.ReportProgress += new EventHandler<LogEventHandler>(TrackProgress_ProgressHandler);

            try
            {
                if (!AzureMigrateExportBackGroundWorker.IsBusy)
                    AzureMigrateExportBackGroundWorker.RunWorkerAsync();
            }
            catch (Exception exRunWorkerAsync)
            {
                UserInputObj.LoggerObj.LogError($"Could not run worker: {exRunWorkerAsync.Message}");
            }

            mainFormObj.MakeTrackProgressActionButtonsEnabledDecisions();
            mainFormObj.MakeTrackProgressTabButtonEnableDecisions();
        }
        #endregion

        #region Process cancellation
        public void CancelProcess()
        {
            AzureMigrateExportBackGroundWorker.CancelAsync();
            UserInputObj.CancellationContext.Cancel();
            ProcessInfoTextBox.Text = "The process is terminating.";
        }

        public bool IsProcessCancellationRequested()
        {
            return UserInputObj.CancellationContext.IsCancellationRequested;
        }
        #endregion

        #region Event handlers
        private void TrackProgress_ProgressHandler(object sender, LogEventHandler e)
        {
            try
            {
                AzureMigrateExportBackGroundWorker.ReportProgress(e.Percentage, e.Message);
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
        public void AppendTextBox(string value)
        {
            if (InvokeRequired)
            {
                this.BeginInvoke(new Action<string>(AppendTextBox), new object[] { value });
                return;
            }
            else if (value == "cls")
            {
                ProcessLogsRichTextBox.Clear();
            }
            else
            {
                ProcessLogsRichTextBox.SelectionStart = ProcessLogsRichTextBox.TextLength;
                ProcessLogsRichTextBox.SelectionLength = 0;

                if (value.Contains(LoggerConstants.InformationLogTypePrefix))
                    ProcessLogsRichTextBox.SelectionColor = Color.White;
                else if (value.Contains(LoggerConstants.WarningLogTypePrefix))
                    ProcessLogsRichTextBox.SelectionColor = Color.Yellow;
                else if (value.Contains(LoggerConstants.ErrorLogTypePrefix))
                    ProcessLogsRichTextBox.SelectionColor = Color.Red;
                else if (value.Contains(LoggerConstants.DebugLogTypePrefix))
                    ProcessLogsRichTextBox.SelectionColor = Color.Cyan;
                else
                    ProcessLogsRichTextBox.SelectionColor = Color.White;

                ProcessLogsRichTextBox.AppendText(Environment.NewLine + $"--> {value}");

                // Scroll to the end
                ProcessLogsRichTextBox.SelectionStart = ProcessLogsRichTextBox.TextLength;
                ProcessLogsRichTextBox.ScrollToCaret();
            }
        }
        #endregion

        #region Background worker running state
        public bool IsBackGroundWorkerRunning()
        {
            if (AzureMigrateExportBackGroundWorker.IsBusy)
                return true;

            return false;
        }
        #endregion

        #region Decision makers
        public int DisplayActionButtonDecision()
        {
            if (IsBackGroundWorkerRunning())
                return 3;

            if (ProgressBar.Value < 100)
                return 2;

            if (!UserInputObj.WorkflowObj.IsExpressWorkflow && !string.IsNullOrEmpty(UserInputObj.WorkflowObj.Module) && UserInputObj.WorkflowObj.Module.Equals("Discovery"))
                return 1;

            return 3;
        }
        #endregion
    }
}

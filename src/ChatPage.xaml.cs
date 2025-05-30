// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text;
using Azure.Migrate.Explore.Common;
using Azure;
using Newtonsoft.Json;
using Orionsoft.MarkdownToPdfLib;
using System.Drawing;
using System.IO;
using Azure.Messaging.WebPubSub;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Net.WebSockets;
using System.Text.Json.Nodes;
using System.Reactive.Subjects;
using System.Collections.Concurrent;
using System.Reactive.Linq;
using System.Threading;
using System.Diagnostics;
using static AzureMigrateExplore.MainWindow;
using Microsoft.UI.Xaml.Navigation;
using Azure.Migrate.Explore.Models.CopilotSummary;
using Azure.Migrate.Explore.Models.CopilotSummary.CopilotClientContract;
using Azure.Messaging.WebPubSub.Clients;
using Windows.Media.Protection.PlayReady;
using FontFamily = Microsoft.UI.Xaml.Media.FontFamily;
using DocumentFormat.OpenXml.ExtendedProperties;
using Microsoft.UI;
using Azure.Migrate.Explore.Logger;
using Microsoft.Windows.AppNotifications.Builder;
using Microsoft.Windows.AppNotifications;
using Microsoft.UI.Xaml.Controls.Primitives;
using System.Collections.ObjectModel;
using System.Windows.Controls.Ribbon.Primitives;
using Markdig;
using CommunityToolkit.Common.Parsers.Markdown.Blocks;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Text;
using ColorCode.Compilation.Languages;
using System.Xml.Linq;
using Markdig.Syntax.Inlines;
using Markdig.Syntax;
using Markdig.Renderers;
using ParagraphBlock = Markdig.Syntax.ParagraphBlock;
using CommunityToolkit.WinUI.UI.Controls.Markdown.Render;
using Run = Microsoft.UI.Xaml.Documents.Run;
using Hyperlink = Microsoft.UI.Xaml.Documents.Hyperlink;
using Paragraph = Microsoft.UI.Xaml.Documents.Paragraph;
using ListBlock = Markdig.Syntax.ListBlock;
using ListItemBlock = Markdig.Syntax.ListItemBlock;
using System.Windows.Documents;
using LineBreak = Microsoft.UI.Xaml.Documents.LineBreak;
using InlineCollection = Microsoft.UI.Xaml.Documents.InlineCollection;
using Span = Microsoft.UI.Xaml.Documents.Span;
using Windows.UI.WebUI;
using System.Net.Http;
using Azure.Migrate.Explore.Excel;
using Azure.Migrate.Explore.Summary;
using Newtonsoft.Json.Serialization;

namespace AzureMigrateExplore
{
    public sealed partial class ChatPage : Page
    {
        private MainWindow mainObj;
        private Dictionary<string, List<UIElement>> chatHistory;
        private Dictionary<string, int> tabPageNumbers;
        private HashSet<string> visitedTabs;
        private string currentTab;
        private int currentPage;
        private int totalPages = 1;
        private string MigrationDetailsNudge1 = "Nudge 1";
        private string MigrationDetailsNudge2 = "Nudge 2";
        private string MigrationDetailsNudge3 = "Nudge 3";

        private string CompanyProfileNudge1 = "CompanyProfile Nudge 1";
        private string CompanyProfileNudge2 = "CompanyProfile Nudge 2";
        private string CompanyProfileNudge3 = "CompanyProfile Nudge 3";

        private string AIOpportunitiesNudge1 = "AIOpportunities Nudge 1";
        private string AIOpportunitiesNudge2 = "AIOpportunities Nudge 2";
        private string AIOpportunitiesNudge3 = "AIOpportunities Nudge 3";

        private string MigrationDetailsContent1 = "Content for Migration Details";
        private string MigrationDetailsContent2 = "Content for Migration Details";
        private string MigrationDetailsContent3 = "Content for Migration Details";

        private string CompanyProfileContent1 = "Content for Company Profile";
        private string CompanyProfileContent2 = "Content for Company Profile";
        private string CompanyProfileContent3 = "Content for Company Profile";

        private string AIOpportunitiesContent1 = "Content for AI Opportunities";
        private string AIOpportunitiesContent2 = "Content for AI Opportunities";
        private string AIOpportunitiesContent3 = "Content for AI Opportunities";

        private int regenerateCount = 1;

        private CopilotQuestionnaire CopilotQuestionnaireObj;

        private WebPubSubClient _webPubSubClient;

        public ChatPage(MainWindow obj, CopilotQuestionnaire copilotQuestionnaireObj)
        {
            this.InitializeComponent();
            mainObj = obj;
            CopilotQuestionnaireObj = copilotQuestionnaireObj;
            mainObj.HideNextButton();
            mainObj.ShowBackButton();
            chatHistory = new Dictionary<string, List<UIElement>>();
            tabPageNumbers = new Dictionary<string, int>();
            currentTab = "Company Profile"; // Default tab
            currentPage = 1; // Default page
            visitedTabs = new HashSet<string>();
            UpdatePageIndicator();
            mainObj.DisableGenerateSummaryButton();
            mainObj.EnableOverlayGrid();
            ;
        }

        private void ScrollToBottomButton_Click(object sender, RoutedEventArgs e)
        {
            MessagesScrollViewer.ChangeView(null, MessagesScrollViewer.ScrollableHeight, null);
        }

        private bool _autoScroll = true;

        private void MessagesScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (MessagesScrollViewer.VerticalOffset < MessagesScrollViewer.ScrollableHeight - 100) // Adjust the threshold as needed
            {
                _autoScroll = false;
                ScrollToBottomButton.Visibility = Visibility.Visible;
            }
            else
            {
                _autoScroll = true;
                ScrollToBottomButton.Visibility = Visibility.Collapsed;
            }
        }


        private void MessagesPanel_LayoutUpdated(object sender, object e)
        {
            if (_autoScroll)
            {
                MessagesScrollViewer.ChangeView(null, MessagesScrollViewer.ScrollableHeight, null);
            }
        }

        private ScrollViewer GetScrollViewer(DependencyObject depObj)
        {
            if (depObj is ScrollViewer)
            {
                return (ScrollViewer)depObj;
            }

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                var child = VisualTreeHelper.GetChild(depObj, i);
                var result = GetScrollViewer(child);
                if (result != null)
                {
                    return result;
                }
            }
            return null;
        }

        public void ClearChatContent()
        {
            // Clear the content of the MessagesPanel for all tabs
            foreach (var tab in chatHistory.Keys)
            {
                chatHistory[tab].Clear();
            }

            // Clear the content of the MessagesPanel
            MessagesPanel.Children.Clear();

            // Clear the chat history
            chatHistory.Clear();

            // Clear the text box content
            MessageTextBox.Text = string.Empty;

            // Clear the tab contents
            MigrationDetailsContent1 = string.Empty;
            MigrationDetailsContent2 = string.Empty;
            MigrationDetailsContent3 = string.Empty;
            CompanyProfileContent1 = string.Empty;
            CompanyProfileContent2 = string.Empty;
            CompanyProfileContent3 = string.Empty;
            AIOpportunitiesContent1 = string.Empty;
            AIOpportunitiesContent2 = string.Empty;
            AIOpportunitiesContent3 = string.Empty;

            // Reset the current page and tab information
            currentPage = 1;
            currentTab = "Company Profile"; // Default tab
            tabPageNumbers.Clear();
            visitedTabs.Clear();
            // Reset total pages to 1 for all tabs
            totalPages = 1;
            tabTotalPages["Migration Details"] = 1;
            tabTotalPages["Company Profile"] = 1;
            tabTotalPages["AI Opportunities"] = 1;

            UpdatePageIndicator();
            mainObj.EnableOverlayGrid();

        }

        private void EnableTabs()
        {
            MainPivot.IsEnabled = true;
            RegenerateSummaryButton.IsEnabled = true;
            LikeButton.IsEnabled = true;
            DislikeButton.IsEnabled = true;
            ExportSummaryButton.IsEnabled = true;
        }

        private void DisableTabs()
        {
            MainPivot.IsEnabled = false;
            RegenerateSummaryButton.IsEnabled = false;
            LikeButton.IsEnabled = false;
            DislikeButton.IsEnabled = false;
            ExportSummaryButton.IsEnabled = false;
        }

        private TextBox loadingMessage;

        private DispatcherTimer loadingMessageTimer;
        private int loadingMessageIndex = 0;
        private readonly string[] loadingMessages = new[]
        {
            "Processing...",
            "Still working...",
            "Almost there...",
            "Just a moment..."
        };

        private void ShowLoadingMessage()
        {
            // Create a new StackPanel for the loading message
            StackPanel loadingMessagePanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(0, 5, 0, 5)
            };

            // Add sender name
            TextBlock senderTextBlock = new TextBlock
            {
                Text = "AI",
                FontWeight = Microsoft.UI.Text.FontWeights.Bold,
                Foreground = new SolidColorBrush(Microsoft.UI.Colors.Gray)
            };
            loadingMessagePanel.Children.Add(senderTextBlock);

            // Add loading message text
            loadingMessage = new TextBox
            {
                Text = loadingMessages[loadingMessageIndex],
                FontStyle = Windows.UI.Text.FontStyle.Italic,
                Foreground = new SolidColorBrush(Microsoft.UI.Colors.Gray),
                Background = new SolidColorBrush(Microsoft.UI.Colors.Transparent),
                BorderBrush = new SolidColorBrush(Microsoft.UI.Colors.Transparent),
                Margin = new Thickness(0, 5, 0, 5),
                HorizontalAlignment = HorizontalAlignment.Left,
                Style = (Style)Resources["TextBoxStyle"],
                IsReadOnly = true,
                BorderThickness = new Thickness(0),
                Padding = new Thickness(10),
                FontSize = 14,
                FontFamily = new FontFamily("Segoe UI")
            };

            Border loadingMessageBorder = new Border
            {
                Background = new SolidColorBrush(Microsoft.UI.Colors.LightGray),
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(10),
                MaxWidth = 600,
                Child = loadingMessage,
                BorderBrush = new SolidColorBrush(Microsoft.UI.Colors.DarkGray),
                BorderThickness = new Thickness(1),
            };

            _ = DispatcherQueue.TryEnqueue(() =>
            {
                loadingMessagePanel.Children.Add(loadingMessageBorder);
                MessagesPanel.Children.Add(loadingMessagePanel);
            });

            // Initialize and start the timer
            loadingMessageTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(2)
            };
            loadingMessageTimer.Tick += LoadingMessageTimer_Tick;
            loadingMessageTimer.Start();
        }


        private void LoadingMessageTimer_Tick(object sender, object e)
        {
            loadingMessageIndex = (loadingMessageIndex + 1) % loadingMessages.Length;
            loadingMessage.Text = loadingMessages[loadingMessageIndex];
        }

        private void RemoveLoadingMessage()
        {
            _ = DispatcherQueue.TryEnqueue(() =>
            {
                if (loadingMessage != null)
                {
                    var parent = loadingMessage.Parent as Border;
                    var grandParent = parent?.Parent as StackPanel;
                    if (grandParent != null)
                    {
                        MessagesPanel.Children.Remove(grandParent);
                    }
                    loadingMessage = null;
                }

                // Stop and reset the timer
                if (loadingMessageTimer != null)
                {
                    loadingMessageTimer.Stop();
                    loadingMessageTimer.Tick -= LoadingMessageTimer_Tick;
                    loadingMessageTimer = null;
                }
            });
        }

        private void AddMessage(string senderName, string message, DateTime time, bool isSender, string tab)
        {
            // Create a new StackPanel for the message
            StackPanel messagePanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                HorizontalAlignment = isSender ? HorizontalAlignment.Right : HorizontalAlignment.Left,
                Margin = new Thickness(0, 5, 0, 5)
            };

            // Add sender name
            TextBlock senderTextBlock = new TextBlock
            {
                Text = senderName,
                FontWeight = Microsoft.UI.Text.FontWeights.Bold,
                Foreground = new SolidColorBrush(Microsoft.UI.Colors.Gray)
            };

            _ = DispatcherQueue.TryEnqueue(() =>
            { messagePanel.Children.Add(senderTextBlock); });

            // Add message text
            Border messageBorder = new Border
            {
                Background = new SolidColorBrush(isSender ? Microsoft.UI.Colors.LightBlue : Microsoft.UI.Colors.LightGray),
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(10),
                MaxWidth = 600
            };
            TextBlock messageTextBlock = new TextBlock
            {
                Text = message,
                TextWrapping = TextWrapping.Wrap,
                Style = (Style)Resources["MessageTextStyle"]
            };

            _ = DispatcherQueue.TryEnqueue(() =>
            {
                messageBorder.Child = messageTextBlock;
                messagePanel.Children.Add(messageBorder);
            });

            // Add time
            TextBlock timeTextBlock = new TextBlock
            {
                Text = time.ToString("hh:mm tt"),
                FontSize = 10,
                Foreground = new SolidColorBrush(Microsoft.UI.Colors.Gray),
                HorizontalAlignment = isSender ? HorizontalAlignment.Right : HorizontalAlignment.Left
            };

            _ = DispatcherQueue.TryEnqueue(() =>
            {
                messagePanel.Children.Add(timeTextBlock);
                // Add the message panel to the MessagesPanel
                MessagesPanel.Children.Add(messagePanel);
            });

            // Save the message to the specified tab's history
            if (!chatHistory.ContainsKey(tab))
            {
                chatHistory[tab] = new List<UIElement>();
            }
            chatHistory[tab].Add(messagePanel);
        }


        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            SendMessage();
        }

        private void MessageTextBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                SendMessage();
                e.Handled = true; // Prevent the TextBox from adding a new line
            }
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

        public async Task ReceiveMessagesFromPubSub(WebPubSubClient webPubSubClient)
        {
            if (webPubSubClient == null)
            {
                throw new ArgumentNullException(nameof(webPubSubClient));
            }

            _webPubSubClient = webPubSubClient;
            webPubSubClient.ServerMessageReceived += async message => // Corrected type name
            {
                try
                {
                    //stop the timer
                    _ = DispatcherQueue.TryEnqueue(() =>
                    {
                        mainObj.StopTimer();
                    });

                    Debug.WriteLine("Timer stopped as Summary Generation was successful");
                    
                    Debug.WriteLine($"Message received: {message.Message.Data}");

                    // Deserialize the message
                    string messageData = message.Message.Data.ToString();

                    var queryOutput = JsonConvert.DeserializeObject<QueryOutput>(messageData);
                    if (queryOutput != null)
                    {
                        if (queryOutput.IsSuccess)
                        {
                            var agentResponses = JsonConvert.DeserializeObject<SuccessfulQueryOutputPayload>(queryOutput.Payload)?.Result;

                            if (agentResponses != null)
                            {
                                if (GlobalConnection.UserAction == "GenerateSummary")
                                {
                                    // Reset the section regenerate count for all sections
                                    sectionRegenerateCount["Migration Details"] = 1;
                                    sectionRegenerateCount["Company Profile"] = 1;
                                    sectionRegenerateCount["AI Opportunities"] = 1;

                                    LogHandler logger = new LogHandler();
                                    logger.LogInformation("Summary has been successfully generated");
                                    
                                    SendToast("Summary Generated", "The AI summary has been successfully generated.");

                                    foreach (var response in agentResponses)
                                    {
                                        Debug.WriteLine(response.Response);
                                        // Ensure the UI update happens on the UI thread
                                        _ = DispatcherQueue.TryEnqueue(async () =>
                                        {
                                            mainObj.DisableOverlayGrid();

                                            if (response.ResponseSectionName == "MigrationDetails")
                                            {
                                                MigrationDetailsContent1 = response.Response;
                                                UpdateMigrationDetailsTabContent();
                                                MigrationDetailsNudge1 = response.Nudges[0];
                                                Debug.WriteLine(MigrationDetailsNudge1);
                                                MigrationDetailsNudge2 = response.Nudges[1];
                                                Debug.WriteLine(MigrationDetailsNudge2);
                                                MigrationDetailsNudge3 = response.Nudges[2];
                                                Debug.WriteLine(MigrationDetailsNudge3);
                                            }
                                            else if (response.ResponseSectionName == "CompanyProfile")
                                            {
                                                CompanyProfileContent1 = response.Response;
                                                UpdateCompanyProfileTabContent();
                                                CompanyProfileNudge1 = response.Nudges[0];
                                                Debug.WriteLine(CompanyProfileNudge1);
                                                CompanyProfileNudge2 = response.Nudges[1];
                                                Debug.WriteLine(CompanyProfileNudge2);
                                                CompanyProfileNudge3 = response.Nudges[2];
                                            }
                                            else
                                            {
                                                AIOpportunitiesContent1 =
                                            response.Response;
                                                UpdateAIOpportunitiesTabContent();
                                                AIOpportunitiesNudge1 = response.Nudges[0];
                                                AIOpportunitiesNudge2 = response.Nudges[1];
                                                AIOpportunitiesNudge3 = response.Nudges[2];
                                                InitializeNudges();
                                               
                                            }
                                        });
                                    }
                                    
                                }
                                else if (GlobalConnection.UserAction == "AnswerQuery")
                                {

                                    foreach (var response in agentResponses)
                                    {
                                        Console.WriteLine($"Section: {response.ResponseSectionName}");
                                        Console.WriteLine($"Response: {response.Response}");

                                        // Ensure the UI update happens on the UI thread
                                        _ = DispatcherQueue.TryEnqueue(() =>
                                        {
                                            RemoveLoadingMessage(); // Remove loading message
                                            AddMessage("AI", response.Response, DateTime.Now, false, currentTab);
                                            // Enable tabs
                                            EnableTabs();
                                            UpdateSummaryButton.IsEnabled = true;
                                        });
                                    }
                                }
                                else if (GlobalConnection.UserAction == "UpdateSummary")
                                {
                                    LogHandler logger = new LogHandler();
                                    logger.LogInformation("Summary has been successfully updated");

                                    SendToast("Summary Updated", "The AI summary has been successfully updated.");

                                    foreach (var response in agentResponses)
                                    {
                                        // Ensure the UI update happens on the UI thread
                                        _ = DispatcherQueue.TryEnqueue(() =>
                                        {
                                            mainObj.DisableOverlayGrid();
                                            if (response.ResponseSectionName == "MigrationDetails")
                                            {
                                                if (sectionRegenerateCount[currentTab] == 2)
                                                {
                                                    MigrationDetailsContent2 = response.Response;
                                                }
                                                else if (sectionRegenerateCount[currentTab] == 3)
                                                {
                                                    MigrationDetailsContent3 = response.Response;
                                                }

                                                UpdateMigrationDetailsTabContent();
                                                MigrationDetailsNudge1 = response.Nudges[0];
                                                Debug.WriteLine(MigrationDetailsNudge1);
                                                MigrationDetailsNudge2 = response.Nudges[1];
                                                Debug.WriteLine(MigrationDetailsNudge2);
                                                MigrationDetailsNudge3 = response.Nudges[2];
                                                Debug.WriteLine(MigrationDetailsNudge3);
                                                ReInitializeNudges("Migration Details");
                                            }
                                            else if (response.ResponseSectionName == "CompanyProfile")
                                            {
                                                if (sectionRegenerateCount[currentTab] == 2)
                                                {
                                                    CompanyProfileContent2 = response.Response;
                                                }
                                                else if (sectionRegenerateCount[currentTab] == 3)
                                                {
                                                    CompanyProfileContent3 = response.Response;
                                                }
                                                UpdateCompanyProfileTabContent();
                                                CompanyProfileNudge1 = response.Nudges[0];
                                                Debug.WriteLine(CompanyProfileNudge1);
                                                CompanyProfileNudge2 = response.Nudges[1];
                                                Debug.WriteLine(CompanyProfileNudge2);
                                                CompanyProfileNudge3 = response.Nudges[2];
                                                ReInitializeNudges("Company Profile");
                                            }
                                            else
                                            {
                                                if (sectionRegenerateCount[currentTab] == 2)
                                                {
                                                    AIOpportunitiesContent2 = response.Response;
                                                }
                                                else if (sectionRegenerateCount[currentTab] == 3)
                                                {
                                                    AIOpportunitiesContent3 = response.Response;
                                                }
                                                UpdateAIOpportunitiesTabContent();
                                                AIOpportunitiesNudge1 = response.Nudges[0];
                                                AIOpportunitiesNudge2 = response.Nudges[1];
                                                AIOpportunitiesNudge3 = response.Nudges[2];
                                                ReInitializeNudges("AI Opportunities");
                                            }
                                        });
                                    }
                                }
                                else if (GlobalConnection.UserAction == "UserFeedback")
                                {
                                    _ = DispatcherQueue.TryEnqueue(() =>
                                    {
                                        ShowDialogAsync("Feedback", "Thank you for your feedback!");
                                        LikeButton.IsEnabled = false;
                                        DislikeButton.IsEnabled = false;
                                    });
                                }
                            }
                        }
                        else if (queryOutput.IsSuccess == false)
                        {
                            var errorOutput = JsonConvert.DeserializeObject<ErrorOutputPayload>(queryOutput.Payload);
                            if (errorOutput != null)
                            {
                                _ = DispatcherQueue.TryEnqueue(async () =>
                                {
                                    // Provide feedback to the user interface
                                    RemoveLoadingMessage();
                                    AddMessage("System", $"Error: {errorOutput.ErrorMessage}", DateTime.Now, false, currentTab);
                                    EnableTabs();
                                    UpdateSummaryButton.IsEnabled = true;
                                    mainObj.DisableOverlayGrid();
                                    // Show error dialog
                                    await ShowErrorDialogAsync("Error", $"An error occurred: {errorOutput.ErrorMessage} Please retry.");
                                    mainObj.SwitchToCopilotQuestionnaireTab();
                                });
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error processing message from WebPubSub: {ex.Message}");
                    _ = DispatcherQueue.TryEnqueue(async () =>
                    {
                        // Provide feedback to the user interface
                        mainObj.DisableOverlayGrid();
                        RemoveLoadingMessage();
                        AddMessage("System", "An error occurred while processing the message. Please try again.", DateTime.Now, false, currentTab);
                        await ShowErrorDialogAsync("Error", $"An error occurred: {ex.Message} Please retry.");
                        EnableTabs();
                        UpdateSummaryButton.IsEnabled = true;
                    });
                }
            };
        }


        private async void ExportSummaryButton_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder summaryBuilder = new StringBuilder();

            int companyProfileVersion = tabPageNumbers.ContainsKey("Company Profile") ? tabPageNumbers["Company Profile"] : 1;
            int migrationDetailsVersion = tabPageNumbers.ContainsKey("Migration Details") ? tabPageNumbers["Migration Details"] : 1;
            int aiOpportunitiesVersion = tabPageNumbers.ContainsKey("AI Opportunities") ? tabPageNumbers["AI Opportunities"] : 1;

            // Iterate through each tab and get the current page content
            foreach (var tab in new[] { "Company Profile", "Migration Details", "AI Opportunities" })
            {
                int page = tabPageNumbers.ContainsKey(tab) ? tabPageNumbers[tab] : 1;
                string content = GetTabContent(tab, page);
                summaryBuilder.AppendLine(tab);
                summaryBuilder.AppendLine(new string('-', tab.Length));
                summaryBuilder.AppendLine(content);
                summaryBuilder.AppendLine();
            }

            string summary = summaryBuilder.ToString();
            // Export the summary to a file or display it as needed
            // For example, you can save it to a file or show it in a message box
            Debug.WriteLine(summary);

            //Save the summary to a file
            if (!Directory.Exists(SummaryConstants.SummaryDirectory))
            {
                Directory.CreateDirectory(SummaryConstants.SummaryDirectory);
            }
            string migrateDataPath = Path.Combine(
                SummaryConstants.SummaryDirectory,
                $"{SummaryConstants.SummaryPath}_{DateTime.Now:yyyy_MM_dd_HH_mm_ss}.md"
            );

            // Write the JSON to a .md file
            File.WriteAllText(migrateDataPath, summary);

            string pdfPath = Path.Combine(
                SummaryConstants.SummaryDirectory,
                $"{SummaryConstants.SummaryPath}_{DateTime.Now:yyyy_MM_dd_HH_mm_ss}.pdf"
            );

            var pdf = new MarkdownToPdf();
            pdf
             .PaperSize(Orionsoft.MarkdownToPdfLib.PaperSize.A4)
             .DefalutDpi(200)
             .Title("AzureMigrateExplore Insights")
             .DefaultFont("Calibri", 12)
             .PageMargins("1cm", "1cm", "1cm", "1.5cm")
             .Add(summary)
             .Save(pdfPath);

            await SendExportButtonClick(companyProfileVersion, migrationDetailsVersion, aiOpportunitiesVersion);

            await ShowExportDialogAsync(pdfPath);
        }

        public async Task InitializeWebView()
        {
            await CompanyProfileContent.EnsureCoreWebView2Async();
            await MigrationDetailsContent.EnsureCoreWebView2Async();
            await AIOpportunitiesContent.EnsureCoreWebView2Async();
        }

        private void WebView_NavigationStarting(WebView2 sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationStartingEventArgs args)
        {
            // Check if this is an external URL (http or https)
            if (args.Uri.StartsWith("http://") || args.Uri.StartsWith("https://"))
            {
                // Cancel the navigation in the WebView2
                args.Cancel = true;

                // Open the URL in the default browser
                var processInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = args.Uri,
                    UseShellExecute = true
                };
                System.Diagnostics.Process.Start(processInfo);
            }
        }
        
        private async Task ShowExportDialogAsync(string filePath)
        {
            ContentDialog dialog = new ContentDialog
            {
                Title = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Children =
                    {
                        new FontIcon
                        {
                            Glyph = "\uE73E",
                            Margin = new Thickness(0, 0, 10, 0),
                            Foreground = new SolidColorBrush(Colors.Green)
                        },
                        new TextBlock { Text = "Export Successful", VerticalAlignment = VerticalAlignment.Center }
                    }
                },
                Content = "The file has been exported successfully.",
                CloseButtonText = "OK",
                PrimaryButtonText = "Open Folder",
                XamlRoot = this.XamlRoot // Set the XamlRoot property
            };

            dialog.PrimaryButtonClick += (s, e) =>
            {
                // Open the folder containing the file
                string folderPath = Path.GetDirectoryName(filePath);
                Process.Start(new ProcessStartInfo
                {
                    FileName = folderPath,
                    UseShellExecute = true,
                    Verb = "open"
                });
            };

            await dialog.ShowAsync();
        }

        private string GetTabContent(string tab, int page)
        {
            switch (tab)
            {
                case "Migration Details":
                    return GetMigrationDetailsContent(page);
                case "Company Profile":
                    return GetCompanyProfileContent(page);
                case "AI Opportunities":
                    return GetAIOpportunitiesContent(page);
                default:
                    return string.Empty;
            }
        }

        private string GetMigrationDetailsContent(int page)
        {
            switch (page)
            {
                case 1:
                    return MigrationDetailsContent1;
                case 2:
                    return MigrationDetailsContent2;
                case 3:
                    return MigrationDetailsContent3;
                default:
                    return string.Empty;
            }
        }

        private string GetCompanyProfileContent(int page)
        {
            switch (page)
            {
                case 1:
                    return CompanyProfileContent1;
                case 2:
                    return CompanyProfileContent2;
                case 3:
                    return CompanyProfileContent3;
                default:
                    return string.Empty;
            }
        }

        private string GetAIOpportunitiesContent(int page)
        {
            switch (page)
            {
                case 1:
                    return AIOpportunitiesContent1;
                case 2:
                    return AIOpportunitiesContent2;
                case 3:
                    return AIOpportunitiesContent3;
                default:
                    return string.Empty;
            }
        }

        private async void SendMessage()
        {
            var sectionName = "MigrationDetails";
            if (currentTab == "Migration Details")
            {
                sectionName = "MigrationDetails";
            }
            else if (currentTab == "Company Profile")
            {
                sectionName = "CompanyProfile";
            }
            else
            {
                sectionName = "AIOpportunities";
            }
            string message = MessageTextBox.Text;
            if (message == "clear")
            {
                MessagesPanel.Children.Clear();
                MessageTextBox.Text = string.Empty;
                return;
            }
            if (!string.IsNullOrWhiteSpace(message))
            {
                _autoScroll = true; // Ensure auto-scroll is enabled
                AddMessage("You", message, DateTime.Now, true, currentTab);
                MessageTextBox.Text = string.Empty;
            }

            _ = DispatcherQueue.TryEnqueue(() =>
            {
                ShowLoadingMessage();
                UpdateSummaryButton.IsEnabled = false;
                DisableTabs();
            });

            GlobalConnection.UserAction = UserAction.AnswerQuery.ToString();
            bool aiAgreementCheckboxState = CopilotQuestionnaireObj.GetAIAgreementCheckboxState();
            var userQueryObject = new UserQuery
            {
                UserAction = GlobalConnection.UserAction,
                SectionName = sectionName,
                Payload = message,
                AIAgreementCheckboxState = aiAgreementCheckboxState
            };

            LogHandler logger = new LogHandler();
            ImportCoreReport coreReportData = SummaryGenerationHelper.FetchCoreReportExcelData(logger);

            var tenantId = coreReportData.CorePropertiesObj.TenantId;
            var subscriptionId = coreReportData.CorePropertiesObj.Subscription;

            var payloadContent = new QueryInput
            {
                UserQuery = userQueryObject,
                TenantId = tenantId,
                SubscriptionId = subscriptionId
            };

            string connectionId = GlobalConnection.ConnectionId;

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
                _ = DispatcherQueue.TryEnqueue(async() =>
                {
                    await ShowErrorDialogAsync("Error", $"An error occurred: {ex.Message} Please retry.");
                });
                throw;
            }
        }

        public async void NudgeButton_Click(object sender, RoutedEventArgs e)
        {
            DisableTabs();

            var sectionName = "MigrationDetails";
            if (currentTab == "Migration Details")
            {
                sectionName = "MigrationDetails";
            }
            else if (currentTab == "Company Profile")
            {
                sectionName = "CompanyProfile";
            }
            else
            {
                sectionName = "AIOpportunities";
            }

            var button = sender as Button;
            if (button == null)
            {
                Debug.WriteLine("Button is null");
                return;
            }

            var textBlock = button.Content as TextBlock;
            if (textBlock == null)
            {
                Debug.WriteLine("TextBlock is null");
                return;
            }

            string message = textBlock.Text;
            if (string.IsNullOrEmpty(message))
            {
                Debug.WriteLine("Message is null or empty");
                return;
            }

            _autoScroll = true; // Ensure auto-scroll is enabled
            _ = DispatcherQueue.TryEnqueue(() =>
            {
                mainObj.StartTimer();
                AddMessage("You", message, DateTime.Now, true, currentTab);
                ShowLoadingMessage();
                UpdateSummaryButton.IsEnabled = false;
            });

            bool aiAgreementCheckboxState = CopilotQuestionnaireObj.GetAIAgreementCheckboxState();
            GlobalConnection.UserAction = UserAction.AnswerQuery.ToString();
            var userQueryObject = new UserQuery
            {
                UserAction = GlobalConnection.UserAction,
                SectionName = sectionName,
                Payload = message,
                AIAgreementCheckboxState = aiAgreementCheckboxState
            };

            LogHandler logger = new LogHandler();
            ImportCoreReport coreReportData = SummaryGenerationHelper.FetchCoreReportExcelData(logger);

            var tenantId = coreReportData.CorePropertiesObj.TenantId;
            var subscriptionId = coreReportData.CorePropertiesObj.Subscription;

            var payloadContent = new QueryInput
            {
                UserQuery = userQueryObject,
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
                _ = DispatcherQueue.TryEnqueue(async () =>
                {
                    await ShowErrorDialogAsync("Error", $"An error occurred: {ex.Message} Please retry.");
                });
                throw;
            }
        }


        private async void LikeButton_Click(object sender, RoutedEventArgs e)
        {
            string feedbackComment = await ShowFeedbackDialogAsync("Liked");
            await SendFeedbackAsync(1, feedbackComment);
        }

        private async void DislikeButton_Click(object sender, RoutedEventArgs e)
        {
            string feedbackComment = await ShowFeedbackDialogAsync("Disliked");
            await SendFeedbackAsync(-1, feedbackComment);
        }

        private async Task SendFeedbackAsync(int feedbackScore, string feedbackComment)
        {
            LogHandler logger = new LogHandler();
            ImportCoreReport coreReportData = SummaryGenerationHelper.FetchCoreReportExcelData(logger);

            var tenantId = coreReportData.CorePropertiesObj.TenantId;
            var subscriptionId = coreReportData.CorePropertiesObj.Subscription;

            GlobalConnection.UserAction = UserAction.UserFeedback.ToString();

            var userFeedbackPayload = new UserFeedbackPayload
            {
                FeedbackScore = feedbackScore,
                FeedbackComment = feedbackComment
            };
            bool aiAgreementCheckboxState = CopilotQuestionnaireObj.GetAIAgreementCheckboxState();
            var userQueryObject = new UserQuery
            {
                UserAction = GlobalConnection.UserAction,
                SectionName = "",
                Payload = JsonConvert.SerializeObject(userFeedbackPayload),
                AIAgreementCheckboxState = aiAgreementCheckboxState
            };

            var payloadContent = new QueryInput
            {
                UserQuery = userQueryObject,
                TenantId = tenantId,
                SubscriptionId = subscriptionId
            };

            string connectionId = GlobalConnection.ConnectionId;

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
                Debug.WriteLine("Feedback sent to WebPubSub successfully.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error sending message to WebPubSub: {ex.Message}");
                _ = DispatcherQueue.TryEnqueue(async () =>
                {
                    await ShowErrorDialogAsync("Error", $"An error occurred: {ex.Message} Please retry.");
                });
                throw;
            }
        }

        private async Task SendExportButtonClick(int companyProfileVersion, int migrationDetailsVersion, int aIOpportunitiesVersion)
        {
            LogHandler logger = new LogHandler();
            ImportCoreReport coreReportData = SummaryGenerationHelper.FetchCoreReportExcelData(logger);

            var tenantId = coreReportData.CorePropertiesObj.TenantId;
            var subscriptionId = coreReportData.CorePropertiesObj.Subscription;

            GlobalConnection.UserAction = UserAction.ExportSummary.ToString();

            var exportSummaryPayload = new ExportSummaryPayload
            {
                CompanyProfileVersion = companyProfileVersion,
                MigrationDetailsVersion = migrationDetailsVersion,
                AIOpportunitiesVersion = aIOpportunitiesVersion,
            };

            bool aiAgreementCheckboxState = CopilotQuestionnaireObj.GetAIAgreementCheckboxState();

            var userQueryObject = new UserQuery
            {
                UserAction = GlobalConnection.UserAction,
                SectionName = "",
                Payload = JsonConvert.SerializeObject(exportSummaryPayload),
                AIAgreementCheckboxState = aiAgreementCheckboxState
            };

            var payloadContent = new QueryInput
            {
                UserQuery = userQueryObject,
                TenantId = tenantId,
                SubscriptionId = subscriptionId
            };

            string connectionId = GlobalConnection.ConnectionId;

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
                Debug.WriteLine("Export Summary payload sent to WebPubSub successfully.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error sending message to WebPubSub: {ex.Message}");
                _ = DispatcherQueue.TryEnqueue(async () =>
                {
                    await ShowErrorDialogAsync("Error", $"An error occurred: {ex.Message} Please close the app and re-try.");
                });
                throw;
            }
        }

        private async Task<string> ShowFeedbackDialogAsync(string feedbackType)
        {
            TextBox feedbackTextBox = new TextBox
            {
                AcceptsReturn = true,
                Height = 100,
                TextWrapping = TextWrapping.Wrap
            };

            ContentDialog feedbackDialog = new ContentDialog
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
                        new TextBlock { Text = $"Please provide your feedback ({feedbackType})", VerticalAlignment = VerticalAlignment.Center }
                    }
                },
                Content = feedbackTextBox,
                PrimaryButtonText = "Submit",
                CloseButtonText = "Cancel",
                XamlRoot = this.XamlRoot
            };

            ContentDialogResult result = await feedbackDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                return feedbackTextBox.Text;
            }
            else
            {
                return "";
            }
        }

        private async Task ShowDialogAsync(string title, string content)
        {
            ContentDialog dialog = new ContentDialog
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
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };

            await dialog.ShowAsync();
        }

        private void InitializeNudges()
        {
            // Initialize nudges for each tab
            chatHistory["Migration Details"] = new List<UIElement>
            {
                CreateNudgeButton(MigrationDetailsNudge1),
                CreateNudgeButton(MigrationDetailsNudge2),
                CreateNudgeButton(MigrationDetailsNudge3),
            };

            chatHistory["Company Profile"] = new List<UIElement>
            {
                CreateNudgeButton(CompanyProfileNudge1),
                CreateNudgeButton(CompanyProfileNudge2),
                CreateNudgeButton(CompanyProfileNudge3)
            };

            chatHistory["AI Opportunities"] = new List<UIElement>
            {
                CreateNudgeButton(AIOpportunitiesNudge1),
                CreateNudgeButton(AIOpportunitiesNudge2),
                CreateNudgeButton(AIOpportunitiesNudge3)
            };

            // Load nudges for the default tab
            foreach (var nudge in chatHistory[currentTab])
            {
                MessagesPanel.Children.Add(nudge);
            }
        }

        private void ReInitializeNudges(string sectionName)
        {

            if (sectionName == "Migration Details")
            {
                chatHistory["Migration Details"] = new List<UIElement>
            {
                CreateNudgeButton(MigrationDetailsNudge1),
                CreateNudgeButton(MigrationDetailsNudge2),
                CreateNudgeButton(MigrationDetailsNudge3),
            };
                // Load nudges for the default tab
                foreach (var nudge in chatHistory[currentTab])
                {
                    MessagesPanel.Children.Add(nudge);
                }
            }

            else if (sectionName == "Company Profile")
            {
                // Initialize nudges for each tab

                chatHistory["Company Profile"] = new List<UIElement>
            {
                CreateNudgeButton(CompanyProfileNudge1),
                CreateNudgeButton(CompanyProfileNudge2),
                CreateNudgeButton(CompanyProfileNudge3)
            };
                // Load nudges for the default tab
                foreach (var nudge in chatHistory[currentTab])
                {
                    MessagesPanel.Children.Add(nudge);
                }
            }

            else
            {
                chatHistory["AI Opportunities"] = new List<UIElement>
            {
                CreateNudgeButton(AIOpportunitiesNudge1),
                CreateNudgeButton(AIOpportunitiesNudge2),
                CreateNudgeButton(AIOpportunitiesNudge3)
            };
                // Load nudges for the default tab
                foreach (var nudge in chatHistory[currentTab])
                {
                    MessagesPanel.Children.Add(nudge);
                }
            }
        }

        private void UpdateChatContent(string tabHeader)
        {
            // Save current messages
            if (!string.IsNullOrEmpty(currentTab))
            {
                chatHistory[currentTab] = new List<UIElement>(MessagesPanel.Children);
                tabPageNumbers[currentTab] = currentPage;
            }

            // Clear current messages
            MessagesPanel.Children.Clear();

            // Update the current tab
            currentTab = tabHeader;

            // Set the current page for the new tab
            if (tabPageNumbers.ContainsKey(currentTab))
            {
                currentPage = tabPageNumbers[currentTab];
            }
            else
            {
                currentPage = 1;
                tabPageNumbers[currentTab] = currentPage;
            }

            // Load stored messages for the selected tab
            if (chatHistory.ContainsKey(tabHeader))
            {
                foreach (var message in chatHistory[tabHeader])
                {
                    MessagesPanel.Children.Add(message);
                }
            }

            // Track visited tabs
            visitedTabs.Add(currentTab);

            // Update the page indicator
            UpdatePageIndicator();
        }

        private void Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is Pivot pivot)
            {
                var selectedItem = pivot.SelectedItem as PivotItem;
                if (selectedItem != null)
                {
                    UpdateChatContent(selectedItem.Header.ToString());
                    //InitializeNudges(); // Reinitialize nudges when the tab changes
                }
            }
        }

        private Button CreateNudgeButton(string content)
        {
            var button = new Button
            {
                Content = new TextBlock
                {
                    Text = content,
                    TextWrapping = TextWrapping.Wrap
                },
                Style = (Style)Resources["AccentButtonStyle"],
                Margin = new Thickness(5),
                Width = double.NaN, // Auto width
                HorizontalAlignment = HorizontalAlignment.Stretch
            };
            button.Click += NudgeButton_Click;
            return button;
        }

        private void PreviousPage_Click(object sender, RoutedEventArgs e)
        {
            if (currentPage > 1)
            {
                currentPage--;
                tabPageNumbers[currentTab] = currentPage;
                UpdatePageIndicator();
            }
        }

        private void NextPage_Click(object sender, RoutedEventArgs e)
        {
            if (currentPage < totalPages)
            {
                currentPage++;
                tabPageNumbers[currentTab] = currentPage;
                UpdatePageIndicator();
            }
        }

        private async Task UpdatePageIndicator()
        {
            await InitializeWebView();
            totalPages = tabTotalPages.ContainsKey(currentTab) ? tabTotalPages[currentTab] : 1;

            switch (currentTab)
            {
                case "Migration Details":
                    UpdateMigrationDetailsTabContent();
                    PageIndicator.Text = $"{currentPage} of {totalPages}";
                    break;
                case "Company Profile":
                    UpdateCompanyProfileTabContent();
                    PageIndicator.Text = $"{currentPage} of {totalPages}";
                    break;
                case "AI Opportunities":
                    UpdateAIOpportunitiesTabContent();
                    PageIndicator.Text = $"{currentPage} of {totalPages}";
                    break;
            }
        }

        private void UpdateMigrationDetailsTabContent()
        {
            switch (currentPage)
            {
                case 1:
                    var htmlContent = Markdig.Markdown.ToHtml(MigrationDetailsContent1);
                    MigrationDetailsContent.CoreWebView2.NavigateToString(htmlContent);
                    break;
                case 2:
                    htmlContent = Markdig.Markdown.ToHtml(MigrationDetailsContent2);
                    MigrationDetailsContent.CoreWebView2.NavigateToString(htmlContent);
                    break;
                case 3:
                    htmlContent = Markdig.Markdown.ToHtml(MigrationDetailsContent3);
                    MigrationDetailsContent.CoreWebView2.NavigateToString(htmlContent);
                    break;
            }
        }


        private void UpdateCompanyProfileTabContent()
        {
            switch (currentPage)
            {
                case 1:
                    var htmlContent = Markdig.Markdown.ToHtml(CompanyProfileContent1);
                    CompanyProfileContent.CoreWebView2.NavigateToString(htmlContent);
                    break;
                case 2:
                    htmlContent = Markdig.Markdown.ToHtml(CompanyProfileContent2);
                    CompanyProfileContent.CoreWebView2.NavigateToString(htmlContent);
                    break;
                case 3:
                    htmlContent = Markdig.Markdown.ToHtml(CompanyProfileContent3);
                    CompanyProfileContent.CoreWebView2.NavigateToString(htmlContent);
                    break;
            }
        }

        private void UpdateAIOpportunitiesTabContent()
        {
            switch (currentPage)
            {
                case 1:
                    var htmlContent = Markdig.Markdown.ToHtml(AIOpportunitiesContent1);
                    AIOpportunitiesContent.CoreWebView2.NavigateToString(htmlContent);
                    break;
                case 2:
                    htmlContent = Markdig.Markdown.ToHtml(AIOpportunitiesContent2);
                    AIOpportunitiesContent.CoreWebView2.NavigateToString(htmlContent);
                    break;
                case 3:
                    htmlContent = Markdig.Markdown.ToHtml(AIOpportunitiesContent3);
                    AIOpportunitiesContent.CoreWebView2.NavigateToString(htmlContent);
                    break;
            }
        }

        private Dictionary<string, int> tabTotalPages = new Dictionary<string, int>
        {
            { "Migration Details", 1 },
            { "Company Profile", 1 },
            { "AI Opportunities", 1 }
        };

        private Dictionary<string, int> sectionRegenerateCount = new Dictionary<string, int>
        {
            { "Migration Details", 1 },
            { "Company Profile", 1 },
            { "AI Opportunities", 1 }
        };

        private async void RegenerateSummaryButton_Click(object sender, RoutedEventArgs e)
        {
            mainObj.EnableOverlayGrid();
            try
            {
                var sectionName = "MigrationDetails";
                if (currentTab == "Migration Details")
                {
                    sectionName = "MigrationDetails";
                }
                else if (currentTab == "Company Profile")
                {
                    sectionName = "CompanyProfile";
                }
                else
                {
                    sectionName = "AIOpportunities";
                }

                // Increment the regenerate count for the current section
                sectionRegenerateCount[currentTab]++;
                if (sectionRegenerateCount[currentTab] > 3)
                {
                    // Show a dialog saying "You can only regenerate summary 2 times"
                    _ = DispatcherQueue.TryEnqueue(async () =>
                    {
                        await ShowErrorDialogAsync("Regenerate Limit Exceeded", "You can only regenerate the summary 2 times.");
                    });
                    mainObj.DisableOverlayGrid();
                    return;
                }

                mainObj.EnableOverlayGrid();
                await mainObj.ReGenerateSummary(sectionName);

                // Add new pages for the current tab
                if (sectionRegenerateCount[currentTab] == 2)
                {
                    currentPage = 2;
                    tabTotalPages[currentTab] = 2;
                }
                else if (sectionRegenerateCount[currentTab] == 3)
                {
                    currentPage = 3;
                    tabTotalPages[currentTab] = 3;
                }
                UpdatePageIndicator();
            }
            catch (Exception ex)
            {
                mainObj.DisableOverlayGrid();
                // Show an error dialog
                _ = DispatcherQueue.TryEnqueue(async () =>
                {
                    await ShowErrorDialogAsync("Error", $"An error occurred: {ex.Message} Please close the app and re-try.");
                });
            }
        }

        private async Task ShowErrorDialogAsync(string title, string content)
        {
            ContentDialog errorDialog = new ContentDialog
            {
                Title = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Children =
                    {
                        new FontIcon
                        {
                            Glyph = "\uE711",
                            Margin = new Thickness(0, 0, 10, 0),
                            Foreground = new SolidColorBrush(Colors.Red)
                        }, // Information icon
                        new TextBlock { Text = title, VerticalAlignment = VerticalAlignment.Center }
                    }
                },
                Content = content,
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot // Set the XamlRoot property
            };

            await errorDialog.ShowAsync();
        }

        public static class GlobalConnection
        {
            public static string ConnectionId { get; set; }
            public static string SessionId { get; set; }
            public static string UserAction { get; set; }
            public static string PubSubUrl { get; set; }
        }
    }
}
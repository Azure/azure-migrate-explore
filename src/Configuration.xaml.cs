// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using Azure.Migrate.Explore.Common;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
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
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace AzureMigrateExplore
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Configuration : Page
    {
        private readonly MainWindow mainObj;
        private readonly AssessmentSettings assessmentPageObj;
        public Configuration(MainWindow obj, AssessmentSettings assessmentSettingsObj)
        {
            this.InitializeComponent();
            mainObj = obj;
            assessmentPageObj = assessmentSettingsObj;
        }

        #region Set Default Values
        public void SetDefaultConfigurationValues()
        {
            if (mainObj.GetHasImportInventory())
            {
                ImportRadioButton.IsEnabled = true;
                ImportRadioButton.IsChecked = true;
                QuickAVSRadioButton.IsChecked = true;
                ComprehensiveRadioButton.IsEnabled = true;
            }
            else
            {
                ImportRadioButton.IsEnabled = false;
                ImportRadioButton.IsChecked = false;
            }
            if (mainObj.GetHasApplianceInventory())
            {
                ApplianceRadioButton.IsEnabled = true;
                ApplianceRadioButton.IsChecked = true;
                VMwareCheckBox.IsEnabled = true;
                VMwareCheckBox.IsChecked = true;
                HyperVCheckBox.IsEnabled = true;
                HyperVCheckBox.IsChecked = true;
                PhysicalCheckBox.IsEnabled = true;
                PhysicalCheckBox.IsChecked = true;
                ComprehensiveRadioButton.IsChecked = true;
            }
            else
            {
                ApplianceRadioButton.IsEnabled = false;
                ApplianceRadioButton.IsChecked = false;
                VMwareCheckBox.IsEnabled = false;
                VMwareCheckBox.IsChecked = false;
                HyperVCheckBox.IsEnabled = false;
                HyperVCheckBox.IsChecked = false;
                PhysicalCheckBox.IsEnabled = false;
                PhysicalCheckBox.IsChecked = false;
                ComprehensiveRadioButton.IsEnabled = true;
            }

            ExpressWorkflowRadioButton.IsChecked = true;
        }
        #endregion

        #region Workflow Radio Buttons Checked Changed
        private void Custom_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (CustomWorkflowRadioButton.IsChecked == true)
            {
                CustomWorkflowPicker.Visibility = Visibility.Visible;
                string selectedModule = (string)CustomWorkflowPicker.SelectedItem;
                if (selectedModule != null && selectedModule.Equals("Assessment"))
                {
                    EnableBusinessProposal();
                    mainObj.EnableNextButton();
                }
                else
                {
                    mainObj.EnableSubmitButton();
                    DisableBusinessProposal();
                }
            }
            mainObj.MakeConfigurationActionButtonsEnabledDecision();
            mainObj.MakeConfigurationTabButtonEnableDecisions();
        }

        private void Express_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (ExpressWorkflowRadioButton.IsChecked == true)
            {
                CustomWorkflowPicker.Visibility = Visibility.Collapsed;

                CustomWorkflowPicker.SelectedItem = null;
                EnableBusinessProposal();
            }

            mainObj.MakeConfigurationTabButtonEnableDecisions();
            mainObj.MakeConfigurationActionButtonsEnabledDecision();
        }
        #endregion

        #region CheckBox Checked Changed
        private void VMWare_CheckedChanged(object sender, RoutedEventArgs e)
        {
            mainObj.MakeConfigurationTabButtonEnableDecisions();
            mainObj.MakeConfigurationActionButtonsEnabledDecision();
        }

        private void HyperV_CheckedChanged(object sender, RoutedEventArgs e)
        {
            mainObj.MakeConfigurationTabButtonEnableDecisions();
            mainObj.MakeConfigurationActionButtonsEnabledDecision();
        }

        private void Physical_CheckedChanged(object sender, RoutedEventArgs e)
        {
            mainObj.MakeConfigurationTabButtonEnableDecisions();
            mainObj.MakeConfigurationActionButtonsEnabledDecision();
        }
        #endregion

        #region ComboBox Selection Change Committed
        private void CustomWorkflowPicker_SelectedIndexChanged(object sender, RoutedEventArgs e)
        {
            string selectedModule = (string)CustomWorkflowPicker.SelectedItem;
            if (selectedModule != null && selectedModule.Equals("Assessment"))
            {
                if (!IsDiscoveryReportPresent())
                {
                    DisplayAlert("Azure Migrate Explore", "No discovery report found. Please complete discovery before running assessments.", "OK");
                    CustomWorkflowPicker.SelectedItem = "Discovery";
                    mainObj.EnableSubmitButton();
                }
                else
                {
                    EnableBusinessProposal();
                    mainObj.HideSubmitButton();
                    //mainObj.ShowNextButton();
                    //mainObj.EnableNextButton();
                }
            }
            else if (selectedModule != null && selectedModule.Equals("Discovery"))
            {
                DisableBusinessProposal();
                mainObj.DisableAssessmentSettingsTabButton();
                mainObj.EnableSubmitButton();
                mainObj.ShowSubmitButton();
                mainObj.HideNextButton();
            }

            mainObj.MakeConfigurationActionButtonsEnabledDecision();
            mainObj.MakeConfigurationTabButtonEnableDecisions();
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
            return result == ContentDialogResult.Primary;
        }
        #endregion

        #region Validation
        public bool ValidateConfiguration()
        {
            if (!ValidateAzureMigrateSourceAppliance())
                return false;

            if (!ValidateWorkflow())
                return false;

            if (!ValidateBusinessProposal())
                return false;

            return true;
        }

        private bool ValidateAzureMigrateSourceAppliance()
        {
            if (!ImportRadioButton.IsChecked == true && !ApplianceRadioButton.IsChecked == true)
                return false;

            if (ImportRadioButton.IsChecked == true && ApplianceRadioButton.IsChecked == true)
                return false;

            if (ApplianceRadioButton.IsChecked == true &&
                !VMwareCheckBox.IsChecked == true &&
                !HyperVCheckBox.IsChecked == true &&
                !PhysicalCheckBox.IsChecked == true)
                return false;

            if (ImportRadioButton.IsChecked == true &&
                (VMwareCheckBox.IsChecked == true ||
                HyperVCheckBox.IsChecked == true ||
                PhysicalCheckBox.IsChecked == true))
                return false;

            return true;
        }

        private bool ValidateWorkflow()
        {
            if (CustomWorkflowRadioButton.IsChecked == false && ExpressWorkflowRadioButton.IsChecked == false)
                return false;

            if (CustomWorkflowRadioButton.IsChecked == true)
            {
                if (CustomWorkflowPicker.SelectedItem == null)
                    return false;

                // Handle the selected item as a string
                string selectedItem = CustomWorkflowPicker.SelectedItem as string;
                if (selectedItem != "Discovery" && selectedItem != "Assessment")
                    return false;
            }

            return true;
        }

        private bool ValidateBusinessProposal()
        {
            string selectedModule = (string)CustomWorkflowPicker.SelectedItem;
            if (((selectedModule != null && selectedModule.Equals("Assessment")) || ExpressWorkflowRadioButton.IsChecked == true) &&
               (ComprehensiveRadioButton.IsChecked == false && QuickAVSRadioButton.IsChecked == false))
                return false;

            if (selectedModule != null && selectedModule.Equals("Discovery") &&
               (ComprehensiveRadioButton.IsChecked == true || QuickAVSRadioButton.IsChecked == true))
                return false;

            return true;
        }

        #endregion

        #region Display assessment settings tab and submit button decision maker
        // If Submit Button is displayed, Assessment settings tab will not be displayed.
        public bool DisplaySubmitButton()
        {
            if (CustomWorkflowRadioButton.IsChecked == true && (string)CustomWorkflowPicker.SelectedItem == "Discovery")
                return true;

            return false;
        }
        #endregion

        #region ComboBox Mouse Click
        private void CustomWorkflowPicker_Focused(object sender, RoutedEventArgs e)
        {
            CustomWorkflowPicker.Focus(FocusState.Programmatic);
        }

        #endregion

        #region Mouse Hover Descriptions
        private void AzureMigrateSourceApplianceGroupBox_MouseHover(object sender, EventArgs e)
        {
            UpdateAzureMigrateSourceApplianceDescription();
        }

        private void VMwareCheckBox_MouseHover(object sender, EventArgs e)
        {
            UpdateAzureMigrateSourceApplianceDescription();
        }

        private void HyperVCheckBox_MouseHover(object sender, EventArgs e)
        {
            UpdateAzureMigrateSourceApplianceDescription();
        }

        private void PhysicalCheckBox_MouseHover(object sender, EventArgs e)
        {
            UpdateAzureMigrateSourceApplianceDescription();
        }

        private void WorkflowGroupBox_MouseHover(object sender, EventArgs e)
        {
            UpdateWorkflowDescription();
        }

        private void CustomWorkflowRadioButton_MouseHover(object sender, EventArgs e)
        {
            UpdateWorkflowDescription();
        }

        private void ExpressWorkflowRadioButton_MouseHover(object sender, EventArgs e)
        {
            UpdateWorkflowDescription();
        }

        private void CustomWorkflowInfoPictureBox_MouseHover(object sender, EventArgs e)
        {
            UpdateWorkflowDescription();
        }

        private void ExpressWorkflowInfoPictureBox_MouseHover(object sender, EventArgs e)
        {
            UpdateWorkflowDescription();
        }

        private void CustomWorkflowPicker_MouseHover(object sender, EventArgs e)
        {
            UpdateWorkflowDescription();
        }

        private void UpdateAzureMigrateSourceApplianceDescription()
        {
            UpdateDescriptionTextBox("Azure Migrate Source Appliance", "Azure Migrate Explore can be used to assess servers in VMware, Hyper-V and Physical/Bare-Metal environments.\n\nSelect appropriate source appliance stacks that were used to discover the respective environments using Azure Migrate: Discovery and Assessment tool.");
        }

        private void UpdateWorkflowDescription()
        {
            UpdateDescriptionTextBox("Workflow", "Azure Migrate Explore supports two workflows:\n\t1. Custom - Enables you to make customizations before generating the reports and presentations - classifying workloads into \"Dev\" or\n\t    \"Prod\", moving servers out of scope, and scoping discovered servers to IaaS assessments. This will help you present advantages of\n\t    Dev/Test pricing and customize the scope of presentation.\n\t2. Express - Enables you to generate reports and presentations quickly - assuming all discovered servers are in-scope and are\n\t    production servers.");
        }

        private void UpdateDescriptionTextBox(string descriptionHeader, string description)
        {
            //ConfigurationDescriptionGroupBox.IsVisible = true;
            //ConfigurationDescriptionGroupBox.Text = descriptionHeader;
            //ConfigurationDescriptionRichTextBox.Text = description;
        }
        #endregion

        #region Getter Methods
        public List<string> GetAzureMigrateSourceAppliances()
        {
            List<string> azureMigrateSourceAppliances = new List<string>();

            if (VMwareCheckBox.IsChecked == true)
                azureMigrateSourceAppliances.Add("vmware");
            if (HyperVCheckBox.IsChecked == true)
                azureMigrateSourceAppliances.Add("hyperv");
            if (PhysicalCheckBox.IsChecked == true)
                azureMigrateSourceAppliances.Add("physical");
            if (ImportRadioButton.IsChecked == true)
                azureMigrateSourceAppliances.Add("import");

            return azureMigrateSourceAppliances;
        }

        public bool IsExpressWorkflowSelected()
        {
            return ExpressWorkflowRadioButton.IsChecked ?? false;
        }

        public string GetModule()
        {
            return (string)CustomWorkflowPicker.SelectedItem;
        }


        public string GetBusinessProposal()
        {
            if (QuickAVSRadioButton.IsChecked == true)
                return BusinessProposal.AVS.ToString();

            if (ComprehensiveRadioButton.IsChecked == true)
                return BusinessProposal.Comprehensive.ToString();

            return string.Empty;
        }

        #endregion

        #region Setter Methods
        public void SetModule(string setModuleAs)
        {
            CustomWorkflowPicker.SelectedItem = setModuleAs;

            mainObj.MakeConfigurationActionButtonsEnabledDecision();
            mainObj.MakeConfigurationTabButtonEnableDecisions();
        }
        #endregion

        #region visit links
        private void CustomWorkflowInfoPictureBox_Clicked(object sender, EventArgs e)
        {
            VisitLink("https://go.microsoft.com/fwlink/?linkid=2215651");
        }

        private void ExpressWorkflowInfoPictureBox_Clicked(object sender, EventArgs e)
        {
            VisitLink("https://go.microsoft.com/fwlink/?linkid=2215823");
        }

        private void VisitLink(string goLinkUrl)
        {
            Launcher.LaunchUriAsync(new Uri(goLinkUrl));
        }
        #endregion

        #region Utilities
        private bool IsDiscoveryReportPresent()
        {
            if (!Directory.Exists(UtilityFunctions.GetReportsDirectory()))
                return false;
            if (!File.Exists(UtilityFunctions.GetReportsDirectory() + "\\" + DiscoveryReportConstants.DiscoveryReportName))
                return false;

            return true;
        }

        private void EnableBusinessProposal()
        {
            QuickAVSRadioButton.IsEnabled = true;
            ComprehensiveRadioButton.IsEnabled = true;
        }

        private void DisableBusinessProposal()
        {
            QuickAVSRadioButton.IsEnabled = false;
            QuickAVSRadioButton.IsChecked = false;
            ComprehensiveRadioButton.IsEnabled = false;
            ComprehensiveRadioButton.IsChecked = false;
            mainObj.ShowSubmitButton();
            mainObj.EnableSubmitButton();
        }

        private void CheckOnlyQuickAvsProposal()
        {
            QuickAVSRadioButton.IsChecked = true;
            QuickAVSRadioButton.IsEnabled = true;
            ComprehensiveRadioButton.IsEnabled = true;
        }

        private void DisableHypervAndPhysicalCheckBoxes()
        {
            ImportRadioButton.IsChecked = false;
            VMwareCheckBox.IsEnabled = true;
            VMwareCheckBox.IsChecked = true;
            HyperVCheckBox.IsEnabled = false;
            HyperVCheckBox.IsChecked = false;
            PhysicalCheckBox.IsEnabled = false;
            PhysicalCheckBox.IsChecked = false;
        }
        #endregion

        private void OnBackButtonClicked(object sender, EventArgs e)
        {
            // Handle back button click
        }

        private void OnNextButtonClicked(object sender, EventArgs e)
        {
            // Handle next button click
        }

        private void ApplianceRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            ApplianceOptionsPanel.Visibility = Visibility.Visible;

            ImportRadioButton.IsChecked = false;
            VMwareCheckBox.IsEnabled = true;
            VMwareCheckBox.IsChecked = true;
            HyperVCheckBox.IsEnabled = true;
            HyperVCheckBox.IsChecked = true;
            PhysicalCheckBox.IsEnabled = true;
            PhysicalCheckBox.IsChecked = true;

            EnableBusinessProposal();
            if (QuickAVSRadioButton.IsChecked == true)
            {
                DisableHypervAndPhysicalCheckBoxes();
            }
        }

        private void ImportRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            ApplianceOptionsPanel.Visibility = Visibility.Collapsed;
            ComprehensiveRadioButton.IsEnabled = false;
            HyperVCheckBox.IsEnabled = false;
            HyperVCheckBox.IsChecked = false;
            VMwareCheckBox.IsEnabled = false;
            VMwareCheckBox.IsChecked = false;
            PhysicalCheckBox.IsEnabled = false;
            PhysicalCheckBox.IsChecked = false;

            string selectedModule = (string)CustomWorkflowPicker.SelectedItem;
            if (ExpressWorkflowRadioButton.IsChecked == true || (selectedModule != null && selectedModule.Equals("Assessment")))
            {
                CheckOnlyQuickAvsProposal();
            }
            else
            {
                DisableBusinessProposal();
            }
            mainObj.MakeConfigurationActionButtonsEnabledDecision();
            mainObj.MakeConfigurationTabButtonEnableDecisions();
        }

        private void ComprehensiveRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (ComprehensiveRadioButton.IsChecked == true)
            {

                //assessmentSettings.EnableOptimizationPreferenceComboBox();
                //assessmentPageObj.EnableAssessmentDurationComboBox();
                if (ApplianceRadioButton.IsChecked == true)
                {
                    VMwareCheckBox.IsEnabled = true;
                    VMwareCheckBox.IsChecked = true;
                    HyperVCheckBox.IsEnabled = true;
                    HyperVCheckBox.IsChecked = true;
                    PhysicalCheckBox.IsEnabled = true;
                    PhysicalCheckBox.IsChecked = true;
                }
            }

            mainObj.MakeConfigurationActionButtonsEnabledDecision();
            mainObj.MakeConfigurationTabButtonEnableDecisions();
        }

        private void QuickAVSRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (QuickAVSRadioButton.IsChecked == true)
            {                
                //assessmentPageObj.DisableOptimizationPreferenceComboBox();
                //assessmentPageObj.DisableAssessmentDurationComboBox();
                if (ApplianceRadioButton.IsChecked == true)
                {
                    DisableHypervAndPhysicalCheckBoxes();
                }
            }

            mainObj.MakeConfigurationActionButtonsEnabledDecision();
            mainObj.MakeConfigurationTabButtonEnableDecisions();
        }

        public bool IsQuickAVSButtonClicked()
        {
            if (QuickAVSRadioButton.IsChecked == true)
            {
                return true;
            }
            return false;
        }
    }
}

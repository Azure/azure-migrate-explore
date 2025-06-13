// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
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
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI;
using Azure.Migrate.Explore.Common;
using Azure.Migrate.Explore.Logger;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace AzureMigrateExplore
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CopilotQuestionnaire : Page
    {
        private MainWindow mainObj;
        LogHandler logger = new LogHandler();
        private const string CheckboxStateFilePath = "AIAgreementCheckBoxState.txt";
        private string placeholderText = "Please provide other relevant details regarding customer's migration scenarios like:\nMigration pain points \nDetails on motivation for migration\nIndustry and compliance requirements\nMigration preference - PaaS/IaaS\nDetails about licenses available with the customer. # of licenses available of each type\nMigration goals";
        public CopilotQuestionnaire(MainWindow obj)
        {
            this.InitializeComponent();
            mainObj = obj;
            // Set default values
            SetDefaultConfigurationValues();

            // Add event handlers for Entry and Editor focus
            //OtherDetailsRichTextBox.GotFocus += OtherDetailsRichTextBox_Focused;
            //OtherDetailsRichTextBox.LostFocus += OtherDetailsRichTextBox_Unfocused;


            // Add event handlers for ComboBox selection change and text change
            MotivationComboBox.SelectionChanged += MotivationComboBox_SelectionChangeCommitted;
            //WindowsLicenseComboBox.SelectionChanged += WindowsLicenseComboBox_SelectionChangeCommitted;
            //SqlLicenseComboBox.SelectionChanged += SqlLicenseComboBox_SelectionChangeCommitted;

            CustomerNameTextBox.TextChanged += CustomerNameTextBox_TextChanged;
            MotivationComboBox.SelectionChanged += MotivationComboBox_TextChanged;
            //WindowsLicenseComboBox.SelectionChanged += WindowsLicenseComboBox_TextChanged;
            //SqlLicenseComboBox.SelectionChanged += SqlLicenseComboBox_TextChanged;

            // Load the checkbox state
            LoadCheckboxState();

        }

        #region Set Default Values
        public void SetDefaultConfigurationValues()
        {
            //PlanToMigrationDateTimePicker.Date = DateTime.Today;

            //OtherDetailsRichTextBox.Text = placeholderText;
            //OtherDetailsRichTextBox.Foreground = new SolidColorBrush(Colors.Gray);
        }
        #endregion

        private void AIAgreementCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (AIAgreementCheckBox.IsChecked == true && ValidateCustomerIndustry() && ValidateCustomerName() && ValidateMotivation())
            {
                mainObj.EnableGenerateSummaryButton();
            }
            else
            {
                mainObj.DisableGenerateSummaryButton();
            }
            // Save the checkbox state
            SaveCheckboxState();
        }

        private void SaveCheckboxState()
        {
            try
            {
                File.WriteAllText(CheckboxStateFilePath, AIAgreementCheckBox.IsChecked.ToString());
            }
            catch (Exception ex)
            {
                logger.LogError($"Error saving checkbox state: {ex.Message}");
            }
        }

        private void LoadCheckboxState()
        {
            try
            {
                if (File.Exists(CheckboxStateFilePath))
                {
                    string state = File.ReadAllText(CheckboxStateFilePath);
                    AIAgreementCheckBox.IsChecked = bool.Parse(state);
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"Error loading checkbox state: {ex.Message}");
            }
        }

        #region ComboBox Selection Change Committed

        private void MotivationComboBox_SelectionChangeCommitted(object sender, RoutedEventArgs e)
        {
            mainObj.MakeCopilotQuestionnaireActionButtonsEnabledDecisions();
            mainObj.MakeCopilotQuestionnaireTabButtonEnableDecisions();
        }

        private void WindowsLicenseComboBox_SelectionChangeCommitted(object sender, RoutedEventArgs e)
        {
            mainObj.MakeCopilotQuestionnaireActionButtonsEnabledDecisions();
            mainObj.MakeCopilotQuestionnaireTabButtonEnableDecisions();
        }

        private void SqlLicenseComboBox_SelectionChangeCommitted(object sender, RoutedEventArgs e)
        {
            mainObj.MakeCopilotQuestionnaireActionButtonsEnabledDecisions();
            mainObj.MakeCopilotQuestionnaireTabButtonEnableDecisions();
        }
        #endregion

        #region Text Change Handler
        private void CustomerNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            mainObj.MakeCopilotQuestionnaireActionButtonsEnabledDecisions();
            mainObj.MakeCopilotQuestionnaireTabButtonEnableDecisions();
        }

        private void MotivationComboBox_TextChanged(object sender, RoutedEventArgs e)
        {
            mainObj.MakeCopilotQuestionnaireActionButtonsEnabledDecisions();
            mainObj.MakeCopilotQuestionnaireTabButtonEnableDecisions();
        }

        private void WindowsLicenseComboBox_TextChanged(object sender, RoutedEventArgs e)
        {
            mainObj.MakeCopilotQuestionnaireActionButtonsEnabledDecisions();
            mainObj.MakeCopilotQuestionnaireTabButtonEnableDecisions();
        }

        private void SqlLicenseComboBox_TextChanged(object sender, RoutedEventArgs e)
        {
            mainObj.MakeCopilotQuestionnaireActionButtonsEnabledDecisions();
            mainObj.MakeCopilotQuestionnaireTabButtonEnableDecisions();
        }
        #endregion

        //private void OtherDetailsRichTextBox_Focused(object sender, RoutedEventArgs e)
        //{
        //    if (OtherDetailsRichTextBox.Text == placeholderText)
        //    {
        //        OtherDetailsRichTextBox.Text = string.Empty;
        //        OtherDetailsRichTextBox.Foreground = new SolidColorBrush(Colors.Black);
        //    }
        //}

        //private void OtherDetailsRichTextBox_Unfocused(object sender, RoutedEventArgs e)
        //{
        //    if (string.IsNullOrWhiteSpace(OtherDetailsRichTextBox.Text))
        //    {
        //        OtherDetailsRichTextBox.Text = placeholderText;
        //        OtherDetailsRichTextBox.Foreground = new SolidColorBrush(Colors.Gray);
        //    }
        //}

        #region Validation
        public bool ValidateQuestionnaireDetails()
        {
            return (
                ValidateCustomerName() &&
                ValidateCustomerIndustry() &&
                ValidateMotivation() &&
                ValidateAIConsent()
                );
        }

        private bool ValidateCustomerIndustry()
        {
            if (string.IsNullOrWhiteSpace(CustomerIndustryTextBox.Text))
            {
                return false;
            }

            return true;
        }

        private bool ValidateCustomerName()
        {
            if (string.IsNullOrWhiteSpace(CustomerNameTextBox.Text))
            {
                return false;
            }

            return true;
        }

        private bool ValidateMotivation()
        {
            if (MotivationComboBox.SelectedItem is not string selectedItem)
            {
                return false;
            }

            if (!(selectedItem == "Datacenter exit"
                || selectedItem == "Cloud transformation"
                || selectedItem == "Legacy servers"
                || selectedItem == "Business innovation"
                || selectedItem == "Security protection"
                || selectedItem == "Other"))
            {
                return false;
            }

            return true;
        }

        private bool ValidateAIConsent()
        {
            if (AIAgreementCheckBox.IsChecked == true)
            {
                return true;
            }
            return false;
        }
        //private bool ValidateMigrationDate()
        //{
        //    if (PlanToMigrationDateTimePicker.Date < DateTime.Today)
        //    {
        //        return false;
        //    }

        //    return true;
        //}

        //private bool ValidateDatacenterLocation()
        //{
        //    if (string.IsNullOrWhiteSpace(DatacenterLocationTextBox.Text) &&
        //        !System.Text.RegularExpressions.Regex.IsMatch(DatacenterLocationTextBox.Text, @"^[a-zA-Z\s,]+$"))
        //    {
        //        return false;
        //    }

        //    return true;
        //}

        //private bool ValidateWindowsLicenseComboBox()
        //{
        //    string selectedItem = WindowsLicenseComboBox.SelectedItem as string;
        //    if (selectedItem != null && !(selectedItem == "Yes" || selectedItem == "No"))
        //    {
        //        return false;
        //    }

        //    return true;
        //}

        //private bool ValidateSqlLicenseComboBox()
        //{
        //    string selectedItem = SqlLicenseComboBox.SelectedItem as string;
        //    if (selectedItem != null && !(selectedItem == "Yes" || selectedItem == "No"))
        //    {
        //        return false;
        //    }

        //    return true;
        //}
        #endregion

        #region Getter Methods
        public string GetCustomerName()
        {
            return CustomerNameTextBox.Text;
        }

        public string GetCustomerIndustry()
        {
            return CustomerIndustryTextBox.Text;
        }

        public string GetMotivation()
        {
            return MotivationComboBox.SelectedItem.ToString();
        }

        //public DateTime GetPlanToMigrate()
        //{
        //    return PlanToMigrationDateTimePicker.Date.HasValue ? PlanToMigrationDateTimePicker.Date.Value.DateTime : DateTime.MinValue;
        //}

        public string GetDatacenterLocation()
        {
            if (string.IsNullOrWhiteSpace(CustomerNameTextBox.Text))
            {
                return "null";
            }

            return DatacenterLocationTextBox.Text;
        }

        //public string GetActiveSqlLicenses()
        //{
        //    if (SqlLicenseComboBox.SelectedItem == null)
        //    {
        //        return null;
        //    }

        //    return (string)SqlLicenseComboBox.SelectedItem;
        //}

        //public string GetActiveWindowsLicense()
        //{
        //    if (WindowsLicenseComboBox.SelectedItem == null)
        //    {
        //        return null;
        //    }

        //    return (string)(WindowsLicenseComboBox.SelectedItem);
        //}

        public string GetAiOpportunities()
        {
            if (string.IsNullOrWhiteSpace(AIOpportunitiesTextBox.Text))
            {
                return null;
            }
            return AIOpportunitiesTextBox.Text;
        }

        public string GetOtherDetails()
        {
            if (string.IsNullOrWhiteSpace(OtherDetailsRichTextBox.Text))
            {
                return null;
            }

            return OtherDetailsRichTextBox.Text;
        }
        
        public bool GetAIAgreementCheckboxState()
        {
            if (AIAgreementCheckBox.IsChecked == true)
            {
                return true;
            }
            return false;
        }
        #endregion
    }
}

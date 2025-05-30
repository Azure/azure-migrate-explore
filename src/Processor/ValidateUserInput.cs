// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using System.Text.RegularExpressions;
using Azure.Migrate.Explore.Common;
using Azure.Migrate.Explore.Models;

namespace Azure.Migrate.Explore.Processor
{
    public static class ValidateUserInput
    {
        public static bool InitiateValidation(UserInput userInputObj)
        {
            return ValidateProjectDetails(userInputObj) && ValidateConfiguration(userInputObj);
        }

        #region Project details
        private static bool ValidateProjectDetails(UserInput userInputObj)
        {
            return IsTenantIdValid(userInputObj) &&
                   IsSubscriptionValid(userInputObj) &&
                   IsResourceGroupValid(userInputObj) &&
                   IsAzureMigrateProjectValid(userInputObj);
        }

        private static bool IsTenantIdValid(UserInput userInputObj)
        {
            userInputObj.LoggerObj.LogInformation($"Validating Tenant ID: {userInputObj.TenantId}");

            if (string.IsNullOrEmpty(userInputObj.TenantId))
            {
                userInputObj.LoggerObj.LogError("Empty Tenant ID");
                return false;
            }
            if (!Regex.IsMatch(userInputObj.TenantId, @"^(\{){0,1}[0-9a-fA-F]{8}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{12}(\}){0,1}$"))
            {
                userInputObj.LoggerObj.LogError("Invalid regular expression for Tenant ID");
                return false;
            }
            return true;
        }

        private static bool IsSubscriptionValid(UserInput userInputObj)
        {
            userInputObj.LoggerObj.LogInformation($"Validating Subscription: {userInputObj.Subscription.Value}");

            if (string.IsNullOrEmpty(userInputObj.Subscription.Key))
            {
                userInputObj.LoggerObj.LogError("Empty Subscription ID");
                return false;
            }
            if (string.IsNullOrEmpty(userInputObj.Subscription.Value))
            {
                userInputObj.LoggerObj.LogError("Empty Subscription display name");
                return false;
            }
            if (!Regex.IsMatch(userInputObj.TenantId, @"^(\{){0,1}[0-9a-fA-F]{8}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{12}(\}){0,1}$"))
            {
                userInputObj.LoggerObj.LogError("Invalid regular expression for Subscription ID");
                return false;
            }
            return true;
        }

        private static bool IsResourceGroupValid(UserInput userInputObj)
        {
            userInputObj.LoggerObj.LogInformation($"Validating Resource group: {userInputObj.ResourceGroupName.Value}");

            if (string.IsNullOrEmpty(userInputObj.ResourceGroupName.Value))
            {
                userInputObj.LoggerObj.LogError("Empty Resource group name");
                return false;
            }
            if (string.IsNullOrEmpty(userInputObj.ResourceGroupName.Key))
            {
                userInputObj.LoggerObj.LogError("Empty Resource group ID");
                return false;
            }
            return true;
        }

        private static bool IsAzureMigrateProjectValid(UserInput userInputObj)
        {
            userInputObj.LoggerObj.LogInformation($"Validating Azure Migrate project: {userInputObj.AzureMigrateProjectName.Value}");

            if (string.IsNullOrEmpty(userInputObj.AzureMigrateProjectName.Key))
            {
                userInputObj.LoggerObj.LogError("Empty Azure Migrate project ID");
                return false;
            }
            if (string.IsNullOrEmpty(userInputObj.AzureMigrateProjectName.Value))
            {
                userInputObj.LoggerObj.LogError("Empty Azure Migrate project name");
                return false;
            }
            return IsDiscoverySiteNameValid(userInputObj) && IsAssessmentProjectNameValid(userInputObj);
        }

        private static bool IsDiscoverySiteNameValid(UserInput userInputObj)
        {
            userInputObj.LoggerObj.LogInformation($"Validating Discovery Site name: {userInputObj.DiscoverySiteName}");

            if (string.IsNullOrEmpty(userInputObj.DiscoverySiteName))
            {
                userInputObj.LoggerObj.LogError("Empty discovery site name");
                return false;
            }
            return true;
        }

        private static bool IsAssessmentProjectNameValid(UserInput userInputObj)
        {
            userInputObj.LoggerObj.LogInformation($"Validating Assessment Project name: {userInputObj.AssessmentProjectName}");

            if (string.IsNullOrEmpty(userInputObj.AssessmentProjectName))
            {
                userInputObj.LoggerObj.LogError("Empty Assessment Project name");
                return false;
            }
            return true;
        }
        #endregion

        #region Configuration
        private static bool ValidateConfiguration(UserInput userInputObj)
        {
            return IsAzureMigrateSourceApplianceValid(userInputObj) &&
                   IsWorkflowValid(userInputObj) &&
                   IsBusinessProposalValid(userInputObj);
        }

        private static bool IsAzureMigrateSourceApplianceValid(UserInput userInputObj)
        {
            userInputObj.LoggerObj.LogInformation($"Validating selected source appliances: {string.Join(",", userInputObj.AzureMigrateSourceAppliances)}");

            if (userInputObj.AzureMigrateSourceAppliances.Count <= 0)
            {
                userInputObj.LoggerObj.LogError("Empty source appliance list");
                return false;
            }
            return true;
        }

        private static bool IsWorkflowValid(UserInput userInputObj)
        {
            if (userInputObj.WorkflowObj == null)
            {
                userInputObj.LoggerObj.LogError("Workflow validation failed");
                return false;
            }

            string workflow = userInputObj.WorkflowObj.IsExpressWorkflow ? "Express" : "Custom";
            userInputObj.LoggerObj.LogInformation($"Validating Workflow: {workflow}");
            
            if (userInputObj.WorkflowObj.IsExpressWorkflow)
                return ValidateAssessmentSettings(userInputObj);
            
            // Validate Custom workflow
            if (string.IsNullOrEmpty(userInputObj.WorkflowObj.Module))
            {
                userInputObj.LoggerObj.LogError("Empty module");
                return false;
            }

            userInputObj.LoggerObj.LogInformation($"Validating Module: {userInputObj.WorkflowObj.Module}");

            if (userInputObj.WorkflowObj.Module.Equals("Discovery"))
                return true;

            // Validate Assessment settings for Assessment module;
            return ValidateAssessmentSettings(userInputObj);
        }

        private static bool IsBusinessProposalValid(UserInput userInputObj)
        {
            string businessProposal = "";
            if (userInputObj.WorkflowObj.IsExpressWorkflow || userInputObj.WorkflowObj.Module.Equals("Assessment"))
            {
                if (userInputObj.BusinessProposal == BusinessProposal.AVS.ToString())
                    businessProposal = "Quick AVS Proposal";
                else
                    businessProposal = "Comprehensive Proposal for IaaS, PaaS and AVS targets​";

                userInputObj.LoggerObj.LogInformation($"Validating Business Proposal: {businessProposal}");
            }

            return true;
        }
        #endregion

        #region Assessment settings
        private static bool ValidateAssessmentSettings(UserInput userInputObj)
        {
            return IsTargetRegionValid(userInputObj) &&
                   IsCurrencyValid(userInputObj) &&
                   IsAssessmentDurationValid(userInputObj) &&
                   IsOptimizationPreferenceValid(userInputObj);
        }

        private static bool IsTargetRegionValid(UserInput userInputObj)
        {
            userInputObj.LoggerObj.LogInformation($"Validating target region: {userInputObj.TargetRegion.Value}");

            if (string.IsNullOrEmpty(userInputObj.TargetRegion.Key))
            {
                userInputObj.LoggerObj.LogError("Empty target region key");
                return false;
            }
            if (string.IsNullOrEmpty(userInputObj.TargetRegion.Value))
            {
                userInputObj.LoggerObj.LogError("Empty target region value");
                return false;
            }
            return true;
        }

        private static bool IsCurrencyValid(UserInput userInputObj)
        {
            userInputObj.LoggerObj.LogInformation($"Validating currency: {userInputObj.Currency.Value}");

            if (string.IsNullOrEmpty(userInputObj.Currency.Key))
            {
                userInputObj.LoggerObj.LogError("Empty currency key");
                return false;
            }
            if (string.IsNullOrEmpty(userInputObj.Currency.Value))
            {
                userInputObj.LoggerObj.LogError("Empty currency value");
            }
            return true;
        }

        private static bool IsAssessmentDurationValid(UserInput userInputObj)
        {
            userInputObj.LoggerObj.LogInformation($"Validating assessment duration: {userInputObj.AssessmentDuration.Value}");

            if (string.IsNullOrEmpty(userInputObj.AssessmentDuration.Key))
            {
                userInputObj.LoggerObj.LogError("Empty assessment duration key");
                return false;
            }
            if (string.IsNullOrEmpty(userInputObj.AssessmentDuration.Value))
            {
                userInputObj.LoggerObj.LogError("Empty assessment duration value");
                return false;
            }
            return true;
        }

        private static bool IsOptimizationPreferenceValid(UserInput userInputObj)
        {
            if (userInputObj.PreferredOptimizationObj == null)
            {
                userInputObj.LoggerObj.LogError("Failed preferred optimization validation");
                return false;
            }

            userInputObj.LoggerObj.LogInformation($"Validating optimization preference: {userInputObj.PreferredOptimizationObj.OptimizationPreference.Value}");

            if (string.IsNullOrEmpty(userInputObj.PreferredOptimizationObj.OptimizationPreference.Key))
            {
                userInputObj.LoggerObj.LogError("Empty optimization preference key");
                return false;
            }
            if (string.IsNullOrEmpty(userInputObj.PreferredOptimizationObj.OptimizationPreference.Value))
            {
                userInputObj.LoggerObj.LogError("Empty optimization preference value");
                return false;
            }
            return true; ;
        }
        #endregion
    }
}
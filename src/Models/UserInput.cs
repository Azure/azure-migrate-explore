// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using Azure.Migrate.Explore.Common;
using Azure.Migrate.Explore.Logger;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Azure.Migrate.Explore.Models
{
    public class UserInput
    {
        public UserInput(
            string tenantId,
            KeyValuePair<string, string> subscription,
            KeyValuePair<string, string> resourceGroupName,
            KeyValuePair<string, string> azureMigrateProjectName,
            string discoverySiteName,
            string assessmentProjectName,
            List<string> azureMigrateSourceAppliances,
            bool isExpressWorkflow,
            string businessProposal,
            string module,
            KeyValuePair<string, string> targetRegion,
            KeyValuePair<string, string> currency,
            KeyValuePair<string, string> assessmentDuration,
            KeyValuePair<string, string> optimizationPreference,
            bool assessSqlServicesSeparately
        )
        {
            TenantId = tenantId;
            Subscription = subscription;
            ResourceGroupName = resourceGroupName;
            AzureMigrateProjectName = azureMigrateProjectName;
            DiscoverySiteName = discoverySiteName;
            AssessmentProjectName = assessmentProjectName;

            AzureMigrateSourceAppliances = azureMigrateSourceAppliances;
            WorkflowObj = new Workflow(isExpressWorkflow, module);
            BusinessProposal = businessProposal;

            TargetRegion = targetRegion;
            Currency = currency;
            AssessmentDuration = assessmentDuration;
            PreferredOptimizationObj = new PreferredOptimization(optimizationPreference, assessSqlServicesSeparately);

            LoggerObj = new LogHandler();
        }

        // Project details tab
        public string TenantId { get; }
        public KeyValuePair<string, string> Subscription { get; }
        public KeyValuePair<string, string> ResourceGroupName { get; }
        public KeyValuePair<string, string> AzureMigrateProjectName { get; }
        public string DiscoverySiteName { get; }
        public string AssessmentProjectName { get; }

        // Configuration tab
        public List<string> AzureMigrateSourceAppliances { get; }
        public Workflow WorkflowObj { get; }
        public string BusinessProposal { get; }

        // Assessment settings tab
        public KeyValuePair<string, string> TargetRegion { get; }
        public KeyValuePair<string, string> Currency { get; }
        public KeyValuePair<string, string> AssessmentDuration { get; }
        public PreferredOptimization PreferredOptimizationObj { get; }

        // Application Settings
        public AMESettings AppSettings { get; set; }

        // Logger
        public LogHandler LoggerObj { get; }

        // Cancellation token
        public CancellationTokenSource CancellationContext = new CancellationTokenSource();

        public class Workflow
        {
            public Workflow(bool isExpressWorkflow, string module)
            {
                this.IsExpressWorkflow = isExpressWorkflow;
                if (this.IsExpressWorkflow)
                    module = null;
                else
                    this.Module = module;
            }

            public bool IsExpressWorkflow { get; }
            public string Module { get; }
        }

        public class PreferredOptimization
        {
            public PreferredOptimization(KeyValuePair<string, string> optimizationPreference, bool assessSqlServicesSeparately)
            {
                this.OptimizationPreference = optimizationPreference;
                if (this.OptimizationPreference.Value.Equals("Minimize time with Azure VM"))
                    this.AssessSqlServicesSeparately = false;
                else
                    this.AssessSqlServicesSeparately = assessSqlServicesSeparately;
            }

            public KeyValuePair<string, string> OptimizationPreference { get; }
            public bool AssessSqlServicesSeparately { get; }
        }

        // Contains all configurable application settings read from settings file during start
        public class AMESettings
        {
            public bool CallBusinessCaseV2Api = false;
        }
    }
}

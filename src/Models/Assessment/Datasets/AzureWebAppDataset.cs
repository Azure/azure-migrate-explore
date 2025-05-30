// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using System.Collections.Generic;

using Azure.Migrate.Explore.Common;

namespace Azure.Migrate.Explore.Models
{
    public class AzureWebAppDataset
    {
        public string MachineName { get; set; }
        public string DiscoveredMachineId { get; set; }
        public string DiscoveredWebAppId { get; set; }
        public string WebAppName { get; set; }
        public string Environment { get; set; }
        public Suitabilities Suitability { get; set; }
        public List<AssessedMigrationIssue> MigrationIssues { get; set; }
        public string AppServicePlanName { get; set; }
        public string WebAppSkuName { get; set; }
        public string AzureRecommendedTarget { get; set; } = "App Service Native";
        public double EstimatedComputeCost { get; set; }
        public double EstimatedComputeCost_RI3year { get; set; }
        public double EstimatedComputeCost_ASP3year { get; set; }
        public double MonthlySecurityCost { get; set; }
        public string GroupName { get; set; }
    }
}
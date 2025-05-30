// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
namespace Azure.Migrate.Explore.Models
{
    public class WebApp_PaaS
    {
        public string MachineName { get; set; }
        public string WebAppName { get; set; }
        public string Environment { get; set; }
        public string AzureAppServiceReadiness { get; set; }
        public string AzureAppServiceReadiness_Warnings { get; set; }
        public string AppServicePlanName { get; set; }
        public string RecommendedSKU { get; set; }
        public double MonthlyComputeCostEstimate { get; set; }
        public double MonthlyComputeCostEstimate_RI3year { get; set; }
        public double MonthlyComputeCostEstimate_ASP3year { get; set; }
        public double MonthlySecurityCostEstimate { get; set; }
        public string AzureRecommendedTarget { get; set; }
        public string GroupName { get; set; }
        public string MachineId { get; set; }
    }
}
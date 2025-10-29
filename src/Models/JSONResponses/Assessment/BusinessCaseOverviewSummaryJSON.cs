// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using Newtonsoft.Json;

namespace Azure.Migrate.Explore.Models
{
    public class BusinessCaseOverviewSummaryJSON
    {
        [JsonProperty("properties")]
        public BusinessCaseOverviewSummaryProperties Properties { get; set; }
    }

    public class BusinessCaseOverviewSummaryProperties
    {
        [JsonProperty("applicationSummary")]
        public BusinessCaseCostDetailsJSON ApplicationSummary { get; set; }

        [JsonProperty("cotsApplicationSummary")]
        public BusinessCaseCostDetailsJSON CotsApplicationSummary { get; set; }

        [JsonProperty("customApplicationSummary")]
        public BusinessCaseCostDetailsJSON CustomApplicationSummary { get; set; }

        [JsonProperty("independentWorkloadsSummary")]
        public BusinessCaseCostDetailsJSON IndependentWorkloadsSummary { get; set; }

        [JsonProperty("totalAzureCostDetails")]
        public BusinessCaseCostDetailsBreakupJSON TotalAzureCostDetails { get; set; }

        [JsonProperty("totalOnPremisesCostDetails")]
        public BusinessCaseCostDetailsBreakupJSON TotalOnPremisesCostDetails { get; set; }

        [JsonProperty("azureArcEnabledOnPremisesCostDetails")]
        public BusinessCaseCostDetailsBreakupJSON AzureArcEnabledOnPremisesCostDetails { get; set; }

        [JsonProperty("futureAzureArcEnabledOnPremisesCostDetails")]
        public BusinessCaseCostDetailsBreakupJSON FutureAzureArcEnabledOnPremisesCostDetails { get; set; }

        [JsonProperty("futureCostDetails")]
        public BusinessCaseCostDetailsBreakupJSON FutureCostDetails { get; set; }

        [JsonProperty("windowsAhubSavings")]
        public double? WindowsAHUBSavings { get; set; }

        [JsonProperty("linuxAhubSavings")]
        public double? LinuxAHUBSavings { get; set; }

        [JsonProperty("sqlAhubSavings")]
        public double? SqlAHUBSavings { get; set; }

        [JsonProperty("sqlAzureCost")]
        public double? SqlAzureCost { get; set; }

        [JsonProperty("machineAzureCost")]
        public double? MachineAzureCost { get; set; }

        [JsonProperty("azureArcEnabledOnPremisesCost")]
        public double? AzureArcEnabledOnPremisesCost { get; set; }

        [JsonProperty("futureCostIncludingAzureArc")]
        public double? FutureCostIncludingAzureArc { get; set; }

        [JsonProperty("futureEsuSavingsFor4YearsIncludingAzureArc")]
        public double? FutureEsuSavingsFor4YearsIncludingAzureArc { get; set; }

        [JsonProperty("futureManagementCostSavingsIncludingAzureArc")]
        public double? FutureManagementCostSavingsIncludingAzureArc { get; set; }

        [JsonProperty("futureSecurityCostSavingsIncludingAzureArc")]
        public double? FutureSecurityCostSavingsIncludingAzureArc { get; set; }

        [JsonProperty("azureArcServicesCost")]
        public double? AzureArcServicesCost { get; set; }

        [JsonProperty("futureAzureArcEnabledOnPremisesCost")]
        public double? FutureAzureArcEnabledOnPremisesCost { get; set; }

        [JsonProperty("futureAzureArcServicesCost")]
        public double? FutureAzureArcServicesCost { get; set; }

        [JsonProperty("yearOnYearEstimates")]
        public BusinessCaseYOYJSON YearOnYearEstimates { get; set; }

        [JsonProperty("totalAzureSustainabilityDetails")]
        public CarbonEmissionsDetails TotalAzureSustainabilityDetails { get; set; }

        [JsonProperty("totalOnPremisesSustainabilityDetails")]
        public CarbonEmissionsDetails TotalOnPremisesSustainabilityDetails { get; set; }
    }
}
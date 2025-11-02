// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
namespace Azure.Migrate.Explore.Models
{
    public class Business_Case
    {
        public BusinessCaseDatasetCostDetails ApplicationSummaryCostDetails { get; set; }
        public BusinessCaseDatasetCostDetails CotsApplicationSummaryCostDetails { get; set; }
        public BusinessCaseDatasetCostDetails CustomApplicationSummaryCostDetails { get; set; }
        public BusinessCaseDatasetCostDetails IndependentWorkloadsSummaryCostDetails { get; set; }
        public BusinessCaseDatasetCostDetailsBreakup TotalAzureCostDetails { get; set; }
        public BusinessCaseDatasetCostDetailsBreakup TotalOnPremisesCostDetails { get; set; }
        public BusinessCaseDatasetCostDetailsBreakup azureArcEnabledOnPremisesCostDetails { get; set; }
        public BusinessCaseDatasetCostDetailsBreakup futureAzureArcEnabledOnPremisesCostDetails { get; set; }
        public BusinessCaseDatasetCostDetailsBreakup futureCostDetails { get; set; }
        public double WindowsAhubSavings { get; set; }
        public double LinuxAhubSavings { get; set; }
        public double SqlAhubSavings { get; set; }
        public double SqlAzureCost { get; set; }
        public double MachineAzureCost { get; set; }
        public double AzureArcEnabledOnPremisesCost { get; set; }
        public double FutureCostIncludingAzureArc { get; set; }
        public double FutureEsuSavingsFor4YearsIncludingAzureArc { get; set; }
        public double FutureManagementCostSavingsIncludingAzureArc { get; set; }
        public double FutureSecurityCostSavingsIncludingAzureArc { get; set; }
        public double AzureArcServicesCost { get; set; }
        public double FutureAzureArcEnabledOnPremisesCost { get; set; }
        public double FutureAzureArcServicesCost { get; set; }

        public Business_Case()
        {
            ApplicationSummaryCostDetails = new BusinessCaseDatasetCostDetails();
            CotsApplicationSummaryCostDetails = new BusinessCaseDatasetCostDetails();
            CustomApplicationSummaryCostDetails = new BusinessCaseDatasetCostDetails();
            IndependentWorkloadsSummaryCostDetails = new BusinessCaseDatasetCostDetails();
            TotalAzureCostDetails = new BusinessCaseDatasetCostDetailsBreakup();
            TotalOnPremisesCostDetails = new BusinessCaseDatasetCostDetailsBreakup();
            azureArcEnabledOnPremisesCostDetails = new BusinessCaseDatasetCostDetailsBreakup();
            futureAzureArcEnabledOnPremisesCostDetails = new BusinessCaseDatasetCostDetailsBreakup();
            futureCostDetails = new BusinessCaseDatasetCostDetailsBreakup();
        }
    }
}
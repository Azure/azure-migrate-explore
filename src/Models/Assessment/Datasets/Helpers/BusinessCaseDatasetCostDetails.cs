// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
namespace Azure.Migrate.Explore.Models
{
    public class BusinessCaseDatasetCostDetails
    {
        public BusinessCaseDatasetCostDetailsBreakup AzureCost { get; set; }
        public BusinessCaseDatasetCostDetailsBreakup OnPremCost { get; set; }
    }
    
    public class BusinessCaseDatasetCostDetailsBreakup
    {
        public double TotalCost { get; set; }
        public double ComputeCost { get; set; }
        public double LicenseCost { get; set; }
        public double StorageCost { get; set; }
        public double NetworkCost { get; set; }
        public double SecurityCost { get; set; }
        public double ITLaborCost { get; set; }
        public double FacilitiesCost { get; set; }
        public double ManagementCost { get; set; }
        public double AhubSavings { get; set; }
        public double EsuSavings { get; set; }
        public double LinuxAhubSavings { get; set; }
    }
}
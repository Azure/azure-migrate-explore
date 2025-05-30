// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
namespace Azure.Migrate.Explore.Models
{
    public class BusinessCaseDatasetCostDetails
    {
        public double ComputeLicenseCost { get; set; }
        public double EsuLicenseCost { get; set; }
        public double StorageCost { get; set; }
        public double NetworkCost { get; set; }
        public double SecurityCost { get; set; }
        public double ITStaffCost { get; set; }
        public double FacilitiesCost { get; set; }
    }
}
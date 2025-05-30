// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
namespace Azure.Migrate.Explore.Models
{
    public class Financial_Summary
    {
        public string MigrationStrategy { get; set; }
        public string Workload { get; set; }
        public int SourceCount { get; set; }
        public int TargetCount { get; set; }
        public double StorageCost { get; set; }
        public double ComputeCost { get; set; }
        public double TotalAnnualCost { get; set; }
    }
}

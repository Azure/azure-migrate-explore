// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
namespace Azure.Migrate.Explore.Models
{
    public class CoreProperties
    {
        public string TenantId { get; set; }
        public string Subscription { get; set; }
        public string ResourceGroupName { get; set; }
        public string AzureMigrateProjectName { get; set; }
        public string AssessmentSiteName { get; set; }
        public string Workflow { get; set; }
        public string BusinessProposal { get; set; }
        public string TargetRegion { get; set; }
        public string Currency { get; set; }
        public string AssessmentDuration { get; set; }
        public string OptimizationPreference { get; set; }
        public string AssessSQLServices { get; set; }
        public string VCpuOverSubscription { get; set; }
        public string MemoryOverCommit { get; set; }
        public double DedupeCompression { get; set; }
    }
}
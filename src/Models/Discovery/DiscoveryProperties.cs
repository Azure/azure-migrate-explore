// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
namespace Azure.Migrate.Explore.Models
{
    public class DiscoveryProperties
    {
        public string TenantId { get; set; }
        public string Subscription { get; set; }
        public string ResourceGroup { get; set; }
        public string AzureMigrateProjectName { get; set; }
        public string DiscoverySiteName { get; set; }
        public string Workflow { get; set; }
        public string SourceAppliances { get; set; }
    }
}
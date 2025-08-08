using System.Security.Policy;

namespace AzureMigrateExplore.Models
{
    public class InventoryInsights
    {
        public string WorkloadName { get; set; }
        public string Category { get; set; }
        public string Type { get; set; }
        public bool HasPatchManagementSoftware { get; set; }
        public bool MissingSecuritySoftware { get; set; }
        public bool HasPendingUpdates { get; set; }
        public string SupportStatus { get; set; }
        public bool HasVulnerabilities { get; set; }
    }
}

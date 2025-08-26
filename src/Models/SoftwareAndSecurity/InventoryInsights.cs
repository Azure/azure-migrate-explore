using System.Security.Policy;

namespace AzureMigrateExplore.Models
{
    public class InventoryInsights
    {
        public string WorkloadName { get; set; } = string.Empty;
        public string OperatingSystem { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string SupportStatus { get; set; } = string.Empty;
        public int VulnerabilityCount { get; set; }
        public int CriticalVulnerabilityCount { get; set; }
        public int PendingUpdateCount { get; set; }
        public int EndOfSupportSoftwareCount { get; set; }
        public bool HasSecuritySoftware { get; set; }
        public bool HasPatchingSoftware { get; set; }
    }
}

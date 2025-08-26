using System.Collections.Generic;

namespace AzureMigrateExplore.Models
{
    public class SoftwareInsights
    {
        public string? Name { get; set; }
        public string? Category { get; set; }
        public string? SubCategory { get; set; }
        public string? SupportStatus { get; set; }
        public string? Version { get; set; }
        public int ServersCount { get; set; }
        public int Vulnerabilities { get; set; }
        public string? Recommendations { get; set; }  // Changed from List<string> to string
        public List<string>? MachineIds { get; set; }
    }
}

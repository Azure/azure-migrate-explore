using System.Collections.Generic;

namespace AzureMigrateExplore.Models
{
    public class SoftwareInsights
    {
        public string Name { get; set; }
        public string Category { get; set; }
        public string SubCategory { get; set; }
        public string SupportStatus { get; set; }
        public string Version { get; set; }
        public int ServersCount { get; set; }
        public int Vulnerabilities { get; set; }
        public List<string> Recommendations { get; set; }
    }
}

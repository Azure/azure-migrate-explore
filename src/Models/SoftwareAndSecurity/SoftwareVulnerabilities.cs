namespace AzureMigrateExplore.Models
{
    public class SoftwareVulnerabilities
    {
        public string SoftwareName { get; set; }
        public string Version { get; set; }
        public string Vulnerability { get; set; }
        public string CveId { get; set; }
        public string Severity { get; set; }
        public int ServerCount { get; set; }
    }
}

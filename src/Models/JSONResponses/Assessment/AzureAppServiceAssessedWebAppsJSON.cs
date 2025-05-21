using Newtonsoft.Json;
using System.Collections.Generic;

using Azure.Migrate.Export.Common;

namespace Azure.Migrate.Export.Models
{
    public class AzureAppServiceAssessedWebAppsJSON
    {
        [JsonProperty("value")]
        public List<AzureAppServiceAssessedWebAppValue> Values { get; set; }

        [JsonProperty("nextLink")]
        public string NextLink { get; set; }
    }

    public class AzureAppServiceAssessedWebAppValue
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("properties")]
        public AzureAppServiceAssessedWebAppProperty Properties { get; set; }
    }

    public class AzureAppServiceAssessedWebAppProperty
    {
        [JsonProperty("machineName")]
        public string MachineName { get; set; }

        [JsonProperty("serverArmId")]
        public string ServerArmId { get; set; }

        [JsonProperty("webServerName")]
        public string WebServerName { get; set; }

        [JsonProperty("webAppName")]
        public string WebAppName { get; set; }

        [JsonProperty("discoveredWebAppId")]
        public string DiscoveredWebAppId { get; set; }

        [JsonProperty("discoveredMachineId")]
        public string DiscoveredMachineId { get; set; }

        [JsonProperty("appServicePlanName")]
        public string AppServicePlanName { get; set; }

        [JsonProperty("createdTimestamp")]
        public string CreatedTimestamp { get; set; }

        [JsonProperty("updatedTimestamp")]
        public string UpdatedTimestamp { get; set; }

        [JsonProperty("webAppType")]
        public string WebAppType { get; set; }

        [JsonProperty("targetSpecificResult")]
        public Dictionary<string, TargetSpecificResult> TargetSpecificResult { get; set; }

        [JsonProperty("confidenceRatingInPercentage")]
        public float? ConfidenceRatingInPercentage { get; set; }
    }

    public class AssessmentResult
    {
        [JsonProperty("appServicePlanName")]
        public string AppServicePlanName { get; set; }

        [JsonProperty("suitability")]
        public Suitabilities Suitability { get; set; }

        [JsonProperty("securitySuitability")]
        public Suitabilities SecuritySuitability { get; set; }

        [JsonProperty("webAppSkuName")]
        public string WebAppSkuName { get; set; }

        [JsonProperty("webAppSkuSize")]
        public string WebAppSkuSize { get; set; }
    }

    public class AzureAppServiceAssessedWebAppMigrationIssueInfo
    {
        [JsonProperty("issueId")]
        public string IssueId { get; set; }

        [JsonProperty("issueCategory")]
        public IssueCategories IssueCategory { get; set; }

        [JsonProperty("issueDescriptionList")]
        public List<string> IssueDescriptionList { get; set; }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class TargetSpecificResult
    {
        [JsonProperty("assessmentResult")]
        public AssessmentResult AssessmentResult { get; set; }

        [JsonProperty("migrationIssues")]
        public List<AzureAppServiceAssessedWebAppMigrationIssueInfo> MigrationIssues { get; set; }
    }
}
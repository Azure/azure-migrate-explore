using Newtonsoft.Json;

namespace Azure.Migrate.Export.Models
{
    public class BusinessCaseOverviewSummaryJSON
    {
        [JsonProperty("properties")]
        public BusinessCaseOverviewSummaryProperties Properties { get; set; }        
    }

    public class BusinessCaseOverviewSummaryProperties
    {
        [JsonProperty("totalAzureAvsCost")]
        public double TotalAzureAvsCost { get; set; }

        [JsonProperty("windowsAhubSavings")]
        public double WindowsAhubSavings { get; set; }

        [JsonProperty("sqlAhubSavings")]
        public double SqlAhubSavings { get; set; }

        [JsonProperty("esuSavingsFor4years")]
        public double EsuSavingsFor4years { get; set; }

        [JsonProperty("yearOnYearEstimates")]
        public BusinessCaseYOYJSON YearOnYearEstimates { get; set; }

        [JsonProperty("totalAzureSustainabilityDetails")]
        public CarbonEmissionsDetails TotalAzureSustainabilityDetails { get; set; }

        [JsonProperty("totalOnPremisesSustainabilityDetails")]
        public CarbonEmissionsDetails TotalOnPremisesSustainabilityDetails { get; set; }
    }
}
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using Newtonsoft.Json;

namespace Azure.Migrate.Explore.Models
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
        public BusinessCaseYOYCostDetailsJSON YearOnYearEstimates { get; set; }
    }
}
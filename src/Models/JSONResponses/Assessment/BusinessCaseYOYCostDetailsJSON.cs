// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using Newtonsoft.Json;

namespace Azure.Migrate.Explore.Models
{
    public class BusinessCaseYOYCostDetailsJSON
    {
        public BusinessCaseYOYCostDetailsJSON()
        {
            OnPremisesCostYOY = new BusinessCaseYOYCostBreakdown();
            AzureCostYOY = new BusinessCaseYOYCostBreakdown();
            SavingsYOY = new BusinessCaseYOYCostBreakdown();
        }

        [JsonProperty("onPremisesCost")]
        public BusinessCaseYOYCostBreakdown OnPremisesCostYOY { get; set; }

        [JsonProperty("azureCost")]
        public BusinessCaseYOYCostBreakdown AzureCostYOY { get; set; }

        [JsonProperty("savings")]
        public BusinessCaseYOYCostBreakdown SavingsYOY { get; set; }
    }

    public class BusinessCaseYOYCostBreakdown
    {
        [JsonProperty("Year0")]
        public double Year0 { get; set;  }

        [JsonProperty("Year1")]
        public double Year1 { get; set; }

        [JsonProperty("Year2")]
        public double Year2 { get; set; }

        [JsonProperty("Year3")]
        public double Year3 { get; set; }
    }
}
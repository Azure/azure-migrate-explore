// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Azure.Migrate.Explore.Models
{
    public class BusinessCaseCostDetailsJSON
    {
        [JsonProperty("azureCostDetails")]
        public BusinessCaseCostDetailsBreakupJSON AzureCost { get; set; }

        [JsonProperty("onPremCostDetails")]
        public BusinessCaseCostDetailsBreakupJSON OnPremCost { get; set; }
    }

    public class BusinessCaseCostDetailsBreakupJSON
    {
        [JsonProperty("totalCost")]
        public double? TotalCost { get; set; }

        [JsonProperty("storageCost")]
        public double? StorageCost { get; set; }

        [JsonProperty("computeCost")]
        public double? ComputeCost { get; set; }

        [JsonProperty("itLaborCost")]
        public double? ITLaborCost { get; set; }

        [JsonProperty("networkCost")]
        public double? NetworkCost { get; set; }

        [JsonProperty("ahubSavings")]
        public double? AHUBSavings { get; set; }

        [JsonProperty("esuSavings")]
        public double? ESUSavings { get; set; }

        [JsonProperty("securityCost")]
        public double? SecurityCost { get; set; }

        [JsonProperty("facilitiesCost")]
        public double? FacilitiesCost { get; set; }

        [JsonProperty("managementCostDetails")]
        public ManagementCostDetails? ManagementCostDetails { get; set; }

        [JsonProperty("licenseCostDetails")]
        public LicenseCostDetails? LicenseCostDetails { get; set; }

        [JsonProperty("linuxAhubSavings")]
        public double? LinuxAHUBSavings { get; set; }
    }

    public class ManagementCostDetails
    {
        [JsonProperty("managementCost")]
        public double? ManagementCost { get; set; }
    }

    public class LicenseCostDetails
    {
        [JsonProperty("licenseCost")]
        public double? LicenseCost { get; set; }
    }
}

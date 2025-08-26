// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using Newtonsoft.Json;

namespace Azure.Migrate.Explore.Models
{
    public class BusinessCaseCompareSummaryJSON
    {
        [JsonProperty("azurePaasCostDetails")]
        public BusinessCaseCostDetailsJSON AzurePaaSCostDetails { get; set; } = null;

        [JsonProperty("azureIaasCostDetails")]
        public BusinessCaseCostDetailsJSON AzureIaaSCostDetails { get; set; } = null;

        [JsonProperty("azureAvsCostDetails")]
        public BusinessCaseCostDetailsJSON AzureAvsCostDetails { get; set; } = null;

        [JsonProperty("onPremisesPaasCostDetails")]
        public BusinessCaseCostDetailsJSON OnPremisesPaaSCostDetails { get; set; } = null;

        [JsonProperty("onPremisesIaasCostDetails")]
        public BusinessCaseCostDetailsJSON OnPremisesIaaSCostDetails { get; set; } = null;

        [JsonProperty("onPremisesAvsCostDetails")]
        public BusinessCaseCostDetailsJSON OnPremisesAvsCostDetails { get; set; } = null;

        [JsonProperty("azureArcEnabledOnPremisesCostDetails")]
        public BusinessCaseCostDetailsJSON AzureArcEnabledOnPremisesCostDetails { get; set; } = null;
    }
}
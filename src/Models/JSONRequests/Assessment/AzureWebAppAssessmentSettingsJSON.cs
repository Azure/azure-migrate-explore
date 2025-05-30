// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using Newtonsoft.Json;

namespace Azure.Migrate.Explore.Models
{
    public class AzureWebAppAssessmentSettingsJSON
    {
        [JsonProperty("properties")]
        public AzureWebAppAssessmentProperty Properties { get; set; } = new AzureWebAppAssessmentProperty();
    }

    public class AzureWebAppAssessmentProperty
    {
        [JsonProperty("azureLocation")]
        public string AzureLocation { get; set; }

        [JsonProperty("reservedInstance")]
        public string ReservedInstance { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("azureSecurityOfferingType")]
        public string AzureSecurityOfferingType = "MDC";

        [JsonProperty("azureOfferCode")]
        public string AzureOfferCode { get; set; }

        [JsonProperty("scalingFactor")]
        public int ScalingFactor { get; set; } = 1;

        [JsonProperty("discountPercentage")]
        public int DiscountPercentage { get; set; } = 0;
    }
}
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Azure.Migrate.Explore.Models
{
    public class AzureVMAssessmentSettingsJSON
    {
        [JsonProperty("properties")]
        public AzureVMAssessmentSettingsProperty Properties { get; set; } = new AzureVMAssessmentSettingsProperty();
    }

    public class AzureVMAssessmentSettingsProperty
    {
        [JsonProperty("sizingCriterion")]
        public string SizingCriterion { get; set; }

        [JsonProperty("azureHybridUseBenefit")]
        public string AzureHybridUseBenefit { get; set; }

        [JsonProperty("reservedInstance")]
        public string ReservedInstance { get; set; }

        [JsonProperty("azureDiskType")]
        public string AzureDiskType { get; set; } = "StandardOrPremium";

        [JsonProperty("scalingFactor")]
        public int ScalingFactor { get; set; } = 1;

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("azureOfferCode")]
        public string AzureOfferCode { get; set; }

        [JsonProperty("azureVmFamilies")]
        public List<string> AzureVMFamilies { get; set; } = new List<string>();

        [JsonProperty("azureSecurityOfferingType")]
        public string AzureSecurityOfferingType = "MDC";

        [JsonProperty("discountPercentage")]
        public int DiscountPercentage { get; set; } = 0;

        [JsonProperty("vmUptime")]
        public AzureVMUptime VMUptime { get; set; } = new AzureVMUptime();

        [JsonProperty("azureLocation")]
        public string AzureLocation { get; set; }

        [JsonProperty("azurePricingTier")]
        public string AzurePricingTier { get; set; } = "Standard";

        [JsonProperty("azureStorageRedundancy")]
        public string AzureStorageRedundancy { get; set; } = "LocallyRedundant";

        [JsonProperty("percentile")]
        public string Percentile { get; set; } = "Percentile95";

        [JsonProperty("timeRange")]
        public string TimeRange { get; set; }
    }

    public class AzureVMUptime
    {
        [JsonProperty("daysPerMonth")]
        public int DaysPerMonth { get; set; } = 31;

        [JsonProperty("hoursPerDay")]
        public int HoursPerDay { get; set; } = 24;
    }
}
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using System.Collections.Generic;
using Newtonsoft.Json;

using Azure.Migrate.Explore.Common;

namespace Azure.Migrate.Explore.Models
{
    public class AzureVMWareSolutionAssessmentSettingsJSON
    {
        [JsonProperty("properties")]
        public AzureVMWareSolutionAssessmentSettingsProperty Properties { get; set; } = new AzureVMWareSolutionAssessmentSettingsProperty();
    }

    public class AzureVMWareSolutionAssessmentSettingsProperty
    {
        [JsonProperty("sizingCriterion")]
        public string SizingCriterion { get; set; } = "PerformanceBased";

        [JsonProperty("azureHybridUseBenefit")]
        public string AzureHybridUseBenefit { get; set; } = "No";

        [JsonProperty("reservedInstance")]
        public string ReservedInstance { get; set; }

        [JsonProperty("nodeTypes")]
        public List<string> NodeTypes { get; set; }

        [JsonProperty("failuresToTolerateAndRaidLevelList")]
        public List<string> FailuresToTolerateAndRaidLevelList { get; set; } = new List<string> { "Ftt1Raid1", "Ftt2Raid6" };

        [JsonProperty("vcpuOversubscription")]
        public string VcpuOversubscription { get; set; } = AvsAssessmentConstants.VCpuOversubscription;

        [JsonProperty("memOvercommit")]
        public string MemOverCommit { get; set; } = "1";

        [JsonProperty("dedupeCompression")]
        public string DedupeCompression { get; set; } = AvsAssessmentConstants.DedupeCompression.ToString();

        [JsonProperty("isStretchClusterEnabled")]
        public string IsStretchClusterEnabled { get; set; } = "No";

        [JsonProperty("percentile")]
        public string Percentile { get; set; } = "Percentile95";

        [JsonProperty("scalingFactor")]
        public int ScalingFactor { get; set; } = 1;

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("azureOfferCode")]
        public string AzureOfferCode { get; set; } = "MSAZR0003P";

        [JsonProperty("discountPercentage")]
        public int DiscountPercentage { get; set; } = 0;

        [JsonProperty("azureLocation")]
        public string AzureLocation { get; set; }

        [JsonProperty("externalStorageTypes")]
        public List<string> ExternalStorageTypes { get; set; }
    }
}
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
        public AzureVMWareSolutionAssessmentProperty Properties { get; set; } = new AzureVMWareSolutionAssessmentProperty();
    }

    public class AzureVMWareSolutionAssessmentProperty
    {
        [JsonProperty("settings")]
        public AzureVMWareSolutionAssessmentSettingsProperty Settings { get; set; } = new AzureVMWareSolutionAssessmentSettingsProperty();

        [JsonProperty("scope")]
        public ScopeDetails Scope { get; set; } = new ScopeDetails();
    }

    public class ScopeDetails
    {
        [JsonProperty("scopeType")]
        public string ScopeType { get; set; } = "Datacenter";

        [JsonProperty("azureResourceGraphQuery")]
        public string AzureResourceGraphQuery { get; set; } = "";
    }

    public class AzureVMWareSolutionAssessmentSettingsProperty
    {
        [JsonProperty("sizingCriterion")]
        public string SizingCriterion { get; set; } = "PerformanceBased";

        [JsonProperty("nodeTypes")]
        public List<string> NodeTypes { get; set; }

        [JsonProperty("failuresToTolerateAndRaidLevelList")]
        public List<string> FailuresToTolerateAndRaidLevelList { get; set; } = new List<string> { "Ftt1Raid1", "Ftt2Raid6" };

        [JsonProperty("vcpuOversubscription")]
        public int VcpuOversubscription { get; set; } = 4;

        [JsonProperty("memOvercommit")]
        public int MemOverCommit { get; set; } = 1;

        [JsonProperty("dedupeCompression")]
        public double DedupeCompression { get; set; } = AvsAssessmentConstants.DedupeCompression;

        [JsonProperty("isStretchClusterEnabled")]
        public bool IsStretchClusterEnabled { get; set; } = false;

        [JsonProperty("isVcfByolEnabled")]
        public bool IsVcfByolEnabled { get; set; } = true;

        [JsonProperty("performanceData")]
        public PerfData PerformanceData { get; set; } = new PerfData();

        [JsonProperty("scalingFactor")]
        public int ScalingFactor { get; set; } = 1;

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("savingsSettings")]
        public SavingsSettings SavingsSettings { get; set; } = new SavingsSettings();

        [JsonProperty("discountPercentage")]
        public int DiscountPercentage { get; set; } = 0;

        [JsonProperty("azureLocation")]
        public string AzureLocation { get; set; }

        [JsonProperty("externalStorageTypes")]
        public List<string> ExternalStorageTypes { get; set; }

        [JsonProperty("billingSettings")]
        public BillingSettings BillingSettings { get; set; } = new BillingSettings();
    }

    public class BillingSettings
    {
        [JsonProperty("licensingProgram")]
        public string LicensingProgram { get; set; } = "retail";

        [JsonProperty("subscriptionId")]
        public string SubscriptionId { get; set; } 
    }

    public class SavingsSettings
    {
        [JsonProperty("savingsOptions")]
        public string SavingsOption { get; set; } = "ri3year";

        [JsonProperty("azureOfferCode")]
        public string AzureOfferCode { get; set; } = "MSAZR0003P";
    }

    public class PerfData
    {
        [JsonProperty("percentile")]
        public string Percentile { get; set; } = "Percentile95";

        [JsonProperty("timeRange")]
        public string TimeRange { get; set; } = "Week";
    }
}
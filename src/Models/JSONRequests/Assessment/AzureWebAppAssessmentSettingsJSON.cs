// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using System.Text.Json.Serialization;
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
        [JsonProperty("settings")]
        public AzureWebAppAssessmentSettingsProperty Settings { get; set; } = new AzureWebAppAssessmentSettingsProperty();

        [JsonProperty("scope")]
        public ScopeDetails Scope { get; set; } = new ScopeDetails();
    }


    public class AzureWebAppAssessmentSettingsProperty
    {
        [JsonProperty("azureLocation")]
        public string AzureLocation { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("azureSecurityOfferingType")]
        public string AzureSecurityOfferingType = "MDC";

        [JsonProperty("scalingFactor")]
        public int ScalingFactor { get; set; } = 1;

        [JsonProperty("discountPercentage")]
        public int DiscountPercentage { get; set; } = 0;

        [JsonProperty("sizingCriterion")]
        public string SizingCriterion { get; set; } = "PerformanceBased";

        [JsonProperty("environmentType")]
        public string EnvironmentType { get; set; } = "Production";

        [JsonProperty("appSvcContainerSettings")]
        public AppSvcContainerSettings AppSvcContainerSettings { get; set; } = new AppSvcContainerSettings();

        [JsonProperty("appSvcNativeSettings")]
        public AppSvcNativeSettings AppSvcNativeSettings { get; set; } = new AppSvcNativeSettings();

        [JsonProperty("billingSettings")]
        public BillingSettings BillingSettings { get; set; } = new BillingSettings();

        [JsonProperty("savingsSettings")]
        public SavingsSettings SavingsSettings { get; set; } = new SavingsSettings();

        [JsonProperty("performanceData")]
        public PerfData PerformanceData { get; set; } = new PerfData();
    }

    public class AppSvcContainerSettings
    {
        [JsonProperty("isolationRequired")]
        public bool IsolationRequired { get; set; } = false;
    }

    public class AppSvcNativeSettings
    {
        [JsonProperty("isolationRequired")]
        public bool IsolationRequired { get; set; } = false;
    }
}
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Azure.Migrate.Explore.Models
{
    public class AzureVMAssessmentSettingsJSON
    {
        [JsonProperty("properties")]
        public AzureVMAssessmentProperty Properties { get; set; } = new AzureVMAssessmentProperty();
    }

    public class AzureVMAssessmentProperty
    {
        [JsonProperty("settings")]
        public AzureVMAssessmentSettingsProperty Settings { get; set; } = new AzureVMAssessmentSettingsProperty();

        [JsonProperty("scope")]
        public ScopeDetails Scope { get; set; } = new ScopeDetails();
    }

    public class AzureVMAssessmentSettingsProperty
    {
        [JsonProperty("sizingCriterion")]
        public string SizingCriterion { get; set; } = "PerformanceBased";

        [JsonProperty("environmentType")]
        public string EnvironmentType { get; set; } = "Production";

        [JsonProperty("azureHybridUseBenefit")]
        public string AzureHybridUseBenefit { get; set; } = "No";

        [JsonProperty("azureDiskTypes")]
        public List<string> AzureDiskTypes { get; set; } = new List<string> { "StandardOrPremium" };

        [JsonProperty("scalingFactor")]
        public int ScalingFactor { get; set; } = 1;

        [JsonProperty("currency")]
        public string Currency { get; set; }

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

        [JsonProperty("billingSettings")]
        public BillingSettings BillingSettings { get; set; } = new BillingSettings();

        [JsonProperty("savingsSettings")]
        public SavingsSettings SavingsSettings { get; set; } = new SavingsSettings();

        [JsonProperty("performanceData")]
        public PerfData PerformanceData { get; set; } = new PerfData();
    }

    public class AzureVMUptime
    {
        [JsonProperty("daysPerMonth")]
        public int DaysPerMonth { get; set; } = 31;

        [JsonProperty("hoursPerDay")]
        public int HoursPerDay { get; set; } = 24;
    }
}
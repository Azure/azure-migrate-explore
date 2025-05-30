// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Azure.Migrate.Explore.Models
{
    public class AzureSQLAssessmentSettingsJSON
    {
        [JsonProperty("properties")]
        public AzureSQLAssessmentProperty Properties { get; set; } = new AzureSQLAssessmentProperty();
    }

    public class AzureSQLAssessmentProperty
    {
        [JsonProperty("azureLocation")]
        public string AzureLocation { get; set; }

        [JsonProperty("environmentType")]
        public string EnvrionmentType { get; set; }

        [JsonProperty("sizingCriterion")]
        public string SizingCriterion { get; set; } = "PerformanceBased";

        [JsonProperty("reservedInstance")]
        public string ReservedInstance { get; set; }

        [JsonProperty("reservedInstanceForVm")]
        public string ReservedInstanceForVM { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("azureSecurityOfferingType")]
        public string AzureSecurityOfferingType = "MDC";

        [JsonProperty("azureOfferCode")]
        public string AzureOfferCode { get; set; } = "MSAZR0003P";

        [JsonProperty("azureOfferCodeForVm")]
        public string AzureOfferCodeForVM { get; set; } = "MSAZR0003P";

        [JsonProperty("percentile")]
        public string Percentile { get; set; } = "Percentile95";

        [JsonProperty("timeRange")]
        public string TimeRange { get; set; }

        [JsonProperty("scalingFactor")]
        public int ScalingFactor { get; set; } = 1;

        [JsonProperty("discountPercentage")]
        public int DiscountPercentage { get; set; } = 0;

        [JsonProperty("optimizationLogic")]
        public string OptimizationLogic { get; set; } = "ModernizeToAzureSqlMi";

        [JsonProperty("osLicense")]
        public string OSLicense { get; set; }

        [JsonProperty("sqlServerLicense")]
        public string SQLServerLicense { get; set; }

        [JsonProperty("azureSqlDatabaseSettings")]
        public AzureSQLDatabaseSettingsInfo AzureSQLDatabaseSettings { get; set; } = new AzureSQLDatabaseSettingsInfo();

        [JsonProperty("azureSqlManagedInstanceSettings")]
        public AzureSQLManagedInstanceSettingsInfo AzureSQLManagedInstanceSettings { get; set; } = new AzureSQLManagedInstanceSettingsInfo();

        [JsonProperty("azureSqlVmSettings")]
        public AzureSQLVMSettingsInfo AzureSQLVMSettings { get; set; } = new AzureSQLVMSettingsInfo();

        [JsonProperty("entityUptime")]
        public AzureSQLEntityUptimeInfo EntityUpTime { get; set; } = new AzureSQLEntityUptimeInfo();
    }

    public class AzureSQLDatabaseSettingsInfo
    {
        [JsonProperty("azureSqlServiceTier")]
        public string AzureSQLServiceTier { get; set; } = "Automatic";

        [JsonProperty("azureSqlDataBaseType")]
        public string AzureSQLDatabaseType { get; set; } = "SingleDatabase";

        [JsonProperty("azureSqlComputeTier")]
        public string AzureSQLComputeTier { get; set; } = "Provisioned";

        [JsonProperty("azureSqlPurchaseModel")]
        public string AzureSQLPurchaseModel { get; set; } = "VCore";
    }

    public class AzureSQLManagedInstanceSettingsInfo
    {
        [JsonProperty("azureSqlServiceTier")]
        public string AzureSQLServiceTier { get; set; } = "Automatic";

        [JsonProperty("azureSqlInstanceType")]
        public string AzureSQInstanceType { get; set; } = "SingleInstance";
    }

    public class AzureSQLVMSettingsInfo
    {
        [JsonProperty("azurePricingTier")]
        public string AzurePricingTier { get; set; } = "Standard";

        [JsonProperty("instanceSeries")]
        public List<string> InstanceSeries { get; set; } = new List<string> { "Dadsv5_series", "Dasv4_series", "Ddsv4_series", "Ddsv5_series", "Edsv5_series", "Eadsv5_series", "Easv4_series", "Edsv4_series", "M_series", "Mdsv2_series" };
    }

    public class AzureSQLEntityUptimeInfo
    {
        [JsonProperty("daysPerMonth")]
        public int DaysPerMonth { get; set; } = 31;

        [JsonProperty("hoursPerDay")]
        public int HoursPerDay { get; set; } = 24;
    }
}
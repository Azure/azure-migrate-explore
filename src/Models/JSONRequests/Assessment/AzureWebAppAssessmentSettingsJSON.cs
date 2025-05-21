using Newtonsoft.Json;

namespace Azure.Migrate.Export.Models
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

        [JsonProperty("appSvcContainerSettings")]
        public AppSvcContainerSettings AppSvcContainerSettings { get; set; } = new AppSvcContainerSettings();

        [JsonProperty("appSvcNativeSettings")]
        public AppSvcNativeSettings AppSvcNativeSettings { get; set; } = new AppSvcNativeSettings();
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
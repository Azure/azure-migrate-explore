using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Azure.Migrate.Export.Models
{
    public class BusinessCaseYOYCostDetailsJSON
    {
        public BusinessCaseYOYCostDetailsJSON()
        {
            OnPremisesCostYOY = new BusinessCaseYOYCostBreakdown();
            AzureCostYOY = new BusinessCaseYOYCostBreakdown();
            SavingsYOY = new BusinessCaseYOYCostBreakdown();
        }

        [JsonProperty("onPremisesCost")]
        public BusinessCaseYOYCostBreakdown OnPremisesCostYOY { get; set; }

        [JsonProperty("azureCost")]
        public BusinessCaseYOYCostBreakdown AzureCostYOY { get; set; }

        [JsonProperty("savings")]
        public BusinessCaseYOYCostBreakdown SavingsYOY { get; set; }
    }

    public class BusinessCaseYOYJSON
    {
        public BusinessCaseYOYJSON()
        {
            OnPremisesCostYOY = new BusinessCaseYOYCostBreakdown();
            AzureCostYOY = new BusinessCaseYOYCostBreakdown();
            SavingsYOY = new BusinessCaseYOYCostBreakdown();
            AzureEmissionsEstimates = new List<YearOnYearEmission>();

            OnPremisesEmissionsEstimates = new List<YearOnYearEmission>();
        }

        [JsonProperty("onPremisesCost")]
        public BusinessCaseYOYCostBreakdown OnPremisesCostYOY { get; set; }

        [JsonProperty("azureCost")]
        public BusinessCaseYOYCostBreakdown AzureCostYOY { get; set; }

        [JsonProperty("savings")]
        public BusinessCaseYOYCostBreakdown SavingsYOY { get; set; }

        [JsonProperty("azureEmissionsEstimates")]
        public List<YearOnYearEmission> AzureEmissionsEstimates { get; set; }

        [JsonProperty("onPremisesEmissionsEstimates")]
        public List<YearOnYearEmission> OnPremisesEmissionsEstimates { get; set; }
    }

    public class BusinessCaseYOYCostBreakdown
    {
        [JsonProperty("Year0")]
        public double Year0 { get; set;  }

        [JsonProperty("Year1")]
        public double Year1 { get; set; }

        [JsonProperty("Year2")]
        public double Year2 { get; set; }

        [JsonProperty("Year3")]
        public double Year3 { get; set; }
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum Year
    {
        [EnumMember(Value = "Year0")]
        Year0,

        [EnumMember(Value = "Year1")]
        Year1,

        [EnumMember(Value = "Year2")]
        Year2,

        [EnumMember(Value = "Year3")]
        Year3
    }

    public class YearOnYearEmission
    {
        [JsonProperty("year")]
        public string Year { get; set; }

        [JsonProperty("emissions")]
        public double? Emissions { get; set; }
    }
}
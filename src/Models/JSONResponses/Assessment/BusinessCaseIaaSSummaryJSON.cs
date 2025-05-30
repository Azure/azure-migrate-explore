// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Azure.Migrate.Explore.Models
{
    public class BusinessCaseIaaSSummaryJSON
    {
        [JsonProperty("properties")]
        public BusinessCaseIaasSummaryProperties Properties { get; set; }
    }

    public class BusinessCaseIaasSummaryProperties
    {
        [JsonProperty("azureIaaSSummary")]
        public BusinessCaseAzureIaaSSummary AzureIaaSSummary { get; set; }

        [JsonProperty("onPremisesIaaSSummary")]
        public BusinessCaseOnPremisesIaaSSummary OnPremisesIaaSSummary { get; set; }
    }

    public class BusinessCaseAzureIaaSSummary
    {
        [JsonProperty("yearOnYearEstimates")]
        public BusinessCaseYOYCostDetailsJSON YearOnYearEstimates { get; set; }
    }

    public class BusinessCaseOnPremisesIaaSSummary
    {
        [JsonProperty("onPremisesIaaSCostDetails")]
        public BusinessCaseCostDetailsJSON OnPremisesIaaSCostDetails { get; set; }

        [JsonProperty("osLicensingDetails")]
        public List<BusinessCaseOsLicensingDetail> OSLicensingDetails { get; set; }
    }

    public class BusinessCaseOsLicensingDetail
    {
        [JsonProperty("osType")]
        public string OSType { get; set; }

        [JsonProperty("totalCost")]
        public double TotalCost { get; set; }

        [JsonProperty("decomissionCost")]
        public double DecommissionCost { get; set; }
    }
}

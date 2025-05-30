// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Azure.Migrate.Explore.Models
{
    public class BusinessCaseAvsSummaryJSON
    {
        [JsonProperty("properties")]
        public BusinessCaseAvsSummaryProperties Properties { get; set; }
    }

    public class BusinessCaseAvsSummaryProperties
    {
        [JsonProperty("azureAvsSummary")]
        public BusinessCaseAzureAvsSummary AzureAvsSummary { get; set; }

        [JsonProperty("onPremisesAvsSummary")]
        public BusinessCaseOnPremisesAvsSummary OnPremisesAvsSummary { get; set; }

    }

    public class BusinessCaseAzureAvsSummary
    {
        [JsonProperty("yearOnYearEstimates")]
        public BusinessCaseYOYCostDetailsJSON YearOnYearEstimates { get; set; }
    }

    public class BusinessCaseOnPremisesAvsSummary
    {
        [JsonProperty("onPremisesIaaSCostDetails")]
        public BusinessCaseCostDetailsJSON OnPremisesIaaSCostDetails { get; set; }

        [JsonProperty("osLicensingDetails")]
        public List<BusinessCaseAvsOsLicensingDetail> OSLicensingDetails { get; set; }
    }

    public class BusinessCaseAvsOsLicensingDetail
    {
        [JsonProperty("osType")]
        public string OsType { get; set; }

        [JsonProperty("totalCost")]
        public double TotalCost { get; set; }

        [JsonProperty("decomissionCost")]
        public double DecommissionCost { get; set; }
    }
}
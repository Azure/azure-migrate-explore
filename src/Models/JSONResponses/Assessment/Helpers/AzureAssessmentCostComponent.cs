// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using Newtonsoft.Json;

namespace Azure.Migrate.Explore.Models
{
    public class AzureAssessmentCostComponent
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("value")]
        public double Value { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }
    }
}

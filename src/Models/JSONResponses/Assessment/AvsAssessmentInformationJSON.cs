// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using Newtonsoft.Json;

namespace Azure.Migrate.Explore.Models
{
    public class AvsAssessmentInformationJSON
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("properties")]
        public AvsAssessmentInformationProperty Properties { get; set; }
    }

    public class AvsAssessmentInformationProperty
    {
        [JsonProperty("details")]
        public AvsAssessmentPropertiesDetails Details { get; set; }
    }

    public class AvsAssessmentPropertiesDetails
    {
        [JsonProperty("status")]
        public string Status { get; set; }
    }
}
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using Newtonsoft.Json;

namespace Azure.Migrate.Explore.Models
{
    public class AssessmentInformationJSON
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("properties")]
        public AssessmentInformationProperty Properties { get; set; }
    }

    public class AssessmentInformationProperty
    {
        [JsonProperty("stage")]
        public string Stage { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }
    }
}
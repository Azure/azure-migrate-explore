// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using Newtonsoft.Json;
using System.Collections.Generic;

using Azure.Migrate.Explore.Common;

namespace Azure.Migrate.Explore.Models
{
    public class AVSAssessmentPropertiesJSON
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("properties")]
        public AVSAssessmentProperty Properties { get; set; }
    }

    public class AVSAssessmentProperty
    {
        [JsonProperty("vcpuOversubscription")]
        public double? VCpuOversubscription { get; set; }

        [JsonProperty("dedupeCompression")]
        public double? DedupeCompression { get; set; }

        [JsonProperty("details")]
        public AVSAssessmentDetails Details { get; set; }
    }

    public class AVSAssessmentDetails
    {
        [JsonProperty("createdTimestamp")]
        public string CreatedTimestamp { get; set; }

        [JsonProperty("confidenceRatingInPercentage")]
        public double? ConfidenceRatingInPercentage { get; set; }
    }

    public class AVSSuitabilitySummary // to check
    {
        [JsonProperty("suitable")]
        public int? Suitable { get; set; }

        [JsonProperty("conditionallySuitable")]
        public int? ConditionallySuitable { get; set; }

        [JsonProperty("notSuitable")]
        public int? NotSuitable { get; set; }

        [JsonProperty("readinessUnknown")]
        public int? ReadinessUnknown { get; set; }
    }
}
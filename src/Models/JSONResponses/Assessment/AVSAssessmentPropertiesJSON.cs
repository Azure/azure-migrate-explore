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
        [JsonProperty("avsEstimatedNodes")]
        public List<AvsEstimatedNodes> EstimatedNodes { get; set; }

        [JsonProperty("totalMonthlyCost")]
        public double? TotalMonthlyCost { get; set; }

        [JsonProperty("suitability")]
        public Suitabilities Suitability { get; set; }

        [JsonProperty("suitabilityExplanation")]
        public string SuitabilityExplanation { get; set; }

        [JsonProperty("numberOfNodes")]
        public int? NumberOfNodes { get; set; }

        [JsonProperty("cpuUtilization")]
        public double? CpuUtilization { get; set; }

        [JsonProperty("ramUtilization")]
        public double? RamUtilization { get; set; }

        [JsonProperty("storageUtilization")]
        public double? StorageUtilization { get; set; }

        [JsonProperty("totalCpuCores")]
        public double? TotalCpuCores { get; set; }

        [JsonProperty("totalRamInGB")]
        public double? TotalRamInGB { get; set; }

        [JsonProperty("totalStorageInGB")]
        public double? TotalStorageInGB { get; set; }

        [JsonProperty("vcpuOversubscription")]
        public double? VCpuOversubscription { get; set; }

        [JsonProperty("dedupeCompression")]
        public double? DedupeCompression { get; set; }

        [JsonProperty("numberOfMachines")]
        public int? NumberOfMachines { get; set; }

        [JsonProperty("suitabilitySummary")]
        public AVSSuitabilitySummary SuitabilitySummary { get; set; }

        [JsonProperty("avsEstimatedExternalStorages")]
        public List<AvsEstimatedExternalStorages> AvsEstimatedExternalStorages { get; set; }

        [JsonProperty("createdTimestamp")]
        public string CreatedTimestamp { get; set; }

        [JsonProperty("confidenceRatingInPercentage")]
        public double? ConfidenceRatingInPercentage { get; set; }
    }

    public class AVSSuitabilitySummary
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

    public class AvsEstimatedNodes
    {
        [JsonProperty("nodeType")]
        public string NodeType { get; set; }

        [JsonProperty("nodeNumber")]
        public int NodeNumber { get; set; }

        [JsonProperty("fttRaidLevel")]
        public string FttRaidLevel { get; set; }
    }

    public class AvsEstimatedExternalStorages
    {
        [JsonProperty("storageType")]
        public string StorageType { get; set; }

        [JsonProperty("totalStorageInGB")]
        public double TotalStorageInGB { get; set; }
    }
}
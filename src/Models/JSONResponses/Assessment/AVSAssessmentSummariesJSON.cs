// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using Newtonsoft.Json;
using System.Collections.Generic;

using Azure.Migrate.Explore.Common;
using System.Text.Json.Serialization;

namespace Azure.Migrate.Explore.Models
{
    public class AVSAssessmentSummariesJSON
    {
        [JsonProperty("value")]
        public List<AVSAssessmentSummaryValue> Values { get; set; }

        [JsonProperty("nextLink")]
        public string NextLink { get; set; }
    }

    public class AVSAssessmentSummaryValue
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("properties")]
        public AVSAssessmentSummaryProperty Properties { get; set; }
    }

    public class AVSAssessmentSummaryProperty
    {
        [JsonProperty("avsEstimatedExternalStorages")]
        public List<AvsEstimatedExternalStorages> AvsEstimatedExternalStorages { get; set; }

        [JsonProperty("avsEstimatedNodes")]
        public List<AvsEstimatedNodes> AvsEstimatedNodes { get; set; }

        [JsonProperty("suitability")]
        public string Suitability { get; set; }

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

        [JsonProperty("sources")]
        public List<Source> Sources { get; set; }

        [JsonProperty("costComponents")]
        public List<CostComponent> CostComponents { get; set; }

        [JsonProperty("targetSourceMapping")]
        public List<TargetSourceMapping> TargetSourceMapping { get; set; }
    }

    public class TargetSourceMapping
    {
        [JsonProperty("sourceCount")]
        public int SourceCount { get; set; }

        [JsonProperty("targetCount")]
        public int TargetCount { get; set; }

        [JsonProperty("migrationDetails")]
        public MigrationDetails MigrationDetails { get; set; }
    }

    public class MigrationDetails
    {
        [JsonProperty("migrationType")]
        public string MigrationType { get; set; }

        [JsonProperty("readinessSummary")]
        public List<ReadinessSummary> ReadinessSummary { get; set; }
    }

    public class ReadinessSummary
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("value")]
        public int Value { get; set; }
    }

    public class CostComponent
    {
        [JsonProperty("costDetail")]
        public List<CostDetail> CostDetail { get; set; }
    }

    public class CostDetail
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("value")]
        public double Value { get; set; }
    }

    public class Source
    {
        [JsonProperty("sourceName")]
        public string SourceName { get; set; }

        [JsonProperty("sourceType")]
        public string SourceType { get; set; }

        [JsonProperty("platform")]
        public string Platform { get; set; }

        [JsonProperty("count")]
        public int Count { get; set; }
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
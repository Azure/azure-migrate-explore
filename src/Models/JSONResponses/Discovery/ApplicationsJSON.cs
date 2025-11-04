// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Azure.Migrate.Explore.Models
{
    public class ApplicationsJSON
    {
        [JsonProperty("properties")]
        public ApplicationsProperty Properties { get; set; }
    }

    public class ApplicationsProperty
    {
        [JsonProperty("appsAndRoles")]
        public AppsAndRolesInfo AppsAndRoles { get; set; }
    }

    public class AppsAndRolesInfo
    {
        [JsonProperty("applications")]
        public List<ApplicationsInfo> Applications { get; set; }
    }

    public class ApplicationsInfo
    {
        [JsonProperty("Name")]
        public string Name { get; set; }
    }

    #region Resource Links Models
    public class ResourceLinksDirectResponse
    {
        [JsonProperty("value")]
        public List<ResourceLink> Value { get; set; }
    }

    public class ResourceLink
    {
        [JsonProperty("properties")]
        public ResourceLinkProperties Properties { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class ResourceLinkProperties
    {
        [JsonProperty("sourceId")]
        public string SourceId { get; set; }

        [JsonProperty("targetId")]
        public string TargetId { get; set; }

        [JsonProperty("notes")]
        public string Notes { get; set; }
    }
    #endregion
}
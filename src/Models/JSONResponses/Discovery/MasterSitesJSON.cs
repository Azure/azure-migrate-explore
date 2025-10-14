// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Azure.Migrate.Explore.Models
{
    public class MasterSitesJSON
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("etag")]
        public string Etag { get; set; }

        [JsonProperty("location")]
        public string Location { get; set; }

        [JsonProperty("kind")]
        public string Kind { get; set; }

        [JsonProperty("tags")]
        public MasterSitesTag Tags { get; set; }

        [JsonProperty("properties")]
        public MasterSitesProperty Properties { get; set; }
    }

    public class MasterSitesTag
    {
        [JsonProperty("Migrate Project")]
        public string MigrateProject { get; set; }
    }

    public class MasterSitesProperty
    {
        [JsonProperty("publicNetworkAccess")]
        public string PublicNetworkAccess { get; set; }

        [JsonProperty("allowMultipleSites")]
        public bool AllowMultipleSites { get; set; }

        [JsonProperty("sites")]
        public List<string> Sites { get; set; }

        [JsonProperty("nestedSites")]
        public List<string> NestedSites { get; set; }

        [JsonProperty("customerStorageAccountArmId")]
        public string CusotmerStorageAccountArmId { get; set; }

        [JsonProperty("provisioningState")]
        public string ProvisioningState { get; set; }
    }
}
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using Newtonsoft.Json;

namespace Azure.Migrate.Explore.Models
{
    public class GroupInformationJSON
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("properties")]
        public GroupInformationProperty Properties { get; set; }
    }

    public class GroupInformationProperty
    {
        [JsonProperty("groupStatus")]
        public string GroupStatus { get; set; }
    }
}
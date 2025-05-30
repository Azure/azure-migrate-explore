// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Azure.Migrate.Explore.Models
{
    public class VMwareVCenterJSON
    {
        [JsonProperty("value")]
        public List<VMwareVCentersMachinesValue> Values { get; set; }

        [JsonProperty("nextLink")]
        public string NextLink { get; set; }
    }

    public class VMwareVCentersMachinesValue
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }
}
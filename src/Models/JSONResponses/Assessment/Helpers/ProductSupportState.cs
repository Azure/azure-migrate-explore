// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using Newtonsoft.Json;
using Azure.Migrate.Explore.Common;

namespace Azure.Migrate.Explore.Models
{
    public class ProductSupportState
    {
        [JsonProperty("supportStatus")]
        [JsonConverter(typeof(SupportabilityStatusEnumConverter))]
        public SupportabilityStatus SupportStatus { get; set; }
    }
}
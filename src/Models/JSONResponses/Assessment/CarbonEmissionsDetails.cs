using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Azure.Migrate.Export.Models
{
    public class CarbonEmissionsDetails
    {
        [JsonProperty("scope1")]
        public CarbonEmissionsScopeDetails Scope1 { get; set; }

        [JsonProperty("scope2")]
        public CarbonEmissionsScopeDetails Scope2 { get; set; }

        [JsonProperty("scope3")]
        public CarbonEmissionsScopeDetails Scope3 { get; set; }
    }

    public class CarbonEmissionsScopeDetails
    {
        [JsonProperty("compute")]
        public double Compute { get; set; }

        [JsonProperty("storage")]
        public double Storage { get; set; }
    }
}

// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using System.Collections.Generic;

namespace Azure.Migrate.Explore.Models
{
    public class AssessedNetworkAdapter
    {
        public string MacAddress { get; set; }
        public List<string> IpAddresses { get; set; }
        public string DisplayName { get; set; }
        public double MegabytesPerSecondReceived { get; set; }
        public double MegaytesPerSecondTransmitted { get; set; }
    }
}
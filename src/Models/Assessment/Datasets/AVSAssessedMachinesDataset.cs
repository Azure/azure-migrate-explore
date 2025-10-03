// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using System.Collections.Generic;

using Azure.Migrate.Explore.Common;

namespace Azure.Migrate.Explore.Models
{
    public class AVSAssessedMachinesDataset
    {
        public string DisplayName { get; set; }
        public string DatacenterMachineArmId { get; set; }
        public string Suitability { get; set; }
        public string SuitabilityExplanation { get; set; }
        public string OperatingSystemName { get; set; }
        public string OperatingSystemVersion { get; set; }
        public string OperatingSystemArchitecture { get; set; }
        public string BootType { get; set; }
        public int NumberOfCores { get; set; }
        public double MegabytesOfMemory { get; set; }
        public List<AssessedDisk> Disks { get; set; }
        public double StorageInUseGB { get; set; }
        public int NetworkAdapters { get; set; }
        public List<AssessedNetworkAdapter> NetworkAdapterList { get; set; }
        public string GroupName { get; set; }
    }
}
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using Azure.Migrate.Explore.Common;

namespace Azure.Migrate.Explore.Models
{
    public class AssessedDisk
    {
        public string DisplayName { get; set; }
        public double GigabytesProvisioned { get; set; }
        public Suitabilities Suitability { get; set; }
        public string RecommendedDiskSku { get; set; }
        public RecommendedDiskTypes DiskType { get; set; }
        public double DiskCost { get; set; }
        public double MegabytesPerSecondOfRead { get; set; }
        public double MegabytesPerSecondOfWrite { get; set; }
        public double NumberOfReadOperationsPerSecond { get; set; }
        public double NumberOfWriteOperationsPerSecond { get; set; }
    }
}
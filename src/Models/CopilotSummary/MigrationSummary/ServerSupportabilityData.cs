// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using Azure.Migrate.Explore.CopilotSummary.FilterExcelData;
using System.ComponentModel;

namespace Azure.Migrate.Explore.Models.CopilotSummary.MigrationSummary
{
    public class ServerSupportabilityData
    {
        [Description("Percentage of Windows servers in Extended Support state")]
        public double WindowsServerExtended { get; set; }

        [Description("Percentage of Windows servers in Out of support")]
        public double WindowsServerOutOfSupport { get; set; }

        [Description("Percentage of SQL servers in Extended Support state")]
        public double SqlServerExtended { get; set; }

        [Description("Percentage of SQL servers in Out of support")]
        public double SqlServerOutOfSupport { get; set; }
    }
}
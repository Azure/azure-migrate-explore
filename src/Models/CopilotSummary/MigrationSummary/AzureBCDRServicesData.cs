// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using System.ComponentModel;

namespace Azure.Migrate.Explore.Models.CopilotSummary.MigrationSummary
{
    public class AzureBcdrServicesData
    {
        [Description("Servers considered for Azure Site recovery.These are Production servers suggested for migration to IaaS targets")]
        public int ASRInstancesCount { get; set; }

        [Description("Annual Azure Site Recovery Cost Estimate for Production Servers")]
        public double AnnualASRCost { get; set; }

        [Description("Annual Azure Backup Cost Estimate for Production Servers")]
        public double AnnualBackupCost { get; set; }
    }
}
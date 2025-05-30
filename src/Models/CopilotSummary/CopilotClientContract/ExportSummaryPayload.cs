// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using DocumentFormat.OpenXml.ExtendedProperties;

namespace Azure.Migrate.Explore.Models.CopilotSummary.CopilotClientContract
{
    /// <summary>
    /// Input for recording export summary payload.
    /// </summary>
    public record ExportSummaryPayload
    {
        /// <summary>
        /// Company Profile Version.
        /// </summary>
        public int CompanyProfileVersion { get; init; }

        /// <summary>
        /// Migration Details Version.
        /// </summary>
        public int MigrationDetailsVersion { get; init; }

        /// <summary>
        /// AI Opportunities Version.
        /// </summary>
        public int AIOpportunitiesVersion { get; init; }
    }
}
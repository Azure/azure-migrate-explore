// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
namespace Azure.Migrate.Explore.Models.CopilotSummary.CopilotClientContract
{
    /// <summary>
    /// Input for a query processing.
    /// </summary>
    public class QueryInput
    {
        /// <summary>
        /// Gets the user query.
        /// </summary>
        public UserQuery UserQuery { get; init; }

        /// <summary>
        /// Gets the migration data.
        /// </summary>
        public string? MigrationData { get; init; }

        /// <summary>
        /// Gets the tenant id.
        /// </summary>
        public string? TenantId { get; init; }

        /// <summary>
        /// Gets the subscription id.
        /// </summary>
        public string? SubscriptionId { get; init; }
    }
}
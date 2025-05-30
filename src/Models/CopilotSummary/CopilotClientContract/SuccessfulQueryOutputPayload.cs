// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using System.Collections.Generic;

namespace Azure.Migrate.Explore.Models.CopilotSummary.CopilotClientContract
{
    /// <summary>
    /// Defines the output payload for a successful query processing.
    /// </summary>
    public record SuccessfulQueryOutputPayload
    {
        /// <summary>
        /// Gets the result of the query.
        /// </summary>
        public List<AgentResponse> Result { get; init; }
    }
}
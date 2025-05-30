// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
namespace Azure.Migrate.Explore.Models.CopilotSummary.CopilotClientContract
{
    /// <summary>
    /// Ouput payload for a failed query.
    /// </summary>
    public record ErrorOutputPayload
    {
        /// <summary>
        /// Gets the error message.
        /// </summary>
        public string ErrorMessage { get; init; }
    }
}
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
namespace Azure.Migrate.Explore.Models.CopilotSummary.CopilotClientContract
{
    /// <summary>
    /// Output for a query processing.
    /// </summary>
    public record QueryOutput
    {
        /// <summary>
        /// Gets a value indicating whether the action was successful.
        /// </summary>
        public bool IsSuccess { get; init; }

        /// <summary>
        /// Gets the payload for the action. This is a JSON string representation of the payload object depending on the action type and the success of the action.
        /// </summary>
        public string Payload { get; init; }
    }
}
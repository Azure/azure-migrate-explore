// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
namespace Azure.Migrate.Explore.Models.CopilotSummary.CopilotClientContract
{
    /// <summary>
    /// Input for processing UserQuery.
    /// </summary>
    public record UserQuery
    {
        /// <summary>
        /// Gets the user action.
        /// </summary>
        public string UserAction { get; init; }

        /// <summary>
        /// Gets the user section name.
        /// </summary>
        public string? SectionName { get; init; }

        /// <summary>
        /// Gets user input payload.
        /// </summary>
        public string? Payload { get; init; }

        /// <summary>
        /// Gets the AI Agreement Checkbox State
        /// </summary>
        public bool? AIAgreementCheckboxState { get; init; }
    }
}
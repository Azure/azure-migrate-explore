// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
namespace Azure.Migrate.Explore.Models.CopilotSummary.CopilotClientContract
{

    /// <summary>
    /// Represents the user action of a query.
    /// </summary>
    public enum UserAction
    {
        /// <summary>
        /// Generate Summary.
        /// </summary>
        GenerateSummary,

        /// <summary>
        /// Update Summary.
        /// </summary>
        UpdateSummary,

        /// <summary>
        /// Answer User Query.
        /// </summary>
        AnswerQuery,

        /// <summary>
        /// Capture user feedback.
        /// </summary>
        UserFeedback,

        /// <summary>
        /// Capture Export Summary.
        /// </summary>
        ExportSummary,
    }
}
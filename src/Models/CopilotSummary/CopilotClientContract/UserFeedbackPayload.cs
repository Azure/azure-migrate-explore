// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
namespace Azure.Migrate.Explore.Models.CopilotSummary.CopilotClientContract
{
    /// <summary>
    /// Input for recording User Feedback.
    /// </summary>
    public record UserFeedbackPayload
    {
        /// <summary>
        /// Gets the feedback score. 1 for positive, 0 for negative.
        /// </summary>
        public int FeedbackScore { get; init; }

        /// <summary>
        /// Gets the user feedback comment.
        /// </summary>
        public string FeedbackComment { get; init; }
    }
}
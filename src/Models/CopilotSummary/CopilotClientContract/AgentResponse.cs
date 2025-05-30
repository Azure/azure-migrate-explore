// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using System.Collections.Generic;

namespace Azure.Migrate.Explore.Models.CopilotSummary.CopilotClientContract
{
    /// <summary>
    /// Response given by an agent.
    /// </summary>
    public class AgentResponse
    {
        /// <summary>
        /// Gets the section name of the response.
        /// </summary>
        public string ResponseSectionName { get; init; }

        /// <summary>
        /// Gets or sets the agent response.
        /// </summary>
        public string Response { get; set; }

        /// <summary>
        /// Gets or sets the list of nudges for the corresponding response.
        /// </summary>
        public List<string>? Nudges { get; set; }
    }
}
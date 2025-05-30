// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
namespace Azure.Migrate.Explore.Models.CopilotSummary.MigrationSummary
{
    public class UserCopilotQuestionnaireInput
    {
        public string CustomerName { get; set; }
        public string CustomerIndustry { get; set; }
        public string Motivation { get; set; }
        public string DatacenterLocation { get; set; }
        public string AiOpportunities { get; set; }
        public string OtherDetails { get; set; }
    }
}
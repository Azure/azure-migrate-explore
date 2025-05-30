// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using Azure.Migrate.Explore.Logger;
using System;
using System.Threading;

namespace Azure.Migrate.Explore.Models.CopilotSummary
{
    public class CopilotInput
    {
        public string CustomerName { get; set; }
        public string CustomerIndustry { get; set; }
        public string Motivation { get; set; }
        public string DatacenterLocation { get; set; }
        public string AiOpportunities { get; set; }
        public string OtherDetails { get; set; }
        public string OptimizationPreference { get; set; }
        public bool isSummaryGenerated { get; set; }
        public LogHandler LoggerObj { get; set; }
        public CancellationTokenSource CancellationContext = new CancellationTokenSource();

        public CopilotInput(
            string customerName,
            string customerIndustry,
            string motivation,
            string datacenterLocation,
            string aiOpportunities,
            string otherDetails,
            string optimizationPreference
            )
        {
            CustomerName = customerName;
            CustomerIndustry = customerIndustry;
            Motivation = motivation;
            DatacenterLocation = datacenterLocation;
            isSummaryGenerated = false;
            AiOpportunities = aiOpportunities;
            OtherDetails = otherDetails;
            OptimizationPreference = optimizationPreference;
            LoggerObj = new LogHandler();
        }
    }
}
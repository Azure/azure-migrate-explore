// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using System.ComponentModel;

namespace Azure.Migrate.Explore.Models.CopilotSummary.MigrationSummary
{
    public class MigrationPaasData
    {
        [Description("Azure PaaS Recommendations for various existing customer workloads to identify readiness for migration on PaaS targets with re-platform strategy.")]
        public string PaasTransformationSummary { get; set; }

        [Description("Total SQL Server Instances")]
        public int ActiveSqlInstances { get; set; }

        [Description("Percentage of average CPU Utilization for SQL Servers")]
        public double SqlAvgCpuUtilization { get; set; }

        [Description("Total Storage in TB for SQL Servers")]
        public double SqlTotalStorageinTB { get; set; }

        [Description("Azure App service plan cost")]
        public double WebappAnnualConsumptionCost { get; set; }

        [Description("Top Recommended SQL Server MI Configs by Cost(Prod) (3RI+AHUB)")]
        public string SqlTopRecommendationCostProd { get; set; }

        [Description("Top Recommended SQL Server MI Configs by Cost(Dev) (3RI+AHUB)")]
        public string SqlTopRecommendationCostDev { get; set; }

        [Description("Top Recommended SQL Server MI Configs by Count")]
        public string SqlTopRecommendationCount { get; set; }

        [Description("Top Readiness SQL Server MI warnings by Count")]
        public string SqlWarnings { get; set; }

        [Description("Total storage cost for Right-sized PaaS targets SQL")]
        public double SqlAnnualStorageCostEstimate { get; set; }

        [Description("Top App Service Plan instances by Cost(Prod)(3RI)")]
        public string WebappTopRecommendationCostProd { get; set; }

        [Description("Top App Service Plan instances by Cost(Dev)(3RI)")]
        public string WebappTopRecommendationCostDev { get; set; }

        [Description("Top App Service Plan instances by Count")]
        public string WebappTopRecommendationCount { get; set; }

        [Description("Top Readiness App Service Plan instances warnings by Count")]
        public string WebappWarnings { get; set; }

        [Description("Comparison between total compute cost for PaaS targets. This includes ASP and 3Year RI for Webapps costs and 3Year RI, AHUB and 3Year RI + AHUB for SQL costs")]
        public string AnnualComputeCostComparison { get; set; }
    }
}
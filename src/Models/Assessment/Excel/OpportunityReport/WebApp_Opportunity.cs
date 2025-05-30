// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
namespace Azure.Migrate.Explore.Models
{
    public class WebApp_Opportunity
    {
        public string MachineName { get; set; }
        public string WebAppName { get; set; }
        public string Environment { get; set; }
        public string AzureAppServiceReadiness { get; set; }
        public string AzureAppServiceReadiness_Issues { get; set; }
        public string AzureRecommendedTarget { get; set; }
        public string GroupName { get; set; }
        public string MachineId { get; set; }
    }
}
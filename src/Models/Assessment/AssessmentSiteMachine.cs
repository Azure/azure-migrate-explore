// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
namespace Azure.Migrate.Explore.Models
{
    public class AssessmentSiteMachine
    {
        public string DisplayName { get; set; }
        public string AssessmentId { get; set; }
        public string DiscoveryMachineArmId { get; set; }
        public int SqlInstancesCount { get; set; }
        public int WebApplicationsCount { get; set; }
    }
}
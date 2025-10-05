// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
namespace Azure.Migrate.Explore.Models
{
    public class AVSAssessmentPropertiesDataset
    {
        public string SubscriptionId { get; set; }
        public string ResourceGroup { get; set; }
        public string AssessmentProjectName { get; set; }
        public string GroupName { get; set; }
        public string AssessmentName { get; set; }
        public string SizingCriterion { get; set; }
        public string AssessmentType { get; set; } = "Azure VMWare Solution";
        public string CreatedOn { get; set; }
        public int TotalMachinesAssessed { get; set; }
        public int MachinesReady { get; set; }
        public int MachinesReadyWithConditions { get; set; }
        public int MachinesReadinessUnknown { get; set; }
        public int MachinesNotReady { get; set; }
        public int TotalRecommendedNumberOfNodes { get; set; }
        public string NodeTypes { get; set; }
        public string RecommendedNodes { get; set; }
        public string RecommendedFttRaidLevels { get; set; }
        public string RecommendedExternalStorage { get; set; }
        public double TotalMonthlyCostEstimate { get; set; }
        public double TotalNodeCost { get; set; }
        public double TotalExternalNetworkCost { get; set; }
        public double TotalExternalStorageCost { get; set; }
        public double PredictedCpuUtilizationPercentage { get; set; }
        public double PredictedMemoryUtilizationPercentage { get; set; }
        public double PredictedStorageUtilizationPercentage { get; set; }
        public double NumberOfCpuCoresAvailable { get; set; }
        public double MemoryInTBAvailable { get; set; }
        public double StorageInTBAvailable { get; set; }
        public double NumberOfCpuCoresUsed { get; set; }
        public double MemoryInTBUsed { get; set; }
        public double StorageInTBUsed { get; set; }
        public double NumberOfCpuCoresFree { get; set; }
        public double MemoryInTBFree { get; set; }
        public double StorageInTBFree { get; set; }
        public string ConfidenceRating { get; set; }
    }
}
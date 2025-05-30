// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using Newtonsoft.Json;
using System.Collections.Generic;

using Azure.Migrate.Explore.Common;

namespace Azure.Migrate.Explore.Models
{
    public class AzureSQLAssessedInstancesJSON
    {
        [JsonProperty("value")]
        public List<AzureSQLAssessedInstanceValue> Values { get; set; }

        [JsonProperty("nextLink")]
        public string NextLink { get; set; }
    }

    public class AzureSQLAssessedInstanceValue
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("properties")]
        public AzureSQLAssessedInstanceProperty Properties { get; set; }
    }

    public class AzureSQLAssessedInstanceProperty
    {
        [JsonProperty("memoryInUseInMB")]
        public double MemoryInUseInMB { get; set; }

        [JsonProperty("hasScanOccurred")]
        public bool HasScanOccurred { get; set; }

        [JsonProperty("recommendedAzureSqlTargetType")]
        public AzureSQLTargetType RecommendedAzureSqlTargetType { get; set; }

        [JsonProperty("recommendedSuitability")]
        public string RecommendedSuitability { get; set; }

        [JsonProperty("azureSqlMISuitabilityDetails")]
        public AzureSqlMISuitabilityDetailsInfo AzureSqlMISuitabilityDetails { get; set; }

        [JsonProperty("azureSqlVMSuitabilityDetails")]
        public AzureSqlVMSuitabilityDetailsInfo AzureSqlVMSuitabilityDetails { get; set; }

        [JsonProperty("storageTypeBasedDetails")]
        public List<AzureSqlStorageTypeBasedDetailsInfo> StorageTypeBasedDetails { get; set; }

        [JsonProperty("machineArmId")]
        public string MachineArmId { get; set; }

        [JsonProperty("machineName")]
        public string MachineName { get; set; }

        [JsonProperty("instanceName")]
        public string InstanceName { get; set; }

        [JsonProperty("sqlInstanceSDSArmId")]
        public string SqlInstanceSDSArmId { get; set; }

        [JsonProperty("sqlEdition")]
        public string SqlEdition { get; set; }

        [JsonProperty("sqlVersion")]
        public string SqlVersion { get; set; }

        [JsonProperty("productSupportStatus")]
        public ProductSupportState ProductSupportStatus { get; set; }

        [JsonProperty("numberOfCoresAllocated")]
        public int NumberOfCoresAllocated { get; set; }

        [JsonProperty("percentageCoresUtilization")]
        public double PercentageCoresUtilization { get; set; }

        [JsonProperty("logicalDisks")]
        public List<AzureSqlLogicalDisk> LogicalDisks { get; set; }

        [JsonProperty("databaseSummary")]
        public AzureSqlDatabaseSummaryInfo DatabaseSummary { get; set; }

        [JsonProperty("confidenceRatingInPercentage")]
        public double ConfidenceRatingInPercentage { get; set; }

        [JsonProperty("createdTimestamp")]
        public string CreatedTimestamp { get; set; }

        [JsonProperty("updatedTimestamp")]
        public string UpdatedTimestamp { get; set; }
    }

    public class AzureSqlLogicalDisk
    {
        [JsonProperty("diskId")]
        public string DiskId { get; set; }

        [JsonProperty("diskSizeInMB")]
        public double DiskSizeInMB { get; set; }

        [JsonProperty("megabytesPerSecondOfRead")]
        public double MegabytesPerSecondOfRead { get; set; }

        [JsonProperty("megabytesPerSecondOfWrite")]
        public double MegabytesPerSecondOfWrite { get; set; }

        [JsonProperty("numberOfReadOperationsPerSecond")]
        public double NumberOfReadOperationsPerSecond { get; set; }

        [JsonProperty("numberOfWriteOperationsPerSecond")]
        public double NumberOfWriteOperationsPerSecond { get; set; }
    }

    public class AzureSqlDatabaseSummaryInfo
    {
        [JsonProperty("numberOfUserDatabases")]
        public int NumberOfUserDatabases { get; set; }

        [JsonProperty("totalDatabaseSizeInMB")]
        public double TotalDatabaseSizeInMB { get; set; }

        [JsonProperty("largestDatabaseSizeInMB")]
        public double LargestDatabaseSizeInMB { get; set; }

        [JsonProperty("totalDiscoveredUserDatabases")]
        public int TotalDiscoveredUserDatabases { get; set; }
    }

    public class AzureSqlStorageTypeBasedDetailsInfo
    {
        [JsonProperty("storageType")]
        public string StorageType { get; set; }

        [JsonProperty("diskSizeInMB")]
        public double DiskSizeInMB { get; set; }

        [JsonProperty("megabytesPerSecondOfRead")]
        public double MegabytesPerSecondOfRead { get; set; }

        [JsonProperty("megabytesPerSecondOfWrite")]
        public double MegabytesPerSecondOfWrite { get; set; }

        [JsonProperty("numberOfReadOperationsPerSecond")]
        public double NumberOfReadOperationsPerSecond { get; set; }

        [JsonProperty("numberOfWriteOperationsPerSecond")]
        public double NumberOfWriteOperationsPerSecond { get; set; }
    }

    public class AzureSqlMISuitabilityDetailsInfo
    {
        [JsonProperty("azureSqlSku")]
        public AzureSqlMISku AzureSqlSku { get; set; }

        [JsonProperty("monthlyComputeCost")]
        public double MonthlyComputeCost { get; set; }

        [JsonProperty("monthlyLicenseCost")]
        public double MonthlyLicenseCost { get; set; }

        [JsonProperty("monthlyStorageCost")]
        public double MonthlyStorageCost { get; set; }

        [JsonProperty("migrationTargetPlatform")]
        public AzureSQLTargetType MigrationTargetPlatform { get; set; }

        [JsonProperty("costComponents")]
        public List<AzureAssessmentCostComponent> CostComponents { get; set; }

        [JsonProperty("suitability")]
        public Suitabilities Suitability { get; set; }

        [JsonProperty("migrationIssues")]
        public List<AzureSqlMigrationIssueInfo> MigrationIssues { get; set; }
    }

    public class AzureSqlVMSuitabilityDetailsInfo
    {
        [JsonProperty("azureSqlSku")]
        public AzureSqlVMSku AzureSqlSku { get; set; }

        [JsonProperty("monthlyComputeCost")]
        public double MonthlyComputeCost { get; set; }

        [JsonProperty("monthlyLicenseCost")]
        public double MonthlyLicenseCost { get; set; }

        [JsonProperty("monthlyStorageCost")]
        public double MonthlyStorageCost { get; set; }

        [JsonProperty("migrationTargetPlatform")]
        public AzureSQLTargetType MigrationTargetPlatform { get; set; }

        [JsonProperty("costComponents")]
        public List<AzureAssessmentCostComponent> CostComponents { get; set; }

        [JsonProperty("suitability")]
        public Suitabilities Suitability { get; set; }

        [JsonProperty("migrationIssues")]
        public List<AzureSqlMigrationIssueInfo> MigrationIssues { get; set; }
    }

    public class AzureSQLVMDiskInfo
    {
        [JsonProperty("diskType")]
        public RecommendedDiskTypes DiskType { get; set; }

        [JsonProperty("diskSize")]
        public string DiskSize { get; set; }

        [JsonProperty("storageCost")]
        public double StorageCost { get; set; }
    }

    public class AzureSqlVMSku
    {
        [JsonProperty("virtualMachineSize")]
        public AzureSqlVirtualMachineSize VirtualMachineSize { get; set; }

        [JsonProperty("dataDiskSizes")]
        public List<AzureSQLVMDiskInfo> DataDiskSizes { get; set; }

        [JsonProperty("logDiskSizes")]
        public List<AzureSQLVMDiskInfo> LogDiskSizes { get; set; }

        [JsonProperty("azureSqlTargetType")]
        public AzureSQLTargetType AzureSqlTargetType { get; set; }
    }

    public class AzureSqlVirtualMachineSize
    {
        [JsonProperty("azureVmFamily")]
        public string AzureVmFamily { get; set; }

        [JsonProperty("cores")]
        public int Cores { get; set; }

        [JsonProperty("azureSkuName")]
        public string AzureSkuName { get; set; }

        [JsonProperty("availableCores")]
        public int AvailableCores { get; set; }

        [JsonProperty("maxNetworkInterfaces")]
        public int MaxNetworkInterfaces { get; set; }
    }

    public class AzureSqlMISku
    {
        [JsonProperty("azureSqlServiceTier")]
        public string AzureSqlServiceTier { get; set; }

        [JsonProperty("azureSqlComputeTier")]
        public string AzureSqlComputeTier { get; set; }

        [JsonProperty("azureSqlHardwareGeneration")]
        public string AzureSqlHardwareGeneration { get; set; }

        [JsonProperty("storageMaxSizeInMB")]
        public double StorageMaxSizeInMB { get; set; }

        [JsonProperty("predictedDataSizeInMB")]
        public double PredictedDataSizeInMB { get; set; }

        [JsonProperty("predictedLogSizeInMB")]
        public double PredictedLogSizeInMB { get; set; }

        [JsonProperty("cores")]
        public int Cores { get; set; }

        [JsonProperty("azureSqlTargetType")]
        public AzureSQLTargetType AzureSqlTargetType { get; set; }
    }

    public class AzureSqlMigrationIssueInfo
    {
        [JsonProperty("issueId")]
        public string IssueId { get; set; }

        [JsonProperty("issueCategory")]
        public IssueCategories IssueCategory { get; set; }

        [JsonProperty("impactedObjects")]
        public List<ImpactedObjectInfo> ImpactedObjects { get; set; }
    }

    public class ImpactedObjectInfo
    {
        [JsonProperty("objectName")]
        public string ObjectName { get; set; }

        [JsonProperty("objectType")]
        public string ObjectType { get; set; }
    }
}
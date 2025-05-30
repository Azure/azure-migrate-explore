// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
namespace Azure.Migrate.Explore.Models
{
    public class SQL_MI_Issues_and_Warnings
    {
        public string MachineName { get; set; }
        public string SQLInstance { get; set; }
        public string MigrationReadinessTarget { get; set; } = "AzureSqlManagedInstance";
        public string Category { get; set; }
        public string Title { get; set; }
        public string ImpactedObjectType { get; set; }
        public string ImpactedObjectName { get; set; }
        public int UserDatabases { get; set; }
        public string MachineId { get; set; }
    }
}
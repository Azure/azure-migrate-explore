// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using System.Collections.Generic;

using Azure.Migrate.Explore.Common;

namespace Azure.Migrate.Explore.Models
{
    public class AssessedMigrationIssue
    {
        public string IssueId { get; set; }
        public IssueCategories IssueCategory { get; set; }
        public List<string> IssueDescriptionList { get; set; } = new List<string>();
        public List<ImpactedObjectInfo> ImpactedObjects { get; set; } = new List<ImpactedObjectInfo>();
    }
}
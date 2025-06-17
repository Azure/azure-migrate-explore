// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using System;
using System.IO;

namespace Azure.Migrate.Explore.Common
{
    public class SummaryConstants
    {
        public static readonly string SummaryDirectory = UtilityFunctions.GetReportsDirectory();
        public const string BackSlash = @"\";
        public const string SummaryPath = "AzureMigrateExploreInsights";
    }
}
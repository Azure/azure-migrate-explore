// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using DocumentFormat.OpenXml.Office2016.Drawing.Command;
using System.Collections.Generic;

namespace Azure.Migrate.Explore.Common
{
    public class AvsAssessmentConstants
    {
        public static readonly Dictionary<string, List<string>> RegionToAvsNodeTypeMap = new Dictionary<string, List<string>>
        {
            { "eastus", new List<string> { "AV36P", "AV64" } },
            { "eastus2", new List<string> { "AV36P", "AV52", "AV64" } },
            { "southcentralus", new List<string> { "AV52", "AV36", "AV36P", "AV64" } },
            { "westus2", new List<string> { "AV36", "AV36P", "AV64" } },
            { "australiaeast", new List<string> { "AV36", "AV36P", "AV64" } },
            { "southeastasia", new List<string> { "AV36", "AV36P" } },
            { "northeurope", new List<string> { "AV36", "AV64" } },
            { "swedencentral", new List<string> { "AV36" } },
            { "uksouth", new List<string> { "AV36P", "AV52", "AV64" } },
            { "westeurope", new List<string> { "AV36P", "AV52", "AV64" } },
            { "centralus", new List<string> { "AV36", "AV36P", "AV64" } },
            { "southafricanorth", new List<string> { "AV36" } },
            { "eastasia", new List<string> { "AV36", "AV36P" } },
            { "japaneast", new List<string> { "AV36", "AV64" } },
            { "canadacentral", new List<string> { "AV36", "AV36P" } },
            { "switzerlandnorth", new List<string> { "AV36", "AV36P", "AV64" } },
            { "switzerlandwest", new List<string> { "AV36", "AV36P", "AV64" } },
            { "italynorth", new List<string> { "AV36", "AV36P", "AV52" } },
            { "centralindia", new List<string> { "AV36P", "AV64" } },
            { "francecentral", new List<string> { "AV36" } },
            { "northcentralus", new List<string> { "AV36P", "AV64" } },
            { "germanywestcentral", new List<string> { "AV36P", "AV64" } },
            { "westus", new List<string> { "AV36P" } },
            { "uaenorth", new List<string> { "AV36P" } },
            { "qatarcentral", new List<string> { "AV36P", "AV64" } },
            { "brazilsouth", new List<string> { "AV36" } },
            { "japanwest", new List<string> { "AV36", "AV64" } },
            { "ukwest", new List<string> { "AV36" } }
        };

        public static List<string> anfStandardStorageRegionList = new List<string>
        {
            "eastasia",
            "southeastasia",
            "australiaeast",
            "brazilsouth",
            "canadacentral",
            "westeurope",
            "northeurope",
            "centralindia",
            "japaneast",
            "japanwest",
            "ukwest",
            "uksouth",
            "northcentralus",
            "eastus",
            "westus2",
            "southcentralus",
            "centralus",
            "eastus2",
            "westus",
            "francecentral",
            "southafricanorth",
            "germanywestcentral",
            "switzerlandnorth",
            "switzerlandwest",
            "uaenorth",
            "swedencentral",
            "qatarcentral",
            "italynorth",
        };

        public static List<string> anfPremiumStorageRegionList = new List<string>
        {
            "eastasia",
            "southeastasia",
            "australiaeast",
            "brazilsouth",
            "canadacentral",
            "westeurope",
            "northeurope",
            "centralindia",
            "japaneast",
            "japanwest",
            "ukwest",
            "uksouth",
            "northcentralus",
            "eastus",
            "westus2",
            "southcentralus",
            "centralus",
            "eastus2",
            "westus",
            "francecentral",
            "southafricanorth",
            "germanywestcentral",
            "switzerlandnorth",
            "switzerlandwest",
            "uaenorth",
            "swedencentral",
            "qatarcentral",
            "italynorth",
        };

        public static List<string> anfUltraStorageRegionLis = new List<string>
        {
            "eastasia",
            "southeastasia",
            "australiaeast",
            "brazilsouth",
            "canadacentral",
            "westeurope",
            "northeurope",
            "centralindia",
            "japaneast",
            "japanwest",
            "ukwest",
            "uksouth",
            "northcentralus",
            "eastus",
            "westus2",
            "southcentralus",
            "centralus",
            "eastus2",
            "westus",
            "francecentral",
            "southafricanorth",
            "germanywestcentral",
            "switzerlandnorth",
            "switzerlandwest",
            "uaenorth",
            "qatarcentral",
            "swedencentral",
            "italynorth",
        };

        public static string VCpuOversubscription = "4:1";
        public static readonly string MemoryOvercommit = "100%";
        public static double DedupeCompression = 1.5;

        public static Dictionary<string, double> perYearMigrationCompletionPercentage = new Dictionary<string, double>()
        {
            { "Year0", 0 },
            { "Year1", 100 },
            { "Year2", 100 },
            { "Year3", 100 },
        };
    }
}
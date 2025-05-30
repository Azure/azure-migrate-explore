// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
namespace Azure.Migrate.Explore.Models
{
    public class BusinessCaseInformation
    {
        public string BusinessCaseName { get; set; }

        public string BusinessCaseSettings { get; set; }

        public BusinessCaseInformation(string businessCaseName, string businessCaseSettings)
        {
            BusinessCaseName = businessCaseName;
            BusinessCaseSettings = businessCaseSettings;
        }
    }
}
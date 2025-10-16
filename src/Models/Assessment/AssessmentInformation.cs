// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using Azure.Migrate.Explore.Common;

namespace Azure.Migrate.Explore.Models
{
    public class AssessmentInformation
    {
        public string AssessmentName { get; set; }
        public AssessmentType AssessmentType{ get; set; }
        public AssessmentTag AssessmentTag { get; set; }
        public string AssessmentSettings { get; set; }

        public AssessmentInformation(string assessmentName, AssessmentType assessmentType, AssessmentTag assessmentTag, string assessmentSettings)
        {
            AssessmentName = assessmentName;
            AssessmentSettings = assessmentSettings;
            AssessmentType = assessmentType;
            AssessmentTag = assessmentTag;
        }
    }
}
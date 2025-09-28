// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using Azure.Migrate.Explore.Common;

namespace Azure.Migrate.Explore.Models
{
    public class AssessmentInformation
    {
        public string GroupName { get; set; }
        public string AssessmentName { get; set; }
        public AssessmentType AssessmentType{ get; set; }
        public AssessmentTag AssessmentTag { get; set; }
        public string AssessmentSettings { get; set; }
        public int AssessmentCreationPriority { get; set; }

        public AssessmentInformation(string assessmentName, AssessmentType assessmentType, AssessmentTag assessmentTag, string assessmentSettings, string groupName = null)
        {
            GroupName = groupName;
            AssessmentName = assessmentName;
            AssessmentSettings = assessmentSettings;
            AssessmentType = assessmentType;
            AssessmentTag = assessmentTag;

            if (AssessmentType == AssessmentType.SQLAssessment)
                AssessmentCreationPriority = 1;
            else if (AssessmentType == AssessmentType.WebAppAssessment)
                AssessmentCreationPriority = 2;
            else if (AssessmentType == AssessmentType.AVSAssessment)
                AssessmentCreationPriority = 3;
            else if (AssessmentType == AssessmentType.MachineAssessment)
                AssessmentCreationPriority = 4;
        }
    }
}
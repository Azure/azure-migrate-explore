// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using System.Collections.Generic;
using System.Linq;

using Azure.Migrate.Explore.Models;

namespace Azure.Migrate.Explore.Common
{
    public class AzureAvsCostCalculator
    {
        //AVS datasets
        private Dictionary<AssessmentInformation, AVSAssessmentPropertiesDataset> AvsAssessmentsData;

        private bool IsCalculated;

        private double TotalAvsComputeCost;

        public AzureAvsCostCalculator()
        {
            AvsAssessmentsData = new Dictionary<AssessmentInformation, AVSAssessmentPropertiesDataset>();

            IsCalculated = false;

            TotalAvsComputeCost = 0.00;
        }

        public bool IsCalculationComplete()
        {
            return IsCalculated;
        }

        public double GetTotalAvsComputeCost()
        {
            return TotalAvsComputeCost;
        }

        public void SetParameters(Dictionary<AssessmentInformation, AVSAssessmentPropertiesDataset> avsAssessmentsData)
        {
            AvsAssessmentsData = avsAssessmentsData;
        }
        
        public void Calculate()
        {
           TotalAvsComputeCost = AvsAssessmentsData.Min(summary => summary.Value.TotalMonthlyCostEstimate);
           IsCalculated = true;
        }
    }
}
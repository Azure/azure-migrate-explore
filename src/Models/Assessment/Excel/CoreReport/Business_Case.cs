// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
namespace Azure.Migrate.Explore.Models
{
    public class Business_Case
    {
        public BusinessCaseDatasetCostDetails OnPremisesIaaSCost { get; set; }
        public BusinessCaseDatasetCostDetails OnPremisesPaaSCost { get; set; }
        public BusinessCaseDatasetCostDetails OnPremisesAvsCost { get; set; }
        public BusinessCaseDatasetCostDetails TotalOnPremisesCost { get; set; }
        public BusinessCaseDatasetCostDetails AzureIaaSCost { get; set; }
        public BusinessCaseDatasetCostDetails AzurePaaSCost { get; set; }
        public BusinessCaseDatasetCostDetails AzureAvsCost { get; set; }
        public BusinessCaseDatasetCostDetails AzureArcEnabledOnPremisesCost { get; set; }
        public BusinessCaseDatasetCostDetails TotalAzureCost { get; set; }
        public BusinessCaseDatasetCostDetails WindowsServerLicense { get; set; }
        public BusinessCaseDatasetCostDetails SqlServerLicense { get; set; }
        public BusinessCaseDatasetCostDetails EsuSavings { get; set; }

        public Business_Case()
        {
            OnPremisesIaaSCost = new BusinessCaseDatasetCostDetails();
            OnPremisesPaaSCost = new BusinessCaseDatasetCostDetails();
            OnPremisesAvsCost = new BusinessCaseDatasetCostDetails();
            TotalOnPremisesCost = new BusinessCaseDatasetCostDetails();
            AzureIaaSCost = new BusinessCaseDatasetCostDetails();
            AzurePaaSCost = new BusinessCaseDatasetCostDetails();
            AzureAvsCost = new BusinessCaseDatasetCostDetails();
            AzureArcEnabledOnPremisesCost = new BusinessCaseDatasetCostDetails();
            TotalAzureCost = new BusinessCaseDatasetCostDetails();
            WindowsServerLicense = new BusinessCaseDatasetCostDetails();
            SqlServerLicense = new BusinessCaseDatasetCostDetails();
            EsuSavings = new BusinessCaseDatasetCostDetails();
        }
    }
}
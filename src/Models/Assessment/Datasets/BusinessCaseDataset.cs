namespace Azure.Migrate.Export.Models
{
    public class BusinessCaseDataset
    {
        public BusinessCaseDataset()
        {
            OnPremIaaSCostDetails = new BusinessCaseDatasetCostDetails();
            OnPremPaaSCostDetails = new BusinessCaseDatasetCostDetails();
            OnPremAvsCostDetails = new BusinessCaseDatasetCostDetails();
            AzureIaaSCostDetails = new BusinessCaseDatasetCostDetails();
            AzurePaaSCostDetails = new BusinessCaseDatasetCostDetails();
            AzureAvsCostDetails = new BusinessCaseDatasetCostDetails();
            WindowsServerLicense = new BusinessCaseDatasetCostDetails();
            SqlServerLicense = new BusinessCaseDatasetCostDetails();
            EsuSavings = new BusinessCaseDatasetCostDetails();
            TotalYOYCashFlowsAndEmissions = new BusinessCaseYOYJSON();
            IaaSYOYCashFlows = new BusinessCaseYOYCostDetailsJSON();
            AvsYOYCashFlows = new BusinessCaseYOYCostDetailsJSON();
            TotalAzureSustainabilityDetails = new CarbonEmissionsDetails();
            TotalOnPremisesSustainabilityDetails = new CarbonEmissionsDetails();
        }

        public BusinessCaseDatasetCostDetails OnPremIaaSCostDetails { get; set; } = null;
        public BusinessCaseDatasetCostDetails OnPremPaaSCostDetails { get; set; } = null;
        public BusinessCaseDatasetCostDetails OnPremAvsCostDetails { get; set; } = null;
        public BusinessCaseDatasetCostDetails AzureIaaSCostDetails { get; set; } = null;
        public BusinessCaseDatasetCostDetails AzurePaaSCostDetails { get; set; } = null;
        public BusinessCaseDatasetCostDetails AzureAvsCostDetails { get; set; } = null;
        public BusinessCaseDatasetCostDetails WindowsServerLicense { get; set; }
        public BusinessCaseDatasetCostDetails SqlServerLicense { get; set; }
        public BusinessCaseDatasetCostDetails EsuSavings { get; set; }
        public BusinessCaseYOYJSON TotalYOYCashFlowsAndEmissions { get; set; } = null;
        public BusinessCaseYOYCostDetailsJSON IaaSYOYCashFlows { get; set; } = null;
        public BusinessCaseYOYCostDetailsJSON AvsYOYCashFlows { get; set; } = null;
        public CarbonEmissionsDetails TotalAzureSustainabilityDetails { get; set; } = null;
        public CarbonEmissionsDetails TotalOnPremisesSustainabilityDetails { get; set; } = null;
    }
}
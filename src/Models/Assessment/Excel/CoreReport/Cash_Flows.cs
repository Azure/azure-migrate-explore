// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
namespace Azure.Migrate.Explore.Models
{
    public class Cash_Flows
    {
        public BusinessCaseYOYCostDetailsJSON TotalYOYCosts { get; set; }
        public Cash_Flows()
        {
            TotalYOYCosts = new BusinessCaseYOYCostDetailsJSON();
        }
    }
}
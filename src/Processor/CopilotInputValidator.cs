// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using Azure.Migrate.Explore.Models.CopilotSummary;
using System.Text.RegularExpressions;

namespace Azure.Migrate.Explore.Processor
{
    public static class CopilotInputValidator
    {
        public static bool ValidateCopilotInput(CopilotInput copilotInputObj)
        {
            return IsCustomerNameValid(copilotInputObj) && IsMotivationValid(copilotInputObj) &&
                   IsDatacenterValid(copilotInputObj) && IsCustomerIndustryValid(copilotInputObj);
        }

        private static bool IsCustomerNameValid(CopilotInput copilotInputObj)
        {
            copilotInputObj.LoggerObj.LogInformation($"Validating Customer Name: {copilotInputObj.CustomerName}");

            if (string.IsNullOrEmpty(copilotInputObj.CustomerName))
            {
                copilotInputObj.LoggerObj.LogInformation("Empty Customer Name");
                return false;
            }
            if (Regex.IsMatch(copilotInputObj.CustomerName, @"[^a-zA-Z0-9\s]"))
            {
                copilotInputObj.LoggerObj.LogInformation("Customer Name contains invalid characters");
                return false;
            }

            return true;
        }

        private static bool IsMotivationValid(CopilotInput copilotInputObj)
        {
            copilotInputObj.LoggerObj.LogInformation($"Validating migration motivation: {copilotInputObj.Motivation}");

            if (string.IsNullOrEmpty(copilotInputObj.Motivation))
            {
                copilotInputObj.LoggerObj.LogInformation("Empty migration motivation field");
                return false;
            }

            if (!(copilotInputObj.Motivation == "Datacenter exit" ||
                  copilotInputObj.Motivation == "Cloud transformation" ||
                  copilotInputObj.Motivation == "Legacy servers" ||
                  copilotInputObj.Motivation == "Business innovation" ||
                  copilotInputObj.Motivation == "Security protection" ||
                  copilotInputObj.Motivation == "Other"))
            {
                return false;
            }

            return true;
        }

        private static bool IsDatacenterValid(CopilotInput copilotInputObj)
        {
            copilotInputObj.LoggerObj.LogInformation($"Validating Datacenter: {copilotInputObj.DatacenterLocation}");

            if (!string.IsNullOrEmpty(copilotInputObj.DatacenterLocation) &&
                !Regex.IsMatch(copilotInputObj.DatacenterLocation, @"^[a-zA-Z\s,]+$"))
            {
                return false;
            }

            return true;
        }

        private static bool IsCustomerIndustryValid(CopilotInput copilotInputObj)
        {
            copilotInputObj.LoggerObj.LogInformation($"Validating Customer Industry: {copilotInputObj.CustomerIndustry}");

            if (string.IsNullOrEmpty(copilotInputObj.CustomerIndustry))
            {
                copilotInputObj.LoggerObj.LogInformation("Empty Customer Industry");
                return false;
            }
            if (Regex.IsMatch(copilotInputObj.CustomerIndustry, @"[^a-zA-Z0-9\s]"))
            {
                copilotInputObj.LoggerObj.LogInformation("Customer Industry contains invalid characters");
                return false;
            }

            return true;
        }
    }
}
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using System.Collections.Generic;

using Azure.Migrate.Explore.Models;
using Microsoft.Extensions.Logging;

namespace Azure.Migrate.Explore.Assessment
{
    public class DiscoveryDataValidation
    {
        public void BeginValidation(Logger.LogHandler logger, List<DiscoveryData> discoveredData)
        {
            bool isEnvironmentValid = ValidateEnvironmentType(logger, discoveredData);
            if (!isEnvironmentValid)
                logger.LogWarning(2, "Discovery data validated with assumptions"); // IsExpressWorkflow ? 22 : 2 % complete
            else
                logger.LogInformation(2, "Discovery data validated successfully"); // IsExpressWorkflow ? 22 : 2 % complete
        }

        private bool ValidateEnvironmentType(Logger.LogHandler logger, List<DiscoveryData> discoveredData)
        {
            bool isValid = true;
            foreach (var machine in discoveredData)
            {
                if (string.IsNullOrEmpty(machine.EnvironmentType)) // Prod
                {
                    machine.EnvironmentType = "Prod";
                    continue;
                }

                else if (machine.EnvironmentType.ToLower().Equals("prod"))
                {
                    machine.EnvironmentType = "Prod";
                    continue;
                }

                else if (machine.EnvironmentType.ToLower().Equals("dev"))
                {
                    machine.EnvironmentType = "Dev";
                    continue;
                }

                // Invalid/Un-recognized envrionment type
                logger.LogWarning($"Treating environment type for {machine.MachineName} as 'Prod' because received input is invalid");
                machine.EnvironmentType = "Prod";
                isValid = false;
            }

            return isValid;
        }
    }
}
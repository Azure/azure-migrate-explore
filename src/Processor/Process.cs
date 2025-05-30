// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using System;

using Azure.Migrate.Explore.Assessment;
using Azure.Migrate.Explore.Common;
using Azure.Migrate.Explore.Discovery;
using Azure.Migrate.Explore.Models;

namespace Azure.Migrate.Explore.Processor
{
    public class Process
    {
        public void Initiate(UserInput userInputObj)
        {
            bool isSuccess = false;
            Discover discoverObj;
            Assess assessObj;

            if (userInputObj == null)
                return;

            bool isUserInputValid = ValidateUserInput.InitiateValidation(userInputObj);

            if (!isUserInputValid)
            {
                userInputObj.LoggerObj.LogError("Received invalid input(s)");
                return;
            }

            userInputObj.LoggerObj.LogInformation(4, "Validated user input"); // 5 % complete

            if (userInputObj.WorkflowObj.IsExpressWorkflow)
            {

                discoverObj = new Discover(userInputObj);
                try
                {
                    isSuccess = discoverObj.BeginDiscovery();
                }
                catch (OperationCanceledException)
                {
                    userInputObj.LoggerObj.LogInformation("Process terminated upon user's request");
                    return;
                }
                catch (Exception exExpressDiscovery)
                {
                    userInputObj.LoggerObj.LogError($"Process terminated due to exception: {exExpressDiscovery.Message}");
                }

                if (!isSuccess)
                {
                    userInputObj.LoggerObj.LogError("Terminating as discovery process failed");
                    return;
                }

                userInputObj.LoggerObj.LogInformation(LoggerConstants.DiscoveryCompletionConstantMessage);

                isSuccess = false;
                assessObj = new Assess(userInputObj, discoverObj.DiscoveredData);
                try
                {
                    isSuccess = assessObj.BeginAssessment();
                }
                catch (OperationCanceledException)
                {
                    userInputObj.LoggerObj.LogInformation("Process terminated upon user's request");
                    return;
                }
                catch (Exception exExpressAssessment)
                {
                    userInputObj.LoggerObj.LogError($"Process terminated due to exception: {exExpressAssessment.Message}");
                }

                if (!isSuccess)
                    userInputObj.LoggerObj.LogError("Terminating as assessment process failed");

                return;
            }

            if (userInputObj.WorkflowObj.Module.Equals("Discovery"))
            {
                discoverObj = new Discover(userInputObj);
                try
                {
                    isSuccess = discoverObj.BeginDiscovery();
                }
                catch (OperationCanceledException)
                {
                    userInputObj.LoggerObj.LogInformation("Process terminated upon user's request");
                    return;
                }
                catch (Exception exCustomDiscovery)
                {
                    userInputObj.LoggerObj.LogError($"Process terminated due to exception: {exCustomDiscovery.Message}");
                }

                if (!isSuccess)
                    userInputObj.LoggerObj.LogError("Terminating as discovery process failed");

                userInputObj.LoggerObj.LogInformation(LoggerConstants.DiscoveryCompletionConstantMessage);
                return;
            }

            isSuccess = false;
            assessObj = new Assess(userInputObj);
            try
            {
                isSuccess = assessObj.BeginAssessment();
            }
            catch (OperationCanceledException)
            {
                userInputObj.LoggerObj.LogInformation("Process terminated upon user's request");
                return;
            }
            catch (Exception exExpressAssessment)
            {
                userInputObj.LoggerObj.LogError($"Process terminated due to exception: {exExpressAssessment.Message}");
            }

            if (!isSuccess)
                userInputObj.LoggerObj.LogError("Terminating as assessment process failed");
            return;
        }
    }
}
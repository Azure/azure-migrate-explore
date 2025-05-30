// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Azure.Migrate.Explore.Common;
using Azure.Migrate.Explore.HttpRequestHelper;
using Azure.Migrate.Explore.Models;

namespace Azure.Migrate.Explore.Assessment
{
    public class BusinessCaseBuilder
    {
        private readonly BusinessCaseInformation BusinessCaseInformationObj;

        public BusinessCaseBuilder(BusinessCaseInformation businessCaseInformationObj)
        {
            BusinessCaseInformationObj = businessCaseInformationObj;
        }

        public KeyValuePair<BusinessCaseInformation, AssessmentPollResponse> BuildBusinessCase(UserInput userInputObj)
        {
            if (userInputObj.CancellationContext.IsCancellationRequested)
                UtilityFunctions.InitiateCancellation(userInputObj);

            AssessmentPollResponse result = AssessmentPollResponse.NotCreated;
            bool isBusinessCaseCreated = false;

            try
            {
                isBusinessCaseCreated = new HttpClientHelper().CreateBusinessCase(userInputObj, BusinessCaseInformationObj).Result;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (AggregateException aeCreateBizCase)
            {
                string errorMessage = "";
                foreach (var e in aeCreateBizCase.Flatten().InnerExceptions)
                {
                    if (e is OperationCanceledException)
                        throw;
                    else
                        errorMessage = errorMessage + e.Message + " ";
                }
                result = AssessmentPollResponse.NotCreated;
                userInputObj.LoggerObj.LogWarning($"Business case {BusinessCaseInformationObj.BusinessCaseName} creation failed: {errorMessage}");
            }
            catch (Exception ex)
            {
                result = AssessmentPollResponse.NotCreated;
                userInputObj.LoggerObj.LogWarning($"Business case {BusinessCaseInformationObj.BusinessCaseName} creation failed: {ex.Message}");
            }

            if (!isBusinessCaseCreated)
                return new KeyValuePair<BusinessCaseInformation, AssessmentPollResponse>(BusinessCaseInformationObj, AssessmentPollResponse.NotCreated);

            result = PollBusinessCaseState(userInputObj).Result;

            return new KeyValuePair<BusinessCaseInformation, AssessmentPollResponse>(BusinessCaseInformationObj, result);
        }

        private async Task<AssessmentPollResponse> PollBusinessCaseState(UserInput userInputObj)
        {
            int numberOfTries = 0;
            AssessmentPollResponse pollResult = AssessmentPollResponse.Created;

            while (numberOfTries < 25)
            {
                Thread.Sleep(60000);
                try
                {
                    pollResult = await new HttpClientHelper().PollBusinessCase(userInputObj, BusinessCaseInformationObj);

                    if (pollResult == AssessmentPollResponse.Error)
                    {
                        userInputObj.LoggerObj.LogInformation($"Polling for business case {BusinessCaseInformationObj.BusinessCaseName} resulted in non-retryable error");
                        numberOfTries += 1;
                    }
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (AggregateException aePollBizCase)
                {
                    string errorMessage = "";
                    foreach (var e in aePollBizCase.Flatten().InnerExceptions)
                    {
                        if (e is OperationCanceledException)
                            throw e;
                        else
                        {
                            errorMessage = errorMessage + e.Message + " ";
                        }
                    }
                    userInputObj.LoggerObj.LogWarning($"Business case {BusinessCaseInformationObj.BusinessCaseName} polling failed: {errorMessage}");
                }
                catch (Exception ex)
                {
                    userInputObj.LoggerObj.LogWarning($"Business case {BusinessCaseInformationObj.BusinessCaseName} polling failed: {ex.Message}");
                }

                if (pollResult == AssessmentPollResponse.Completed ||
                    pollResult == AssessmentPollResponse.OutDated ||
                    pollResult == AssessmentPollResponse.Invalid)
                    break;
            }

            userInputObj.LoggerObj.LogInformation($"Polling for business case {BusinessCaseInformationObj.BusinessCaseName} completed");
            return pollResult;
        }
    }
}
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
    public class BatchAssessments
    {
        const int MaxAssessmentsComputingInParallel = 8; // 7 (max) for SQL assessments + 1 for easily computed assessments
        private readonly SemaphoreSlim Semaphore;
        private List<Task> Tasks;
        private readonly List<AssessmentInformation> AllAssessments;
        private Dictionary<AssessmentInformation, AssessmentPollResponse> AssessmentStatusMap;

        public BatchAssessments(List<AssessmentInformation> allAssessments)
        {
            Semaphore = new SemaphoreSlim(MaxAssessmentsComputingInParallel, MaxAssessmentsComputingInParallel);
            AllAssessments = allAssessments;
            if (AllAssessments != null)
                Tasks = new List<Task>();
            AssessmentStatusMap = new Dictionary<AssessmentInformation, AssessmentPollResponse>();
        }

        public Dictionary<AssessmentInformation, AssessmentPollResponse> CreateAssessmentsInBatch(UserInput userInputObj)
        {
            foreach (AssessmentInformation assessmentInfo in AllAssessments)
            {
                Semaphore.Wait();

                if (AssessmentStatusMap.ContainsKey(assessmentInfo))
                    AssessmentStatusMap.Add(assessmentInfo, AssessmentPollResponse.NotCreated);

                if (userInputObj.CancellationContext.IsCancellationRequested)
                    break; // we need all tasks to terminate first

                bool isAssessmentCreated = false;
                try
                {
                    isAssessmentCreated = new HttpClientHelper().CreateAssessment(userInputObj, assessmentInfo).Result;
                }
                catch (OperationCanceledException)
                {
                    break; // we need all tasks to terminate
                }
                catch (AggregateException aeCreateAssessment)
                {
                    string errorMessage = "";
                    bool isOperationCancelledException = false;
                    foreach (var e in aeCreateAssessment.Flatten().InnerExceptions)
                    {
                        if (e is OperationCanceledException)
                        {
                            isOperationCancelledException = true;
                            break; // from inner for
                        }
                        else
                        {
                            errorMessage = errorMessage + e.Message + " ";
                        }
                    }
                    if (isOperationCancelledException)
                        break; // from outer while, we need all processes to terminate
                    AssessmentStatusMap[assessmentInfo] = AssessmentPollResponse.NotCreated;
                    userInputObj.LoggerObj.LogWarning($"Assessment {assessmentInfo.AssessmentName} creation failed: {errorMessage}");
                }
                catch (Exception ex)
                {
                    AssessmentStatusMap[assessmentInfo] = AssessmentPollResponse.NotCreated;
                    userInputObj.LoggerObj.LogWarning($"Assessment {assessmentInfo.AssessmentName} creation failed: {ex.Message}");
                }

                if (isAssessmentCreated)
                    AssessmentStatusMap[assessmentInfo] = AssessmentPollResponse.Created;

                if (AssessmentStatusMap[assessmentInfo] != AssessmentPollResponse.Created)
                {
                    Semaphore.Release();
                    continue;
                }

                var taskToAdd = Task.Run(async () =>
                {
                    try
                    {
                        await PollAssessmentInParallel(assessmentInfo, userInputObj);
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                    finally
                    {
                        // Always release semaphore
                        Semaphore.Release();
                    }
                });

                Tasks.Add(taskToAdd);
            }

            // wait for all tasks to terminate, even if user cancelled the process
            Task.WaitAll(Tasks.ToArray());

            if (userInputObj.CancellationContext.IsCancellationRequested)
                UtilityFunctions.InitiateCancellation(userInputObj);

            userInputObj.LoggerObj.LogInformation("Assessment batch job completed");

            return AssessmentStatusMap;
        }

        private async Task PollAssessmentInParallel(AssessmentInformation assessmentInfo, UserInput userInputObj)
        {
            int numberOfTries = 0;

            while (numberOfTries < 25)
            {
                Thread.Sleep(60000);
                AssessmentPollResponse pollResult;
                try
                {
                    pollResult = await new HttpClientHelper().PollAssessment(userInputObj, assessmentInfo);

                    if (pollResult == AssessmentPollResponse.Error)
                    {
                        userInputObj.LoggerObj.LogInformation($"Polling for assessment {assessmentInfo.AssessmentName} resulted in non-retryable error");
                        numberOfTries += 1;
                    }

                    if (!AssessmentStatusMap.ContainsKey(assessmentInfo))
                        AssessmentStatusMap.Add(assessmentInfo, pollResult);
                    else
                        AssessmentStatusMap[assessmentInfo] = pollResult;
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (AggregateException aePollAssessment)
                {
                    string errorMessage = "";
                    foreach (var e in aePollAssessment.Flatten().InnerExceptions)
                    {
                        if (e is OperationCanceledException)
                            throw e;
                        else
                        {
                            errorMessage = errorMessage + e.Message + " ";
                        }
                    }
                    userInputObj.LoggerObj.LogWarning($"Assessment {assessmentInfo.AssessmentName} polling failed: {errorMessage}");
                }
                catch (Exception ex)
                {
                    userInputObj.LoggerObj.LogWarning($"Assessment {assessmentInfo.AssessmentName} polling failed: {ex.Message}");
                }

                if (AssessmentStatusMap[assessmentInfo] == AssessmentPollResponse.Completed ||
                    AssessmentStatusMap[assessmentInfo] == AssessmentPollResponse.OutDated ||
                    AssessmentStatusMap[assessmentInfo] == AssessmentPollResponse.Invalid)
                    break;
            }

            userInputObj.LoggerObj.LogInformation(1, $"Polling for assessment {assessmentInfo.AssessmentName} completed"); // 1 % increase for every polling completion. Total should be less than 65 %
        }
    }
}
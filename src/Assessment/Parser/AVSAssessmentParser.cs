// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

using Azure.Migrate.Explore.Common;
using Azure.Migrate.Explore.HttpRequestHelper;
using Azure.Migrate.Explore.Models;

namespace Azure.Migrate.Explore.Assessment.Parser
{
    public class AVSAssessmentParser
    {
        private readonly Dictionary<AssessmentInformation, AssessmentPollResponse> AVSAssessmentStatusMap;
        public AVSAssessmentParser(Dictionary<AssessmentInformation, AssessmentPollResponse> avsAssessmentStatusMap)
        {
            AVSAssessmentStatusMap = avsAssessmentStatusMap;
        }

        public void ParseAVSAssessment(Dictionary<AssessmentInformation, AVSAssessmentPropertiesDataset> AVSAssessmentsData, Dictionary<string, AVSAssessedMachinesDataset> AVSAssessedMachinesData, UserInput userInputObj)
        {
            if (userInputObj == null)
                throw new Exception("Received null user input object.");

            if (AVSAssessmentStatusMap == null)
            {
                userInputObj.LoggerObj.LogError($"AVS assessment status map is null, terminating parsing");
                return;
            }

            if (AVSAssessmentStatusMap.Count <= 0)
            {
                userInputObj.LoggerObj.LogError($"AVS assessment status map is empty, terminating parsing");
                return;
            }

            foreach (var kvp in AVSAssessmentStatusMap)
            {
                if (!UtilityFunctions.IsAssessmentCompleted(kvp))
                {
                    userInputObj.LoggerObj.LogWarning($"Skipping parsing assessment {kvp.Key.AssessmentName} as it is in {new EnumDescriptionHelper().GetEnumDescription(kvp.Value)} state");
                    continue;
                }

                userInputObj.LoggerObj.LogInformation($"Parsing AVS assessment {kvp.Key.AssessmentName}");

                string apiVersion = Routes.AssessmentMachineListApiVersion;
                if (kvp.Key.AssessmentType == AssessmentType.AVSAssessment)
                    apiVersion = Routes.AvsAssessmentApiVersion;

                string url = Routes.ProtocolScheme + Routes.AzureManagementApiHostname + Routes.ForwardSlash +
                             Routes.SubscriptionPath + Routes.ForwardSlash + userInputObj.Subscription.Key + Routes.ForwardSlash +
                             Routes.ResourceGroupPath + Routes.ForwardSlash + userInputObj.ResourceGroupName.Value + Routes.ForwardSlash +
                             Routes.ProvidersPath + Routes.ForwardSlash + Routes.MigrateProvidersPath + Routes.ForwardSlash +
                             Routes.AssessmentProjectsPath + Routes.ForwardSlash + userInputObj.AssessmentProjectName + Routes.ForwardSlash +
                             new EnumDescriptionHelper().GetEnumDescription(kvp.Key.AssessmentType) + Routes.ForwardSlash + kvp.Key.AssessmentName +
                             Routes.QueryStringQuestionMark +
                             Routes.QueryParameterApiVersion + Routes.QueryStringEquals + apiVersion;

                string summariesUrl = Routes.ProtocolScheme + Routes.AzureManagementApiHostname + Routes.ForwardSlash +
                             Routes.SubscriptionPath + Routes.ForwardSlash + userInputObj.Subscription.Key + Routes.ForwardSlash +
                             Routes.ResourceGroupPath + Routes.ForwardSlash + userInputObj.ResourceGroupName.Value + Routes.ForwardSlash +
                             Routes.ProvidersPath + Routes.ForwardSlash + Routes.MigrateProvidersPath + Routes.ForwardSlash +
                             Routes.AssessmentProjectsPath + Routes.ForwardSlash + userInputObj.AssessmentProjectName + Routes.ForwardSlash +
                             new EnumDescriptionHelper().GetEnumDescription(kvp.Key.AssessmentType) + Routes.ForwardSlash + kvp.Key.AssessmentName +
                             Routes.ForwardSlash + Routes.SummariesPath + Routes.QueryStringQuestionMark + Routes.QueryParameterApiVersion +
                             Routes.QueryStringEquals + apiVersion;

                string assessmentPropertiesResponse = "";
                string assessmentSummariesResponse = "";
                try
                {
                    assessmentPropertiesResponse = new HttpClientHelper().GetHttpRequestJsonStringResponse(url, userInputObj).Result;
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (AggregateException aeAVSProperties)
                {
                    string errorMessage = "";
                    foreach (var e in aeAVSProperties.Flatten().InnerExceptions)
                    {
                        if (e is OperationCanceledException)
                            throw e;
                        else
                        {
                            errorMessage = errorMessage + e.Message + " ";
                        }
                    }
                    userInputObj.LoggerObj.LogError($"Failed to retrieve AVS assessment properties for assessment: {errorMessage}");
                    url = "";
                    continue;
                }
                catch (Exception exAVSProperties)
                {
                    userInputObj.LoggerObj.LogError($"Failed to retrieve AVS assessment properties for assessment: {exAVSProperties.Message}");
                    url = "";
                    continue;
                }

                try
                {
                    assessmentSummariesResponse = new HttpClientHelper().GetHttpRequestJsonStringResponse(summariesUrl, userInputObj).Result;
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (AggregateException aeAVSSummaries)
                {
                    string errorMessage = "";
                    foreach (var e in aeAVSSummaries.Flatten().InnerExceptions)
                    {
                        if (e is OperationCanceledException)
                            throw e;
                        else
                        {
                            errorMessage = errorMessage + e.Message + " ";
                        }
                    }
                    userInputObj.LoggerObj.LogError($"Failed to retrieve AVS assessment summaries for assessment: {errorMessage}");
                    url = "";
                    continue;
                }
                catch (Exception exAVSSummaries)
                {
                    userInputObj.LoggerObj.LogError($"Failed to retrieve AVS assessment summaries for assessment: {exAVSSummaries.Message}");
                    url = "";
                    continue;
                }

                AVSAssessmentPropertiesJSON avsPropertiesObj = JsonConvert.DeserializeObject<AVSAssessmentPropertiesJSON>(assessmentPropertiesResponse);
                AVSAssessmentSummariesJSON avsSummariesObj = JsonConvert.DeserializeObject<AVSAssessmentSummariesJSON>(assessmentSummariesResponse);
                UpdateAVSPropertiesDataset(avsPropertiesObj, avsSummariesObj, AVSAssessmentsData, kvp.Key, userInputObj);

                url = Routes.ProtocolScheme + Routes.AzureManagementApiHostname + Routes.ForwardSlash +
                      Routes.SubscriptionPath + Routes.ForwardSlash + userInputObj.Subscription.Key + Routes.ForwardSlash +
                      Routes.ResourceGroupPath + Routes.ForwardSlash + userInputObj.ResourceGroupName.Value + Routes.ForwardSlash +
                      Routes.ProvidersPath + Routes.ForwardSlash + Routes.MigrateProvidersPath + Routes.ForwardSlash +
                      Routes.AssessmentProjectsPath + Routes.ForwardSlash + userInputObj.AssessmentProjectName + Routes.ForwardSlash +
                      new EnumDescriptionHelper().GetEnumDescription(kvp.Key.AssessmentType) + Routes.ForwardSlash + kvp.Key.AssessmentName + Routes.ForwardSlash +
                      Routes.AVSAssessedMachinesPath +
                      Routes.QueryStringQuestionMark +
                      Routes.QueryParameterApiVersion + Routes.QueryStringEquals + apiVersion;

                while (!string.IsNullOrEmpty(url))
                {
                    if (userInputObj.CancellationContext.IsCancellationRequested)
                        UtilityFunctions.InitiateCancellation(userInputObj);

                    string assessedMachinesResponse = "";
                    try
                    {
                        assessedMachinesResponse = new HttpClientHelper().GetHttpRequestJsonStringResponse(url, userInputObj).Result;
                    }
                    catch (OperationCanceledException)
                    {
                        throw;
                    }
                    catch (AggregateException aeAssessedMachines)
                    {
                        string errorMessage = "";
                        foreach (var e in aeAssessedMachines.Flatten().InnerExceptions)
                        {
                            if (e is OperationCanceledException)
                                throw e;
                            else
                            {
                                errorMessage = errorMessage + e.Message + " ";
                            }
                        }
                        userInputObj.LoggerObj.LogError($"Failed to retrieve machine data from assessment: {errorMessage}");
                        url = "";
                        continue;
                    }
                    catch (Exception exAVSAssessedMachines)
                    {
                        userInputObj.LoggerObj.LogError($"Failed to retrieve machine data from assessment: {exAVSAssessedMachines.Message}");
                        url = "";
                        continue;
                    }

                    AVSAssessedMachinesJSON obj = JsonConvert.DeserializeObject<AVSAssessedMachinesJSON>(assessedMachinesResponse);
                    url = obj.NextLink;

                    foreach (var value in obj.Values)
                    {
                        string key = value.Properties?.Linkages?.Find(x => x.LinkageType == "Source" && x.Kind == "Machine")?
                            .ArmId?.ToLower();
                        if (string.IsNullOrEmpty(key))
                            continue;
                        UpdateAVSAssessedMachinesDataset(AVSAssessedMachinesData, value, key, kvp.Key, avsSummariesObj?.Values[0]?.Properties?.SuitabilityExplanation);
                    }
                }
            }
        }

        private void UpdateAVSAssessedMachinesDataset(Dictionary<string, AVSAssessedMachinesDataset> AVSAssessedMachinesData, AVSAssessedMachineValue value, string key, AssessmentInformation assessmentInfo, string suitabilityExplanation)
        {
            if (AVSAssessedMachinesData.ContainsKey(key))
                return;

            AVSAssessedMachinesData.Add(key, new AVSAssessedMachinesDataset());

            AVSAssessedMachinesData[key].DisplayName = value?.Properties?.ExtendedDetails?.DisplayName;
            AVSAssessedMachinesData[key].DatacenterMachineArmId = value?.Properties?.Linkages?
                .Find(x => x.LinkageType == "Source" && x.Kind == "Machine")?.ArmId ?? "";
            AVSAssessedMachinesData[key].Suitability = value.Properties?.Recommendations[0]?.MigrationSuitability?.Readiness ?? "Unknown";
            AVSAssessedMachinesData[key].SuitabilityExplanation = suitabilityExplanation ?? "NotApplicable";
            AVSAssessedMachinesData[key].OperatingSystemName = value.Properties?.ExtendedDetails?.OperatingSystemName;
            AVSAssessedMachinesData[key].OperatingSystemVersion = value.Properties?.ExtendedDetails?.OperatingSystemVersion;
            AVSAssessedMachinesData[key].OperatingSystemArchitecture = value.Properties?.ExtendedDetails?.OperatingSystemArchitecture;
            AVSAssessedMachinesData[key].BootType = value.Properties?.ExtendedDetails?.BootType;
            AVSAssessedMachinesData[key].NumberOfCores = value.Properties?.ExtendedDetails?.NumberOfCores ?? 0;
            AVSAssessedMachinesData[key].MegabytesOfMemory = value.Properties?.ExtendedDetails?.MegabytesOfMemory ?? 0;
            AVSAssessedMachinesData[key].Disks = GetAssessedDiskList(value.Properties?.ExtendedDetails?.Disks);
            AVSAssessedMachinesData[key].StorageInUseGB = value.Properties?.ExtendedDetails?.StorageInUseGB ?? 0;
            AVSAssessedMachinesData[key].NetworkAdapters = value.Properties?.ExtendedDetails?.NetworkAdapters == null ? 0 : value.Properties?.ExtendedDetails?.NetworkAdapters.Count ?? 0;
            AVSAssessedMachinesData[key].NetworkAdapterList = GetAssessedNetworkAdapterList(value.Properties?.ExtendedDetails?.NetworkAdapters);
            AVSAssessedMachinesData[key].GroupName = assessmentInfo.GroupName;
        }

        private void UpdateAVSPropertiesDataset(AVSAssessmentPropertiesJSON avsPropertiesObj, AVSAssessmentSummariesJSON avsSummariesObj, Dictionary<AssessmentInformation, AVSAssessmentPropertiesDataset> AVSAssessmentsData, AssessmentInformation assessmentInfo, UserInput userInputObj)
        {
            if (AVSAssessmentsData.ContainsKey(assessmentInfo))
                return;

            AVSAssessmentsData.Add(assessmentInfo, new AVSAssessmentPropertiesDataset());

            string recommendedNodes = "";
            string recommendedFttRaidLevels = "";
            string nodeTypes = "";
            string recommendedExternalStorages = "";
            foreach (var item in avsSummariesObj?.Values[0]?.Properties?.AvsEstimatedNodes)
            {
                recommendedNodes += item.NodeNumber + " nodes of " + item.NodeType + ", ";
                string uppercaseFttRaidLevel = item.FttRaidLevel.ToUpper();
                recommendedFttRaidLevels += uppercaseFttRaidLevel.Substring(0, 3) + "-" + uppercaseFttRaidLevel.Substring(3, 1) + " & " +
                                            uppercaseFttRaidLevel.Substring(4, 4) + "-" + uppercaseFttRaidLevel.Substring(8, 1) + " on " +
                                            item.NodeType + ", ";
                nodeTypes += item.NodeType + ", ";
            }

            nodeTypes = nodeTypes.Substring(0, nodeTypes.Length - 2);
            recommendedNodes = recommendedNodes.Substring(0, recommendedNodes.Length - 2);
            recommendedFttRaidLevels = recommendedFttRaidLevels.Substring(0, recommendedFttRaidLevels.Length - 2);

            foreach (var item in avsSummariesObj?.Values[0]?.Properties?.AvsEstimatedExternalStorages)
            {
                string storageType = item.StorageType.Substring(0, 3).ToUpper() + "-" + item.StorageType.Substring(3);
                recommendedExternalStorages += item.TotalStorageInGB / 1024 + " TB of " + storageType + ", ";
            }

            if (recommendedExternalStorages.Length > 0)
            {
                recommendedExternalStorages = recommendedExternalStorages.Substring(0, recommendedExternalStorages.Length - 2);
            }

            AVSAssessmentsData[assessmentInfo].SubscriptionId = userInputObj.Subscription.Key;
            AVSAssessmentsData[assessmentInfo].ResourceGroup = userInputObj.ResourceGroupName.Value;
            AVSAssessmentsData[assessmentInfo].AssessmentProjectName = userInputObj.AssessmentProjectName;
            AVSAssessmentsData[assessmentInfo].GroupName = assessmentInfo.GroupName;
            AVSAssessmentsData[assessmentInfo].AssessmentName = avsPropertiesObj.Name;
            AVSAssessmentsData[assessmentInfo].SizingCriterion = new EnumDescriptionHelper().GetEnumDescription(assessmentInfo.AssessmentTag);
            AVSAssessmentsData[assessmentInfo].CreatedOn = avsPropertiesObj.Properties.Details?.CreatedTimestamp;
            var sourceWithTypeMachine = avsSummariesObj?.Values[0]?.Properties?.Sources?.Find(x => x.SourceType == "Machine");
            AVSAssessmentsData[assessmentInfo].TotalMachinesAssessed = sourceWithTypeMachine?.Count ?? 0;
            var readinessSummary = avsSummariesObj?.Values[0]?.Properties?.TargetSourceMapping[0]?.MigrationDetails?.ReadinessSummary;
            var suitableSummary = readinessSummary?.Find(x => x.Name == "Suitable")?.Value ?? 0;
            var conditionallySuitableSummary = readinessSummary?.Find(x => x.Name == "ConditionallySuitable")?.Value ?? 0;
            var readinessUnknownSummary = readinessSummary?.Find(x => x.Name == "ReadinessUnknown")?.Value ?? 0;
            var notSuitableSummary = readinessSummary?.Find(x => x.Name == "NotSuitable")?.Value ?? 0;
            AVSAssessmentsData[assessmentInfo].MachinesReady = suitableSummary;
            AVSAssessmentsData[assessmentInfo].MachinesReadyWithConditions = conditionallySuitableSummary;
            AVSAssessmentsData[assessmentInfo].MachinesReadinessUnknown = readinessUnknownSummary;
            AVSAssessmentsData[assessmentInfo].MachinesNotReady = notSuitableSummary;
            AVSAssessmentsData[assessmentInfo].TotalRecommendedNumberOfNodes = avsSummariesObj?.Values[0]?.Properties?.NumberOfNodes ?? 0;
            AVSAssessmentsData[assessmentInfo].NodeTypes = nodeTypes;
            AVSAssessmentsData[assessmentInfo].RecommendedNodes = recommendedNodes;
            AVSAssessmentsData[assessmentInfo].RecommendedFttRaidLevels = recommendedFttRaidLevels;
            AVSAssessmentsData[assessmentInfo].RecommendedExternalStorage = recommendedExternalStorages;
            var costComponentsWithTotalMonthlyCost = new List<CostComponent>(avsSummariesObj?.Values[0]?.Properties?.CostComponents?.FindAll(x => x.CostDetail.Exists(y => y.Name == "TotalMonthlyCost")) ?? new List<CostComponent>());
            // in each of the cost components, get the cost detail with name "TotalMonthlyCost" and get its value and add them up
            double? totalMonthlyCost = 0.00;
            foreach (var costComponent in costComponentsWithTotalMonthlyCost)
            {
                var totalMonthlyCostDetail = costComponent.CostDetail?.Find(x => x.Name == "TotalMonthlyCost")?.Value;
                if (totalMonthlyCostDetail != null)
                {
                    totalMonthlyCost += totalMonthlyCostDetail ?? 0.00;
                }
            }
            var costComponentsWithTotalMonthlyAvsNodeCost = new List<CostComponent>(avsSummariesObj?.Values[0]?.Properties?.CostComponents?.FindAll(x => x.CostDetail.Exists(y => y.Name == "MonthlyAvsNodeCost")) ?? new List<CostComponent>());
            // in each of the cost components, get the cost detail with name "TotalMonthlyCost" and get its value and add them up
            double? totalMonthlyAvsNodeCost = 0.00;
            foreach (var costComponent in costComponentsWithTotalMonthlyAvsNodeCost)
            {
                var totalMonthlyCostDetail = costComponent.CostDetail?.Find(x => x.Name == "MonthlyAvsNodeCost")?.Value;
                if (totalMonthlyCostDetail != null)
                {
                    totalMonthlyAvsNodeCost += totalMonthlyCostDetail ?? 0.00;
                }
            }
            var costComponentsWithTotalMonthlyExternalStorageCost = new List<CostComponent>(avsSummariesObj?.Values[0]?.Properties?.CostComponents?.FindAll(x => x.CostDetail.Exists(y => y.Name == "MonthlyAvsExternalStorageCost")) ?? new List<CostComponent>());
            // in each of the cost components, get the cost detail with name "TotalMonthlyCost" and get its value and add them up
            double? totalMonthlyExternalStorageCost = 0.00;
            foreach (var costComponent in costComponentsWithTotalMonthlyExternalStorageCost)
            {
                var totalMonthlyCostDetail = costComponent.CostDetail?.Find(x => x.Name == "MonthlyAvsExternalStorageCost")?.Value;
                if (totalMonthlyCostDetail != null)
                {
                    totalMonthlyExternalStorageCost += totalMonthlyCostDetail ?? 0.00;
                }
            }
            var costComponentsWithTotalMonthlyExternalNetworkCost = new List<CostComponent>(avsSummariesObj?.Values[0]?.Properties?.CostComponents?.FindAll(x => x.CostDetail.Exists(y => y.Name == "MonthlyAvsNetworkCost")) ?? new List<CostComponent>());
            // in each of the cost components, get the cost detail with name "TotalMonthlyCost" and get its value and add them up
            double? totalMonthlyExternalNetworkCost = 0.00;
            foreach (var costComponent in costComponentsWithTotalMonthlyExternalNetworkCost)
            {
                var totalMonthlyCostDetail = costComponent.CostDetail?.Find(x => x.Name == "MonthlyAvsNetworkCost")?.Value;
                if (totalMonthlyCostDetail != null)
                {
                    totalMonthlyExternalNetworkCost += totalMonthlyCostDetail ?? 0.00;
                }
            }
            AVSAssessmentsData[assessmentInfo].TotalMonthlyCostEstimate = totalMonthlyCost ?? 0.00;
            AVSAssessmentsData[assessmentInfo].TotalExternalStorageCost = totalMonthlyExternalStorageCost ?? 0.00;
            AVSAssessmentsData[assessmentInfo].TotalNodeCost = totalMonthlyAvsNodeCost ?? 0.00;
            AVSAssessmentsData[assessmentInfo].TotalExternalNetworkCost = totalMonthlyExternalNetworkCost ?? 0.00;
            AVSAssessmentsData[assessmentInfo].PredictedCpuUtilizationPercentage = avsSummariesObj?.Values[0]?.Properties?.CpuUtilization ?? 0.00;
            AVSAssessmentsData[assessmentInfo].PredictedMemoryUtilizationPercentage = avsSummariesObj?.Values[0]?.Properties?.RamUtilization ?? 0.00;
            AVSAssessmentsData[assessmentInfo].PredictedStorageUtilizationPercentage = avsSummariesObj?.Values[0]?.Properties?.StorageUtilization ?? 0.00;
            AVSAssessmentsData[assessmentInfo].NumberOfCpuCoresAvailable = avsSummariesObj?.Values[0]?.Properties?.TotalCpuCores ?? 0.00;
            AVSAssessmentsData[assessmentInfo].MemoryInTBAvailable = avsSummariesObj?.Values[0]?.Properties?.TotalRamInGB / 1024.0 ?? 0.00;
            AVSAssessmentsData[assessmentInfo].StorageInTBAvailable = avsSummariesObj?.Values[0]?.Properties?.TotalStorageInGB / 1024.0 ?? 0.00;
            AVSAssessmentsData[assessmentInfo].NumberOfCpuCoresUsed = Math.Ceiling(AVSAssessmentsData[assessmentInfo].NumberOfCpuCoresAvailable * AVSAssessmentsData[assessmentInfo].PredictedCpuUtilizationPercentage / 100.0);
            AVSAssessmentsData[assessmentInfo].MemoryInTBUsed = AVSAssessmentsData[assessmentInfo].MemoryInTBAvailable * AVSAssessmentsData[assessmentInfo].PredictedMemoryUtilizationPercentage / 100.0;
            AVSAssessmentsData[assessmentInfo].StorageInTBUsed = AVSAssessmentsData[assessmentInfo].StorageInTBAvailable * AVSAssessmentsData[assessmentInfo].PredictedStorageUtilizationPercentage / 100.0;
            AVSAssessmentsData[assessmentInfo].NumberOfCpuCoresFree = AVSAssessmentsData[assessmentInfo].NumberOfCpuCoresAvailable - AVSAssessmentsData[assessmentInfo].NumberOfCpuCoresUsed;
            AVSAssessmentsData[assessmentInfo].MemoryInTBFree = AVSAssessmentsData[assessmentInfo].MemoryInTBAvailable - AVSAssessmentsData[assessmentInfo].MemoryInTBUsed;
            AVSAssessmentsData[assessmentInfo].StorageInTBFree = AVSAssessmentsData[assessmentInfo].StorageInTBAvailable - AVSAssessmentsData[assessmentInfo].StorageInTBUsed;
            AVSAssessmentsData[assessmentInfo].ConfidenceRating = UtilityFunctions.GetConfidenceRatingInStars(avsPropertiesObj.Properties?.Details?.ConfidenceRatingInPercentage ?? 0);

            AvsAssessmentConstants.VCpuOversubscription = (avsPropertiesObj.Properties.VCpuOversubscription ?? 4).ToString() + ":1";
            AvsAssessmentConstants.DedupeCompression = avsPropertiesObj.Properties.DedupeCompression ?? 1.5;
        }

        #region Utilities
        private List<AssessedDisk> GetAssessedDiskList(List<AVSAssessedMachineDisk> disks)
        {
            List<AssessedDisk> diskList = new List<AssessedDisk>();

            foreach (var kvp in disks)
            {
                AssessedDisk obj = new AssessedDisk();
                obj.DisplayName = kvp.DisplayName;
                obj.GigabytesProvisioned = kvp.GigabytesProvisioned;
                obj.MegabytesPerSecondOfRead = kvp.MegabytesPerSecondOfRead;
                obj.MegabytesPerSecondOfWrite = kvp.MegabytesPerSecondOfWrite;
                obj.NumberOfReadOperationsPerSecond = kvp.NumberOfReadOperationsPerSecond;
                obj.NumberOfWriteOperationsPerSecond = kvp.NumberOfWriteOperationsPerSecond;

                diskList.Add(obj);
            }

            return diskList;
        }

        private List<AssessedNetworkAdapter> GetAssessedNetworkAdapterList(List<AVSAssessedMachineNetworkAdapter> networkAdapters)
        {
            List<AssessedNetworkAdapter> networkAdapterList = new List<AssessedNetworkAdapter>();

            foreach (var kvp in networkAdapters)
            {
                AssessedNetworkAdapter obj = new AssessedNetworkAdapter();
                obj.DisplayName = kvp.DisplayName;
                obj.MacAddress = kvp.MacAddress;
                obj.IpAddresses = kvp.IpAddresses;
                obj.MegabytesPerSecondReceived = kvp.MegabytesPerSecondReceived;
                obj.MegaytesPerSecondTransmitted = kvp.MegabytesPerSecondTransmitted;

                networkAdapterList.Add(obj);
            }

            return networkAdapterList;
        }
        #endregion
    }
}
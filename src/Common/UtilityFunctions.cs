// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using ClosedXML.Excel;
using System;
using System.IO;
using System.Collections.Generic;

using Azure.Migrate.Explore.Forex;
using Azure.Migrate.Explore.Models;
using Azure.Migrate.Explore.Models.CopilotSummary;
using Azure.Migrate.Explore.Models.CopilotSummary.MigrationSummary;
using System.Reflection;
using System.ComponentModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;
using Orionsoft.MarkdownToPdfLib;

namespace Azure.Migrate.Explore.Common
{
    public static class UtilityFunctions
    {
        private static string selectedDirectory = "";

        public static void SetSelectedDirectory(string directory)
        {
            selectedDirectory = directory;
        }

        public static T FindFirstExceptionOfType<T>(Exception e)
            where T : Exception
        {
            if (e == null)
            {
                return null;
            }

            Stack<Exception> stack = new Stack<Exception>();
            stack.Push(e);

            while (stack.Count != 0)
            {
                var ex = stack.Pop();
                T retval = ex as T;

                if (retval != null)
                {
                    return retval;
                }

                if (ex.InnerException != null)
                {
                    stack.Push(ex.InnerException);
                }
            }

            return null;
        }

        public static Exception FindFirstExceptionOfType(Exception ex)
        {
            Exception result = ex;
            while (result.InnerException != null)
            {
                result = result.InnerException;
            }

            return result;
        }

        public static void InitiateCancellation(UserInput userInputObj)
        {
            userInputObj.LoggerObj.LogInformation("Initiating process termination upon user request");
            userInputObj.CancellationContext.Token.ThrowIfCancellationRequested();
        }

        public static void InitiateCopilotCancellation(CopilotInput copilotInputObj)
        {
            copilotInputObj.LoggerObj.LogInformation("Initiating process termination upon user request");
            copilotInputObj.CancellationContext.Token.ThrowIfCancellationRequested();
        }

        public static string PrependErrorLogType()
        {
            return LoggerConstants.ErrorLogTypePrefix + LoggerConstants.LogTypeMessageSeparator;
        }

        public static bool IsAssessmentCompleted(KeyValuePair<AssessmentInformation, AssessmentPollResponse> assessmentInfo)
        {
            return (assessmentInfo.Value == AssessmentPollResponse.Completed ||
                    assessmentInfo.Value == AssessmentPollResponse.OutDated);
        }

        public static double GetAzureBackupMonthlyCostEstimate(List<AssessedDisk> disks, string currencySymbol)
        {
            double exRate = ForexConstants.Instance.GetExchangeRate(currencySymbol);
            double totalDiskStorage = 0;

            foreach (var disk in disks)
                totalDiskStorage += disk.GigabytesProvisioned;

            double storageCost = totalDiskStorage * 3.38 * 0.0224 * exRate;
            double backupCost = exRate;

            if (totalDiskStorage <= 50)
                backupCost *= 5;
            else if (totalDiskStorage > 50 && totalDiskStorage <= 500)
                backupCost *= 10;
            else if (totalDiskStorage > 500)
                backupCost *= Math.Ceiling(totalDiskStorage / 500) * 10;

            return backupCost + storageCost;
        }

        public static double GetAzureSiteRecoveryMonthlyCostEstimate(string currencySymbol)
        {
            return 25.0 * ForexConstants.Instance.GetExchangeRate(currencySymbol);
        }

        public static string GetConfidenceRatingInStars(double confidenceRatingInPercentage)
        {
            string result = "";

            if (confidenceRatingInPercentage <= 20)
                result = "1 Star";
            if (confidenceRatingInPercentage > 20 && confidenceRatingInPercentage <= 40)
                result = "2 Stars";
            if (confidenceRatingInPercentage > 40 && confidenceRatingInPercentage <= 60)
                result = "3 Stars";
            if (confidenceRatingInPercentage > 60 && confidenceRatingInPercentage <= 80)
                result = "4 Stars";
            if (confidenceRatingInPercentage > 80)
                result = "5 Stars";

            return result;
        }

        public static string GetStringValue(string value)
        {
            if (string.IsNullOrEmpty(value))
                return "";
            return value;
        }

        public static double GetTotalStorage(List<AssessedDisk> disks)
        {
            double total = 0;

            foreach (var disk in disks)
                total += disk.GigabytesProvisioned;

            return total;
        }

        public static KeyValuePair<string, string> ParseMacIpAddress(List<AssessedNetworkAdapter> networkAdapters)
        {
            string ipAddresses = "";
            string macAddresses = "";

            foreach (var networkAdapter in networkAdapters)
            {
                macAddresses = macAddresses + "[" +networkAdapter.MacAddress + "];";
                ipAddresses = ipAddresses + "[" + string.Join(",", networkAdapter.IpAddresses) + "];";
            }

            return new KeyValuePair<string, string>(macAddresses, ipAddresses);
        }

        public static string GetDiskNames(List<AssessedDisk> disks)
        {
            string diskNames = "";

            foreach (var disk in disks)
                diskNames = diskNames + disk.DisplayName + ";";

            return diskNames;
        }

        public static string GetDiskReadiness(List<AssessedDisk> disks)
        {
            string diskReadiness = "";

            foreach (var disk in disks)
                diskReadiness = diskReadiness + new EnumDescriptionHelper().GetEnumDescription(disk.Suitability) + ";";

            return diskReadiness;
        }

        public static string GetRecommendedDiskSKUs(List<AssessedDisk> disks)
        {
            string skus = "";

            foreach (var disk in disks)
                skus = skus + disk.RecommendedDiskSku + ";";

            return skus;
        }

        public static int GetDiskTypeCount(List<AssessedDisk> disks, RecommendedDiskTypes type)
        {
            int count = 0;

            foreach (var disk in disks)
            {
                if (disk.DiskType == type)
                    count += 1;
            }

            return count;
        }

        public static double GetDiskTypeStorageCost(List<AssessedDisk> disks, RecommendedDiskTypes type)
        {
            double cost = 0;

            foreach (var disk in disks)
                if (disk.DiskType == type)
                    cost += disk.DiskCost;

            return cost;
        }

        public static double GetDiskReadInOPS(List<AssessedDisk> disks)
        {
            double value = 0;
            foreach (var disk in disks)
                value += disk.NumberOfReadOperationsPerSecond;
            
            return value;
        }

        public static double GetDiskWriteInOPS(List<AssessedDisk> disks)
        {
            double value = 0;
            foreach (var disk in disks)
                value += disk.NumberOfWriteOperationsPerSecond;
            
            return value;
        }

        public static double GetDiskReadInMBPS(List<AssessedDisk> disks)
        {
            double value = 0;
            foreach (var disk in disks)
                value += disk.MegabytesPerSecondOfRead;
            
            return value;
        }

        public static double GetDiskWriteInMBPS(List<AssessedDisk> disks)
        {
            double value = 0;
            foreach (var disk in disks)
                value += disk.MegabytesPerSecondOfWrite;
            
            return value;
        }

        public static double GetNetworkInMBPS(List<AssessedNetworkAdapter> networkAdapters)
        {
            double value = 0;
            foreach (var networkAdapter in networkAdapters)
                value += networkAdapter.MegabytesPerSecondReceived;
            return value;
        }

        public static double GetNetworkOutMBPS(List<AssessedNetworkAdapter> networkAdapters)
        {
            double value = 0;
            foreach (var networkAdapter in networkAdapters)
                value += networkAdapter.MegaytesPerSecondTransmitted;
            
            return value;
        }

        public static string GetMigrationIssueWarnings(List<AssessedMigrationIssue> migrationIssues)
        {
            string value = "";
            foreach (var migrationIssue in migrationIssues)
                if (migrationIssue.IssueCategory == IssueCategories.Warning)
                    value = value + migrationIssue.IssueId + ";";
            
            return value;
        }

        public static string GetMigrationIssueByType(List<AssessedMigrationIssue> migrationIssues, IssueCategories category)
        {
            string value = "";
            foreach (var migrationIssue in migrationIssues)
                if (migrationIssue.IssueCategory == category)
                    value = value + migrationIssue.IssueId + ";";
            
            return value;
        }

        public static double GetBusinessCaseTotalOsLicensingCost(List<BusinessCaseOsLicensingDetail> details)
        {
            double total = 0;
            foreach (var detail in details)
                total += detail.TotalCost;

            return total;
        }

        public static double GetBusinessCaseTotalPaaSLicensingCost(List<BusinessCaseOnPremisesPaaSLicensingCost> details)
        {
            double total = 0;
            foreach (var detail in details)
                total += detail.TotalCost;

            return total;
        }

        public static string GetSQLMIConfiguration(AzureSQLInstanceDataset azureSqlInstance)
        {
            string value = "";

            string serviceTier = GetStringValue(azureSqlInstance.AzureSQLMISkuServiceTier);
            string computeTier = GetStringValue(azureSqlInstance.AzureSQLMISkuComputeTier);
            string hardwareGeneration = GetStringValue(azureSqlInstance.AzureSQLMISkuHardwareGeneration);
            int cores = azureSqlInstance.AzureSQLMISkuCores;
            double storageMaxSizeInGB = Math.Round(azureSqlInstance.AzureSQLMISkuStorageMaxSizeInMB / 1024.0);
            
            value = serviceTier + "," +
                    computeTier + "," +
                    hardwareGeneration + "," +
                    cores.ToString() + "vCore," +
                    storageMaxSizeInGB + " GB Storage";

            return value;
        }

        public static void AddColumnHeadersToWorksheet(IXLWorksheet sheet, List<string> columns)
        {
            for (int i = 0; i < columns.Count; i++)
                sheet.Cell(1, i + 1).Value = columns[i];
        }

        public static void AddNewColumnToEnd(IXLWorksheet sheet, string newColumnName)
        {
            // Get the next available column (based on row 1)
            int lastCol = sheet.Row(1).LastCellUsed()?.Address.ColumnNumber ?? 0;
            int nextCol = lastCol + 1;

            sheet.Cell(1, nextCol).Value = newColumnName;
        }


        /// <summary>
        /// Saves ARG (Azure Resource Graph) JSON response to an Excel worksheet.
        /// This method specifically handles ARG responses with a "data" root element.
        /// </summary>
        /// <param name="argJsonResponse">ARG JSON response containing a "data" array</param>
        /// <param name="worksheet">Excel worksheet to populate</param>
        public static void SaveARGJsonDataToWorksheet(string argJsonResponse, IXLWorksheet worksheet)
        {
            SaveJsonDataToWorksheet(argJsonResponse, worksheet, "data");
        }

        /// <summary>
        /// Saves JSON data to an Excel worksheet by flattening the JSON structure
        /// </summary>
        /// <param name="jsonResponse">JSON response containing an array of objects</param>
        /// <param name="worksheet">Excel worksheet to populate</param>
        /// <param name="rootElementName">Root element name containing the array data</param>
        public static void SaveJsonDataToWorksheet(string jsonResponse, IXLWorksheet worksheet, string rootElementName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(jsonResponse))
                {
                    Console.WriteLine("JSON response is empty or null");
                    return;
                }

                if (string.IsNullOrWhiteSpace(rootElementName))
                {
                    Console.WriteLine("Root element name is required");
                    return;
                }

                JToken parsedJson = JToken.Parse(jsonResponse);
                JArray dataArray = null;

                // Handle case where the entire JSON is already an array
                if (parsedJson is JArray directArray)
                {
                    dataArray = directArray;
                }
                else if (parsedJson is JObject jsonObj)
                {
                    dataArray = jsonObj[rootElementName] as JArray;
                    if (dataArray == null)
                    {
                        Console.WriteLine($"Root element '{rootElementName}' not found or is not an array");
                        return;
                    }
                }

                if (dataArray == null || dataArray.Count == 0)
                {
                    Console.WriteLine("No array data found in JSON response");
                    return;
                }

                WriteJsonArrayToWorksheetWithFlattening(dataArray, worksheet);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error exporting JSON to Excel: {ex.Message}");
            }
        }

        /// <summary>
        /// Writes a JArray to an Excel worksheet by flattening nested JSON objects into columns with dot-notation headers.
        /// Each object in the array becomes a row, with nested properties flattened into separate columns.
        /// </summary>
        /// <param name="jsonArray">JArray containing JSON objects to export</param>
        /// <param name="worksheet">Excel worksheet to populate with headers and data</param>
        private static void WriteJsonArrayToWorksheetWithFlattening(JArray jsonArray, IXLWorksheet worksheet)
        {
            if (jsonArray == null || jsonArray.Count == 0)
            {
                Console.WriteLine("No data found in JSON array");
                return;
            }

            // Flatten the first object to determine all possible column names
            var firstItem = jsonArray[0] as JObject;
            if (firstItem != null)
            {
                var flattenedColumns = FlattenJObject(firstItem);
                var columnNames = flattenedColumns.Keys.ToList();

                // Write column headers to the first row
                for (int i = 0; i < columnNames.Count; i++)
                {
                    worksheet.Cell(1, i + 1).Value = columnNames[i];
                }

                // Write data rows starting from row 2
                for (int rowIndex = 0; rowIndex < jsonArray.Count; rowIndex++)
                {
                    var item = jsonArray[rowIndex] as JObject;
                    if (item != null)
                    {
                        var flattenedData = FlattenJObject(item);

                        for (int colIndex = 0; colIndex < columnNames.Count; colIndex++)
                        {
                            var columnName = columnNames[colIndex];
                            var cellValue = flattenedData.ContainsKey(columnName) ? flattenedData[columnName] : "";
                            worksheet.Cell(rowIndex + 2, colIndex + 1).Value = cellValue;
                        }
                    }
                }

                // Auto-fit columns for better readability
                worksheet.Columns().AdjustToContents();
            }
        }

        /// <summary>
        /// Recursively flattens a JObject into a dictionary with dot-notation keys
        /// </summary>
        /// <param name="obj">JObject to flatten</param>
        /// <param name="prefix">Current prefix for nested properties</param>
        /// <returns>Flattened dictionary</returns>
        private static Dictionary<string, string> FlattenJObject(JObject obj, string prefix = "")
        {
            var result = new Dictionary<string, string>();
            
            foreach (var property in obj.Properties())
            {
                var key = string.IsNullOrEmpty(prefix) ? property.Name : $"{prefix}.{property.Name}";
                
                if (property.Value.Type == JTokenType.Object)
                {
                    // Recursively flatten nested objects
                    var nestedObj = property.Value as JObject;
                    if (nestedObj != null)
                    {
                        var nestedFlattened = FlattenJObject(nestedObj, key);
                        foreach (var kvp in nestedFlattened)
                        {
                            result[kvp.Key] = kvp.Value;
                        }
                    }
                }
                else if (property.Value.Type == JTokenType.Array)
                {
                    var array = property.Value as JArray;
                    if (array != null)
                    {
                        // For arrays, either flatten each object or create indexed entries
                        if (array.Count > 0 && array[0].Type == JTokenType.Object)
                        {
                            // Array of objects - flatten each with index
                            for (int i = 0; i < array.Count; i++)
                            {
                                var arrayObj = array[i] as JObject;
                                if (arrayObj != null)
                                {
                                    var arrayFlattened = FlattenJObject(arrayObj, $"{key}[{i}]");
                                    foreach (var kvp in arrayFlattened)
                                    {
                                        result[kvp.Key] = kvp.Value;
                                    }
                                }
                            }
                        }
                        else
                        {
                            // Array of primitives - join as comma-separated string
                            var values = array.Select(token => token.ToString()).ToArray();
                            result[key] = string.Join(", ", values);
                        }
                    }
                }
                else
                {
                    // Simple value
                    result[key] = property.Value?.ToString() ?? "";
                }
            }
            
            return result;
        }

        public static double GetSecurityCost(List<AzureAssessmentCostComponent> costComponents)
        {
            foreach (var component in costComponents)
                if (component.Name.Equals("MonthlySecurityCost"))
                    return component.Value;

            return 0;
        }

        public static void ValidateReportPresence(string directoryPath, string filePath)
        {
            if (!Directory.Exists(directoryPath))
                throw new Exception($"Report directory {directoryPath} not found.");
            if (!File.Exists(filePath))
                throw new Exception($"Report file {filePath} not found");
        }

        public static bool IsReportPresentForTabVisibility(string directoryPath, string filePath)
        {
            try
            {
                ValidateReportPresence(directoryPath, filePath);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static string GetReportsDirectory()
        {
            return selectedDirectory;
        }

        public static bool CheckCopilotQuestionnaireTabVisibility()
        {
            string reportsPath = Path.Combine(AppContext.BaseDirectory, selectedDirectory);
            bool isDiscoveryReportPresent = IsReportPresentForTabVisibility(
                reportsPath,
                selectedDirectory + "\\" +
                DiscoveryReportConstants.DiscoveryReportName
            );

            bool isCoreReportPresent = IsReportPresentForTabVisibility(
                reportsPath,
                selectedDirectory + "\\" +
                CoreReportConstants.CoreReportName
            );

            bool isOpportunityReportPresent = IsReportPresentForTabVisibility(
                reportsPath,
                selectedDirectory + "\\" +
                OpportunityReportConstants.OpportunityReportName
            );

            return (isDiscoveryReportPresent && isCoreReportPresent && isOpportunityReportPresent);
        }

        public static List<MigrationDataProperty> FetchProperties(Type dataClassType, object dataClassInstance)
        {
            var properties = dataClassType.GetProperties();
            var copilotProperties = new List<MigrationDataProperty>();
            foreach (var property in properties)
            {
                copilotProperties.Add(new MigrationDataProperty
                {
                    Label = GetLabel(property) ?? property.Name,
                    Value = property.GetValue(dataClassInstance)?.ToString()
                });
            }
            return copilotProperties;
        }

        public static string GetLabel(PropertyInfo property)
        {
            var attribute = (DescriptionAttribute)Attribute.GetCustomAttribute(property, typeof(DescriptionAttribute));
            return attribute?.Description;
        }

        public static class SaveAmeSummary
        {
            public static void PersistResponse(string response)
            {
                if (!Directory.Exists(SummaryConstants.SummaryDirectory))
                {
                    Directory.CreateDirectory(SummaryConstants.SummaryDirectory);
                }
                string migrateDataPath = Path.Combine(
                    SummaryConstants.SummaryDirectory,
                    $"{SummaryConstants.SummaryPath}_{DateTime.Now:yyyy_MM_dd_HH_mm_ss}.md"
                );

                // Pretty-print the JSON
                var parsedJson = JsonConvert.DeserializeObject<Dictionary<string, string>>(response);
                string readableString = parsedJson["Result"];
                // var prettyPrintedJson = JsonConvert.SerializeObject(parsedJson, Formatting.Indented);

                // Write the JSON to a .md file
                File.WriteAllText(migrateDataPath, readableString);

                string pdfPath = Path.Combine(
                    SummaryConstants.SummaryDirectory,
                    $"{SummaryConstants.SummaryPath}_{DateTime.Now:yyyy_MM_dd_HH_mm_ss}.pdf"
                );

                var pdf = new MarkdownToPdf();
                pdf
                 .PaperSize(Orionsoft.MarkdownToPdfLib.PaperSize.A4)
                 .DefalutDpi(200)
                 .Title("AzureMigrateExplore Insights")
                 .DefaultFont("Calibri", 12)
                 .PageMargins("1cm", "1cm", "1cm", "1.5cm")
                 .Add(readableString)
                 .Save(pdfPath);

            }           
        }
    }
}
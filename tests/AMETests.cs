// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Windows.UI.Xaml.Automation;
using OpenQA.Selenium.Appium.Windows;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Support.UI;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Linq;

namespace AzureMigrateExplore.Tests
{
    [TestClass]
    public class AMETests
    {
        private const string WindowsApplicationDriverUrl = "http://127.0.0.1:4723";
        
        private string AppPath = AppContext.BaseDirectory;
        private string AppDirectory;
        private string AppId;
        private string AppExecutableDirectory;
        private static ILogger _logger;

        private WindowsDriver<WindowsElement> session;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            // Setup logging
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder
                    .SetMinimumLevel(LogLevel.Information);
            });

            _logger = loggerFactory.CreateLogger<AMETests>();
            _logger.LogInformation("Starting Export Summary Button Tests");
        }

        private string GetBuildConfiguration()
        {
            #if DEBUG
                return "Debug";
            #elif RELEASE
                return "Release";
            #else
                return "Debug"; // Default to Debug if configuration is neither Debug nor Release
            #endif
        }

        [TestInitialize]
        public void TestInitialize()
        {
            string configuration = GetBuildConfiguration();
            AppExecutableDirectory = FindAppExecutablePath(configuration);

            if (session == null)
            {
                DesiredCapabilities appCapabilities = new DesiredCapabilities();
                appCapabilities.SetCapability("app", AppExecutableDirectory);
                appCapabilities.SetCapability("deviceName", "WindowsPC");

                session = new WindowsDriver<WindowsElement>(new Uri("http://127.0.0.1:4723"), appCapabilities);

                Thread.Sleep(2000); // Wait for the app to load
            }
        }

        [TestMethod]
        public void TestExportSummaryButton()
        {
            try
            {
                _logger.LogInformation("Starting Export Summary test");

                // Step 1: Login
                _logger.LogInformation("Clicking login button");
                WindowsElement element = session.FindElementByAccessibilityId("LoginButton");
                element.Click();

                Thread.Sleep(2000); // Wait for the login button to be clickable

                Thread.Sleep(5000); // Wait for the app to load

                // Step 2: Fill customer information
                _logger.LogInformation("Entering customer information");
                WindowsElement customerName = session.FindElementByAccessibilityId("CustomerNameTextBox");
                customerName.SendKeys("TestCustomer");

                WindowsElement customerIndustry = session.FindElementByAccessibilityId("CustomerIndustryTextBox");
                customerIndustry.SendKeys("TestIndustry");

                //Step 3: Enter DataCenter Location
                _logger.LogInformation("Entering DataCenter location");
                WindowsElement dataCenterLocation = session.FindElementByAccessibilityId("DatacenterLocationTextBox");
                dataCenterLocation.SendKeys("India");

                // Step 4: Select motivation
                _logger.LogInformation("Selecting motivation for migration");
                WindowsElement motivationForMigration = session.FindElementByAccessibilityId("MotivationComboBox");
                motivationForMigration.Click();

                WindowsElement motivationForMigrationOption = session.FindElementByName("Other");
                motivationForMigrationOption.Click();

                // Step 5: Check the AI Consent Box
                _logger.LogInformation("Checking the AI Consent Box");
                WindowsElement aiConsentBox = session.FindElementByAccessibilityId("AIAgreementCheckBox");
                aiConsentBox.Click();

                // Step 6: Generate summary
                _logger.LogInformation("Generating summary");
                WindowsElement generateSummaryButton = session.FindElementByAccessibilityId("GenerateSummaryButton");
                generateSummaryButton.Click();

                // Wait for summary generation
                _logger.LogInformation("Waiting for summary generation to complete");
                Thread.Sleep(3 * 60 * 1000);

                // Step 7: Export summary
                _logger.LogInformation("Exporting summary");
                WindowsElement exportButton = session.FindElementByAccessibilityId("ExportSummaryButton");
                exportButton.Click();

                // Wait for export operation to complete
                Thread.Sleep(3000);

                // Step 8: Verify export was successful
                _logger.LogInformation("Verifying export was successful");
                string summaryDirectory = Path.Combine(AppExecutableDirectory, "Summary");
                var files = Directory.GetFiles(summaryDirectory, "AzureMigrateExploreInsights_*.pdf");

                // Step 9: Confirm and close dialog
                _logger.LogInformation("Confirming export dialog");
                WindowsElement exportDialog = session.FindElementByName("Export Successful");
                Thread.Sleep(2000);

                WindowsElement okButton = session.FindElementByName("OK");
                okButton.Click();

                _logger.LogInformation("Export Summary test completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Test failed");
                throw;
            }
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _logger.LogInformation("Cleaning up test resources");

            try
            {
                if (session != null)
                {
                    session.Quit();
                    session = null;

                    // Clean up any test files
                    string testAccessTokenFilePath = Path.Combine(AppExecutableDirectory, "testAccessToken.txt");
                    if (File.Exists(testAccessTokenFilePath))
                    {
                        File.Delete(testAccessTokenFilePath);
                        _logger.LogInformation($"Deleted test access token file: {testAccessTokenFilePath}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during cleanup");
            }
            finally
            {
                _logger.LogInformation("Test cleanup completed");
            }
        }

        public static string FindAppExecutablePath(string configuration = "Debug")
        {
            // Base directory of the test project (e.g., bin/Debug/netX.X)
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;

            // Go up to the solution root (adjust as needed)
            string solutionRoot = Path.GetFullPath(Path.Combine(baseDir, @"..\..\..\..\..\..\src\"));

            // Path to the bin/configuration folder of the target project
            string appBinConfigPath = Path.Combine(solutionRoot, "bin\\x64\\", configuration, "net8.0-windows10.0.19041.0\\");

            if (!Directory.Exists(appBinConfigPath))
                throw new DirectoryNotFoundException($"Bin path not found: {appBinConfigPath}");

            // Look for the .exe inside framework subdirectories
            var exePath = Directory.GetDirectories(appBinConfigPath)
                .SelectMany(dir => Directory.GetFiles(dir, "AzureMigrateExplore.exe", SearchOption.TopDirectoryOnly))
                .FirstOrDefault();

            if (exePath == null)
                throw new FileNotFoundException("Executable not found in any framework folder under: " + appBinConfigPath);

            return exePath;
        }
    }
}

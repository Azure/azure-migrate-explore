// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using Azure.Core;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using static Azure.Migrate.Explore.Authentication.AzureAuthenticationHandler;
using System.Net.Http.Headers;
using System.Net.Http;
using static AzureMigrateExplore.ChatPage;
using Azure.Migrate.Explore.Authentication;
using Microsoft.Identity.Client;
using System.Linq;
using System.Collections.Generic;

namespace AzureMigrateExplore.Authentication
{
    /// <summary>
    /// Service that manages periodic tasks that need to run across the entire application
    /// regardless of which page is currently active.
    /// </summary>
    public static class PeriodicSessionRefresh
    {
        private static DispatcherTimer _periodicTimer;
        private static DispatcherTimer _sessionTimer;
        private static DispatcherQueue _dispatcherQueue;

        /// <summary>
        /// Initializes the periodic task service with a specified dispatcher queue.
        /// </summary>
        /// <param name="dispatcherQueue">The dispatcher queue to use for running tasks on the UI thread</param>
        public static void Initialize(DispatcherQueue dispatcherQueue)
        {
            if (_periodicTimer != null)
            {
                // Service already initialized
                return;
            }

            _dispatcherQueue = dispatcherQueue;

            // Create timer that runs every 20 minutes
            _periodicTimer = new DispatcherTimer();
            _periodicTimer.Interval = TimeSpan.FromMinutes(20);
            _periodicTimer.Tick += PeriodicTimer_Tick;
            
            // Start the timer
            _periodicTimer.Start();

            Debug.WriteLine("PeriodicTaskService initialized - timer set for 20 minute intervals");
            
            /// Create a session timer that logs out the user after 24 hours
            _sessionTimer = new DispatcherTimer();
            _sessionTimer.Interval = TimeSpan.FromDays(1); // 24 hours
            _sessionTimer.Tick += (s, e) =>
            {
                // Log out the user after 24 hours
                Logout().Wait();
                Debug.WriteLine("User logged out.");
            };
            
            _sessionTimer.Start();

            Debug.WriteLine("Session timer started - user will be logged out after 24 hours");

        }

        /// <summary>
        /// Handles the timer tick event and executes the periodic function.
        /// </summary>
        private static void PeriodicTimer_Tick(object sender, object e)
        {
            // Make sure we execute on the UI thread
            _dispatcherQueue.TryEnqueue(() =>
            {
                ExecuteSessionRefreshFunction();
            });
        }

        /// <summary>
        /// Executes the refresh function that should run every 20 minutes.
        /// </summary>
        private static async void ExecuteSessionRefreshFunction()
        {
            try
            {
                Debug.WriteLine($"Executing session refresh task at {DateTime.Now}");

                var authResult = await RetrieveAuthenticationToken();
                string accessToken = authResult.AccessToken;

                using (var httpClient = new HttpClient())
                {
                    string sessionId = GlobalConnection.SessionId;

                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                    var requestBody = new
                    {
                        accessToken
                    };

                    var content = new StringContent(JsonConvert.SerializeObject(requestBody), System.Text.Encoding.UTF8, "application/json");

                    var authUrl = "#{AuthURL}#";
                    var sessionRegenerateURL = $"{authUrl}/session/{sessionId}/refresh";

                    var response = await httpClient.PostAsync(sessionRegenerateURL, content);
                    
                    if (!response.IsSuccessStatusCode)
                    {
                        throw new Exception($"Error calling API: {response.ReasonPhrase}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error executing periodic task: {ex.Message}");
            }
        }

        /// <summary>
        /// Executes the periodic function immediately without waiting for the timer.
        /// </summary>
        public static void ExecuteNow()
        {
            if (_dispatcherQueue != null)
            {
                _dispatcherQueue.TryEnqueue(() =>
                {
                    ExecuteSessionRefreshFunction();
                });
            }
        }

        /// <summary>
        /// Stops the periodic timer and cleans up resources.
        /// </summary>
        public static void Shutdown()
        {
            if (_periodicTimer != null)
            {
                _periodicTimer.Stop();
                _periodicTimer = null;
                Debug.WriteLine("PeriodicTaskService shut down");
            }

            if (_sessionTimer != null)
            {
                _sessionTimer.Stop();
                _sessionTimer = null;
                Debug.WriteLine("Session timer shut down");
            }
        }
    }
}

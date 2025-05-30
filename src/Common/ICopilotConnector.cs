// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using Microsoft.Identity.Client;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Azure.Migrate.Explore.Common
{
    public interface ICopilotConnector
    {
        Task<HttpResponseMessage> SubmitRequestAsync(Func<Task<AuthenticationResult>> loginCallback, string request, int numberOfTries = 0);
        Task ReceiveResponseAsync();
    }
}
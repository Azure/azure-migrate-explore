// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;

using Azure.Migrate.Explore.Common;

namespace Azure.Migrate.Explore.HttpRequestHelper
{
    public static class HttpUtilities
    {
        
        public const int MaxProjectDetailsRetries = 3;
        public const int MaxInformationDataRetries = 10;

        public static bool IsRetryNeeded(HttpResponseMessage response, Exception exception)
        {
            return IsRetryableHttpStatusCode(response) || IsRetryableException(exception);
        }

        public static bool IsRetryableHttpStatusCode(HttpResponseMessage response)
        {
            if (response == null)
                return false;
            
            List<HttpStatusCode> httpStatusCodesWorthRetrying = new List<HttpStatusCode>{
                HttpStatusCode.RequestTimeout, // 408
                HttpStatusCode.InternalServerError, // 500
                HttpStatusCode.BadGateway, // 502
                HttpStatusCode.ServiceUnavailable, // 503
                HttpStatusCode.GatewayTimeout // 504
            };

            return httpStatusCodesWorthRetrying.Contains(response.StatusCode);
        }

        public static bool IsRetryableException(Exception exception)
        {
            if (exception == null)
                return false;

             if (UtilityFunctions.FindFirstExceptionOfType<HttpRequestException>(exception) != null
                || UtilityFunctions.FindFirstExceptionOfType<WebException>(exception) != null
                || UtilityFunctions.FindFirstExceptionOfType<SocketException>(exception) != null
                || UtilityFunctions.FindFirstExceptionOfType<TaskCanceledException>(exception) != null
                || UtilityFunctions.FindFirstExceptionOfType<TimeoutException>(exception) != null)
            {
                return true;
            }

            return false;
        }
    }
}
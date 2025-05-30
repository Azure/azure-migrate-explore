// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using System;

namespace Azure.Migrate.Explore.Logger
{
    public class LogEventHandler : EventArgs
    {
        public int Percentage { get; set; }
        public string Message { get; set; }

        public LogEventHandler(int percentage, string message)
        {
            Percentage = percentage;
            Message = message;
        }
    }
}
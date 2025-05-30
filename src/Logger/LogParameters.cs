// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Azure.Migrate.Explore.Logger
{
    internal class LogParameters
    {
        internal enum LogType
        {
            Information,
            Warning,
            Debug,
            Error
        };

        internal int ProgressIncrease { get; set; }
        internal LogType Ltype { get; set; }
        internal string Message { get; set; }

        internal LogParameters()
        {
            Ltype = LogType.Information;
            Message = "";
            ProgressIncrease = 0;
        }

        internal LogParameters (LogType logType, string logMsg)
        {
            ProgressIncrease = 0;
            Ltype = logType;
            Message = logMsg;
        }

        internal LogParameters (LogType logType, int progressIncrease, string logMsg)
        {
            Ltype = logType;
            ProgressIncrease = progressIncrease;
            Message = logMsg;
        }
    }
}
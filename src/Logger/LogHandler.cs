// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Azure.Migrate.Explore.Common;

namespace Azure.Migrate.Explore.Logger
{
    public class LogHandler : ILogHandler
    {
        private BlockingCollection<LogParameters> LogsConsumer;
        private static readonly object _FileLock = new object();
        private static readonly string _FilePath = Path.Combine(AppContext.BaseDirectory, LoggerConstants.LogFileName);

        public EventHandler<LogEventHandler> ReportProgress;

        private int PercentProgress;

        public LogHandler()
        {
            LogsConsumer = new BlockingCollection<LogParameters>();
            PercentProgress = 0;
            StartLoggingTask();
        }

        public async void StartLoggingTask()
        {
            await Task.Factory.StartNew(() =>
            {
                foreach (LogParameters log in LogsConsumer.GetConsumingEnumerable())
                {
                    string message =  "";
                    switch (log.Ltype)
                    {
                        case LogParameters.LogType.Information:
                            message = currentTimeStamp() + LoggerConstants.LogTypeMessageSeparator + LoggerConstants.InformationLogTypePrefix + LoggerConstants.LogTypeMessageSeparator + log.Message;
                            writeLogToFile(message);
                            PercentProgress = log.ProgressIncrease + PercentProgress;
                            ReportProgress?.Invoke(this, new LogEventHandler(PercentProgress, message));
                            break;
                        case LogParameters.LogType.Warning:
                            message = currentTimeStamp() + LoggerConstants.LogTypeMessageSeparator + LoggerConstants.WarningLogTypePrefix + LoggerConstants.LogTypeMessageSeparator + log.Message;
                            writeLogToFile(message);
                            PercentProgress = log.ProgressIncrease + PercentProgress;
                            ReportProgress?.Invoke(this, new LogEventHandler(PercentProgress, message));
                            break;
                        case LogParameters.LogType.Debug:
                            message = currentTimeStamp() + LoggerConstants.LogTypeMessageSeparator + LoggerConstants.DebugLogTypePrefix + LoggerConstants.LogTypeMessageSeparator + log.Message;
                            writeLogToFile(message);
                            PercentProgress = log.ProgressIncrease + PercentProgress;
                            ReportProgress?.Invoke(this, new LogEventHandler(PercentProgress, message));
                            break;
                        case LogParameters.LogType.Error:
                            message = currentTimeStamp() + LoggerConstants.LogTypeMessageSeparator + LoggerConstants.ErrorLogTypePrefix + LoggerConstants.LogTypeMessageSeparator + log.Message;
                            writeLogToFile(message);
                            PercentProgress = log.ProgressIncrease + PercentProgress;
                            ReportProgress?.Invoke(this, new LogEventHandler(PercentProgress, message));
                            break;
                        default:
                            message = currentTimeStamp() + LoggerConstants.LogTypeMessageSeparator + LoggerConstants.UnknownLogTypePrefix + LoggerConstants.LogTypeMessageSeparator + log.Message;
                            writeLogToFile(message);
                            PercentProgress = log.ProgressIncrease + PercentProgress;
                            ReportProgress?.Invoke(this, new LogEventHandler(PercentProgress, message));
                            break;
                    }
                }
            });
        }

        ~LogHandler()
        {
            LogsConsumer.CompleteAdding();
        }

        public void LogInformation(int progressIncrease, string msg)
        {
            LogParameters infoObj = new LogParameters(LogParameters.LogType.Information, progressIncrease, msg);
            LogsConsumer.Add(infoObj);
        }

        public void LogWarning(int progressIncrease, string msg)
        {
            LogParameters warnObj = new LogParameters(LogParameters.LogType.Warning, progressIncrease, msg);
            LogsConsumer.Add(warnObj);
        }

        public void LogDebug(int progressIncrease, string msg)
        {
            LogParameters debugObj = new LogParameters(LogParameters.LogType.Debug, progressIncrease, msg);
            LogsConsumer.Add(debugObj);
        }

        public void LogError(int progressIncrease, string msg)
        {
            LogParameters errorObj = new LogParameters(LogParameters.LogType.Error, progressIncrease, msg);
            LogsConsumer.Add(errorObj);
        }

        public void LogInformation(string msg)
        {
            LogParameters infoObj = new LogParameters(LogParameters.LogType.Information, msg);
            LogsConsumer.Add(infoObj);
        }

        public void LogWarning(string msg)
        {
            LogParameters warnObj = new LogParameters(LogParameters.LogType.Warning, msg);
            LogsConsumer.Add(warnObj);
        }

        public void LogDebug(string msg)
        {
            LogParameters debugObj = new LogParameters(LogParameters.LogType.Debug, msg);
            LogsConsumer.Add(debugObj);
        }

        public void LogError(string msg)
        {
            LogParameters errorObj = new LogParameters(LogParameters.LogType.Error, msg);
            LogsConsumer.Add(errorObj);
        }

        // only use this method when there is no percent increase in parallel
        public int GetCurrentProgress()
        {
            return PercentProgress;
        }

        private void writeLogToFile(string message)
        {
            try
            {
                lock (_FileLock)
                {
                    using (StreamWriter writer = File.AppendText(_FilePath))
                    {
                        writer.WriteLine(message);
                    }
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                // Log the exception details
                Console.WriteLine($"UnauthorizedAccessException: {ex.Message}");
            }
            catch (System.Runtime.InteropServices.COMException ex)
            {
                // Log the exception details
                Console.WriteLine($"COMException: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Log any other exceptions
                Console.WriteLine($"Exception: {ex.Message}");
            }
        }

        private string currentTimeStamp()
        {
            DateTime now = DateTime.UtcNow;
            return now.ToString("yyyy-MM-dd-HH:mm:ss");
        }
    }
}
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
namespace Azure.Migrate.Explore.Logger
{
    public interface ILogHandler
    {
        void LogInformation(int progressIncrease, string msg);
        void LogWarning(int progressIncrease, string msg);
        void LogDebug(int progressIncrease, string msg);
        void LogError(int progressIncrease, string msg);
        void LogInformation(string msg);
        void LogWarning(string msg);
        void LogDebug(string msg);
        void LogError(string msg);
        
    }
}
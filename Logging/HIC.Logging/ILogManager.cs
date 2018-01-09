using System;
using HIC.Logging.PastEvents;

namespace HIC.Logging
{
    /// <summary>
    /// See LogManager
    /// </summary>
    public interface ILogManager
    {
        string[] ListDataTasks(bool hideTests=false);
        DateTime? GetDateOfLastLoadAttemptForTask(string nameOfTask,bool onlyIfSuccessful);
        IDataLoadInfo CreateDataLoadInfo(string dataLoadTaskName, string packageName, string description, string suggestedRollbackCommand, bool isTest);
        void LogProgress(IDataLoadInfo dataLoadInfo, ProgressLogging.ProgressEventType eventType, string source, string description);
        void LogFatalError(IDataLoadInfo dataLoadInfo, string errorSource, string errorDescription);
        void LogRowError(ITableLoadInfo tableLoadInfo, RowErrorLogging.RowErrorType typeOfError, string description, string locationOfRow, bool requiresReloading, string columnName);
        ArchivalDataLoadInfo GetLoadStatusOf(PastEventType mostRecent, string newLoggingDataTask);

        ProgressLogging ProgressLogging { get; }
    }
}
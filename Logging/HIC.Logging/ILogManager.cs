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
        ArchivalDataLoadInfo GetLoadStatusOf(PastEventType mostRecent, string newLoggingDataTask);
    }
}
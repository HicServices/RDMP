// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Rdmp.Core.Curation;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.DataLoad.Engine.DatabaseManagement.EntityNaming;
using Rdmp.Core.DataLoad.Engine.DatabaseManagement.Operations;
using Rdmp.Core.DataLoad.Engine.LoadProcess;
using Rdmp.Core.Logging;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.DataLoad.Engine.Job;

/// <inheritdoc/>
public class DataLoadJob : IDataLoadJob
{
    public string Description { get; private set; }
    public IDataLoadInfo DataLoadInfo { get; private set; }
    public ILoadDirectory LoadDirectory { get; set; }
    private readonly IDataLoadEventListener _listener;

    public int JobID { get; set; }

    private readonly ILogManager _logManager;
    public ILoadMetadata LoadMetadata { get; private set; }

    public List<ITableInfo> RegularTablesToLoad { get; private set; }
    public List<ITableInfo> LookupTablesToLoad { get; private set; }
    public IRDMPPlatformRepositoryServiceLocator RepositoryLocator { get; private set; }

    private Stack<IDisposeAfterDataLoad> _disposalStack = new();

    public HICDatabaseConfiguration Configuration { get; set; }
    public object Payload { get; set; }

    public bool PersistentRaw { get; set; }


    private List<NotifyEventArgs> _crashAtEnd = new();

    public IReadOnlyCollection<NotifyEventArgs> CrashAtEndMessages => _crashAtEnd.AsReadOnly();


    private string _loggingTask;

    public DataLoadJob(IRDMPPlatformRepositoryServiceLocator repositoryLocator, string description,
        ILogManager logManager, ILoadMetadata loadMetadata, ILoadDirectory directory, IDataLoadEventListener listener,
        HICDatabaseConfiguration configuration)
    {
        _logManager = logManager;
        RepositoryLocator = repositoryLocator;
        LoadMetadata = loadMetadata;
        LoadDirectory = directory;
        Configuration = configuration;
        _listener = listener;
        Description = description;

        var catalogues = LoadMetadata.GetAllCatalogues().ToList();

        if (LoadMetadata != null)
            _loggingTask = GetLoggingTask(catalogues);

        RegularTablesToLoad = catalogues.SelectMany(catalogue => catalogue.GetTableInfoList(false)).Distinct().ToList();
        LookupTablesToLoad = catalogues.SelectMany(catalogue => catalogue.GetLookupTableInfoList()).Distinct().ToList();
    }

    private string GetLoggingTask(IEnumerable<ICatalogue> cataloguesToLoad)
    {
        var distinctLoggingTasks = cataloguesToLoad.Select(catalogue => catalogue.LoggingDataTask).Distinct().ToList();
        if (distinctLoggingTasks.Count > 1)
            throw new Exception(
                $"The catalogues to be loaded do not share the same logging task: {string.Join(", ", distinctLoggingTasks)}");

        _loggingTask = distinctLoggingTasks.First();
        return string.IsNullOrWhiteSpace(_loggingTask)
            ? throw new Exception("There is no logging task specified for this load (the name is blank)")
            : _loggingTask;
    }

    private void CreateDataLoadInfo()
    {
        if (string.IsNullOrWhiteSpace(Description))
            throw new Exception(
                "The data load description (for the DataLoadInfo object) must not be empty, please provide a relevant description");

        DataLoadInfo = _logManager.CreateDataLoadInfo(_loggingTask, nameof(DataLoadProcess), Description, "", false);

        if (DataLoadInfo == null)
            throw new Exception("DataLoadInfo is null");

        JobID = DataLoadInfo.ID;
    }

    public void LogProgress(string senderName, string message)
    {
        if (DataLoadInfo == null)
            throw new Exception("Logging hasn't been started for this job (call StartLogging first)");

        if (!DataLoadInfo.IsClosed)
            DataLoadInfo.LogProgress(Logging.DataLoadInfo.ProgressEventType.OnProgress, senderName, message);
    }

    public void LogError(string message, Exception exception)
    {
        // we are bailing out before the load process has had a chance to create a DataLoadInfo object
        if (DataLoadInfo == null)
            CreateDataLoadInfo();

        DataLoadInfo.LogFatalError(nameof(DataLoadProcess),
            message + Environment.NewLine + ExceptionHelper.ExceptionToListOfInnerMessages(exception, true));
        DataLoadInfo.CloseAndMarkComplete();
    }

    public void StartLogging()
    {
        CreateDataLoadInfo();
    }

    public void CloseLogging()
    {
        DataLoadInfo?.CloseAndMarkComplete();
    }

    public void LogInformation(string senderName, string message)
    {
        if (DataLoadInfo == null)
            throw new Exception("Logging hasn't been started for this job (call StartLogging first)");

        if (!DataLoadInfo.IsClosed)
            DataLoadInfo.LogProgress(Logging.DataLoadInfo.ProgressEventType.OnInformation, senderName, message);
    }

    public void LogWarning(string senderName, string message)
    {
        if (DataLoadInfo == null)
            throw new Exception("Logging hasn't been started for this job (call StartLogging first)");

        if (!DataLoadInfo.IsClosed)
            DataLoadInfo.LogProgress(Logging.DataLoadInfo.ProgressEventType.OnWarning, senderName, message);
    }

    public void CreateTablesInStage(DatabaseCloner cloner, LoadBubble stage)
    {
        bool allowReservedPrefixColumns = stage == LoadBubble.Raw ? LoadMetadata.AllowReservedPrefix : true;
        foreach (TableInfo regularTableInfo in RegularTablesToLoad)
            cloner.CreateTablesInDatabaseFromCatalogueInfo(_listener, regularTableInfo, stage, allowReservedPrefixColumns);

        foreach (TableInfo lookupTableInfo in LookupTablesToLoad)
            cloner.CreateTablesInDatabaseFromCatalogueInfo(_listener, lookupTableInfo, stage, allowReservedPrefixColumns);

        PushForDisposal(cloner);
    }

    public void PushForDisposal(IDisposeAfterDataLoad disposeable)
    {
        _disposalStack.Push(disposeable);
    }

    public void OnNotify(object sender, NotifyEventArgs e)
    {
        if (DataLoadInfo != null)
            switch (e.ProgressEventType)
            {
                case ProgressEventType.Trace:
                case ProgressEventType.Debug:
                    break;
                case ProgressEventType.Information:
                    DataLoadInfo.LogProgress(Logging.DataLoadInfo.ProgressEventType.OnInformation,
                        sender.GetType().Name, e.Message + (e.Exception != null
                            ? $"Exception={ExceptionHelper.ExceptionToListOfInnerMessages(e.Exception, true)}"
                            : ""));
                    break;
                case ProgressEventType.Warning:
                    DataLoadInfo.LogProgress(Logging.DataLoadInfo.ProgressEventType.OnWarning, sender.GetType().Name,
                        e.Message + (e.Exception != null
                            ? $"Exception={ExceptionHelper.ExceptionToListOfInnerMessages(e.Exception, true)}"
                            : ""));
                    break;
                case ProgressEventType.Error:
                    DataLoadInfo.LogProgress(Logging.DataLoadInfo.ProgressEventType.OnTaskFailed, sender.GetType().Name,
                        e.Message);
                    DataLoadInfo.LogFatalError(sender.GetType().Name,
                        e.Exception != null
                            ? ExceptionHelper.ExceptionToListOfInnerMessages(e.Exception, true)
                            : e.Message);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

        _listener.OnNotify(sender, e);
    }

    public void OnProgress(object sender, ProgressEventArgs e)
    {
        _listener.OnProgress(sender, e);
    }

    public string ArchiveFilepath => Path.Combine(LoadDirectory.ForArchiving.FullName, $"{DataLoadInfo.ID}.zip");

    public void LoadCompletedSoDispose(ExitCodeType exitCode, IDataLoadEventListener postLoadEventsListener)
    {
        while (_disposalStack.Any())
        {
            var disposable = _disposalStack.Pop();
            disposable.LoadCompletedSoDispose(exitCode, postLoadEventsListener);
        }
    }

    public ColumnInfo[] GetAllColumns()
    {
        return RegularTablesToLoad.SelectMany(t => t.ColumnInfos)
            .Union(LookupTablesToLoad.SelectMany(t => t.ColumnInfos)).Distinct().ToArray();
    }

    public void CrashAtEnd(NotifyEventArgs because)
    {
        _crashAtEnd.Add(because);
    }
}
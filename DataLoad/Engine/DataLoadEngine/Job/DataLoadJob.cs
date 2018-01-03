using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CatalogueLibrary;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Data.EntityNaming;
using CatalogueLibrary.Repositories;
using DataLoadEngine.DatabaseManagement.Operations;
using DataLoadEngine.LoadExecution;
using DataLoadEngine.LoadProcess;
using HIC.Logging;
using ReusableLibraryCode;
using ReusableLibraryCode.Progress;

namespace DataLoadEngine.Job
{
    /// <summary>
    /// Documents an ongoing load that is executing in the Data Load Engine.  This includes the load configuration (LoadMetadata), Logging object (DataLoadInfo),
    /// file system (HICProjectDirectory) etc.
    /// </summary>
    public class DataLoadJob : IDataLoadJob
    {
        public string Description { get; private set; }
        public IDataLoadInfo DataLoadInfo { get; private set; }
        public IHICProjectDirectory HICProjectDirectory { get; set; }
        private readonly IDataLoadEventListener _listener;

        public int JobID { get; set; }
        
        private readonly ILogManager _logManager;
        public ILoadMetadata LoadMetadata { get; private set; }

        public List<TableInfo> RegularTablesToLoad { get; private set; }
        public List<TableInfo> LookupTablesToLoad { get; private set; }

        private Stack<IDisposeAfterDataLoad> _disposalStack = new Stack<IDisposeAfterDataLoad>();


        private string _loggingTask;

        public DataLoadJob(string description, ILogManager logManager, ILoadMetadata loadMetadata, IHICProjectDirectory hicProjectDirectory, IDataLoadEventListener listener)
        {
            _logManager = logManager;
            LoadMetadata = loadMetadata;
            HICProjectDirectory = hicProjectDirectory;
            _listener = listener;
            Description = description;

            List<ICatalogue> catalogues = LoadMetadata.GetAllCatalogues().ToList();
            
            if (LoadMetadata != null)
                _loggingTask = GetLoggingTask(catalogues);

            RegularTablesToLoad = catalogues.SelectMany(catalogue => catalogue.GetTableInfoList(false)).Distinct().ToList();
            LookupTablesToLoad = catalogues.SelectMany(catalogue => catalogue.GetLookupTableInfoList()).Distinct().ToList();
        }

        private string GetLoggingTask(IEnumerable<ICatalogue> cataloguesToLoad)
        {
            var distinctLoggingTasks = cataloguesToLoad.Select(catalogue => catalogue.LoggingDataTask).Distinct().ToList();
            if (distinctLoggingTasks.Count() > 1)
                throw new Exception("The catalogues to be loaded do not share the same logging task: " + string.Join(", ", distinctLoggingTasks));

            _loggingTask = distinctLoggingTasks.First();
            if (string.IsNullOrWhiteSpace(_loggingTask))
                throw new Exception("There is no logging task specified for this load (the name is blank)");

            return _loggingTask;
        }

        private void CreateDataLoadInfo()
        {
            if (string.IsNullOrWhiteSpace(Description))
                throw new Exception("The data load description (for the DataLoadInfo object) must not be empty, please provide a relevant description");

            DataLoadInfo = _logManager.CreateDataLoadInfo(_loggingTask, typeof(DataLoadProcess).Name, Description, "", false);

            if (DataLoadInfo == null)
                throw new Exception("DataLoadInfo is null");

            JobID = DataLoadInfo.ID;
        }

        public void LogProgress(string senderName, string message)
        {
            if (DataLoadInfo == null)
                throw new Exception("Logging hasn't been started for this job (call StartLogging first)");

            if (!DataLoadInfo.IsClosed)
                _logManager.LogProgress(DataLoadInfo, ProgressLogging.ProgressEventType.OnProgress, senderName, message);
        }

        public void LogError(string message, Exception exception)
        {
            // we are bailing out before the load process has had a chance to create a DataLoadInfo object
            if (DataLoadInfo == null)
                CreateDataLoadInfo();

            _logManager.LogFatalError(DataLoadInfo, typeof(DataLoadProcess).Name, message + Environment.NewLine + ExceptionHelper.ExceptionToListOfInnerMessages(exception,true));
            DataLoadInfo.CloseAndMarkComplete();
        }

        public void StartLogging()
        {
            CreateDataLoadInfo();
        }

        public void CloseLogging()
        {
            if (DataLoadInfo != null)
                DataLoadInfo.CloseAndMarkComplete();
        }

        public void LogInformation(string senderName, string message)
        {
            if (DataLoadInfo == null)
                throw new Exception("Logging hasn't been started for this job (call StartLogging first)");

            if(!DataLoadInfo.IsClosed)
                _logManager.LogProgress(DataLoadInfo, ProgressLogging.ProgressEventType.OnInformation, senderName, message);
        }

        public void LogWarning(string senderName, string message)
        {
            if (DataLoadInfo == null)
                throw new Exception("Logging hasn't been started for this job (call StartLogging first)");

            if (!DataLoadInfo.IsClosed)
                _logManager.LogProgress(DataLoadInfo, ProgressLogging.ProgressEventType.OnWarning, senderName, message);
        }

        public void CreateTablesInStage(DatabaseCloner cloner, LoadBubble stage)
        {
            foreach (TableInfo regularTableInfo in RegularTablesToLoad)
                cloner.CreateTablesInDatabaseFromCatalogueInfo(regularTableInfo, stage);

            foreach (TableInfo lookupTableInfo in LookupTablesToLoad)
                 cloner.CreateTablesInDatabaseFromCatalogueInfo(lookupTableInfo, stage);
            
            PushForDisposal(cloner);
        }

        public void PushForDisposal(IDisposeAfterDataLoad disposeable)
        {
            _disposalStack.Push(disposeable);
        }

        public void OnNotify(object sender, NotifyEventArgs e)
        {
            if(DataLoadInfo != null)
                switch (e.ProgressEventType)
                {
                    case ProgressEventType.Information:
                        _logManager.LogProgress(DataLoadInfo, ProgressLogging.ProgressEventType.OnInformation, sender.GetType().Name, e.Message + (e.Exception != null ? "Exception=" + ExceptionHelper.ExceptionToListOfInnerMessages(e.Exception, true) : ""));
                        break;
                    case ProgressEventType.Warning:
                        _logManager.LogProgress(DataLoadInfo, ProgressLogging.ProgressEventType.OnWarning, sender.GetType().Name, e.Message + (e.Exception != null ? "Exception=" + ExceptionHelper.ExceptionToListOfInnerMessages(e.Exception,true) : ""));
                        break;
                    case ProgressEventType.Error:
                        _logManager.LogProgress(DataLoadInfo, ProgressLogging.ProgressEventType.OnTaskFailed, sender.GetType().Name, e.Message);
                        _logManager.LogFatalError(DataLoadInfo, sender.GetType().Name, e.Exception != null ? ExceptionHelper.ExceptionToListOfInnerMessages(e.Exception,true) : e.Message);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            _listener.OnNotify(sender,e);
        }

        public void OnProgress(object sender, ProgressEventArgs e)
        {
            _listener.OnProgress(sender,e);
        }

        public string ArchiveFilepath
        {
            get { return Path.Combine(HICProjectDirectory.ForArchiving.FullName, DataLoadInfo.ID + ".zip"); }
        }

        public void LoadCompletedSoDispose(ExitCodeType exitCode, IDataLoadEventListener postLoadEventsListener)
        {
            while (_disposalStack.Any())
            {
                var disposable = _disposalStack.Pop();
                disposable.LoadCompletedSoDispose(exitCode, postLoadEventsListener);
            }
        }
    }
}
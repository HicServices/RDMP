using System.Collections.Generic;
using CatalogueLibrary;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Repositories;
using DataLoadEngine.DatabaseManagement.EntityNaming;
using DataLoadEngine.DatabaseManagement.Operations;
using HIC.Logging;
using ReusableLibraryCode.Progress;

namespace DataLoadEngine.Job
{
    /// <summary>
    /// Empty implementation of IDataLoadJob that can be used during Checking / Tests etc 
    /// </summary>
    public class ThrowImmediatelyDataLoadJob: IDataLoadJob
    {
        private readonly IDataLoadEventListener _listener;

        public ThrowImmediatelyDataLoadJob()
        {
            _listener = new ThrowImmediatelyDataLoadEventListener();
        }

        public ThrowImmediatelyDataLoadJob(IDataLoadEventListener listener)
        {
            _listener = listener;
        }


        public string Description { get; private set; }
        public IDataLoadInfo DataLoadInfo { get; set; }
        public IHICProjectDirectory HICProjectDirectory { get; set; }
        public int JobID { get; set; }
        public ILoadMetadata LoadMetadata { get; private set; }
        public bool DisposeImmediately { get; private set; }
        public string ArchiveFilepath { get; private set; }
        public List<TableInfo> RegularTablesToLoad { get; set; }
        public List<TableInfo> LookupTablesToLoad { get; private set; }
        public IRDMPPlatformRepositoryServiceLocator RepositoryLocator { get { return null; }}

        public void StartLogging()
        {
        }

        public void CloseLogging()
        {
        }

        public HICDatabaseConfiguration Configuration { get; private set; }

        public object Payload { get; set; }

        public void AddForDisposalAfterCompletion(IDisposeAfterDataLoad disposable)
        {
        }
        public void CreateTablesInStage(DatabaseCloner cloner, LoadBubble stage)
        {
        }

        public void PushForDisposal(IDisposeAfterDataLoad disposeable)
        {
        }

        public void LoadCompletedSoDispose(ExitCodeType exitCode, IDataLoadEventListener postLoadEventsListener)
        {
            
        }

        public void OnNotify(object sender, NotifyEventArgs e)
        {
            _listener.OnNotify(sender,e);
        }

        public void OnProgress(object sender, ProgressEventArgs e)
        {
            _listener.OnProgress(sender,e);
        }
    }
}
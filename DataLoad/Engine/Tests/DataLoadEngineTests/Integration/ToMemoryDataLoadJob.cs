using System.Collections.Generic;
using CatalogueLibrary;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Data.EntityNaming;
using DataLoadEngine;
using DataLoadEngine.DatabaseManagement.Operations;
using DataLoadEngine.Job;
using HIC.Logging;
using ReusableLibraryCode.Progress;

namespace DataLoadEngineTests.Integration
{
    public class ToMemoryDataLoadJob : ToMemoryDataLoadEventListener, IDataLoadJob
    {
        public ToMemoryDataLoadJob(bool throwOnErrorEvents = true): base(throwOnErrorEvents)
        {
        }

        public string Description { get; private set; }
        public IDataLoadInfo DataLoadInfo { get; private set; }
        public IHICProjectDirectory HICProjectDirectory { get; set; }
        public int JobID { get; set; }
        public ILoadMetadata LoadMetadata { get; private set; }
        public bool DisposeImmediately { get; private set; }
        public string ArchiveFilepath { get; private set; }
        public List<TableInfo> RegularTablesToLoad { get; private set; }
        public List<TableInfo> LookupTablesToLoad { get; private set; }
        public void StartLogging()
        {
        }

        public void CloseLogging()
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
    }
}
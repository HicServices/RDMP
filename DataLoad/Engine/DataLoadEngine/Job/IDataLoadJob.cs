using System.Collections.Generic;
using CatalogueLibrary;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Data.EntityNaming;
using CatalogueLibrary.Repositories;
using DataLoadEngine.DatabaseManagement.Operations;
using DataLoadEngine.LoadExecution.Components;
using HIC.Logging;
using ReusableLibraryCode.Progress;

namespace DataLoadEngine.Job
{
    public interface IDataLoadJob : IDataLoadEventListener, IDisposeAfterDataLoad
    {
        string Description { get; }
        IDataLoadInfo DataLoadInfo { get; }
        IHICProjectDirectory HICProjectDirectory { get; set; }
        int JobID { get; set; }
        ILoadMetadata LoadMetadata { get; }
        string ArchiveFilepath { get; }
        
        List<TableInfo> RegularTablesToLoad { get; }
        List<TableInfo> LookupTablesToLoad { get; }
        
        void StartLogging();
        void CloseLogging();
        
        /// <summary>
        /// Orders the job to create the tables it requires in the given stage (e.g. RAW/STAGING), the job will also take ownership of the cloner for the purposes
        /// of disposal (DO NOT DISPOSE OF CLONER YOURSELF)
        /// </summary>
        /// <param name="cloner"></param>
        /// <param name="namingScheme"></param>
        /// <param name="namingConvention"></param>
        /// <param name="stage"></param>
        void CreateTablesInStage(DatabaseCloner cloner,LoadBubble stage);

        void PushForDisposal(IDisposeAfterDataLoad disposeable);
    }
}
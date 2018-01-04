using System;
using System.Collections.Generic;
using System.Linq;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Repositories;
using DataLoadEngine.DatabaseManagement.EntityNaming;
using DataLoadEngine.LoadExecution.Components;
using DataLoadEngine.LoadExecution.Components.Arguments;
using DataLoadEngine.LoadExecution.Components.Standard;
using DataLoadEngine.LoadExecution.Components.Runtime;
using DataLoadEngine.LoadProcess;
using HIC.Logging;
using ReusableLibraryCode.Progress;

namespace DataLoadEngine.LoadExecution
{
    /// <summary>
    /// This is factored out more for documentation's sake. It is a description of the HIC data load pipeline, in factory form!
    /// </summary>
    public class HICDataLoadFactory
    {
        private readonly HICDatabaseConfiguration _databaseConfiguration;
        private readonly HICLoadConfigurationFlags _loadConfigurationFlags;
        private readonly CatalogueRepository _repository;
        private readonly ILogManager _logManager;
        private readonly IList<ICatalogue> _cataloguesToLoad;

        public ILoadMetadata LoadMetadata { get; private set; }

        public HICDataLoadFactory(ILoadMetadata loadMetadata, HICDatabaseConfiguration databaseConfiguration, HICLoadConfigurationFlags loadConfigurationFlags, CatalogueRepository repository, ILogManager logManager)
        {
            _databaseConfiguration = databaseConfiguration;
            _loadConfigurationFlags = loadConfigurationFlags;
            _repository = repository;
            _logManager = logManager;
            LoadMetadata = loadMetadata;

            // If we are not supplied any catalogues to load, it is expected that we will load all catalogues associated with the provided ILoadMetadata
            _cataloguesToLoad = LoadMetadata.GetAllCatalogues().ToList();
            if (!_cataloguesToLoad.Any())
                throw new InvalidOperationException("LoadMetadata " + LoadMetadata.ID + " is not related to any Catalogues, there is nothing to load");
        }

        public IDataLoadExecution Create(IDataLoadEventListener postLoadEventListener)
        {
            var loadArgsDictionary = new LoadArgsDictionary(LoadMetadata, _databaseConfiguration.DeployInfo);

            //warn user about disabled tasks
            var processTasks = LoadMetadata.ProcessTasks.ToList();
            foreach (ProcessTask task in processTasks
                .Where(p => p.IsDisabled))
                postLoadEventListener.OnNotify(this,
                    new NotifyEventArgs(ProgressEventType.Warning, "Found disabled ProcessTask" + task));

            //Get all the runtime tasks which are not disabled
            var factory = new RuntimeTaskPackager(processTasks.Where(p => !p.IsDisabled), loadArgsDictionary.LoadArgs, _cataloguesToLoad, _repository);

            var getFiles = new LoadFiles(factory.GetRuntimeTasksForStage(LoadStage.GetFiles));
            
            var mounting = new PopulateRAW(factory.GetRuntimeTasksForStage(LoadStage.Mounting), _databaseConfiguration);
            
            var adjustRaw = factory.CreateCompositeDataLoadComponentFor(LoadStage.AdjustRaw, "Adjust RAW");

            var migrateToStaging = new MigrateRAWToStaging(_databaseConfiguration, LoadMetadata, _loadConfigurationFlags);
            
            var adjustStaging = factory.CreateCompositeDataLoadComponentFor(LoadStage.AdjustStaging, "Adjust Staging");

            var migrateStagingToLive = new MigrateStagingToLive(_cataloguesToLoad, _databaseConfiguration,_loadConfigurationFlags, _logManager);

            var postLoad = factory.CreateCompositeDataLoadComponentFor(LoadStage.PostLoad, "Post Load");

            var archiveFiles = new ArchiveFiles(_loadConfigurationFlags);
                    
            var loadStagingDatabase = new CompositeDataLoadComponent(new List<IDataLoadComponent>
            {
                mounting,
                adjustRaw,
                migrateToStaging
            });

            var adjustStagingAndMigrateToLive = new CompositeDataLoadComponent(new List<IDataLoadComponent>
            {
                loadStagingDatabase,
                adjustStaging,
                migrateStagingToLive,
                postLoad
            });
            
            var components = new List<DataLoadComponent>
            {
                getFiles,
                adjustStagingAndMigrateToLive,
                archiveFiles
            };
            
            return new SingleJobExecution(components);

        }
    }
}
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using CatalogueLibrary;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Data.DataLoad.Extensions;
using DataLoadEngine.DatabaseManagement;
using DataLoadEngine.DatabaseManagement.EntityNaming;

namespace DataLoadEngine.LoadExecution.Components.Arguments
{
    /// <summary>
    /// Creates StageArgs for each LoadStage based on the supplied LoadMetadata (load configuration).  This tells the DLE where each database is etc in the 
    /// RAW => STAGING => LIVE model rdmp uses for data loading.
    /// </summary>
    public class LoadArgsDictionary
    {
        private readonly ILoadMetadata _loadMetadata;
        private readonly StandardDatabaseHelper _dbDeployInfo;
        
        public Dictionary<LoadStage, IStageArgs> LoadArgs { get; private set; }

        public LoadArgsDictionary(ILoadMetadata loadMetadata, StandardDatabaseHelper dbDeployInfo)
        {
            if(string.IsNullOrWhiteSpace(loadMetadata.LocationOfFlatFiles))
                throw new Exception(@"No Project Directory (LocationOfFlatFiles) has been configured on LoadMetadata " + loadMetadata.Name);

            _dbDeployInfo = dbDeployInfo;
            _loadMetadata = loadMetadata;

            LoadArgs = new Dictionary<LoadStage, IStageArgs>();
            foreach (LoadStage loadStage in Enum.GetValues(typeof(LoadStage)))
            {
                LoadArgs.Add(loadStage, CreateLoadArgs(loadStage));
            }
        }

        protected IStageArgs CreateLoadArgs(LoadStage loadStage)
        {
            return
                new StageArgs(loadStage,
                _dbDeployInfo[loadStage.ToLoadBubble()]
                , new HICProjectDirectory(_loadMetadata.LocationOfFlatFiles.TrimEnd(new[] { '\\' })));
        }
    }
}
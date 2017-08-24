using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using DataLoadEngine.DatabaseManagement;
using DataLoadEngine.DatabaseManagement.EntityNaming;

namespace DataLoadEngine.LoadExecution.Components.Arguments
{
    public class LoadArgsDictionary
    {
        private readonly ILoadMetadata _loadMetadata;
        private readonly StandardDatabaseHelper _dbDeployInfo;
        private readonly bool _isTestload;

        public Dictionary<LoadStage, IStageArgs> LoadArgs { get; private set; }

        public LoadArgsDictionary(ILoadMetadata loadMetadata, StandardDatabaseHelper dbDeployInfo, bool isTestload)
        {
            if(string.IsNullOrWhiteSpace(loadMetadata.LocationOfFlatFiles))
                throw new Exception(@"No Project Directory (LocationOfFlatFiles) has been configured on LoadMetadata " + loadMetadata.Name);

            _dbDeployInfo = dbDeployInfo;
            _isTestload = isTestload;
            _loadMetadata = loadMetadata;

            LoadArgs = new Dictionary<LoadStage, IStageArgs>();
            foreach (LoadStage loadStage in Enum.GetValues(typeof(LoadStage)))
            {
                LoadArgs.Add(loadStage, CreateLoadArgs(loadStage));
            }
        }

        protected IStageArgs CreateLoadArgs(LoadStage loadStage)
        {
            string locationOfFlatFiles = _loadMetadata.LocationOfFlatFiles.TrimEnd(new[] { '\\' });
            StageArgs args = new StageArgs(loadStage,_isTestload) { RootDir = locationOfFlatFiles};

            switch (loadStage)
            {
                case LoadStage.GetFiles:
                    break;
                case LoadStage.Mounting:
                    args.DbInfo = _dbDeployInfo[LoadBubble.Raw];
                    break;
                case LoadStage.AdjustRaw:
                    args.DbInfo = _dbDeployInfo[LoadBubble.Raw];
                    break;
                case LoadStage.AdjustStaging:
                    args.DbInfo = _dbDeployInfo[LoadBubble.Staging];
                    break;
                case LoadStage.PostLoad:
                    args.DbInfo = _dbDeployInfo[LoadBubble.Live];
                    break;
                default:
                    throw new Exception("Unknown load stage: " + loadStage);
            }

            return args;
        }
    }
}
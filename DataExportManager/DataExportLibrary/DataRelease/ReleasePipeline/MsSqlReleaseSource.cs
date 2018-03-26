using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using CatalogueLibrary.Data;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.Repositories;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DataAccess;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using ReusableLibraryCode.Progress;
using Xceed.Words.NET;

namespace DataExportLibrary.DataRelease.ReleasePipeline
{
    /// <summary>
    /// Prepares the Environment and the Source Database for the ReleaseEngine.
    /// </summary>
    /// <typeparam name="T">The ReleaseAudit object passed around in the pipeline</typeparam>
    public class MsSqlReleaseSource<T> : FixedReleaseSource<ReleaseAudit>
    {
        private readonly CatalogueRepository _catalogueRepository;
        private DiscoveredDatabase _database;

        public MsSqlReleaseSource(CatalogueRepository catalogueRepository) : base()
        {
            _catalogueRepository = catalogueRepository;
        }

        public override ReleaseAudit GetChunk(IDataLoadEventListener listener, GracefulCancellationToken cancellationToken)
        {
            var sourceFolder = new DirectoryInfo(_releaseData.EnvironmentPotential.Configuration.Project.ExtractionDirectory);
            var tempStorage = sourceFolder.CreateSubdirectory(Guid.NewGuid().ToString("N"));
            if (_database != null)
                _database.Detach(tempStorage);
            return new ReleaseAudit()
            {
                SourceGlobalFolder = tempStorage
            };
        }

        public override void Dispose(IDataLoadEventListener listener, Exception pipelineFailureExceptionIfAny)
        {
            throw new NotImplementedException();
        }

        public override void Abort(IDataLoadEventListener listener)
        {
            throw new NotImplementedException();
        }

        protected override void RunSpecificChecks(ICheckNotifier notifier)
        {
            var foundConnection = String.Empty;
            var tables = new List<string>();
            foreach (var configs in _releaseData.ConfigurationsForRelease.SelectMany(x => x.Key.CumulativeExtractionResults))
            {
                var candidate = configs.DestinationDescription.Split('|')[0] + "|" +
                                configs.DestinationDescription.Split('|')[1];

                tables.Add(configs.DestinationDescription.Split('|')[2]);

                if (String.IsNullOrEmpty(foundConnection))
                    foundConnection = candidate;
                if (foundConnection != candidate)
                    throw new Exception("You are trying to extract from multiple servers or databases. This is not allowed! " +
                                        "Please re-run the extracts against the same database.");
            }

            var externalServerId = int.Parse(foundConnection.Split('|')[0]);
            var dbName = foundConnection.Split('|')[1];

            var externalServer = _catalogueRepository.GetObjectByID<ExternalDatabaseServer>(externalServerId);
            var server = DataAccessPortal.GetInstance().ExpectServer(externalServer, DataAccessContext.DataExport, setInitialDatabase: false);
            _database = server.ExpectDatabase(dbName);

            if (!_database.Exists())
            {
                throw new Exception("Database does not exist!");
            }
            foreach (var table in tables)
            {
                var foundTable = _database.ExpectTable(table);
                if (!foundTable.Exists())
                {
                    throw new Exception("Table does not exist!");
                }
            }
        }
    }
}
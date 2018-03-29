using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.Linq;
using CatalogueLibrary.Data;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.Repositories;
using DataExportLibrary.ExtractionTime;
using DataExportLibrary.Interfaces.Data.DataTables;
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
        private DirectoryInfo _dataPathMap;
        private bool firstTime = true;

        public MsSqlReleaseSource(CatalogueRepository catalogueRepository) : base()
        {
            _catalogueRepository = catalogueRepository;
        }

        public override ReleaseAudit GetChunk(IDataLoadEventListener listener, GracefulCancellationToken cancellationToken)
        {
            if (firstTime)
            {
                firstTime = false;
                DirectoryInfo sourceFolder = GetSourceFolder();
                Debug.Assert(sourceFolder != null, "sourceFolder != null");
                var dbOutputFolder = sourceFolder.CreateSubdirectory(ExtractionDirectory.OtherDataFolderName);
                
                if (_database != null)
                {
                    _database.Detach();
                    var databaseName = _database.GetRuntimeName();

                    // TODO: Discover mapping between Server DATA folder and this machine!
                    File.Copy(Path.Combine(_dataPathMap.FullName, databaseName + ".mdf"), Path.Combine(dbOutputFolder.FullName, databaseName + ".mdf"));
                    File.Copy(Path.Combine(_dataPathMap.FullName, databaseName + "_log.ldf"), Path.Combine(dbOutputFolder.FullName, databaseName + "_log.ldf"));
                    File.Delete(Path.Combine(_dataPathMap.FullName, databaseName + ".mdf"));
                    File.Delete(Path.Combine(_dataPathMap.FullName, databaseName + "_log.mdf"));
                }

                return new ReleaseAudit()
                {
                    SourceGlobalFolder = PrepareSourceGlobalFolder()
                };
            }
            return null;
        }

        private DirectoryInfo GetSourceFolder()
        {
            foreach (KeyValuePair<IExtractionConfiguration, List<ReleasePotential>> releasePotentials in _releaseData.ConfigurationsForRelease)
            {
                foreach (ReleasePotential releasePotential in releasePotentials.Value)
                {
                    return releasePotential.ExtractDirectory.Parent;
                }
            }
            return null;
        }

        public override void Dispose(IDataLoadEventListener listener, Exception pipelineFailureExceptionIfAny)
        {
            firstTime = true;
        }

        public override void Abort(IDataLoadEventListener listener)
        {
            firstTime = true;
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
            if (!String.IsNullOrWhiteSpace(externalServer.MappedDataPath))
                _dataPathMap = new DirectoryInfo(externalServer.MappedDataPath);

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
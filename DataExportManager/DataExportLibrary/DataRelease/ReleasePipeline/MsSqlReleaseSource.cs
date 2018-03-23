using System;
using System.Collections.Generic;
using System.Data.Common;
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
        private readonly CatalogueRepository catalogueRepository;

        public MsSqlReleaseSource(CatalogueRepository catalogueRepository) : base()
        {
            this.catalogueRepository = catalogueRepository;
        }

        public override ReleaseAudit GetChunk(IDataLoadEventListener listener, GracefulCancellationToken cancellationToken)
        {
            throw new NotImplementedException();
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
            var servers = new Dictionary<ExternalDatabaseServer, List<string>>();
            foreach (var config in _releaseData.ConfigurationsForRelease.SelectMany(x => x.Value))
            {
                var externalServerId = int.Parse(config.ExtractionResults.DestinationDescription.Split('|')[0]);
                var externalServer = catalogueRepository.GetObjectByID<ExternalDatabaseServer>(externalServerId);
                var tblName = config.ExtractionResults.DestinationDescription.Split('|')[1];
                if (!servers.ContainsKey(externalServer))
                    servers.Add(externalServer, new List<string>());
                servers[externalServer].Add(tblName);
            }
            foreach (var foundServer in servers)
            {
                var server = DataAccessPortal.GetInstance().ExpectServer(foundServer.Key, DataAccessContext.DataExport);
                using (DbConnection con = server.GetConnection())
                {
                    con.Open();
                    var database = server.ExpectDatabase(foundServer.Key.Database);
                    if (!database.Exists())
                    {
                        throw new Exception("Database does not exist!");
                    }

                    foreach (var table in foundServer.Value)
                    {
                        var foundTable = database.ExpectTable(table);
                        if (!foundTable.Exists())
                        {
                            throw new Exception("Table does not exist!");
                        }   
                    }
                }
            }
        }
    }
}
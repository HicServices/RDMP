using FAnsi.Discovery;
using Microsoft.Data.SqlClient;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.DataExport.DataExtraction.Commands;
using Rdmp.Core.DataExport.DataRelease.Pipeline;
using Rdmp.Core.DataExport.DataRelease.Potential;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataLoad.Triggers;
using Rdmp.Core.DataLoad.Triggers.Exceptions;
using Rdmp.Core.DataLoad.Triggers.Implementations;
using Rdmp.Core.Logging;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.DataAccess;
using Rdmp.Core.ReusableLibraryCode.Progress;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.DirectoryServices.Protocols;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TypeGuesser;

namespace Rdmp.Core.DataExport.DataExtraction.Pipeline.Destinations
{
    public class MSSqlMergeDestination : ExtractionDestination
    {
        [DemandsInitialization(
       "External server to create the extraction into, a new database will be created for the project based on the naming pattern provided",
       Mandatory = true)]
        public IExternalDatabaseServer TargetDatabaseServer { get; set; }
        [DemandsInitialization(
       "External server to create the extraction into, a new database will be created for the project based on the naming pattern provided",
       Mandatory = true)]
        public string DatabaseNamingPattern { get; set; }

        [DemandsInitialization("Delete the temporary table used to perform the merge", DefaultValue = true)]
        public bool DeleteMergeTempTable { get; set; }

        [DemandsInitialization("Ensure the destination table has an archive trigger", DefaultValue = true)]
        public bool UseArchiveTrigger { get; set; }


        [DemandsInitialization("Allow the merge to perform deletes when records are missing from the source.", DefaultValue = false)]
        public bool AllowMergeToPerformDeletes { get; set; }

        public MSSqlMergeDestination() : base(false)
        {
        }

        public override void Abort(IDataLoadEventListener listener)
        {
            //throw new NotImplementedException();
        }

        public override void Dispose(IDataLoadEventListener listener, Exception pipelineFailureExceptionIfAny)
        {
            //throw new NotImplementedException();
        }

        public override string GetDestinationDescription()
        {
            return "SQL MERGE - TODO";
        }

        public override GlobalReleasePotential GetGlobalReleasabilityEvaluator(IRDMPPlatformRepositoryServiceLocator repositoryLocator, ISupplementalExtractionResults globalResult, IMapsDirectlyToDatabaseTable globalToCheck)
        {
            throw new NotImplementedException();
        }

        public override ReleasePotential GetReleasePotential(IRDMPPlatformRepositoryServiceLocator repositoryLocator, ISelectedDataSets selectedDataSet)
        {
            return new MsSqlExtractionReleasePotential(repositoryLocator, selectedDataSet);
        }

        public override FixedReleaseSource<ReleaseAudit> GetReleaseSource(ICatalogueRepository catalogueRepository)
        {
            return new MsSqlReleaseSource(catalogueRepository);
        }

        protected override void Open(DataTable toProcess, IDataLoadEventListener job, GracefulCancellationToken cancellationToken)
        {
        }

        protected override void PreInitializeImpl(IExtractCommand request, IDataLoadEventListener listener)
        {
        }

        protected override void WriteRows(DataTable toProcess, IDataLoadEventListener job, GracefulCancellationToken cancellationToken, Stopwatch stopwatch)
        {
            var discoveredServer = DataAccessPortal.ExpectServer(TargetDatabaseServer, DataAccessContext.DataExport, false);

            //sort out the naming 
            var dbName = DatabaseNamingPattern;

            dbName = dbName.Replace("$p", _project.Name)
                .Replace("$n", _project.ProjectNumber.ToString())
                .Replace("$t", _project.MasterTicket)
                .Replace("$r", _request.Configuration.RequestTicket)
                .Replace("$l", _request.Configuration.ReleaseTicket)
                .Replace("$e", _request.Configuration.ID.ToString());

            //make sure the db exist
            var db = discoveredServer.ExpectDatabase(dbName);
            if (!db.Exists())
                db.Create();

            var destinationTable = db.ExpectTable(toProcess.TableName);

            //ensure there are some pks
            bool hasPrimaryKeys = toProcess.PrimaryKey != null && toProcess.PrimaryKey.Length > 0;
            if (!destinationTable.Exists())
            {
                //create
                destinationTable = db.CreateTable(out _, toProcess.TableName, toProcess, null,
                         true, null);
            }
            else
            {
                hasPrimaryKeys = destinationTable.DiscoverColumns().Any(col => col.IsPrimaryKey);
            }

            if (!hasPrimaryKeys)
            {
                job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Error, "No Primary Keys were found in the destination table or source configuration"));
                return;
            }

            if (UseArchiveTrigger)
            {

                var listeners = ((ForkDataLoadEventListener)job).GetToLoggingDatabaseDataLoadEventListenersIfany();
                foreach (var dleListener in listeners)
                {
                    IDataLoadInfo dataLoadInfo = dleListener.DataLoadInfo;
                    DataColumn newColumn = new(SpecialFieldNames.DataLoadRunID, typeof(int))
                    {
                        DefaultValue = dataLoadInfo.ID
                    };
                    try
                    {
                        destinationTable.DiscoverColumn(SpecialFieldNames.DataLoadRunID);
                    }
                    catch (Exception)
                    {
                        destinationTable.AddColumn(SpecialFieldNames.DataLoadRunID, new DatabaseTypeRequest(typeof(int)), true, 30000);

                    }
                    if (!toProcess.Columns.Contains(SpecialFieldNames.DataLoadRunID))
                        toProcess.Columns.Add(newColumn);
                    foreach (DataRow dr in toProcess.Rows)
                        dr[SpecialFieldNames.DataLoadRunID] = dataLoadInfo.ID;

                }


                TriggerImplementerFactory triggerFactory = new TriggerImplementerFactory(FAnsi.DatabaseType.MicrosoftSQLServer);
                var implementor = triggerFactory.Create(destinationTable);
                bool present;
                try
                {
                    present = implementor.GetTriggerStatus() == DataLoad.Triggers.TriggerStatus.Enabled;
                }
                catch (TriggerMissingException)
                {
                    present = false;
                }
                if (!present)
                {
                    implementor.CreateTrigger(ThrowImmediatelyCheckNotifier.Quiet);
                }
            }

            var pkColumns = toProcess.PrimaryKey;
            var nonPkColumns = toProcess.Columns.Cast<DataColumn>().Where(dc => !pkColumns.Contains(dc)).ToArray();
            //merge
            var tmpTbl = db.CreateTable(
                out Dictionary<string, Guesser> _dataTypeDictionary,
                $"{(DeleteMergeTempTable ? "##" : "")}mergeTempTable_{toProcess.TableName}_{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff",CultureInfo.InvariantCulture).Replace('.','-')}",
                toProcess, null,true, null);
            var _managedConnection = tmpTbl.Database.Server.GetManagedConnection();
            var _bulkcopy = tmpTbl.BeginBulkInsert(CultureInfo.CurrentCulture, _managedConnection.ManagedTransaction);
            _bulkcopy.Upload(toProcess);
            _bulkcopy.Dispose();
            var mergeSql = $"""
                MERGE INTO {destinationTable.GetFullyQualifiedName()} WITH (HOLDLOCK) AS target
                USING {tmpTbl.GetFullyQualifiedName()} AS source
                    ON {string.Join(" AND ", pkColumns.Select(pkc => $"target.{pkc.ColumnName} = source.{pkc.ColumnName}"))}
                WHEN MATCHED THEN 
                    UPDATE SET {string.Join(" , ", nonPkColumns.Select(pkc => $"target.{pkc.ColumnName} = source.{pkc.ColumnName}"))}
                WHEN NOT MATCHED BY TARGET THEN
                    INSERT ({string.Join(" , ", toProcess.Columns.Cast<DataColumn>().Select(pkc => pkc.ColumnName))})
                    VALUES ({string.Join(" , ", toProcess.Columns.Cast<DataColumn>().Select(pkc => $"source.{pkc.ColumnName}"))});
                """;
            job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, $"{mergeSql}"));

            var cmd = new SqlCommand(mergeSql, (SqlConnection)_managedConnection.Connection);
            var rowCount = cmd.ExecuteNonQuery();
            job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, $"Merged {rowCount} rows into {destinationTable.GetFullyQualifiedName()}."));

            _managedConnection.Dispose();



        }
    }
}

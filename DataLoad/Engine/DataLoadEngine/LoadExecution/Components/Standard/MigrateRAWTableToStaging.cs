using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using CatalogueLibrary;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.DataFlowPipeline.Requirements;
using DataLoadEngine.DatabaseManagement;
using DataLoadEngine.DatabaseManagement.EntityNaming;
using DataLoadEngine.DataFlowPipeline.Components;
using DataLoadEngine.DataFlowPipeline.Components.Anonymisation;
using DataLoadEngine.DataFlowPipeline.Destinations;
using DataLoadEngine.DataFlowPipeline.Sources;
using DataLoadEngine.Job;
using DataLoadEngine.LoadProcess;
using HIC.Logging;
using ReusableLibraryCode;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using ReusableLibraryCode.Progress;

namespace DataLoadEngine.LoadExecution.Components.Standard
{
    /// <summary>
    /// Streams records from a single table in the RAW database and writes it to the corresponding table in the STAGING database during data load.  RAW is an 
    /// unconstrained identifiable version of the LIVE table created at the start of an RMDP data load (the RAW=>STAGING=>LIVE model).  STAGING is a constrained
    /// (has primary keys / not nulls etc) version of the LIVE table.  This class uses a DataFlowPipelineEngine to stream the records and this includes (optionally)
    /// any anonymisation operations (dropping columns, substituting identifiers etc) configured on the TableInfo (See BasicAnonymisationEngine).
    /// </summary>
    public class MigrateRAWTableToStaging : DataLoadComponent
    {
        private readonly TableInfo _tableInfo;
        private readonly bool _isLookupTable;
        private readonly HICDatabaseConfiguration _databaseConfiguration;

        public MigrateRAWTableToStaging(TableInfo tableInfo, bool isLookupTable, HICDatabaseConfiguration databaseConfiguration)
        {
            _tableInfo = tableInfo;
            _isLookupTable = isLookupTable;
            _databaseConfiguration = databaseConfiguration;
        }


        DataFlowPipelineEngine<DataTable> _pipeline;
        public override ExitCodeType Run(IDataLoadJob job, GracefulCancellationToken cancellationToken)
        {
            if(_pipeline != null)
                throw new Exception("Pipeline already executed once");
            
            var contextFactory = new DataFlowPipelineContextFactory<DataTable>();
            var context = contextFactory.Create(PipelineUsage.LoadsSingleTableInfo | PipelineUsage.FixedDestination | PipelineUsage.LogsToTableLoadInfo);
            
            //where we are coming from (source)
            var sourceConvention = LoadBubble.Raw;
            DiscoveredDatabase sourceDatabase = _databaseConfiguration.DeployInfo[sourceConvention];
            var sourceTableName = _tableInfo.GetRuntimeName(sourceConvention, _databaseConfiguration.DatabaseNamer);

            //What to do if where we are coming from does not have the table existing on it
            if (!sourceDatabase.ExpectTable(sourceTableName).Exists())
                if (_isLookupTable)
                {
                    job.OnNotify(this,
                        new NotifyEventArgs(ProgressEventType.Warning,
                            "Lookup table " + sourceTableName + " did not exist on RAW so was not migrated to STAGING"));
                    return ExitCodeType.Success;
                }
                else
                    job.OnNotify(this,
                        new NotifyEventArgs(ProgressEventType.Error,
                            "Table " + sourceTableName + " did not exist in RAW database " + sourceDatabase +
                            " when it came time to migrate RAW to STAGING (and the table is not a lookup)"));

            
            // where we are going to (destination)
            // ignore any columns that are marked for discard
            var destinationConvention = LoadBubble.Staging;
            DiscoveredDatabase destinationDatabase = _databaseConfiguration.DeployInfo[LoadBubble.Staging];
            var destinationTableName = _tableInfo.GetRuntimeName(destinationConvention, _databaseConfiguration.DatabaseNamer);
        
            DeleteFullyNullRecords(sourceTableName, sourceDatabase,job);

            //audit
            ITableLoadInfo tableLoadInfo = job.DataLoadInfo.CreateTableLoadInfo(
                "None required, if fails then simply drop Staging database and reload dataset", "STAGING:" + destinationTableName,
                new DataSource[] { new DataSource("RAW:" + sourceTableName, DateTime.Now) }, -1);

            //connect to source and open a reader! note that GetReaderForRAW will at this point preserve the state of the database such that any commands e.g. deletes will not have any effect even though ExecutePipeline has not been called!
            var source = new DbDataCommandDataFlowSource(
                "Select distinct * from "+sourceTableName,
                "Fetch data from " + sourceTableName,
                sourceDatabase.Server.Builder, 50000);
            
            //ignore those that are pre load discarded columns (unless they are dilution in which case they get passed through in a decrepid state instead of dumped entirely - these fields will still bein ANODump in pristene state btw)
            var columnNamesToIgnoreForBulkInsert = _tableInfo.PreLoadDiscardedColumns.Where(c => c.Destination != DiscardedColumnDestination.Dilute).Select(column => column.RuntimeColumnName).ToList();
            
            //pass pre load discard
            var destination = new SqlBulkInsertDestination(destinationDatabase, destinationTableName, columnNamesToIgnoreForBulkInsert);
            
            //engine that will move data
            _pipeline = new DataFlowPipelineEngine<DataTable>(context, source, destination, job);

            //add clean strings component
            _pipeline.Components.Add(new CleanStrings());

            //add dropping of preload discard columns
            _pipeline.Components.Add(new BasicAnonymisationEngine());
            
            _pipeline.Initialize(tableLoadInfo,_tableInfo);

            //tell it to move data
            _pipeline.ExecutePipeline(cancellationToken);

            return ExitCodeType.Success;
        }

    
        private void DeleteFullyNullRecords(string sourceTableName, DiscoveredDatabase dbInfo,IDataLoadJob job)
        {
            try
            {
                using (SqlConnection con = (SqlConnection) dbInfo.Server.GetConnection())
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand(

                        //Magical code that nukes blank/null rows - where all rows are blank/null
                        @"  ;with C(XmlCol) as
(
  select
    (select T.*
     for xml path('row'), type)
  from " + sourceTableName + @" as T
)
delete from C
where C.XmlCol.exist('row/*[. != """"]') = 0",con);

                    cmd.CommandTimeout = 500000;

                    int affectedRows = cmd.ExecuteNonQuery();

                    if (affectedRows != 0)
                        job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Warning, "Deleted " + affectedRows + " fully blank/null rows from RAW database"));
                }
            }
            catch (Exception e)
            {
                job.OnNotify(this,new NotifyEventArgs(ProgressEventType.Warning, "Could not delete fully null records, this will not prevent the data load ocurring",e));
            }
        }


        public override void LoadCompletedSoDispose(ExitCodeType exitCode,IDataLoadEventListener postLoadEventListener)
        {
        }

        /*
  
        private DataTable CreateAnonymisedDataTable(DataTable rawTable, TableInfo tableInfo, ReflectionFactory<IAnonymisationEngine> anonymisationEngineFactory,IDataLoadJob job)
        {
            DataTable stagingTable = DatabaseOperations.CreateDataTableFromDbOnServer(_databaseConfiguration.GetInfoForStage(HICTableNamingConvention.Staging), tableInfo.GetRuntimeName());
            for (var i = 0; i < rawTable.Rows.Count; ++i)
                stagingTable.Rows.Add(stagingTable.NewRow());

            var anoEngine = anonymisationEngineFactory.Create(_loadMetadata.AnonymisationEngineClass);
            anoEngine.Initialize(tableInfo, job, null);
            stagingTable = anoEngine.ProcessPipelineData(rawTable, job);
            return stagingTable;
        }*/
    }
}
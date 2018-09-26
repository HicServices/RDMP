using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using CatalogueLibrary.Data;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.DataFlowPipeline.Requirements;
using CatalogueLibrary.Repositories;
using HIC.Logging;
using HIC.Logging.Listeners;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DataAccess;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using ReusableLibraryCode.DatabaseHelpers.Discovery.Exceptions;
using ReusableLibraryCode.DatabaseHelpers.Discovery.TypeTranslation;
using ReusableLibraryCode.DatabaseHelpers.Discovery.TypeTranslation.TypeDeciders;
using ReusableLibraryCode.Progress;

namespace DataLoadEngine.DataFlowPipeline.Destinations
{
    /// <summary>
    /// Pipeline component (destination) which commits the DataTable(s) (in batches) to the DiscoveredDatabase (PreInitialize argument).  Supports cross platform 
    /// targets (MySql , Sql Server etc).  Normally the SQL Data Types and column names will be computed from the DataTable and a table will be created with the
    /// name of the DataTable being processed.  If a matching table already exists you can choose to load it anyway in which case a basic bulk insert will take 
    /// place.
    /// </summary>
    public class DataTableUploadDestination : IPluginDataFlowComponent<DataTable>, IDataFlowDestination<DataTable>, IPipelineRequirement<DiscoveredDatabase>
    {
        public const string LoggingServer_Description = "The logging server to log the upload to (leave blank to not bother auditing)";
        public const string AllowResizingColumnsAtUploadTime_Description = "If the target table being loaded has columns that are too small the destination will attempt to resize them";
        public const string OnlyUseOldDateTimes_Description = "Set to true if you want to restrict table creation to datetime instead of datetime2 (means any dates before 1753 will crash)";
        public const string AllowLoadingPopulatedTables_Description = "Normally when DataTableUploadDestination encounters a table that already contains records it will abandon the insertion attempt.  Set this to true to instead continue with the load.";
        public const string AlterTimeout_Description = "Timeout to perform all ALTER TABLE operations (column resize and PK creation)";

        [DemandsInitialization(LoggingServer_Description)]
        public ExternalDatabaseServer LoggingServer { get; set; }

        [DemandsInitialization(AllowResizingColumnsAtUploadTime_Description, DefaultValue = true)]
        public bool AllowResizingColumnsAtUploadTime { get; set; }
        
        [DemandsInitialization(OnlyUseOldDateTimes_Description)]
        public bool OnlyUseOldDateTimes { get; set; }

        [DemandsInitialization(AllowLoadingPopulatedTables_Description, DefaultValue = false)]
        public bool AllowLoadingPopulatedTables { get; set; }
        
        [DemandsInitialization(AlterTimeout_Description, DefaultValue = 300)]
        public int AlterTimeout { get; set; }

        public string TargetTableName { get; private set; }
        
        private IBulkCopy _bulkcopy;
        private int _affectedRows = 0;
        Stopwatch swTimeSpentWritting = new Stopwatch();

        private DiscoveredServer _loggingDatabaseSettings;

        private DiscoveredServer _server;
        private DiscoveredDatabase _database;

        private DataLoadInfo _dataLoadInfo;

        private IManagedConnection _managedConnection;
        private ToLoggingDatabaseDataLoadEventListener _loggingDatabaseListener;

        public List<DatabaseColumnRequest> ExplicitTypes { get; set; }

        private bool _firstTime = true;
        private HashSet<string> _primaryKey = new HashSet<string>(StringComparer.CurrentCultureIgnoreCase);
        private DiscoveredTable discoveredTable;

        //All column values sent to server so far
        Dictionary<string, DataTypeComputer> _dataTypeDictionary;

        public DataTableUploadDestination()
        {
            ExplicitTypes = new List<DatabaseColumnRequest>();
        }

        public DataTable ProcessPipelineData(DataTable toProcess, IDataLoadEventListener listener, GracefulCancellationToken cancellationToken)
        {
            if (toProcess == null)
                return null;

            //work out the table name for the table we are going to create
            if (TargetTableName == null)
            {
                if (string.IsNullOrWhiteSpace(toProcess.TableName))
                    throw new Exception("Chunk did not have a TableName, did not know what to call the newly created table");
                
                TargetTableName = QuerySyntaxHelper.MakeHeaderNameSane(toProcess.TableName);
            }

            //handle primary keyness by removing it until Dispose step
            foreach (var pkCol in toProcess.PrimaryKey.Select(dc => dc.ColumnName))
                _primaryKey.Add(pkCol);
            
            toProcess.PrimaryKey = new DataColumn[0];

            foreach (var dcr in ExplicitTypes.Where(dcr => dcr.IsPrimaryKey))
            {
                dcr.IsPrimaryKey = false;
                _primaryKey.Add(dcr.ColumnName);
            }

            StartAuditIfExists(TargetTableName);

            if (_loggingDatabaseListener != null)
                listener = new ForkDataLoadEventListener(listener, _loggingDatabaseListener);

            bool createdTable = false;

            if (_firstTime)
            {
                bool tableAlreadyExistsButEmpty = false;

                if (!_database.Exists())
                    throw new Exception("Database " + _database + " does not exist");

                discoveredTable = _database.ExpectTable(TargetTableName);

                //table already exists
                if (discoveredTable.Exists())
                {
                    tableAlreadyExistsButEmpty = true;
                    
                    if(!AllowLoadingPopulatedTables)
                        if (discoveredTable.IsEmpty())
                            listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Warning, "Found table " + TargetTableName + " already, normally this would forbid you from loading it (data duplication / no primary key etc) but it is empty so we are happy to load it, it will not be created"));
                        else
                            throw new Exception("There is already a table called " + TargetTableName + " at the destination " + _database);

                    _dataTypeDictionary = discoveredTable.DiscoverColumns().ToDictionary(k => k.GetRuntimeName(), v => v.GetDataTypeComputer());
                }
                else
                    listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "Determined that the table name " + TargetTableName + " is unique at destination " + _database));
                
                EnsureTableHasDataInIt(toProcess);
                
                //create connection to destination
               if (!tableAlreadyExistsButEmpty)
               {
                   createdTable = true;
                   _database.CreateTable(out _dataTypeDictionary, TargetTableName, toProcess, ExplicitTypes.ToArray(), true);
                   listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "Created table " + TargetTableName + " successfully."));
                }

                _managedConnection = _server.BeginNewTransactedConnection();
                _bulkcopy = discoveredTable.BeginBulkInsert(_managedConnection.ManagedTransaction);

                _firstTime = false;
            }

            try
            {
                if (AllowResizingColumnsAtUploadTime && !createdTable)
                    ResizeColumnsIfRequired(toProcess, listener);

                //push the data
                swTimeSpentWritting.Start();

                _affectedRows += _bulkcopy.Upload(toProcess);
                    
                swTimeSpentWritting.Stop();
                listener.OnProgress(this, new ProgressEventArgs("Uploading to " + TargetTableName, new ProgressMeasurement(_affectedRows, ProgressType.Records), swTimeSpentWritting.Elapsed));
            }
            catch (Exception e)
            {
                _managedConnection.ManagedTransaction.AbandonAndCloseConnection();

                if (LoggingServer != null)
                    FatalErrorLogging.GetInstance().LogFatalError(_dataLoadInfo, GetType().Name, ExceptionHelper.ExceptionToListOfInnerMessages(e, true));

                throw new Exception("Failed to write rows (in transaction) to table " + TargetTableName, e);
            }

            return null;
        }

        private void EnsureTableHasDataInIt(DataTable toProcess)
        {
            if(toProcess.Columns.Count == 0)
                throw new Exception("DataTable '" + toProcess + "' had no Columns!");

            if (toProcess.Rows.Count == 0)
                throw new Exception("DataTable '" + toProcess + "' had no Rows!");
        }

        private void ResizeColumnsIfRequired(DataTable toProcess, IDataLoadEventListener listener)
        {
            var tbl = _database.ExpectTable(TargetTableName);
            var typeTranslater = tbl.GetQuerySyntaxHelper().TypeTranslater;
            
            //Get the current estimates from the datatype computer
            Dictionary<string, string> oldTypes = _dataTypeDictionary.ToDictionary(k => k.Key, v => v.Value.GetSqlDBType(typeTranslater));

            //adjust the computer to 
            //work out the max sizes - expensive bit
            foreach (DataRow row in toProcess.Rows)
                //for each destination column
                foreach (string col in _dataTypeDictionary.Keys)
                    //if it appears in the toProcess DataTable
                    if (toProcess.Columns.Contains(col))
                        //run the datatype computer over it to compute max lengths
                        _dataTypeDictionary[col].AdjustToCompensateForValue(row[col]);

            //see if any have changed
            foreach (DataColumn column in toProcess.Columns)
            {
                //get what is required for the current batch and the current type that is configured in the live table
                string oldSqlType = oldTypes[column.ColumnName];
                string newSqlType = _dataTypeDictionary[column.ColumnName].GetSqlDBType(typeTranslater);

                bool changesMade = false;

                //if the SQL data type has degraded e.g. varchar(10) to varchar(50) or datetime to varchar(20)
                if(oldSqlType != newSqlType)
                {
                    listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Warning, "Resizing column '" + column + "' from '" + oldSqlType + "' to '" + newSqlType + "'"));

                    var col = tbl.DiscoverColumn(column.ColumnName,_managedConnection.ManagedTransaction);

                    //try changing the Type to the legit type
                    col.DataType.AlterTypeTo(newSqlType, _managedConnection.ManagedTransaction, AlterTimeout);
                    
                    changesMade = true;
                }

                if(changesMade)
                    _bulkcopy.InvalidateTableSchema();
            }
        }

        public void Abort(IDataLoadEventListener listener)
        {
            _managedConnection.ManagedTransaction.AbandonAndCloseConnection();
        }

        public void Dispose(IDataLoadEventListener listener, Exception pipelineFailureExceptionIfAny)
        {
            try
            {
                if (_managedConnection != null)
                {
                    //if there was an error
                    if (pipelineFailureExceptionIfAny != null)
                    {
                        _managedConnection.ManagedTransaction.AbandonAndCloseConnection();
                        
                        listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "Transaction rolled back sucessfully"));

                        if (_bulkcopy != null)
                            _bulkcopy.Dispose();
                    }
                    else
                    {
                        _managedConnection.ManagedTransaction.CommitAndCloseConnection();

                        if (_bulkcopy != null)
                            _bulkcopy.Dispose();

                        listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "Transaction committed sucessfully"));
                    }
                }
            }
            catch (Exception e)
            {
                listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Error, "Commit failed on transaction (probably there was a previous error?)", e));
            }

            //if we have a primary key to create
            if (pipelineFailureExceptionIfAny == null && _primaryKey != null && _primaryKey.Any() && discoveredTable != null && discoveredTable.Exists())
            {
                //Find the columns in the destination
                var allColumns = discoveredTable.DiscoverColumns();
                
                //if there are not yet any primary keys
                if(allColumns.All(c=>!c.IsPrimaryKey))
                {
                    //find the columns the user decorated in his DataTable
                    DiscoveredColumn[] pkColumnsToCreate = allColumns.Where(c => _primaryKey.Any(pk => pk.Equals(c.GetRuntimeName(), StringComparison.CurrentCultureIgnoreCase))).ToArray();

                    //make sure we found all of them
                    if(pkColumnsToCreate.Length != _primaryKey.Count)
                        throw new Exception("Could not find primary key column(s) " + string.Join(",",_primaryKey) + " in table " + discoveredTable);

                    //create the primary key to match user provided columns
                    discoveredTable.CreatePrimaryKey(AlterTimeout, pkColumnsToCreate);
                }
            }

            EndAuditIfExists();
        }

        private void EndAuditIfExists()
        {
            //user is auditing
            if (_loggingDatabaseListener != null)
                _loggingDatabaseListener.FinalizeTableLoadInfos();
        }

        public void Check(ICheckNotifier notifier)
        {
            if (LoggingServer != null)
            {
                new LoggingDatabaseChecker(LoggingServer).Check(notifier);
            }
            else
            {
                notifier.OnCheckPerformed(
                    new CheckEventArgs(
                        "There is no logging server so there will be no audit of this destinations activities",
                        CheckResult.Success));
            }
        }

        private void StartAuditIfExists(string tableName)
        {
            if (LoggingServer != null)
            {
                
                _loggingDatabaseSettings = DataAccessPortal.GetInstance().ExpectServer(LoggingServer, DataAccessContext.Logging);
                var logManager = new LogManager(_loggingDatabaseSettings);
                _dataLoadInfo = (DataLoadInfo) logManager.CreateDataLoadInfo("Internal", GetType().Name, "Loading table " + tableName, "", false);
                _loggingDatabaseListener = new ToLoggingDatabaseDataLoadEventListener(logManager, _dataLoadInfo);
            }
        }

        public void PreInitialize(DiscoveredDatabase value, IDataLoadEventListener listener)
        {
            _database = value;
            _server = value.Server;
        }
        
        /// <summary>
        /// Declare that the column of name columnName (which might or might not appear in DataTables being uploaded) should always have the associated database type (e.g. varchar(59))
        /// The columnName is Case insensitive.  Note that if AllowResizingColumnsAtUploadTime is true then these datatypes are only the starting types and might get changed later to
        /// accomodate new data.
        /// </summary>
        /// <param name="columnName"></param>
        /// <param name="explicitType"></param>
        /// <param name="columnFlags"></param>
        /// <returns>The Column Request that has been added to the array</returns>
        public DatabaseColumnRequest AddExplicitWriteType(string columnName, string explicitType, ISupplementalColumnInformation columnFlags = null)
        {
            DatabaseColumnRequest columnRequest;
            
            if (columnFlags == null)
            {
                columnRequest = new DatabaseColumnRequest(columnName, explicitType, true);
                ExplicitTypes.Add(columnRequest);
                return columnRequest;
            }
            else
            {
                columnRequest = new DatabaseColumnRequest(columnName, explicitType, !columnFlags.IsPrimaryKey && !columnFlags.IsAutoIncrement)
                {
                    IsPrimaryKey = columnFlags.IsPrimaryKey,
                    IsAutoIncrement = columnFlags.IsAutoIncrement,
                    Collation = columnFlags.Collation
                };

                ExplicitTypes.Add(columnRequest);
                return columnRequest;
            }
        }
    }
}

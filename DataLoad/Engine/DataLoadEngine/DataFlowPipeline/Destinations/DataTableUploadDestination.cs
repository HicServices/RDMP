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
using ReusableLibraryCode.DatabaseHelpers.Discovery.TypeTranslation;
using ReusableLibraryCode.Progress;

namespace DataLoadEngine.DataFlowPipeline.Destinations
{
    public class DataTableUploadDestination : IPluginDataFlowComponent<DataTable>, IDataFlowDestination<DataTable>, IPipelineRequirement<DiscoveredDatabase>
    {
        [DemandsInitialization("The logging server to log the upload to (leave blank to not bother auditing)")]
        public ExternalDatabaseServer LoggingServer { get; set; }

        [DemandsInitialization("If the target table being loaded has columns that are too small the destination will attempt to resize them", DemandType.Unspecified, true)]
        public bool AllowResizingColumnsAtUploadTime { get; set; }

        [DemandsInitialization("Set to true if you want to restrict table creation to datetime instead of datetime2 (means any dates before 1753 will crash)", DemandType.Unspecified,false)]
        public bool OnlyUseOldDateTimes { get; set; }

        [DemandsInitialization("Normally when DataTableUploadDestination encounters a table that already contains records it will abandon the insertion attempt.  Set this to true to instead continue with the load.", DefaultValue = false)]
        public bool AllowLoadingPopulatedTables { get; set; }

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

        List<DatabaseColumnRequest> explicitTypes = new List<DatabaseColumnRequest>();

        private bool _firstTime = true;

        public DataTable ProcessPipelineData(DataTable toProcess, IDataLoadEventListener listener, GracefulCancellationToken cancellationToken)
        {
            string whoCares;//used for out parameters we are not using

            //work out the table name for the table we are going to create
            if (TargetTableName == null)
            {
                if (string.IsNullOrWhiteSpace(toProcess.TableName))
                    throw new Exception("Chunk did not have a TableName, did not know what to call the newly created table");


                TargetTableName = QuerySyntaxHelper.MakeHeaderNameSane(toProcess.TableName);
            }

            StartAuditIfExists(TargetTableName);

            if (_loggingDatabaseListener != null)
                listener = new ForkDataLoadEventListener(listener, _loggingDatabaseListener);

            if (_firstTime)
            {
                bool tableAlreadyExistsButEmpty = false;

                if (!_database.Exists())
                    throw new Exception("Database " + _database + " does not exist");

                var discoveredTable = _database.ExpectTable(TargetTableName);

                //table already exists
                if (discoveredTable.Exists())
                {
                    tableAlreadyExistsButEmpty = true;
                    
                    if(!AllowLoadingPopulatedTables)
                        if (discoveredTable.GetRowCount() == 0)
                            listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Warning, "Found table " + TargetTableName + " already, normally this would forbid you from loading it (data duplication / no primary key etc) but it is empty so we are happy to load it, it will not be created"));
                        else
                            throw new Exception("There is already a table called " + TargetTableName + " at the destination " + _database);
                }
                else
                    listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "Determined that the table name " + TargetTableName + " is unique at destination " + _database));
                
                EnsureTableHasDataInIt(toProcess);
                
                //create connection to destination
               if (!tableAlreadyExistsButEmpty)
               {
                   _database.CreateTable(TargetTableName, toProcess, explicitTypes.ToArray(), true);
                   listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "Created table " + TargetTableName + " successfully."));
                }

                _managedConnection = _server.BeginNewTransactedConnection();
                _bulkcopy = discoveredTable.BeginBulkInsert(_managedConnection.ManagedTransaction);

                _firstTime = false;
            }

            try
            {
                if (AllowResizingColumnsAtUploadTime)
                    ResizeColumnsIfRequired(toProcess, listener);

                //push the data
                if (toProcess != null)
                {
                    swTimeSpentWritting.Start();

                    _affectedRows += _bulkcopy.Upload(toProcess);
                    
                    swTimeSpentWritting.Stop();
                    listener.OnProgress(this, new ProgressEventArgs("Uploading to " + TargetTableName, new ProgressMeasurement(_affectedRows, ProgressType.Records), swTimeSpentWritting.Elapsed));

                }
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

            Dictionary<string, int> currentDatabaseStringSizes = new Dictionary<string, int>();
            Dictionary<string, Pair<int, int>> currentDatabaseDecimalSizes = new Dictionary<string, Pair<int, int>>();

            Dictionary<string, DataTypeComputer> thisBatch = new Dictionary<string, DataTypeComputer>();

            List<string> bitColumns = new List<string>();

            //go interrogate the database about the current size
            DiscoveredColumn[] columns = _database.ExpectTable(TargetTableName).DiscoverColumns(_managedConnection.ManagedTransaction).ToArray();

            //build 2 dictionaries one with the current database size, the other with 0s which will become the size of the toProcess columns
            foreach (var column in columns)
            {
                int length = column.DataType.GetLengthIfString();
                if (length > 0)
                {
                    currentDatabaseStringSizes.Add(column.GetRuntimeName(), length);
                    thisBatch.Add(column.GetRuntimeName(), new DataTypeComputer());
                }

                var pair = column.DataType.GetDigitsBeforeAndAfterDecimalPointIfDecimal();
                if (pair != null)
                {
                    currentDatabaseDecimalSizes.Add(column.GetRuntimeName(), pair);
                    thisBatch.Add(column.GetRuntimeName(), new DataTypeComputer());
                }

                if (column.DataType.SQLType.Equals("bit"))
                {
                    bitColumns.Add(column.GetRuntimeName());
                    thisBatch.Add(column.GetRuntimeName(), new DataTypeComputer());
                }
            }

            //work out the max sizes - expensive bit
            foreach (DataRow row in toProcess.Rows)
                foreach (string col in thisBatch.Keys.ToArray())
                    thisBatch[col].AdjustToCompensateForValue(row[col]);//run the datatype computer over it

            //cheap bit
            foreach (KeyValuePair<string, DataTypeComputer> kvp in thisBatch)
            {
                //handle type change from bit to more advanced type
                var currentDatatype = kvp.Value.GetSqlDBType(_database.Server);


                //if it is currently a bit in the database and it isn't a bit in the DataTypeComputer (current batch)
                if (bitColumns.Contains(kvp.Key) && !currentDatatype.Equals("bit"))
                {

                    //warn the user of the change
                    listener.OnNotify(this,
                        new NotifyEventArgs(ProgressEventType.Warning,
                            "Altering Column '" + kvp.Key + "' to " + currentDatatype +
                            " based on the latest batch containing values non bit/null values"));

                    columns.Single(c => c.GetRuntimeName().Equals(kvp.Key)).DataType.AlterTypeTo(currentDatatype, _managedConnection.ManagedTransaction);

                }


                //handle longer strings
                if (currentDatabaseStringSizes.ContainsKey(kvp.Key))
                    if (currentDatabaseStringSizes[kvp.Key] < kvp.Value.Length) //string is longer in the batch than the database
                    {
                        //warn the user of the change
                        listener.OnNotify(this,
                            new NotifyEventArgs(ProgressEventType.Warning,
                                "Resizing column '" + kvp.Key + "' to size " + kvp.Value.Length +
                                " based on the latest batch containing values longer than the first seen batch values! Old size was " +
                                currentDatabaseStringSizes[kvp.Key]));

                        //attempt to make the change
                        columns.Single(c => c.GetRuntimeName().Equals(kvp.Key)).DataType.Resize(kvp.Value.Length, _managedConnection.ManagedTransaction);
                    }

                //handle longer decimals
                if (currentDatabaseDecimalSizes.ContainsKey(kvp.Key))
                {
                    //if we need more space on either side of the decimal point
                    if (currentDatabaseDecimalSizes[kvp.Key].First < kvp.Value.numbersBeforeDecimalPlace
                        ||
                        currentDatabaseDecimalSizes[kvp.Key].Second < kvp.Value.numbersAfterDecimalPlace)
                    {
                        int newBeforeDecimalPoint = Math.Max(currentDatabaseDecimalSizes[kvp.Key].First, kvp.Value.numbersBeforeDecimalPlace);
                        int newAfterDecimalPoint = Math.Max(currentDatabaseDecimalSizes[kvp.Key].Second, kvp.Value.numbersAfterDecimalPlace);

                        var col = columns.Single(c => c.GetRuntimeName().Equals(kvp.Key));

                        //warn the user of the change
                        listener.OnNotify(this,
                            new NotifyEventArgs(ProgressEventType.Warning,
                                "Resizing column '" + kvp.Key + "' to size (" + (newBeforeDecimalPoint + newAfterDecimalPoint) + "," + newAfterDecimalPoint + ")" +
                                " based on the latest batch containing values longer than the first seen batch values! Old size was " +
                                col.DataType.SQLType));

                        col.DataType.Resize(newBeforeDecimalPoint, newAfterDecimalPoint, _managedConnection.ManagedTransaction);

                    }
                }
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
                _managedConnection.ManagedTransaction.CommitAndCloseConnection();
                
                if (_bulkcopy != null)
                    _bulkcopy.Dispose();

                listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "Transaction committed sucessfully"));
            }
            catch (Exception e)
            {
                listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Error, "Commit failed on transaction (probably there was a previous error?)", e));

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

        public void AddExplicitWriteType(string columnName, string explicitType)
        {
            explicitTypes.Add(new DatabaseColumnRequest(columnName, explicitType, true));
        }
    }
}

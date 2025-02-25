// Copyright (c) The University of Dundee 2018-2024
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using FAnsi.Connections;
using FAnsi.Discovery;
using FAnsi.Discovery.TableCreation;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataFlowPipeline.Requirements;
using Rdmp.Core.DataLoad.Triggers.Implementations;
using Rdmp.Core.DataLoad.Triggers;
using Rdmp.Core.Logging;
using Rdmp.Core.Logging.Listeners;
using Rdmp.Core.Repositories.Construction;
using Rdmp.Core.ReusableLibraryCode;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.DataAccess;
using Rdmp.Core.ReusableLibraryCode.Progress;
using TypeGuesser;
using FAnsi;

namespace Rdmp.Core.DataLoad.Engine.Pipeline.Destinations;

/// <summary>
/// Pipeline component (destination) which commits the DataTable(s) (in batches) to the DiscoveredDatabase (PreInitialize argument).  Supports cross platform
/// targets (MySql , Sql Server etc).  Normally the SQL Data Types and column names will be computed from the DataTable and a table will be created with the
/// name of the DataTable being processed.  If a matching table already exists you can choose to load it anyway in which case a basic bulk insert will take
/// place.
/// </summary>
public class DataTableUploadDestination : IPluginDataFlowComponent<DataTable>, IDataFlowDestination<DataTable>,
    IPipelineRequirement<DiscoveredDatabase>
{
    public const string LoggingServer_Description =
        "The logging server to log the upload to (leave blank to not bother auditing)";

    public const string AllowResizingColumnsAtUploadTime_Description =
        "If the target table being loaded has columns that are too small the destination will attempt to resize them";

    public const string AllowLoadingPopulatedTables_Description =
        "Normally when DataTableUploadDestination encounters a table that already contains records it will abandon the insertion attempt.  Set this to true to instead continue with the load.";

    public const string AlterTimeout_Description =
        "Timeout to perform all ALTER TABLE operations (column resize and PK creation)";

    [DemandsInitialization(LoggingServer_Description)]
    public ExternalDatabaseServer LoggingServer { get; set; }

    [DemandsInitialization(AllowResizingColumnsAtUploadTime_Description, DefaultValue = true)]
    public bool AllowResizingColumnsAtUploadTime { get; set; }

    [DemandsInitialization(AllowLoadingPopulatedTables_Description, DefaultValue = false)]
    public bool AllowLoadingPopulatedTables { get; set; }

    [DemandsInitialization(AlterTimeout_Description, DefaultValue = 300)]
    public int AlterTimeout { get; set; }

    [DemandsInitialization("Optional - Change system behaviour when a new table is being created by the component",
        TypeOf = typeof(IDatabaseColumnRequestAdjuster))]
    public Type Adjuster { get; set; }

    public bool AppendDataIfTableExists { get; set; }

    public bool IncludeTimeStamp { get; set; }

    private CultureInfo _culture;

    [DemandsInitialization("The culture to use for uploading (determines date format etc)")]
    public CultureInfo Culture
    {
        get => _culture ?? CultureInfo.CurrentCulture;
        set => _culture = value;
    }

    public string TargetTableName { get; private set; }

    /// <summary>
    /// True if a new table was created or re-created by the execution of this destination.  False if
    /// the table already existed e.g. data was simply added
    /// </summary>
    public bool CreatedTable { get; private set; }
    public bool UseTrigger { get; set; } = false;

    public bool IndexTables { get; set; } = false;
    public string IndexTableName { get; set; }
    public List<String> UserDefinedIndexes { get; set; } = new();

    private IBulkCopy _bulkcopy;
    private int _affectedRows;

    private Stopwatch swTimeSpentWriting = new();
    private Stopwatch swMeasuringStrings = new();

    private DiscoveredServer _loggingDatabaseSettings;

    private DiscoveredServer _server;
    private DiscoveredDatabase _database;

    private DataLoadInfo _dataLoadInfo;

    private IManagedConnection _managedConnection;
    private ToLoggingDatabaseDataLoadEventListener _loggingDatabaseListener;

    public List<DatabaseColumnRequest> ExplicitTypes { get; set; }

    private bool _firstTime = true;
    private HashSet<string> _primaryKey = new(StringComparer.CurrentCultureIgnoreCase);
    private DiscoveredTable _discoveredTable;
    private readonly string _extractionTimeStamp = "extraction_timestamp";

    private readonly IExternalCohortTable _externalCohortTable;

    //All column values sent to server so far
    private Dictionary<string, Guesser> _dataTypeDictionary;

    /// <summary>
    /// Optional function called when a name is needed for the table being uploaded (this overrides
    /// upstream components naming of tables - e.g. from file names).
    /// </summary>
    public Func<string> TableNamerDelegate { get; set; }

    public DataTableUploadDestination()
    {
        ExplicitTypes = new List<DatabaseColumnRequest>();
    }

    public DataTableUploadDestination(IExternalCohortTable externalCohortTable)
    {
        ExplicitTypes = new List<DatabaseColumnRequest>();
        _externalCohortTable = externalCohortTable;
    }

    private static object[] FilterOutItemAtIndex(object[] itemArray, int[] indexes)
    {
        if (indexes.Length == 0) return itemArray;
        return itemArray.Where((source, idx) => !indexes.Contains(idx)).ToArray();
    }
    private string GetPKValue(DataColumn pkColumn, DataRow row)
    {
        var pkName = pkColumn.ColumnName;
        var value = row[pkName];
        if (_externalCohortTable is not null)
        {
            var privateIdentifierField = _externalCohortTable.PrivateIdentifierField.Split('.').Last()[1..^1];//remove the "[]" from the identifier field
            var releaseIdentifierField = _externalCohortTable.ReleaseIdentifierField.Split('.').Last()[1..^1];//remove the "[]" from the identifier field
            if (pkName == releaseIdentifierField)
            {
                //going to have to look up the previous relaseID to match
                DiscoveredTable cohortTable = _externalCohortTable.DiscoverCohortTable();
                using var lookupDT = cohortTable.GetDataTable();
                var releaseIdIndex = lookupDT.Columns.IndexOf(releaseIdentifierField);
                var privateIdIndex = lookupDT.Columns.IndexOf(privateIdentifierField);
                var foundRow = lookupDT.Rows.Cast<DataRow>().Where(r => r.ItemArray[releaseIdIndex].ToString() == value.ToString()).LastOrDefault();
                if (foundRow is not null)
                {
                    var originalValue = foundRow.ItemArray[privateIdIndex];
                    var existingIDsforReleaseID = lookupDT.Rows.Cast<DataRow>().Where(r => r.ItemArray[privateIdIndex].ToString() == originalValue.ToString()).Select(s => s.ItemArray[releaseIdIndex].ToString());
                    if (existingIDsforReleaseID.Count() > 0)
                    {
                        //we don't know what the current release ID is ( there may be ones from multiple cohorts)
                        var ids = existingIDsforReleaseID.Select(id => $"'{id}'");
                        return $"{pkName} in ({string.Join(',', ids)})";
                    }
                }
            }
        }
        return $"{pkName} = '{value}'";
    }


    public DataTable ProcessPipelineData(DataTable toProcess, IDataLoadEventListener listener,
        GracefulCancellationToken cancellationToken)
    {
        if (toProcess == null)
            return null;

        var rowsToModify = new List<DataRow>();
        var pkColumns = toProcess.PrimaryKey;
        RemoveInvalidCharactersInSchema(toProcess);

        IDatabaseColumnRequestAdjuster adjuster = null;
        if (Adjuster != null) adjuster = (IDatabaseColumnRequestAdjuster)ObjectConstructor.Construct(Adjuster);

        //work out the table name for the table we are going to create
        if (TargetTableName == null)
        {
            if (TableNamerDelegate != null)
            {
                TargetTableName = TableNamerDelegate();
                if (string.IsNullOrWhiteSpace(TargetTableName))
                    throw new Exception("No table name specified (TableNamerDelegate returned null)");
            }
            else if (string.IsNullOrWhiteSpace(toProcess.TableName))
            {
                throw new Exception(
                    "Chunk did not have a TableName, did not know what to call the newly created table");
            }
            else
            {
                TargetTableName = QuerySyntaxHelper.MakeHeaderNameSensible(toProcess.TableName);
            }
        }

        ClearPrimaryKeyFromDataTableAndExplicitWriteTypes(toProcess);

        StartAuditIfExists(TargetTableName);

        if (IncludeTimeStamp)
        {
            AddTimeStampToExtractionData(toProcess);
        }

        if (_loggingDatabaseListener != null)
            listener = new ForkDataLoadEventListener(listener, _loggingDatabaseListener);

        EnsureTableHasDataInIt(toProcess);

        CreatedTable = false;

        if (_firstTime)
        {
            var tableAlreadyExistsButEmpty = false;

            if (!_database.Exists())
                throw new Exception($"Database {_database} does not exist");

            _discoveredTable = _database.ExpectTable(TargetTableName);

            //table already exists
            if (_discoveredTable.Exists())
            {
                tableAlreadyExistsButEmpty = true;

                if (!AllowLoadingPopulatedTables)
                    if (_discoveredTable.IsEmpty())
                        listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Warning,
                            $"Found table {TargetTableName} already, normally this would forbid you from loading it (data duplication / no primary key etc) but it is empty so we are happy to load it, it will not be created"));
                    else if (!AppendDataIfTableExists)
                        throw new Exception(
                            $"There is already a table called {TargetTableName} at the destination {_database}");

                if (AllowResizingColumnsAtUploadTime)
                    _dataTypeDictionary = _discoveredTable.DiscoverColumns().ToDictionary(k => k.GetRuntimeName(),
                        v => v.GetGuesser(), StringComparer.CurrentCultureIgnoreCase);
            }
            else
            {
                listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information,
                    $"Determined that the table name {TargetTableName} is unique at destination {_database}"));
            }

            //create connection to destination
            if (!tableAlreadyExistsButEmpty)
            {
                CreatedTable = true;

                if (AllowResizingColumnsAtUploadTime)
                    _database.CreateTable(out _dataTypeDictionary, TargetTableName, toProcess, ExplicitTypes.ToArray(),
                        true, adjuster);
                else
                    _database.CreateTable(TargetTableName, toProcess, ExplicitTypes.ToArray(), true, adjuster);

                listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information,
                    $"Created table {TargetTableName} successfully."));
            }

            _managedConnection = _server.BeginNewTransactedConnection();
            _bulkcopy = _discoveredTable.BeginBulkInsert(Culture, _managedConnection.ManagedTransaction);
            _firstTime = false;
        }

        if (IncludeTimeStamp && _discoveredTable.DiscoverColumns().All(c => c.GetRuntimeName() != _extractionTimeStamp))
        {
            _discoveredTable.AddColumn(_extractionTimeStamp, new DatabaseTypeRequest(typeof(DateTime)), true, 30000);
        }

        if (IndexTables)
        {
            var indexes = UserDefinedIndexes.Count != 0 ? UserDefinedIndexes : pkColumns.Select(c => c.ColumnName);
            try
            {
                _discoveredTable.CreateIndex(IndexTableName, _discoveredTable.DiscoverColumns().Where(c => indexes.Contains(c.GetRuntimeName())).ToArray());
            }
            catch (Exception e)
            {
                //We only warn about not creating the index, as it's not  critical
                listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Warning, e.Message));
            }
        }
        if (UseTrigger && pkColumns.Length > 0)
        {
            if (listener.GetType() == typeof(ForkDataLoadEventListener)) //need to add special fields to the datatable if we are logging to a database
            {
                var job = (ForkDataLoadEventListener)listener;
                var listeners = job.GetToLoggingDatabaseDataLoadEventListenersIfany();
                foreach (var dleListener in listeners)
                {
                    IDataLoadInfo dataLoadInfo = dleListener.DataLoadInfo;
                    DataColumn newColumn = new(SpecialFieldNames.DataLoadRunID, typeof(int))
                    {
                        DefaultValue = dataLoadInfo.ID
                    };
                    try
                    {
                        _discoveredTable.DiscoverColumn(SpecialFieldNames.DataLoadRunID);
                    }
                    catch (Exception)
                    {
                        _discoveredTable.AddColumn(SpecialFieldNames.DataLoadRunID, new DatabaseTypeRequest(typeof(int)), true, 30000);

                    }
                    if (!toProcess.Columns.Contains(SpecialFieldNames.DataLoadRunID))
                        toProcess.Columns.Add(newColumn);
                    foreach (DataRow dr in toProcess.Rows)
                        dr[SpecialFieldNames.DataLoadRunID] = dataLoadInfo.ID;

                }
            }
        }

        try
        {
            if (AllowResizingColumnsAtUploadTime && !CreatedTable)
                ResizeColumnsIfRequired(toProcess, listener);

            swTimeSpentWriting.Start();
            if (AppendDataIfTableExists && pkColumns.Length > 0) //assumes columns are the same
            {
                //drop any pk clashes
                var existingData = _discoveredTable.GetDataTable();
                var rowsToDelete = new List<DataRow>();
                var releaseIdentifier = _externalCohortTable is not null ? _externalCohortTable.ReleaseIdentifierField.Split('.').Last()[1..^1] : "ReleaseId";
                int[] toProcessIgnoreColumns = [toProcess.Columns.IndexOf(SpecialFieldNames.DataLoadRunID), toProcess.Columns.IndexOf(releaseIdentifier)];
                int[] existingDataIgnoreColumns = [existingData.Columns.IndexOf(SpecialFieldNames.DataLoadRunID), existingData.Columns.IndexOf(releaseIdentifier), existingData.Columns.IndexOf(SpecialFieldNames.ValidFrom)];
                foreach (DataRow row in toProcess.Rows)
                {

                    foreach (DataColumn pkCol in pkColumns)
                    {
                        bool clash = false;
                        if (_externalCohortTable is not null && pkCol.ColumnName == _externalCohortTable.ReleaseIdentifierField.Split('.').Last()[1..^1])
                        {
                            // If it's a cohort release identifier
                            // look up the original value and check we've not already extected the same value under a different release ID
                            var privateIdentifierField = _externalCohortTable.PrivateIdentifierField.Split('.').Last()[1..^1];//remove the "[]" from the identifier field
                            var releaseIdentifierField = _externalCohortTable.ReleaseIdentifierField.Split('.').Last()[1..^1];//remove the "[]" from the identifier field
                            var cohortTable = _externalCohortTable.DiscoverCohortTable();
                            using var lookupDT = cohortTable.GetDataTable();
                            var releaseIdIndex = lookupDT.Columns.IndexOf(releaseIdentifierField);
                            var privateIdIndex = lookupDT.Columns.IndexOf(privateIdentifierField);
                            var foundRow = lookupDT.Rows.Cast<DataRow>().FirstOrDefault(r => r.ItemArray[releaseIdIndex].ToString() == row[pkCol.ColumnName].ToString());
                            if (foundRow is not null)
                            {
                                var originalValue = foundRow.ItemArray[privateIdIndex];
                                var existingIDsforReleaseID = lookupDT.Rows.Cast<DataRow>().Where(r => r.ItemArray[privateIdIndex].ToString() == originalValue.ToString()).Select(s => s.ItemArray[releaseIdIndex].ToString());
                                clash = existingData.AsEnumerable().Any(r => existingIDsforReleaseID.Contains(r[pkCol.ColumnName].ToString()));
                            }
                        }
                        else
                        {
                            var val = row[pkCol.ColumnName];
                            clash = existingData.AsEnumerable().Any(r => r[pkCol.ColumnName].ToString() == val.ToString());

                        }
                        if (clash && UseTrigger)
                        {
                            if (existingData.AsEnumerable().Any(r => FilterOutItemAtIndex(r.ItemArray, existingDataIgnoreColumns).ToList().SequenceEqual(FilterOutItemAtIndex(row.ItemArray, toProcessIgnoreColumns).ToList()))) //do we have to worry about special field? what if the load ids are different?
                            {
                                //the row is the exact same,so there is no clash
                                clash = false;
                                rowsToDelete.Add(row);
                            }
                            else //row needs updated, but only if we're tracking history
                            {
                                rowsToModify.Add(row);//need to know what releaseId to replace
                                break;
                            }
                        }
                    }
                }
                foreach (DataRow row in rowsToDelete.Distinct())
                    toProcess.Rows.Remove(row);

            }


            foreach (DataRow row in rowsToModify.Distinct())
            {
                //replace existing 
                var args = new DatabaseOperationArgs();
                List<String> columns = [];
                foreach (DataColumn column in toProcess.Columns)
                {
                    //if (!pkColumns.Contains(column))
                    //{
                    columns.Add(column.ColumnName);
                    //}
                }
                //need to check for removed column and null them out
                var existingColumns = _discoveredTable.DiscoverColumns().Select(c => c.GetRuntimeName());
                var columnsThatPreviouslyExisted = existingColumns.Where(c => !pkColumns.Select(pk => pk.ColumnName).Contains(c) && !columns.Contains(c) && c != SpecialFieldNames.DataLoadRunID && c != SpecialFieldNames.ValidFrom);
                var nullEntries = string.Join(" ,", columnsThatPreviouslyExisted.Select(c => $"{c} = NULL"));
                var nullText = nullEntries.Length > 0 ? $" , {nullEntries}" : "";
                var columnString = string.Join(" , ", columns.Select(col => $"{col} = '{row[col]}'").ToList());
                var pkMatch = string.Join(" AND ", pkColumns.Select(pk => GetPKValue(pk, row)).ToList());
                var sql = $"update {_discoveredTable.GetFullyQualifiedName()} set {columnString} {nullText} where {pkMatch}";
                var cmd = _discoveredTable.GetCommand(sql, args.GetManagedConnection(_discoveredTable).Connection);
                cmd.ExecuteNonQuery();
            }

            foreach (DataRow row in rowsToModify.Distinct())
            {
                toProcess.Rows.Remove(row);
            }
            if (toProcess.Rows.Count == 0 && !rowsToModify.Any()) return null;
            if (toProcess.Rows.Count > 0)
            {
                _affectedRows += _bulkcopy.Upload(toProcess);
            }

            swTimeSpentWriting.Stop();
            listener.OnProgress(this,
                new ProgressEventArgs($"Uploading to {TargetTableName}",
                    new ProgressMeasurement(_affectedRows, ProgressType.Records), swTimeSpentWriting.Elapsed));
        }
        catch (Exception e)
        {
            _managedConnection.ManagedTransaction.AbandonAndCloseConnection();

            if (LoggingServer != null)
                _dataLoadInfo.LogFatalError(GetType().Name, ExceptionHelper.ExceptionToListOfInnerMessages(e, true));

            throw new Exception($"Failed to write rows (in transaction) to table {TargetTableName}", e);
        }


        _dataLoadInfo?.CloseAndMarkComplete();
        return null;
    }


    private static void RemoveInvalidCharactersInSchema(DataTable toProcess)
    {
        var invalidSymbols = new[] { '.' };

        if (!string.IsNullOrWhiteSpace(toProcess.TableName) && invalidSymbols.Any(c => toProcess.TableName.Contains(c)))
            foreach (var symbol in invalidSymbols)
                toProcess.TableName = toProcess.TableName.Replace(symbol.ToString(), "");

        foreach (DataColumn col in toProcess.Columns)
            if (!string.IsNullOrWhiteSpace(col.ColumnName) && invalidSymbols.Any(c => col.ColumnName.Contains(c)))
                foreach (var symbol in invalidSymbols)
                    col.ColumnName = col.ColumnName.Replace(symbol.ToString(), "");
    }


    /// <summary>
    /// Clears the primary key status of the DataTable / <see cref="ExplicitTypes"/>.  These are recorded in <see cref="_primaryKey"/> and applied at Dispose time
    /// in order that primary key in the destination database table does not interfere with ALTER statements (see <see cref="ResizeColumnsIfRequired"/>)
    /// </summary>
    /// <param name="toProcess"></param>
    private void ClearPrimaryKeyFromDataTableAndExplicitWriteTypes(DataTable toProcess)
    {
        //handle primary keyness by removing it until Dispose step
        foreach (var pkCol in toProcess.PrimaryKey.Select(dc => dc.ColumnName))
            _primaryKey.Add(pkCol);

        toProcess.PrimaryKey = Array.Empty<DataColumn>();

        //also get rid of any ExplicitTypes primary keys
        foreach (var dcr in ExplicitTypes.Where(dcr => dcr.IsPrimaryKey))
        {
            dcr.IsPrimaryKey = false;
            _primaryKey.Add(dcr.ColumnName);
        }
    }


    private void AddTimeStampToExtractionData(DataTable toProcess)
    {
        var timeStamp = DateTime.Now;
        toProcess.Columns.Add(_extractionTimeStamp);
        foreach (DataRow row in toProcess.Rows)
        {
            row[_extractionTimeStamp] = timeStamp;
        }
    }

    private static void EnsureTableHasDataInIt(DataTable toProcess)
    {
        if (toProcess.Columns.Count == 0)
            throw new Exception($"DataTable '{toProcess}' had no Columns!");

        if (toProcess.Rows.Count == 0)
            throw new Exception($"DataTable '{toProcess}' had no Rows!");
    }

    private void ResizeColumnsIfRequired(DataTable toProcess, IDataLoadEventListener listener)
    {
        swMeasuringStrings.Start();

        var tbl = _database.ExpectTable(TargetTableName);
        var typeTranslater = tbl.GetQuerySyntaxHelper().TypeTranslater;

        //Get the current estimates from the datatype computer
        var oldTypes = _dataTypeDictionary.ToDictionary(k => k.Key,
            v => typeTranslater.GetSQLDBTypeForCSharpType(v.Value.Guess), StringComparer.CurrentCultureIgnoreCase);

        //columns in
        var sharedColumns = new List<string>();

        //for each destination column
        foreach (var col in _dataTypeDictionary.Keys)
            //if it appears in the toProcess DataTable
            if (toProcess.Columns.Contains(col))
                sharedColumns.Add(col); //it is a shared column

        //for each shared column adjust the corresponding computer for all rows
        Parallel.ForEach(sharedColumns, col =>
        {
            var guesser = _dataTypeDictionary[col];
            foreach (DataRow row in toProcess.Rows)
                guesser.AdjustToCompensateForValue(row[col]);
        });

        //see if any have changed
        foreach (DataColumn column in toProcess.Columns)
        {
            if (column.ColumnName == _extractionTimeStamp && IncludeTimeStamp)
            {
                continue; //skip internally generated columns
            }
            //get what is required for the current batch and the current type that is configured in the live table
            oldTypes.TryGetValue(column.ColumnName, out var oldSqlType);
            _dataTypeDictionary.TryGetValue(column.ColumnName, out var knownType);

            var newSqlType = knownType is not null ? typeTranslater.GetSQLDBTypeForCSharpType(knownType.Guess) : null;

            var changesMade = false;

            //if the SQL data type has degraded e.g. varchar(10) to varchar(50) or datetime to varchar(20)
            if (oldSqlType != newSqlType)
            {
                var col = tbl.DiscoverColumn(column.ColumnName, _managedConnection.ManagedTransaction);


                if (AbandonAlter(col.DataType.SQLType, newSqlType, out var reason))
                {
                    listener.OnNotify(this,
                        new NotifyEventArgs(ProgressEventType.Warning,
                            $"Considered resizing column '{column}' from '{col.DataType.SQLType}' to '{newSqlType}' but decided not to because:{reason}"));
                    continue;
                }

                listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Warning,
                    $"Resizing column '{column}' from '{col.DataType.SQLType}' to '{newSqlType}'"));

                //try changing the Type to the legit type
                col.DataType.AlterTypeTo(newSqlType, _managedConnection.ManagedTransaction, AlterTimeout);

                changesMade = true;
            }

            if (changesMade)
                _bulkcopy.InvalidateTableSchema();
        }

        swMeasuringStrings.Stop();
        listener.OnProgress(this,
            new ProgressEventArgs("Measuring DataType Sizes",
                new ProgressMeasurement(_affectedRows + toProcess.Rows.Count, ProgressType.Records),
                swMeasuringStrings.Elapsed));
    }

    /// <summary>
    /// Returns true if we should not be trying to do this alter after all
    /// </summary>
    /// <param name="oldSqlType">The database proprietary type you are considering altering from</param>
    /// <param name="newSqlType">The ANSI SQL type you are considering altering to</param>
    /// <param name="reason">Null or the reason we are returning true</param>
    /// <returns>True if the proposed alter is a bad idea and shouldn't be attempted</returns>
    protected virtual bool AbandonAlter(string oldSqlType, string newSqlType, out string reason)
    {
        var basicallyDecimalAlready = new List<string> { "real", "double", "float", "single" };

        var first = basicallyDecimalAlready.FirstOrDefault(c =>
            oldSqlType.Contains(c, StringComparison.InvariantCultureIgnoreCase));

        if (first != null && newSqlType.Contains("decimal", StringComparison.InvariantCultureIgnoreCase))
        {
            reason = $"Resizing from {first} to decimal is a bad idea and likely to fail";
            return true;
        }

        reason = null;
        return false;
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

                    listener.OnNotify(this,
                        new NotifyEventArgs(ProgressEventType.Information, "Transaction rolled back successfully"));

                    _bulkcopy?.Dispose();
                }
                else
                {
                    _managedConnection.ManagedTransaction.CommitAndCloseConnection();

                    _bulkcopy?.Dispose();

                    listener.OnNotify(this,
                        new NotifyEventArgs(ProgressEventType.Information, "Transaction committed successfully"));
                }
            }
        }
        catch (Exception e)
        {
            listener.OnNotify(this,
                new NotifyEventArgs(ProgressEventType.Error,
                    "Commit failed on transaction (probably there was a previous error?)", e));
        }

        //if we have a primary key to create
        if (pipelineFailureExceptionIfAny == null && _primaryKey?.Any() == true && _discoveredTable?.Exists() == true)
        {
            //Find the columns in the destination
            var allColumns = _discoveredTable.DiscoverColumns();

            //if there are not yet any primary keys
            if (allColumns.All(c => !c.IsPrimaryKey))
            {
                //find the columns the user decorated in his DataTable
                var pkColumnsToCreate = allColumns.Where(c =>
                        _primaryKey.Any(pk => pk.Equals(c.GetRuntimeName(), StringComparison.CurrentCultureIgnoreCase)))
                    .ToArray();

                //make sure we found all of them
                if (pkColumnsToCreate.Length != _primaryKey.Count)
                    throw new Exception(
                        $"Could not find primary key column(s) {string.Join(",", _primaryKey)} in table {_discoveredTable}");

                //create the primary key to match user provided columns
                _discoveredTable.CreatePrimaryKey(AlterTimeout, pkColumnsToCreate);
            }
        }

        if (UseTrigger && _discoveredTable?.DiscoverColumns().Any(static col => col.IsPrimaryKey) == true) //can't use triggers without a PK
        {
            var factory = new TriggerImplementerFactory(_database.Server.DatabaseType);
            var triggerImplementer = factory.Create(_discoveredTable);
            var currentStatus = triggerImplementer.GetTriggerStatus();
            if (currentStatus == TriggerStatus.Missing)
                try
                {
                    triggerImplementer.CreateTrigger(ThrowImmediatelyCheckNotifier.Quiet);
                }
                catch (Exception e)
                {
                    listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Warning, e.Message));
                }
        }

        EndAuditIfExists();
    }

    private void EndAuditIfExists()
    {
        //user is auditing
        _loggingDatabaseListener?.FinalizeTableLoadInfos();
    }

    public void Check(ICheckNotifier notifier)
    {
        if (LoggingServer != null)
            new LoggingDatabaseChecker(LoggingServer).Check(notifier);
        else
            notifier.OnCheckPerformed(
                new CheckEventArgs(
                    "There is no logging server so there will be no audit of this destinations activities",
                    CheckResult.Success));
    }

    private void StartAuditIfExists(string tableName)
    {
        if (LoggingServer != null && _dataLoadInfo == null)
        {
            _loggingDatabaseSettings = DataAccessPortal.ExpectServer(LoggingServer, DataAccessContext.Logging);
            var logManager = new LogManager(_loggingDatabaseSettings);
            logManager.CreateNewLoggingTaskIfNotExists("Internal");

            _dataLoadInfo = (DataLoadInfo)logManager.CreateDataLoadInfo("Internal", GetType().Name,
                $"Loading table {tableName}", "", false);
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
    /// accommodate new data.
    /// </summary>
    /// <param name="columnName"></param>
    /// <param name="explicitType"></param>
    /// <param name="columnFlags"></param>
    /// <returns>The Column Request that has been added to the array</returns>
    public DatabaseColumnRequest AddExplicitWriteType(string columnName, string explicitType,
        ISupplementalColumnInformation columnFlags = null)
    {
        DatabaseColumnRequest columnRequest;

        if (columnFlags == null)
        {
            columnRequest = new DatabaseColumnRequest(columnName, explicitType, true);
            ExplicitTypes.Add(columnRequest);
            return columnRequest;
        }

        columnRequest = new DatabaseColumnRequest(columnName, explicitType,
            !columnFlags.IsPrimaryKey && !columnFlags.IsAutoIncrement)
        {
            IsPrimaryKey = columnFlags.IsPrimaryKey,
            IsAutoIncrement = columnFlags.IsAutoIncrement,
            Collation = columnFlags.Collation
        };

        ExplicitTypes.Add(columnRequest);
        return columnRequest;
    }
}
// Copyright (c) The University of Dundee 2018-2025
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using FAnsi;
using FAnsi.Discovery.QuerySyntax;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.DataExport.DataExtraction.Commands;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataFlowPipeline.Requirements;
using Rdmp.Core.DataLoad.Engine.Pipeline.Components;
using Rdmp.Core.DataLoad.Engine.Pipeline.Sources;
using Rdmp.Core.QueryBuilding;
using Rdmp.Core.ReusableLibraryCode;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.DataAccess;
using Rdmp.Core.ReusableLibraryCode.Progress;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IContainer = Rdmp.Core.Curation.Data.IContainer;

namespace Rdmp.Core.DataExport.DataExtraction.Pipeline.Sources;

/// <summary>
/// Executes a single Dataset extraction by linking a cohort with a dataset (either core or custom data - See IExtractCommand).  Also calculates the number
/// of unique identifiers seen, records row validation failures etc.
/// </summary>
public class ExecuteDatasetExtractionSource : IPluginDataFlowSource<DataTable>, IPipelineRequirement<IExtractCommand>
{
    //Request is either for one of these
    public ExtractDatasetCommand Request { get; protected set; }
    public ExtractGlobalsCommand GlobalsRequest { get; protected set; }

    public const string AuditTaskName = "DataExtraction";

    private readonly List<string> _extractionIdentifiersidx = new();

    private bool _cancel;

    private ICatalogue _catalogue;

    protected const string ValidationColumnName = "RowValidationResult";

    public ExtractionTimeValidator ExtractionTimeValidator { get; protected set; }
    public Exception ValidationFailureException { get; protected set; }

    public HashSet<object> UniqueReleaseIdentifiersEncountered { get; set; }

    public ExtractionTimeTimeCoverageAggregator ExtractionTimeTimeCoverageAggregator { get; set; }

    [DemandsInitialization(
        "Determines the systems behaviour when an extraction query returns 0 rows.  Default (false) is that an error is reported.  If set to true (ticked) then instead a DataTable with 0 rows but all the correct headers will be generated usually resulting in a headers only 0 line/empty extract file")]
    public bool AllowEmptyExtractions { get; set; }

    [DemandsInitialization(
        "Batch size, number of records to read from source before releasing it into the extraction pipeline",
        DefaultValue = 10000, Mandatory = true)]
    public int BatchSize { get; set; }

    [DemandsInitialization(
        "In seconds. Overrides the global timeout for SQL query execution. Use 0 for infinite timeout.",
        DefaultValue = 50000, Mandatory = true)]
    public int ExecutionTimeout { get; set; }

    [DemandsInitialization(@"Determines how the system achieves DISTINCT on extraction.  These include:
None - Do not DISTINCT the records, can result in duplication in your extract (not recommended)
SqlDistinct - Adds the DISTINCT keyword to the SELECT sql sent to the server
OrderByAndDistinctInMemory - Adds an ORDER BY statement to the query and applies the DISTINCT in memory as records are read from the server (this can help when extracting very large data sets where DISTINCT keyword blocks record streaming until all records are ready to go)
DistinctByDestinationPKs - Performs a GROUP BY on each batch of records to ensure unique extraction primary key values in the batch"
        , DefaultValue = DistinctStrategy.SqlDistinct)]
    public DistinctStrategy DistinctStrategy { get; set; }


    [DemandsInitialization("When DBMS is SqlServer then HASH JOIN should be used instead of regular JOINs")]
    public bool UseHashJoins { get; set; }

    [DemandsInitialization(
        "When DBMS is SqlServer and the extraction is for any of these datasets then HASH JOIN should be used instead of regular JOINs")]
    public Catalogue[] UseHashJoinsForCatalogues { get; set; }

    [DemandsInitialization(
        "Exclusion list.  A collection of Catalogues which will never be considered for HASH JOIN even when UseHashJoins is enabled.  Being on this list takes precedence for a Catalogue even if it is on UseHashJoinsForCatalogues.")]
    public Catalogue[] DoNotUseHashJoinsForCatalogues { get; set; }

    [DemandsInitialization("When performing an extracton, copy the cohort into a temporary table to improve extraction speed", defaultValue: false)]
    public bool UseTempTablesWhenExtractingCohort { get; set; }


    /// <summary>
    /// This is a dictionary containing all the CatalogueItems used in the query, the underlying datatype in the origin database and the
    /// actual datatype that was output after the transform operation e.g. a varchar(10) could be converted into a bona fide DateTime which
    /// would be an sql Date.  Finally
    /// a recommended SqlDbType is passed back.
    /// </summary>
    public Dictionary<ExtractableColumn, ExtractTimeTransformationObserved> ExtractTimeTransformationsObserved;

    private DbDataCommandDataFlowSource _hostedSource;

    private IExternalCohortTable _externalCohortTable;
    private string _whereSQL;
    private DbConnection _con;
    private string _uuid;
    private List<string> _knownPKs = new();
    protected virtual void Initialize(ExtractDatasetCommand request)
    {
        Request = request;

        if (request == ExtractDatasetCommand.EmptyCommand)
            return;
        _externalCohortTable = request.ExtractableCohort.ExternalCohortTable;
        _whereSQL = request.ExtractableCohort.WhereSQL();
        _timeSpentValidating = new Stopwatch();
        _timeSpentCalculatingDISTINCT = new Stopwatch();
        _timeSpentBuckettingDates = new Stopwatch();

        Request.ColumnsToExtract.Sort(); //ensure they are in the right order so we can record the release identifiers

        //if we have a cached builder already
        if (request.QueryBuilder == null)
            request.GenerateQueryBuilder();

        foreach (var substitution in Request.ReleaseIdentifierSubstitutions)
            _extractionIdentifiersidx.Add(substitution.GetRuntimeName());

        UniqueReleaseIdentifiersEncountered = new HashSet<object>();

        _catalogue = request.Catalogue;

        if (DistinctStrategy == DistinctStrategy.DistinctByDestinationPKs)
        {
            _knownPKs = _catalogue.CatalogueItems.Where(ci => ci.ExtractionInformation != null && ci.ExtractionInformation.IsPrimaryKey).Select(ci => ci.ColumnInfo.GetRuntimeName()).ToList();
        }
        if (!string.IsNullOrWhiteSpace(_catalogue.ValidatorXML))
            ExtractionTimeValidator = new ExtractionTimeValidator(_catalogue, request.ColumnsToExtract);

        //if there is a time periodicity ExtractionInformation (AND! it is among the columns the user selected to be extracted)
        if (_catalogue.TimeCoverage_ExtractionInformation_ID != null && request.ColumnsToExtract
                .Cast<ExtractableColumn>().Any(c =>
                    c.CatalogueExtractionInformation_ID == _catalogue.TimeCoverage_ExtractionInformation_ID))
            ExtractionTimeTimeCoverageAggregator =
                new ExtractionTimeTimeCoverageAggregator(_catalogue, request.ExtractableCohort);
        else
            ExtractionTimeTimeCoverageAggregator = null;
    }

    private void Initialize(ExtractGlobalsCommand request)
    {
        GlobalsRequest = request;
    }

    public bool WasCancelled => _cancel;

    private Stopwatch _timeSpentValidating;
    private int _rowsValidated;

    private Stopwatch _timeSpentCalculatingDISTINCT;
    private Stopwatch _timeSpentBuckettingDates;
    private int _rowsBucketted;

    private bool firstChunk = true;
    private bool firstGlobalChunk = true;
    private int _rowsRead;

    private RowPeeker _peeker = new();

    private static readonly Random random = new Random();

    private static string RandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }

    private void CreateCohortTempTable(DbConnection con, IDataLoadEventListener listener)
    {
        _uuid = $"#{RandomString(24)}";
        var sql = "";
        var db = _externalCohortTable.Discover();
        switch (db.Server.DatabaseType)
        {
            case DatabaseType.MicrosoftSQLServer:
                sql = $"""
                    SELECT *
                    INTO {_uuid}
                    FROM(
                    SELECT * FROM {_externalCohortTable.TableName}
                    WHERE {_whereSQL}
                    ) as cohortTempTable
                """;
                break;
            case DatabaseType.MySql:
                sql = $"""
                    CREATE TEMPORARY TABLE {_uuid} ENGINE=MEMORY
                    as (SELECT * FROM {_externalCohortTable.TableName} WHERE {_whereSQL})
                """;
                break;
            case DatabaseType.Oracle:
                sql = $"""
                    CREATE TEMPORARY TABLE {_uuid} SELECT * FROM {_externalCohortTable.TableName} WHERE {_whereSQL}
                """;
                break;
            case DatabaseType.PostgreSql:
                sql = $"""
                    CREATE TEMP TABLE {_uuid} AS
                    SELECT * FROM {_externalCohortTable.TableName} WHERE {_whereSQL}
                """;
                break;
            default:
                listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Warning, "Unable to create temporary table for cohort. Original cohort table will be used"));
                return;


        }
        listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, $"About to copy the cohort into a temporary table using the SQL: {sql}"));

        using var cmd = db.Server.GetCommand(sql, con);
        cmd.CommandTimeout = ExecutionTimeout;
        try
        {
            cmd.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Warning, "Unable to create temporary table for cohort. Original cohort table will be used", ex));
            _uuid = null;
        }
        listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "Cohort successfully copied to temporary table"));

    }

    public virtual DataTable GetChunk(IDataLoadEventListener listener, GracefulCancellationToken cancellationToken)
    {
        // we are in the Global Commands case, let's return an empty DataTable (not null)
        // so we can trigger the destination to extract the globals docs and sql
        if (GlobalsRequest != null)
        {
            GlobalsRequest.ElevateState(ExtractCommandState.WaitingForSQLServer);
            if (firstGlobalChunk)
            {
                //unless we are checking, start auditing
                StartAuditGlobals();

                firstGlobalChunk = false;
                return new DataTable(ExtractionDirectory.GLOBALS_DATA_NAME);
            }

            return null;
        }

        if (Request == null)
            throw new Exception("Component has not been initialized before being asked to GetChunk(s)");

        Request.ElevateState(ExtractCommandState.WaitingForSQLServer);

        if (_cancel)
            throw new Exception("User cancelled data extraction");

        if (_hostedSource == null)
        {
            if (UseTempTablesWhenExtractingCohort)
            {
                _con = DatabaseCommandHelper.GetConnection(Request.GetDistinctLiveDatabaseServer().Builder);
                _con.Open();
                CreateCohortTempTable(_con, listener);
            }
            var cmdSql = GetCommandSQL(listener);
            StartAudit(cmdSql);

            if (Request.DatasetBundle.DataSet.DisableExtraction)
                throw new Exception(
                    $"Cannot extract {Request.DatasetBundle.DataSet} because DisableExtraction is set to true");

            _hostedSource = UseTempTablesWhenExtractingCohort ? new DbDataCommandDataFlowSource(cmdSql,
                $"ExecuteDatasetExtraction {Request.DatasetBundle.DataSet}",
               _con,
                ExecutionTimeout)
            {
                AllowEmptyResultSets = AllowEmptyExtractions || Request.IsBatchResume,
                BatchSize = BatchSize
            }
                : new DbDataCommandDataFlowSource(cmdSql,
                $"ExecuteDatasetExtraction {Request.DatasetBundle.DataSet}",
               Request.GetDistinctLiveDatabaseServer().Builder,
                ExecutionTimeout)
                {
                    // If we are running in batches then always allow empty extractions
                    AllowEmptyResultSets = AllowEmptyExtractions || Request.IsBatchResume,
                    BatchSize = BatchSize
                };
        }

        DataTable chunk = null;

        try
        {
            chunk = _hostedSource.GetChunk(listener, cancellationToken);


            chunk = _peeker.AddPeekedRowsIfAny(chunk);

            if (Request != null && Request.DatasetBundle.DataSet is not null && chunk is not null)
                chunk.TableName = $"{Request.DatasetBundle.DataSet}";

            //if we are trying to distinct the records in memory based on release id
            if (DistinctStrategy == DistinctStrategy.OrderByAndDistinctInMemory)
            {
                var releaseIdentifierColumn = Request.ReleaseIdentifierSubstitutions.First().GetRuntimeName();

                if (chunk is { Rows.Count: > 0 })
                {
                    //last release id in the current chunk
                    var lastReleaseId = chunk.Rows[^1][releaseIdentifierColumn];

                    _peeker.AddWhile(_hostedSource, r => Equals(r[releaseIdentifierColumn], lastReleaseId), chunk);
                    chunk = MakeDistinct(chunk, listener, cancellationToken);
                }
            }
        }
        catch (AggregateException a)
        {
            if (a.GetExceptionIfExists<TaskCanceledException>() != null)
                _cancel = true;

            throw;
        }
        catch (Exception e)
        {
            listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Error, "Read from source failed", e));
        }

        if (cancellationToken.IsCancellationRequested)
            throw new Exception("Data read cancelled because our cancellationToken was set, aborting data reading");

        //if the first chunk is null
        if (firstChunk && chunk == null && !AllowEmptyExtractions)
            throw new Exception(
                $"There is no data to load, query returned no rows, query was:{Environment.NewLine}{_hostedSource.Sql ?? Request.QueryBuilder.SQL}");

        //not the first chunk anymore
        firstChunk = false;

        //data exhausted
        if (chunk == null)
        {
            listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information,
                $"Data exhausted after reading {_rowsRead} rows of data ({UniqueReleaseIdentifiersEncountered.Count} unique release identifiers seen)"));
            if (Request != null)
                Request.CumulativeExtractionResults.DistinctReleaseIdentifiersEncountered =
                    Request.IsBatchResume ? -1 : UniqueReleaseIdentifiersEncountered.Count;
            return null;
        }

        _rowsRead += chunk.Rows.Count;
        //chunk will have datatypes for all the things in the buffer so we can populate our dictionary of facts about what columns/catalogue items have spontaneously changed name/type etc
        if (ExtractTimeTransformationsObserved == null)
            GenerateExtractionTransformObservations(chunk);


        //see if the SqlDataReader has a column with the same name as the ReleaseIdentifierSQL (if so then we can use it to count the number of distinct subjects written out to the csv)
        var includesReleaseIdentifier = _extractionIdentifiersidx.Count > 0;


        //first line - let's see what columns we wrote out
        //looks at the buffer and computes any transforms performed on the column


        _timeSpentValidating.Start();
        //build up the validation report (Missing/Wrong/Etc) - this has no mechanical effect on the extracted data just some metadata that goes into a flat file
        if (ExtractionTimeValidator != null && Request.IncludeValidation)
            try
            {
                chunk.Columns.Add(ValidationColumnName);

                ExtractionTimeValidator.Validate(chunk, ValidationColumnName);

                _rowsValidated += chunk.Rows.Count;
                listener.OnProgress(this,
                    new ProgressEventArgs("Validation", new ProgressMeasurement(_rowsValidated, ProgressType.Records),
                        _timeSpentValidating.Elapsed));
            }
            catch (Exception ex)
            {
                listener.OnNotify(this,
                    new NotifyEventArgs(ProgressEventType.Error, "Could not validate data chunk", ex));
                ValidationFailureException = ex;
                ExtractionTimeValidator = null;
            }

        _timeSpentValidating.Stop();

        _timeSpentBuckettingDates.Start();
        if (ExtractionTimeTimeCoverageAggregator != null)
        {
            _rowsBucketted += chunk.Rows.Count;

            foreach (DataRow row in chunk.Rows)
                ExtractionTimeTimeCoverageAggregator.ProcessRow(row);

            listener.OnProgress(this,
                new ProgressEventArgs("Bucketting Dates", new ProgressMeasurement(_rowsBucketted, ProgressType.Records),
                    _timeSpentCalculatingDISTINCT.Elapsed));
        }

        _timeSpentBuckettingDates.Stop();

        _timeSpentCalculatingDISTINCT.Start();


        if (DistinctStrategy == DistinctStrategy.DistinctByDestinationPKs && _knownPKs.Any())
        {
            var columnNames = _knownPKs;
            Func<DataRow, String> groupingFunction = (DataRow dr) => GroupData(dr, _knownPKs.ToArray());

            chunk = chunk.AsEnumerable()
                        .GroupBy(groupingFunction)
                        .Select(g => g.First())
                   .CopyToDataTable();
        }

        var pks = new List<DataColumn>();

        //record unique release identifiers found
        if (includesReleaseIdentifier)
            foreach (var idx in _extractionIdentifiersidx.Distinct().ToList())
            {
                var sub = Request.ReleaseIdentifierSubstitutions.FirstOrDefault(s => s.Alias == chunk.Columns[idx].ColumnName);
                if (sub?.ColumnInfo.ExtractionInformations.FirstOrDefault()?.IsPrimaryKey == true)
                {
                    pks.Add(chunk.Columns[idx]);
                }

                foreach (DataRow r in chunk.Rows)
                {
                    if (r[idx] == DBNull.Value)
                        if (_extractionIdentifiersidx.Count == 1)
                            throw new Exception(
                                $"Null release identifier found in extract of dataset {Request.DatasetBundle.DataSet}");
                        else
                            continue; //there are multiple extraction identifiers that's fine if one or two are null

                    UniqueReleaseIdentifiersEncountered.Add(r[idx]);
                }

                listener.OnProgress(this,
                    new ProgressEventArgs("Calculating Distinct Release Identifiers",
                        new ProgressMeasurement(UniqueReleaseIdentifiersEncountered.Count, ProgressType.Records),
                        _timeSpentCalculatingDISTINCT.Elapsed));
            }

        _timeSpentCalculatingDISTINCT.Stop();
        pks.AddRange(Request.ColumnsToExtract.Where(static c => ((ExtractableColumn)c).CatalogueExtractionInformation.IsPrimaryKey).Select(static column => ((ExtractableColumn)column).CatalogueExtractionInformation.ToString()).Select(name => chunk.Columns[name]));
        chunk.PrimaryKey = pks.ToArray();

        return chunk;
    }

    /// <summary>
    /// Makes the current batch ONLY distinct.  This only works if you have a bounded batch (see OrderByAndDistinctInMemory)
    /// </summary>
    /// <param name="chunk"></param>
    /// <param name="listener"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private static DataTable MakeDistinct(DataTable chunk, IDataLoadEventListener listener,
        GracefulCancellationToken cancellationToken)
    {
        var removeDuplicates = new RemoveDuplicates { NoLogging = true };
        return removeDuplicates.ProcessPipelineData(chunk, listener, cancellationToken);
    }

    private void GenerateExtractionTransformObservations(DataTable chunk)
    {
        ExtractTimeTransformationsObserved = new Dictionary<ExtractableColumn, ExtractTimeTransformationObserved>();

        //create the Types dictionary
        foreach (ExtractableColumn column in Request.ColumnsToExtract)
        {
            ExtractTimeTransformationsObserved.Add(column, new ExtractTimeTransformationObserved());

            //record catalogue information about what it is supposed to be.
            if (!column.HasOriginalExtractionInformationVanished())
            {
                var extractionInformation = column.CatalogueExtractionInformation;

                //what the catalogue says it is
                ExtractTimeTransformationsObserved[column].DataTypeInCatalogue =
                    extractionInformation.ColumnInfo.Data_type;
                ExtractTimeTransformationsObserved[column].CatalogueItem = extractionInformation.CatalogueItem;

                //what it actually is
                if (chunk.Columns.Contains(column.GetRuntimeName()))
                {
                    ExtractTimeTransformationsObserved[column].FoundAtExtractTime = true;
                    ExtractTimeTransformationsObserved[column].DataTypeObservedInRuntimeBuffer =
                        chunk.Columns[column.GetRuntimeName()].DataType;
                }
                else
                {
                    ExtractTimeTransformationsObserved[column].FoundAtExtractTime = false;
                }
            }
        }
    }

    private string GetCommandSQL(IDataLoadEventListener listener)
    {
        //if the user wants some custom logic for removing identical duplicates
        switch (DistinctStrategy)
        {
            //user doesn't care about identical duplicates
            case DistinctStrategy.None:
                ((QueryBuilder)Request.QueryBuilder).SetLimitationSQL("");
                break;

            //system default behaviour
            case DistinctStrategy.DistinctByDestinationPKs:
            case DistinctStrategy.SqlDistinct:
                break;

            //user wants to run order by the release ID and resolve duplicates in batches as they are read
            case DistinctStrategy.OrderByAndDistinctInMemory:

                //remove the DISTINCT keyword from the query
                ((QueryBuilder)Request.QueryBuilder).SetLimitationSQL("");

                //find the release identifier substitution (e.g. chi for PROCHI)
                var substitution = Request.ReleaseIdentifierSubstitutions.First();

                //add a line at the end of the query to ORDER BY the ReleaseId column (e.g. PROCHI)
                var orderBySql = $"ORDER BY {substitution.SelectSQL}";

                // don't add the line if it is already there (e.g. because of Retry)
                if (!Request.QueryBuilder.CustomLines.Any(l => string.Equals(l.Text, orderBySql)))
                    Request.QueryBuilder.AddCustomLine(orderBySql, QueryComponent.Postfix);

                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        var sql = Request.QueryBuilder.SQL;

        sql = HackExtractionSQL(sql, listener);

        if (ShouldUseHashedJoins())
        {
            //use hash joins!
            listener.OnNotify(this,
                new NotifyEventArgs(ProgressEventType.Information, "Substituting JOIN for HASH JOIN"));
            sql = sql.Replace(" JOIN ", " HASH JOIN ");
        }

        listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information,
            $"/*Decided on extraction SQL:*/{Environment.NewLine}{sql}"));
        if (UseTempTablesWhenExtractingCohort && _uuid is not null)
        {
            sql = sql.Replace(_externalCohortTable.TableName, _uuid);
        }
        return sql;
    }

    private bool ShouldUseHashedJoins()
    {
        var dbms = Request?.QueryBuilder?.QuerySyntaxHelper?.DatabaseType;

        //must be sql server
        if (dbms == null || dbms.Value != DatabaseType.MicrosoftSQLServer)
            return false;

        // this Catalogue is explicitly marked as never hash join? i.e. its on the exclusion list
        if (DoNotUseHashJoinsForCatalogues?.Contains(Request.Catalogue) ?? false)
            return false;

        if (UseHashJoins)
            return true;

        if (UseHashJoinsForCatalogues != null)
            return UseHashJoinsForCatalogues.Contains(Request.Catalogue);

        //user doesn't want to use hash joins
        return false;
    }

    public virtual string HackExtractionSQL(string sql, IDataLoadEventListener listener) => sql;

    private void StartAudit(string sql)
    {
        var dataExportRepo = Request.DataExportRepository;

        var previousAudit = dataExportRepo
            .GetAllCumulativeExtractionResultsFor(Request.Configuration, Request.DatasetBundle.DataSet).ToArray();

        if (Request.IsBatchResume)
        {
            var match =
                previousAudit.FirstOrDefault(a => a.ExtractableDataSet_ID == Request.DatasetBundle.DataSet.ID) ??
                throw new Exception(
                    $"Could not find previous CumulativeExtractionResults for dataset {Request.DatasetBundle.DataSet} despite the Request being marked as a batch resume");
            Request.CumulativeExtractionResults = match;
        }
        else
        {
            //delete old audit records
            foreach (var audit in previousAudit)
                audit.DeleteInDatabase();

            var extractionResults = new CumulativeExtractionResults(dataExportRepo, Request.Configuration,
                Request.DatasetBundle.DataSet, sql);

            var filterDescriptions =
                RecursivelyListAllFilterNames(
                    Request.Configuration.GetFilterContainerFor(Request.DatasetBundle.DataSet));

            extractionResults.FiltersUsed = filterDescriptions.TrimEnd(',');
            extractionResults.SaveToDatabase();

            Request.CumulativeExtractionResults = extractionResults;
        }
    }

    private void StartAuditGlobals()
    {
        var repo = GlobalsRequest.RepositoryLocator.DataExportRepository;

        var previousAudit = repo
            .GetAllObjectsWhere<SupplementalExtractionResults>("ExtractionConfiguration_ID",
                GlobalsRequest.Configuration.ID)
            .Where(c => c.CumulativeExtractionResults_ID == null);

        //delete old audit records
        foreach (var audit in previousAudit)
            audit.DeleteInDatabase();
    }

    private string RecursivelyListAllFilterNames(IContainer filterContainer)
    {
        if (filterContainer == null)
            return "";

        var toReturn = "";

        if (filterContainer.GetSubContainers() != null)
            foreach (var subContainer in filterContainer.GetSubContainers())
                toReturn += RecursivelyListAllFilterNames(subContainer);

        if (filterContainer.GetFilters() != null)
            foreach (var f in filterContainer.GetFilters())
                toReturn += $"{f.Name},";

        return toReturn;
    }

    public virtual void Dispose(IDataLoadEventListener job, Exception pipelineFailureExceptionIfAny)
    {
    }

    public void Abort(IDataLoadEventListener listener)
    {
    }

    public virtual DataTable TryGetPreview()
    {
        if (Request == ExtractDatasetCommand.EmptyCommand)
            return new DataTable();

        var toReturn = new DataTable();
        toReturn.BeginLoadData();
        var server = _catalogue.GetDistinctLiveDatabaseServer(DataAccessContext.DataExport, false);

        using var con = server.GetConnection();
        con.Open();

        var da = server.GetDataAdapter(Request.QueryBuilder.SQL, con);

        //get up to 1000 records
        da.Fill(0, 1000, toReturn);
        toReturn.EndLoadData();

        con.Close();

        return toReturn;
    }

    public void PreInitialize(IExtractCommand value, IDataLoadEventListener listener)
    {
        if (value is ExtractDatasetCommand datasetCommand)
            Initialize(datasetCommand);
        if (value is ExtractGlobalsCommand command)
            Initialize(command);
    }

    public virtual void Check(ICheckNotifier notifier)
    {
        if (Request == ExtractDatasetCommand.EmptyCommand)
        {
            notifier.OnCheckPerformed(new CheckEventArgs(
                "Request is ExtractDatasetCommand.EmptyCommand, checking will not be carried out",
                CheckResult.Warning));
            return;
        }

        if (GlobalsRequest != null)
        {
            notifier.OnCheckPerformed(new CheckEventArgs(
                "Request is for Globals, checking will not be carried out at source", CheckResult.Success));
            return;
        }

        if (Request == null)
        {
            notifier.OnCheckPerformed(new CheckEventArgs("ExtractionRequest has not been set", CheckResult.Fail));
            return;
        }
    }

    private static String GroupData(DataRow dataRow, String[] columnNames)
    {

        StringBuilder stringBuilder = new StringBuilder();
        stringBuilder.Remove(0, stringBuilder.Length);
        foreach (String column in columnNames)
        {
            stringBuilder.Append(dataRow[column].ToString());
        }
        return stringBuilder.ToString();

    }
}
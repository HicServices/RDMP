// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using FAnsi.Discovery;
using FAnsi.Discovery.QuerySyntax;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataLoad.Engine.Pipeline.Destinations;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.DataExport.DataExtraction.Pipeline.Sources;

/// <summary>
/// Data Extraction Source which can fulfill the IExtractCommand even when the dataset in the command is on a different server from the cohort.  This is done
/// by copying the Cohort from the cohort database into tempdb for the duration of the pipeline execution and doing the linkage against that instead of
/// the original cohort table.
/// 
/// </summary>
public class ExecuteCrossServerDatasetExtractionSource : ExecuteDatasetExtractionSource
{
    private bool _haveCopiedCohortAndAdjustedSql;

    [DemandsInitialization("Database to upload the cohort to prior to linking", defaultValue: "tempdb",
        mandatory: true)]
    public string TemporaryDatabaseName { get; set; }

    [DemandsInitialization(
        "Determines behaviour if TemporaryDatabaseName is not found on the dataset server.  True to create it as a new database, False to crash",
        defaultValue: true)]
    public bool CreateTemporaryDatabaseIfNotExists { get; set; }

    [DemandsInitialization(
        "Determines behaviour if TemporaryDatabaseName already contains a Cohort table.  True to drop it, False to crash",
        defaultValue: true)]
    public bool DropExistingCohortTableIfExists { get; set; }

    [DemandsInitialization(
        "Naming pattern for the temporary cohort database table created on the data server(s) for extraction. Use $g for a guid.",
        defaultValue: "$g")]
    public string TemporaryTableName { get; set; }

    public override DataTable GetChunk(IDataLoadEventListener listener, GracefulCancellationToken cancellationToken)
    {
        SetServer();

        if (!_haveCopiedCohortAndAdjustedSql && Request != null && _doNotMigrate == false)
            CopyCohortToDataServer(listener, cancellationToken);

        return base.GetChunk(listener, cancellationToken);
    }

    private List<DiscoveredTable> tablesToCleanup = new();

    public static Semaphore OneCrossServerExtractionAtATime = new(1, 1);
    private DiscoveredServer _server;
    private DiscoveredDatabase _tempDb;
    private bool _semaphoreObtained;

    /// <summary>
    /// True if we decided not to move the cohort after all (e.g. if one or more datasets being extracted are already on the same server).
    /// </summary>
    private bool _doNotMigrate;

    private string _tablename;
    private object _tableName = new();

    public override string HackExtractionSQL(string sql, IDataLoadEventListener listener)
    {
        SetServer();

        //call base hacks
        sql = base.HackExtractionSQL(sql, listener);

        if (_doNotMigrate)
        {
            listener.OnNotify(this,
                new NotifyEventArgs(ProgressEventType.Information,
                    "Cohort and Data are on same server so no migration will occur"));
            return sql;
        }

        listener.OnNotify(this,
            new NotifyEventArgs(ProgressEventType.Information, $"Original (unhacked) SQL was {sql}", null));

        //now replace database with tempdb
        var extractableCohort = Request.ExtractableCohort;
        var extractableCohortSource = extractableCohort.ExternalCohortTable;

        var sourceSyntax = QuerySyntaxHelperFactory.Create(extractableCohortSource.DatabaseType);
        var destinationSyntax = QuerySyntaxHelperFactory.Create(_server.DatabaseType);

        //To replace (in this order)
        //Cohort database.table.privateId
        //Cohort database.table.releaseId
        //Cohort database.table.cohortdefinitionId
        //Cohort database.table name
        var replacementStrings = new Dictionary<string, string>();

        var sourceDb = sourceSyntax.GetRuntimeName(extractableCohortSource.Database);
        var sourceTable = sourceSyntax.GetRuntimeName(extractableCohortSource.TableName);
        var destinationTable = GetTableName() ?? sourceTable;
        var sourcePrivateId = sourceSyntax.GetRuntimeName(extractableCohort.GetPrivateIdentifier());
        var sourceReleaseId = sourceSyntax.GetRuntimeName(extractableCohort.GetReleaseIdentifier());
        var sourceCohortDefinitionId =
            sourceSyntax.GetRuntimeName(extractableCohortSource.DefinitionTableForeignKeyField);

        //Swaps the given entity for the same entity but in _tempDb
        AddReplacement(replacementStrings, sourceDb, sourceTable, destinationTable, sourcePrivateId, sourceSyntax,
            destinationSyntax);

        // If it is not an identifiable extraction (private and release are different)
        if (!string.Equals(sourcePrivateId, sourceReleaseId))
            AddReplacement(replacementStrings, sourceDb, sourceTable, destinationTable, sourceReleaseId, sourceSyntax,
                destinationSyntax);

        AddReplacement(replacementStrings, sourceDb, sourceTable, destinationTable, sourceCohortDefinitionId,
            sourceSyntax, destinationSyntax);
        AddReplacement(replacementStrings, sourceDb, sourceTable, destinationTable, sourceSyntax, destinationSyntax);

        foreach (var r in replacementStrings)
        {
            listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information,
                $"Replacing '{r.Key}' with '{r.Value}'", null));

            if (!sql.Contains(r.Key))
                listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Warning,
                    $"SQL extraction query string did not contain the text '{r.Key}' (which we expected to replace with '{r.Value}"));

            sql = sql.Replace(r.Key, r.Value);
        }

        listener.OnNotify(this,
            new NotifyEventArgs(ProgressEventType.Information, $"Adjusted (hacked) SQL was {sql}", null));

        //replace [MyCohortDatabase].. with [tempdb].. (while dealing with Cohort..Cohort replacement correctly as well as 'Cohort.dbo.Cohort.Fish' correctly)
        return sql;
    }

    private string GetTableName()
    {
        lock (_tableName)
        {
            if (_tablename != null)
                return _tablename;

            if (string.IsNullOrWhiteSpace(TemporaryTableName))
                return null;

            // add a g to avoid creating a table name that starts with a number (can cause problems and always requires wrapping etc... just bad)
            var guid = $"g{Guid.NewGuid():N}";

            return _tablename = TemporaryTableName.Replace("$g", guid);
        }
    }

    private void AddReplacement(Dictionary<string, string> replacementStrings, string sourceDb, string sourceTable,
        string destinationTable, string col, IQuerySyntaxHelper sourceSyntax, IQuerySyntaxHelper destinationSyntax)
    {
        replacementStrings.Add(
            sourceSyntax.EnsureFullyQualified(sourceDb, null, sourceTable, col),
            destinationSyntax.EnsureFullyQualified(_tempDb.GetRuntimeName(), null, destinationTable, col)
        );
    }

    private void AddReplacement(Dictionary<string, string> replacementStrings, string sourceDb, string sourceTable,
        string destinationTable, IQuerySyntaxHelper sourceSyntax, IQuerySyntaxHelper destinationSyntax)
    {
        replacementStrings.Add(
            sourceSyntax.EnsureFullyQualified(sourceDb, null, sourceTable),
            destinationSyntax.EnsureFullyQualified(_tempDb.GetRuntimeName(), null, destinationTable)
        );
    }

    private void SetServer()
    {
        if (_server == null && Request != null)
        {
            //it's a legit dataset being extracted?
            _server = Request.GetDistinctLiveDatabaseServer();

            //expect a database called called tempdb
            _tempDb = _server.ExpectDatabase(TemporaryDatabaseName);

            var cohortServer = Request.ExtractableCohort.ExternalCohortTable.Discover();
            if (AreOnSameServer(_server, cohortServer.Server))
                _doNotMigrate = true;
        }
    }

    /// <summary>
    /// Returns true if the two databases are on the same server (do not have to be on the same database).  Also confirms that the access
    /// credentials are compatible.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    protected static bool AreOnSameServer(DiscoveredServer a, DiscoveredServer b) =>
        string.Equals(a.Name, b.Name, StringComparison.CurrentCultureIgnoreCase) &&
        a.DatabaseType == b.DatabaseType &&
        a.ExplicitUsernameIfAny == b.ExplicitUsernameIfAny &&
        a.ExplicitPasswordIfAny == b.ExplicitPasswordIfAny;


    private void CopyCohortToDataServer(IDataLoadEventListener listener, GracefulCancellationToken cancellationToken)
    {
        DataTable cohortDataTable;
        SetServer();

        listener.OnNotify(this,
            new NotifyEventArgs(ProgressEventType.Information,
                "About to wait for Semaphore OneCrossServerExtractionAtATime to become available"));
        OneCrossServerExtractionAtATime.WaitOne(-1);
        _semaphoreObtained = true;
        listener.OnNotify(this,
            new NotifyEventArgs(ProgressEventType.Information, "Captured Semaphore OneCrossServerExtractionAtATime"));

        try
        {
            var cohort = Request.ExtractableCohort;
            cohortDataTable = cohort.FetchEntireCohort();
        }
        catch (Exception e)
        {
            throw new Exception(
                "An error occurred while trying to download the cohort from the Cohort server (in preparation for transferring it to the data server for linkage and extraction)",
                e);
        }

        //make sure tempdb exists (this covers you for servers where it doesn't exist e.g. mysql or when user has specified a different database name)
        if (!_tempDb.Exists())
            if (CreateTemporaryDatabaseIfNotExists)
                _tempDb.Create();
            else
                throw new Exception(
                    $"Database '{_tempDb}' did not exist on server '{_server}' and CreateAndDestroyTemporaryDatabaseIfNotExists was false");


        var tbl = _tempDb.ExpectTable(GetTableName() ?? cohortDataTable.TableName);

        if (tbl.Exists())
        {
            listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Warning,
                $"Found existing table called '{tbl}' in '{_tempDb}'"));

            if (DropExistingCohortTableIfExists)
            {
                listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Warning,
                    $"About to drop existing table '{tbl}'"));

                try
                {
                    tbl.Drop();
                    listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Warning,
                        $"Dropped existing table '{tbl}'"));
                }
                catch (Exception ex)
                {
                    listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Warning,
                        $"Warning dropping '{tbl}' failed", ex));
                }
            }
            else
            {
                listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Warning,
                    $"'{_tempDb}' contains a table called '{tbl}' and DropExistingCohortTableIfExists is false"));
            }
        }

        // ensures the uploaded table has the correct name
        cohortDataTable.TableName = tbl.GetRuntimeName();

        try
        {
            // attempt to set primary key of the private identifier to improve
            // query performance. e.g. chi
            cohortDataTable.PrimaryKey = new[]
                { cohortDataTable.Columns[Request.ExtractableCohort.GetPrivateIdentifier(true)] };
        }
        catch (Exception ex)
        {
            listener.OnNotify(this,
                new NotifyEventArgs(ProgressEventType.Warning,
                    "Failed to set primary key on cross server copied cohort.  Query performance may be slow", ex));
        }

        var destination = new DataTableUploadDestination();
        destination.PreInitialize(_tempDb, listener);
        destination.ProcessPipelineData(cohortDataTable, listener, cancellationToken);
        destination.Dispose(listener, null);


        if (!tbl.Exists())
            throw new Exception(
                $"Table '{tbl}' did not exist despite DataTableUploadDestination completing Successfully!");

        tablesToCleanup.Add(tbl);

        //table will now be in tempdb
        _haveCopiedCohortAndAdjustedSql = true;
    }


    public override void Dispose(IDataLoadEventListener listener, Exception pipelineFailureExceptionIfAny)
    {
        listener.OnNotify(this,
            new NotifyEventArgs(ProgressEventType.Information,
                "About to release Semaphore OneCrossServerExtractionAtATime"));
        if (_semaphoreObtained)
            OneCrossServerExtractionAtATime.Release(1);
        listener.OnNotify(this,
            new NotifyEventArgs(ProgressEventType.Information, "Released Semaphore OneCrossServerExtractionAtATime"));

        foreach (var table in tablesToCleanup)
        {
            listener.OnNotify(this,
                new NotifyEventArgs(ProgressEventType.Information, $"About to drop table '{table}'"));
            table.Drop();
            listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, $"Dropped table '{table}'"));
        }

        base.Dispose(listener, pipelineFailureExceptionIfAny);
    }

    public override void Check(ICheckNotifier notifier)
    {
    }

    public override DataTable TryGetPreview() => throw new NotSupportedException(
        "Previews are not supported for Cross Server extraction since it involves shipping off the cohort into tempdb.");
}
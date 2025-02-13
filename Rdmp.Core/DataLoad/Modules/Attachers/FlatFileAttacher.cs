// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using FAnsi.Discovery;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataLoad.Engine.Attachers;
using Rdmp.Core.DataLoad.Engine.Job;
using Rdmp.Core.DataLoad.Modules.Exceptions;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.DataLoad.Modules.Attachers;

/// <summary>
/// Base class for an Attacher which expects to be passed a Filepath which is the location of a textual file in which values for a single DataTable are stored
///  (e.g. csv or fixed width etc).  This attacher requires that the RAW database server be setup and contain the correct tables for loading (it is likely that
/// the DataLoadEngine handles all this - as a user you don't need to worry about this).
/// </summary>
public abstract class FlatFileAttacher : Attacher, IPluginAttacher
{
    [DemandsInitialization("The file to attach, e.g. \"*hic*.csv\" - this is NOT a Regex", Mandatory = true)]
    public string FilePattern { get; set; }

    [DemandsInitialization(
        "The table name to load with data from the file (this will be the RAW version of the table)")]
    public ITableInfo TableToLoad { get; set; }

    [DemandsInitialization(
        "Alternative to `TableToLoad`, type table name in if you want to load a custom table e.g. one created by another load component (that doesn't exist in LIVE).  The table name should should not contain wrappers such as square brackets (e.g. \"My Table1\")")]
    public string TableName { get; set; }

    [DemandsInitialization(
        "Determines the behaviour of the system when no files are matched by FilePattern.  If true the entire data load process immediately stops with exit code LoadNotRequired, if false then the load proceeds as normal (useful if for example if you have multiple Attachers and some files are optional)")]
    public bool SendLoadNotRequiredIfFileNotFound { get; set; }

    [DemandsInitialization(
        "If enabled then file(s) that could not be loaded are reported as warnings and the load only marked as failed after completion (including archiving etc)")]
    public bool DelayLoadFailures { get; set; }

    public FlatFileAttacher() : base(true)
    {
    }

    public override ExitCodeType Attach(IDataLoadJob job, GracefulCancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(TableName) && TableToLoad != null)
        {
            var allTables = job.RegularTablesToLoad.Union(job.LookupTablesToLoad).Distinct().ToArray();

            if (!allTables.Contains(TableToLoad))
                job.OnNotify(this,
                    new NotifyEventArgs(ProgressEventType.Warning,
                        $"FlatFileAttacher TableToLoad was '{TableToLoad}' (ID={TableToLoad.ID}) but that table was not one of the tables in the load:{string.Join(",", allTables.Select(t => $"'{t.Name}'"))}"));

            TableName = TableToLoad.GetRuntimeName(LoadBubble.Raw, job.Configuration.DatabaseNamer);
        }


        TableName = TableName?.Trim();

        var timer = new Stopwatch();
        timer.Start();


        if (string.IsNullOrWhiteSpace(TableName))
            throw new ArgumentNullException(nameof(TableName),
                "TableName has not been set, set it in the DataCatalogue");

        var table = _dbInfo.ExpectTable(TableName);

        //table didn't exist!
        if (!table.Exists())
            throw new FlatFileLoadException(_dbInfo.DiscoverTables(false).Any()
                ? $"RAW database did not have a table called:{TableName}"
                : "Raw database had 0 tables we could load");


        //load the flat file
        var filePattern = FilePattern ?? "*";

        var filesToLoad = LoadDirectory.ForLoading.EnumerateFiles(filePattern)
            .OrderBy(a => a.Name, StringComparer.InvariantCultureIgnoreCase).ToList();

        if (!filesToLoad.Any())
        {
            job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Warning,
                $"Did not find any files matching pattern {filePattern} in forLoading directory"));

            return SendLoadNotRequiredIfFileNotFound ? ExitCodeType.OperationNotRequired : ExitCodeType.Success;
        }

        foreach (var fileToLoad in filesToLoad)
            if (DelayLoadFailures)
                try
                {
                    LoadFile(table, fileToLoad, _dbInfo, timer, job, cancellationToken);
                }
                catch (Exception ex)
                {
                    job.CrashAtEnd(new NotifyEventArgs(ProgressEventType.Warning, $"Failed to load {fileToLoad}", ex));
                }
            else
                LoadFile(table, fileToLoad, _dbInfo, timer, job, cancellationToken);

        timer.Stop();

        return ExitCodeType.Success;
    }

    private void LoadFile(DiscoveredTable tableToLoad, FileInfo fileToLoad, DiscoveredDatabase dbInfo, Stopwatch timer,
        IDataLoadJob job, GracefulCancellationToken token)
    {
        using var con = dbInfo.Server.GetConnection();
        var dt = tableToLoad.GetDataTable(0);

        using var insert = tableToLoad.BeginBulkInsert(Culture);
        // setup bulk insert it into destination
        insert.Timeout = 500000;

        //if user wants to use a specific explicit format for datetimes
        if (ExplicitDateTimeFormat != null)
            insert.DateTimeDecider.Settings.ExplicitDateFormats = new string[] { ExplicitDateTimeFormat };

        //bulk insert ito destination
        job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information,
            $"About to open file {fileToLoad.FullName}"));
        OpenFile(fileToLoad, job, token);

        //confirm the validity of the headers
        ConfirmFlatFileHeadersAgainstDataTable(dt, job);

        con.Open();

        //now we will read data out of the file in batches
        const int batchNumber = 1;
        const int maxBatchSize = 10000;
        var recordsCreatedSoFar = 0;

        try
        {
            //while there is data to be loaded into table
            while (IterativelyBatchLoadDataIntoDataTable(dt, maxBatchSize, token) != 0)
            {
                DropEmptyColumns(dt);
                ConfirmFitToDestination(dt, tableToLoad, job);
                try
                {
                    recordsCreatedSoFar += insert.Upload(dt);

                    dt.Rows.Clear(); //very important otherwise we add more to the end of the table but still insert last batches records resulting in exponentially multiplying upload sizes of duplicate records!

                    job.OnProgress(this,
                        new ProgressEventArgs(tableToLoad.GetFullyQualifiedName(),
                            new ProgressMeasurement(recordsCreatedSoFar, ProgressType.Records), timer.Elapsed));
                }
                catch (Exception e)
                {
                    throw new Exception(
                        $"Error processing batch number {batchNumber} (of batch size {maxBatchSize})", e);
                }
            }
        }
        catch (Exception e)
        {
            throw new FlatFileLoadException($"Error processing file {fileToLoad}", e);
        }
        finally
        {
            CloseFile();
        }
    }

    protected abstract void OpenFile(FileInfo fileToLoad, IDataLoadEventListener listener,
        GracefulCancellationToken cancellationToken);

    protected abstract void CloseFile();

    public override void Check(ICheckNotifier notifier)
    {
        if (string.IsNullOrWhiteSpace(TableName) && TableToLoad == null)
            notifier.OnCheckPerformed(new CheckEventArgs(
                $"Either argument TableName or TableToLoad must be set {this}, you should specify this value.",
                CheckResult.Fail));

        if (string.IsNullOrWhiteSpace(FilePattern))
            notifier.OnCheckPerformed(new CheckEventArgs(
                $"Argument FilePattern has not been set on {this}, you should specify this value in the LoadMetadataUI",
                CheckResult.Fail));

        if (!string.IsNullOrWhiteSpace(TableName) && TableToLoad != null)
            notifier.OnCheckPerformed(
                new CheckEventArgs("You should only specify argument TableName or TableToLoad, not both",
                    CheckResult.Fail));
    }

    private void ConfirmFitToDestination(DataTable dt, DiscoveredTable tableToLoad, IDataLoadJob job)
    {
        var columnsAtDestination = tableToLoad.DiscoverColumns().Select(c => c.GetRuntimeName()).ToArray();

        //see if there is a shape problem between stuff that is on the server and stuff that is in the flat file
        if (dt.Columns.Count != columnsAtDestination.Length)
            job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Warning,
                $"There was a mismatch between the number of columns in the flat file ({columnsAtDestination.Aggregate((s, n) => s + Environment.NewLine + n)}) and the number of columns in the RAW database table ({dt.Columns.Count})"));

        foreach (DataColumn column in dt.Columns)
            if (!columnsAtDestination.Contains(column.ColumnName, StringComparer.CurrentCultureIgnoreCase))
                throw new FlatFileLoadException(
                    $"Column in flat file called {column.ColumnName} does not appear in the RAW database table (after fixing potentially silly names)");
    }


    /// <summary>
    /// DataTable dt is a copy of what is in RAW, your job (if you choose to accept it) is to look in your file and work out what headers you can see
    /// and then complain to job (or throw) if what you see in the file does not match the RAW target
    /// </summary>
    protected abstract void ConfirmFlatFileHeadersAgainstDataTable(DataTable loadTarget, IDataLoadJob job);


    /// <summary>
    /// 
    /// </summary>
    /// <param name="dt"></param>
    /// <param name="maxBatchSize"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>return the number of rows read, if you return >0 then you will be called again to get more data (if during this second or subsequent call there is no more data to read from source, return 0)</returns>
    protected abstract int IterativelyBatchLoadDataIntoDataTable(DataTable dt, int maxBatchSize,
        GracefulCancellationToken cancellationToken);


    private static void DropEmptyColumns(DataTable dt)
    {
        var emptyColumnsSyntheticNames = new Regex("^Column[0-9]+$");

        //deal with any ending columns which have nothing but whitespace
        for (var i = dt.Columns.Count - 1; i >= 0; i--)
            if (emptyColumnsSyntheticNames.IsMatch(dt.Columns[i].ColumnName) ||
                string.IsNullOrWhiteSpace(dt.Columns[i].ColumnName)) //is synthetic column or blank, nuke it
            {
                var foundValue = false;
                foreach (DataRow dr in dt.Rows)
                {
                    if (dr.ItemArray[i] == null)
                        continue;

                    if (string.IsNullOrWhiteSpace(dr.ItemArray[i].ToString()))
                        continue;

                    foundValue = true;
                    break;
                }

                if (!foundValue)
                    dt.Columns.Remove(dt.Columns[i]);
            }
    }

    protected virtual object HackValueReadFromFile(string s) => s;

    public override void LoadCompletedSoDispose(ExitCodeType exitCode, IDataLoadEventListener postLoadEventListener)
    {
    }
}
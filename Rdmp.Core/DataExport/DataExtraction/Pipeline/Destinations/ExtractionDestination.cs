// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Data;
using System.Diagnostics;
using System.IO;
using FAnsi.Discovery;
using Microsoft.Data.SqlClient;
using Rdmp.Core.Curation;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.DataExport.DataExtraction.Commands;
using Rdmp.Core.DataExport.DataExtraction.UserPicks;
using Rdmp.Core.DataExport.DataRelease.Pipeline;
using Rdmp.Core.DataExport.DataRelease.Potential;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataFlowPipeline.Requirements;
using Rdmp.Core.Logging;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.DataAccess;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.DataExport.DataExtraction.Pipeline.Destinations;

public abstract class ExtractionDestination : IExecuteDatasetExtractionDestination, IPipelineRequirement<IProject>
{
    //user configurable properties

    [DemandsInitialization(
        "Naming of flat files is usually based on Catalogue.Name, if this is true then the Catalogue.Acronym will be used instead",
        defaultValue: false)]
    public bool UseAcronymForFileNaming { get; set; }

    [DemandsInitialization(
        "The date format to output all datetime fields in e.g. dd/MM/yyyy for uk format yyyy-MM-dd for something more machine processable, see https://msdn.microsoft.com/en-us/library/8kb3ddd4(v=vs.110).aspx",
        DemandType.Unspecified, "yyyy-MM-dd", Mandatory = true)]
    public string DateFormat { get; set; }

    [DemandsInitialization(
        "If this is true, the dataset/globals extraction folder will be wiped clean before extracting the dataset. Useful if you suspect there are spurious files in the folder",
        defaultValue: false)]
    public bool CleanExtractionFolderBeforeExtraction { get; set; }

    public bool GeneratesFiles { get; }

    [DemandsInitialization(@"Overrides the extraction sub directory of datasets as they are extracted
         $c - Configuration Name (e.g. 'Cases')
         $i - Configuration ID (e.g. 459)
         $d - Dataset name (e.g. 'Prescribing')
         $a - Dataset acronym (e.g. 'Presc')
         $n - Dataset ID (e.g. 459)

e.g. /$i/$a")]
    public string ExtractionSubdirectoryPattern { get; set; }

    //PreInitialize fields
    protected IExtractCommand _request;
    protected DataLoadInfo _dataLoadInfo;
    protected IProject _project;

    //state variables
    protected bool haveOpened;
    private bool haveWrittenBundleContents;
    private readonly Stopwatch stopwatch = new();

    public TableLoadInfo TableLoadInfo { get; private set; }

    public DirectoryInfo DirectoryPopulated { get; private set; }

    public int SeparatorsStrippedOut { get; set; }

    public int LinesWritten { get; protected set; }
    public string OutputFile { get; protected set; } = string.Empty;

    public ExtractionDestination(bool generatesFiles)
    {
        GeneratesFiles = generatesFiles;
    }

    #region PreInitialize

    public void PreInitialize(IExtractCommand request, IDataLoadEventListener listener)
    {
        _request = request;

        if (_request == ExtractDatasetCommand.EmptyCommand)
        {
            listener.OnNotify(this,
                new NotifyEventArgs(ProgressEventType.Information,
                    "Request is ExtractDatasetCommand.EmptyCommand, checking will not be carried out"));
            return;
        }

        LinesWritten = 0;

        DirectoryPopulated = request.GetExtractionDirectory();

        PreInitializeImpl(request, listener);
    }

    protected abstract void PreInitializeImpl(IExtractCommand request, IDataLoadEventListener listener);


    public virtual void PreInitialize(DataLoadInfo value, IDataLoadEventListener listener)
    {
        _dataLoadInfo = value;
    }

    public virtual void PreInitialize(IProject value, IDataLoadEventListener listener)
    {
        _project = value;
    }

    #endregion

    /// <inheritdoc />
    public virtual string GetFilename()
    {
        var filename = _request.ToString();

        if (_request is IExtractDatasetCommand datasetCommand && UseAcronymForFileNaming)
        {
            filename = datasetCommand.Catalogue.Acronym;
            if (string.IsNullOrWhiteSpace(filename))
                throw new Exception(
                    $"Catalogue '{datasetCommand.Catalogue}' does not have an Acronym but UseAcronymForFileNaming is true");
        }

        return filename;
    }

    /// <summary>
    ///     Extracts the rows in <paramref name="toProcess" /> to the extraction destination
    /// </summary>
    /// <param name="toProcess"></param>
    /// <param name="job"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual DataTable ProcessPipelineData(DataTable toProcess, IDataLoadEventListener job,
        GracefulCancellationToken cancellationToken)
    {
        _request.ElevateState(ExtractCommandState.WritingToFile);

        if (!haveWrittenBundleContents && _request is ExtractDatasetCommand extractDatasetCommand)
        {
            WriteBundleContents(extractDatasetCommand.DatasetBundle, job, cancellationToken);
            haveWrittenBundleContents = true;
        }

        if (_request is ExtractGlobalsCommand extractGlobalsCommand)
        {
            ExtractGlobals(extractGlobalsCommand, job, _dataLoadInfo);
            return null;
        }

        stopwatch.Start();
        if (!haveOpened)
        {
            haveOpened = true;
            LinesWritten = 0;
            Open(toProcess, job, cancellationToken);

            //create an audit object
            TableLoadInfo = new TableLoadInfo(_dataLoadInfo, "", OutputFile,
                new DataSource[] { new(_request.DescribeExtractionImplementation(), DateTime.Now) }, -1);
        }

        WriteRows(toProcess, job, cancellationToken, stopwatch);

        if (TableLoadInfo.IsClosed)
            throw new Exception(
                $"TableLoadInfo was closed so could not write number of rows ({LinesWritten}) to audit object - most likely the extraction crashed?");
        TableLoadInfo.Inserts = LinesWritten;

        Flush(job, cancellationToken, stopwatch);
        stopwatch.Stop();

        return null;
    }

    #region Abstract Extraction Methods

    /// <inheritdoc />
    public abstract string GetDestinationDescription();

    /// <summary>
    ///     Called once on receiving the first batch of records, this is where you should create / open your output stream (or
    ///     create a table
    ///     if you are extracting to database).
    /// </summary>
    /// <param name="toProcess"></param>
    /// <param name="job"></param>
    /// <param name="cancellationToken"></param>
    protected abstract void Open(DataTable toProcess, IDataLoadEventListener job,
        GracefulCancellationToken cancellationToken);

    /// <summary>
    ///     Called once per batch of records to be extracted, these should be written to the output stream you opened in
    ///     <see cref="Open" />
    /// </summary>
    /// <param name="toProcess"></param>
    /// <param name="job"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="stopwatch"></param>
    protected abstract void WriteRows(DataTable toProcess, IDataLoadEventListener job,
        GracefulCancellationToken cancellationToken, Stopwatch stopwatch);

    /// <summary>
    ///     Called after each batch is written, allows you to flush your stream (if required)
    /// </summary>
    /// <param name="job"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="stopwatch"></param>
    protected virtual void Flush(IDataLoadEventListener job, GracefulCancellationToken cancellationToken,
        Stopwatch stopwatch)
    {
    }

    /// <inheritdoc />
    public abstract void Dispose(IDataLoadEventListener listener, Exception pipelineFailureExceptionIfAny);

    /// <inheritdoc />
    public abstract void Abort(IDataLoadEventListener listener);

    /// <inheritdoc />
    public virtual void Check(ICheckNotifier notifier)
    {
        if (!string.IsNullOrWhiteSpace(ExtractionSubdirectoryPattern))
        {
            if (ExtractionSubdirectoryPattern.Contains('.'))
                notifier.OnCheckPerformed(new CheckEventArgs(
                    "ExtractionSubdirectoryPattern cannot contain dots, it must be relative e.g. $c/$d",
                    CheckResult.Fail));

            if (!ExtractionSubdirectoryPattern.Contains("$i") && !ExtractionSubdirectoryPattern.Contains("$c"))
                notifier.OnCheckPerformed(new CheckEventArgs(
                    "ExtractionSubdirectoryPattern must contain a Configuration element ($i or $c)",
                    CheckResult.Fail));

            if (!ExtractionSubdirectoryPattern.Contains("$a") && !ExtractionSubdirectoryPattern.Contains("$d") &&
                !ExtractionSubdirectoryPattern.Contains("$n"))
                notifier.OnCheckPerformed(new CheckEventArgs(
                    "ExtractionSubdirectoryPattern must contain a Dataset element ($d, $a or $n)",
                    CheckResult.Fail));
        }
    }

    #endregion


    #region Release Related Methods

    /// <inheritdoc />
    public abstract ReleasePotential GetReleasePotential(IRDMPPlatformRepositoryServiceLocator repositoryLocator,
        ISelectedDataSets selectedDataSet);

    /// <inheritdoc />
    public abstract GlobalReleasePotential GetGlobalReleasabilityEvaluator(
        IRDMPPlatformRepositoryServiceLocator repositoryLocator, ISupplementalExtractionResults globalResult,
        IMapsDirectlyToDatabaseTable globalToCheck);

    /// <inheritdoc />
    public abstract FixedReleaseSource<ReleaseAudit> GetReleaseSource(ICatalogueRepository catalogueRepository);

    #endregion


    #region Bundled Content (and Globals)

    private void ExtractGlobals(ExtractGlobalsCommand request, IDataLoadEventListener listener,
        DataLoadInfo dataLoadInfo)
    {
        var globalsDirectory = GetDirectoryFor(request);
        if (CleanExtractionFolderBeforeExtraction)
        {
            globalsDirectory.Delete(true);
            globalsDirectory.Create();
        }

        foreach (var doc in request.Globals.Documents)
            request.Globals.States[doc] = TryExtractSupportingDocument(doc, globalsDirectory, listener)
                ? ExtractCommandState.Completed
                : ExtractCommandState.Crashed;

        foreach (var sql in request.Globals.SupportingSQL)
            request.Globals.States[sql] =
                TryExtractSupportingSQLTable(sql, globalsDirectory, request.Configuration, listener, dataLoadInfo)
                    ? ExtractCommandState.Completed
                    : ExtractCommandState.Crashed;
    }

    private void WriteBundleContents(IExtractableDatasetBundle datasetBundle, IDataLoadEventListener job,
        GracefulCancellationToken cancellationToken)
    {
        var rootDir = GetDirectoryFor(_request);
        var supportingSQLFolder =
            new DirectoryInfo(Path.Combine(rootDir.FullName, SupportingSQLTable.ExtractionFolderName));
        var lookupDir = rootDir.CreateSubdirectory("Lookups");

        //extract the documents
        foreach (var doc in datasetBundle.Documents)
            datasetBundle.States[doc] = TryExtractSupportingDocument(doc, rootDir, job)
                ? ExtractCommandState.Completed
                : ExtractCommandState.Crashed;

        //extract supporting SQL
        foreach (var sql in datasetBundle.SupportingSQL)
            datasetBundle.States[sql] =
                TryExtractSupportingSQLTable(sql, supportingSQLFolder, _request.Configuration, job, _dataLoadInfo)
                    ? ExtractCommandState.Completed
                    : ExtractCommandState.Crashed;

        //extract lookups
        foreach (BundledLookupTable lookup in datasetBundle.LookupTables)
            datasetBundle.States[lookup] = TryExtractLookupTable(lookup, lookupDir, job)
                ? ExtractCommandState.Completed
                : ExtractCommandState.Crashed;
    }

    public DirectoryInfo GetDirectoryFor(IExtractCommand request)
    {
        if (string.IsNullOrWhiteSpace(ExtractionSubdirectoryPattern) || request is not IExtractDatasetCommand cmd)
            return request.GetExtractionDirectory();

        var cata = cmd.SelectedDataSets.ExtractableDataSet.Catalogue;

        if (ExtractionSubdirectoryPattern.Contains("$a") && string.IsNullOrWhiteSpace(cata.Acronym))
            throw new Exception(
                $"Catalogue {cata} does not have an Acronym and ExtractionSubdirectoryPattern contains $a");

        var path = Path.Combine(cmd.Project.ExtractionDirectory,
            ExtractionSubdirectoryPattern
                .Replace("$c", QuerySyntaxHelper.MakeHeaderNameSensible(cmd.Configuration.Name))
                .Replace("$i", cmd.Configuration.ID.ToString())
                .Replace("$d", QuerySyntaxHelper.MakeHeaderNameSensible(cata.Name))
                .Replace("$a", QuerySyntaxHelper.MakeHeaderNameSensible(cata.Acronym))
                .Replace("$n", cata.ID.ToString())
        );

        var dir = new DirectoryInfo(path);
        if (!dir.Exists)
            dir.Create();

        return dir;
    }

    protected bool TryExtractLookupTable(BundledLookupTable lookup, DirectoryInfo lookupDir, IDataLoadEventListener job)
    {
        var sw = new Stopwatch();
        sw.Start();

        job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, $"About to extract lookup {lookup}"));

        try
        {
            TryExtractLookupTableImpl(lookup, lookupDir, _request.Configuration, job, out var linesWritten,
                out var destinationDescription);

            sw.Stop();
            job.OnProgress(this,
                new ProgressEventArgs($"Lookup {lookup}", new ProgressMeasurement(linesWritten, ProgressType.Records),
                    sw.Elapsed));

            //audit in the log the extraction
            var tableLoadInfo = _dataLoadInfo.CreateTableLoadInfo("", destinationDescription, new[]
            {
                new DataSource(
                    $"SELECT * FROM {lookup.TableInfo.Name}", DateTime.Now)
            }, -1);
            tableLoadInfo.Inserts = linesWritten;
            tableLoadInfo.CloseAndArchive();

            //audit in cumulative extraction results (determines release-ability of artifacts).
            if (_request is ExtractDatasetCommand command)
            {
                var result = command.CumulativeExtractionResults;
                var supplementalResult = result.AddSupplementalExtractionResult(
                    $"SELECT * FROM {lookup.TableInfo.Name}", lookup.TableInfo);
                supplementalResult.CompleteAudit(GetType(), destinationDescription, linesWritten, false, false);
            }

            return true;
        }
        catch (Exception e)
        {
            job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Error,
                $"Error occurred trying to extract lookup {lookup} on server {lookup.TableInfo.Server}", e));

            return false;
        }
    }

    /// <summary>
    ///     Extracts the <paramref name="doc" /> into the supplied <paramref name="directory" /> (unless overridden to put it
    ///     somewhere else)
    /// </summary>
    /// <param name="doc"></param>
    /// <param name="directory"></param>
    /// <param name="listener"></param>
    /// <returns></returns>
    protected virtual bool TryExtractSupportingDocument(SupportingDocument doc, DirectoryInfo directory,
        IDataLoadEventListener listener)
    {
        var fetcher = new SupportingDocumentsFetcher(doc);

        listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information,
            $"Preparing to copy {doc} to directory {directory.FullName}"));
        try
        {
            var outputPath = fetcher.ExtractToDirectory(directory);
            if (_request is ExtractDatasetCommand command)
            {
                var result = command.CumulativeExtractionResults;
                var supplementalResult = result.AddSupplementalExtractionResult(null, doc);
                supplementalResult.CompleteAudit(GetType(), outputPath, 0, false, false);
            }
            else
            {
                var extractGlobalsCommand = _request as ExtractGlobalsCommand;
                Debug.Assert(extractGlobalsCommand != null, "extractGlobalsCommand != null");
                var result = new SupplementalExtractionResults(
                    extractGlobalsCommand.RepositoryLocator.DataExportRepository,
                    extractGlobalsCommand.Configuration,
                    null,
                    doc);
                result.CompleteAudit(GetType(), outputPath, 0, false, false);
                extractGlobalsCommand.ExtractionResults.Add(result);
            }

            return true;
        }
        catch (Exception e)
        {
            listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Error,
                $"Failed to copy file {doc} to directory {directory.FullName}", e));
            return false;
        }
    }

    protected bool TryExtractSupportingSQLTable(SupportingSQLTable sql, DirectoryInfo directory,
        IExtractionConfiguration configuration, IDataLoadEventListener listener, DataLoadInfo dataLoadInfo)
    {
        try
        {
            listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information,
                $"Preparing to extract Supporting SQL {sql} to directory {directory.FullName}"));

            var sw = new Stopwatch();
            sw.Start();

            //start auditing it as a table load
            var target = Path.Combine(directory.FullName, $"{sql.Name}.csv");
            var tableLoadInfo =
                dataLoadInfo.CreateTableLoadInfo("", target, new[] { new DataSource(sql.SQL, DateTime.Now) }, -1);

            TryExtractSupportingSQLTableImpl(sql, directory, configuration, listener, out var sqlLinesWritten,
                out var description);

            sw.Stop();

            //end auditing it
            tableLoadInfo.Inserts = sqlLinesWritten;
            tableLoadInfo.CloseAndArchive();

            if (_request is ExtractDatasetCommand command)
            {
                var result = command.CumulativeExtractionResults;
                var supplementalResult = result.AddSupplementalExtractionResult(sql.SQL, sql);
                supplementalResult.CompleteAudit(GetType(), description, sqlLinesWritten, false, false);
            }
            else
            {
                var extractGlobalsCommand = _request as ExtractGlobalsCommand;
                Debug.Assert(extractGlobalsCommand != null, "extractGlobalsCommand != null");
                var result =
                    new SupplementalExtractionResults(extractGlobalsCommand.RepositoryLocator.DataExportRepository,
                        extractGlobalsCommand.Configuration,
                        sql.SQL,
                        sql);
                result.CompleteAudit(GetType(), description, sqlLinesWritten, false, false);
                extractGlobalsCommand.ExtractionResults.Add(result);
            }

            listener.OnProgress(this,
                new ProgressEventArgs($"Extract {sql}", new ProgressMeasurement(sqlLinesWritten, ProgressType.Records),
                    sw.Elapsed));
            listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information,
                $"Extracted {sqlLinesWritten} records from SupportingSQL {sql} into directory {directory.FullName}"));

            return true;
        }
        catch (Exception e)
        {
            if (e is SqlException)
                listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Error,
                    $"Failed to run extraction SQL (make sure to fully specify all database/table/column objects completely):{Environment.NewLine}{sql.SQL}",
                    e));
            else
                listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Error,
                    $"Failed to extract {sql} into directory {directory.FullName}", e));

            return false;
        }
    }

    protected virtual void TryExtractSupportingSQLTableImpl(SupportingSQLTable sqlTable, DirectoryInfo directory,
        IExtractionConfiguration configuration, IDataLoadEventListener listener, out int linesWritten,
        out string destinationDescription)
    {
        var extractor = new ExtractTableVerbatim(sqlTable.GetServer(), sqlTable.SQL, sqlTable.Name, directory,
            configuration.Separator, DateFormat);
        linesWritten = extractor.DoExtraction();
        destinationDescription = extractor.OutputFilename;
    }

    protected virtual void TryExtractLookupTableImpl(BundledLookupTable lookup, DirectoryInfo lookupDir,
        IExtractionConfiguration requestConfiguration, IDataLoadEventListener listener, out int linesWritten,
        out string destinationDescription)
    {
        //extract the lookup table SQL
        var sql = lookup.GetDataTableFetchSql();

        var extractTableVerbatim = new ExtractTableVerbatim(
            lookup.TableInfo.Discover(DataAccessContext.DataExport).Database.Server,
            sql, lookup.TableInfo.GetRuntimeName(), lookupDir, _request.Configuration.Separator, DateFormat);

        linesWritten = extractTableVerbatim.DoExtraction();
        destinationDescription = extractTableVerbatim.OutputFilename;
    }

    #endregion
}
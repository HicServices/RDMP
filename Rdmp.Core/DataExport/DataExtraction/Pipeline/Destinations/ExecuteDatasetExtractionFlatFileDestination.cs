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
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.DataExport.DataExtraction.Commands;
using Rdmp.Core.DataExport.DataExtraction.FileOutputFormats;
using Rdmp.Core.DataExport.DataRelease.Pipeline;
using Rdmp.Core.DataExport.DataRelease.Potential;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.DataExport.DataExtraction.Pipeline.Destinations;

public enum ExecuteExtractionToFlatFileType
{
    CSV
}

/// <summary>
///     Writes the pipeline DataTable (extracted dataset/custom data) to disk (as ExecuteExtractionToFlatFileType e.g.
///     CSV).  Also copies SupportingDocuments,
///     lookups etc into accompanying folders in the ExtractionDirectory.
/// </summary>
public class ExecuteDatasetExtractionFlatFileDestination : ExtractionDestination
{
    private FileOutputFormat _output;

    [DemandsInitialization("The kind of flat file to generate for the extraction", DemandType.Unspecified,
        ExecuteExtractionToFlatFileType.CSV)]
    public ExecuteExtractionToFlatFileType FlatFileType { get; set; }

    [DemandsInitialization(
        "The number of decimal places to round floating point numbers to.  This only applies to data in the pipeline which is hard typed Float and not to string values")]
    public int? RoundFloatsTo { get; internal set; }

    public ExecuteDatasetExtractionFlatFileDestination() : base(true)
    {
    }

    protected override void PreInitializeImpl(IExtractCommand request, IDataLoadEventListener listener)
    {
        if (_request is ExtractGlobalsCommand)
        {
            listener.OnNotify(this,
                new NotifyEventArgs(ProgressEventType.Information, "Request is for the extraction of Globals."));
            OutputFile = _request.GetExtractionDirectory().FullName;
            return;
        }

        switch (FlatFileType)
        {
            case ExecuteExtractionToFlatFileType.CSV:
                OutputFile = Path.Combine(DirectoryPopulated.FullName, $"{GetFilename()}.csv");
                _output = request.Configuration != null
                    ? new CSVOutputFormat(OutputFile, request.Configuration.Separator, DateFormat)
                    : new CSVOutputFormat(OutputFile, ",", DateFormat);

                _output.RoundFloatsTo = RoundFloatsTo;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information,
            $"Setup data extraction destination as {OutputFile} (will not exist yet)"));
    }

    protected override void Open(DataTable toProcess, IDataLoadEventListener job,
        GracefulCancellationToken cancellationToken)
    {
        if (_request.IsBatchResume)
        {
            if (File.Exists(_output.OutputFilename))
            {
                // if it is a batch resume then create a backup of the file as it looked at the start of the process
                _backupFile = $"{_output.OutputFilename}.bak";
                job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, $"Creating {_backupFile}"));
                File.Copy(_output.OutputFilename, _backupFile, true);
            }
            else
            {
                throw new Exception(
                    $"Batch resume is true but there was no file to append to (expected a file to exist at '{_output.OutputFilename}')");
            }
        }

        _output.Open(_request.IsBatchResume);

        // write the headers for the file unless we are resuming
        if (!_request.IsBatchResume) _output.WriteHeaders(toProcess);
    }

    protected override void WriteRows(DataTable toProcess, IDataLoadEventListener job,
        GracefulCancellationToken cancellationToken, Stopwatch stopwatch)
    {
        foreach (DataRow row in toProcess.Rows)
        {
            _output.Append(row);

            LinesWritten++;

            if (LinesWritten % 1000 == 0)
                job.OnProgress(this,
                    new ProgressEventArgs($"Write to file {OutputFile}",
                        new ProgressMeasurement(LinesWritten, ProgressType.Records), stopwatch.Elapsed));
        }

        job.OnProgress(this,
            new ProgressEventArgs($"Write to file {OutputFile}",
                new ProgressMeasurement(LinesWritten, ProgressType.Records), stopwatch.Elapsed));
    }

    protected override void Flush(IDataLoadEventListener job, GracefulCancellationToken cancellationToken,
        Stopwatch stopwatch)
    {
        _output.Flush();
    }

    public override void Dispose(IDataLoadEventListener listener, Exception pipelineFailureExceptionIfAny)
    {
        CloseFile(listener, pipelineFailureExceptionIfAny != null);

        // if pipeline execution failed and we are doing a batch resume
        if (pipelineFailureExceptionIfAny != null &&
            (_request?.IsBatchResume ?? false) && _backupFile != null && _output?.OutputFilename != null)
            if (File.Exists(_backupFile))
            {
                listener.OnNotify(this,
                    new NotifyEventArgs(ProgressEventType.Information,
                        $"Pipeline crashed so restoring backup file {_backupFile}"));
                File.Copy(_backupFile, _output.OutputFilename, true);
            }

        if (_backupFile != null && File.Exists(_backupFile))
        {
            listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, $"Deleting {_backupFile}"));
            File.Delete(_backupFile);
        }
    }

    public override void Abort(IDataLoadEventListener listener)
    {
        CloseFile(listener, true);
    }

    private bool _fileAlreadyClosed;

    /// <summary>
    ///     If performing a batch resume then this file will be a copy of the flat file
    ///     before we began appending data to it in case the pipeline execution fails
    /// </summary>
    private string _backupFile;


    private void CloseFile(IDataLoadEventListener listener, bool failed)
    {
        //we never even started or have already closed
        if (!haveOpened || _fileAlreadyClosed)
            return;

        _fileAlreadyClosed = true;

        try
        {
            //whatever happens in the writing block, make sure to at least attempt to close off the file
            _output.Close();
            GC.Collect(); //prevents file locks from sticking around

            if (TableLoadInfo == null) return;

            //close audit object - unless it was prematurely closed e.g. by a failure somewhere
            if (!TableLoadInfo.IsClosed)
                TableLoadInfo.CloseAndArchive();

            // TODO: Make sure to only increment if batch succeeded

            // also close off the cumulative extraction result
            var result = ((IExtractDatasetCommand)_request).CumulativeExtractionResults;
            result?.CompleteAudit(GetType(), GetDestinationDescription(), LinesWritten, _request.IsBatchResume, failed);
        }
        catch (Exception e)
        {
            listener.OnNotify(this,
                new NotifyEventArgs(ProgressEventType.Error, "Error when trying to close csv file", e));
        }
    }


    public override string GetDestinationDescription()
    {
        return OutputFile;
    }

    public override ReleasePotential GetReleasePotential(IRDMPPlatformRepositoryServiceLocator repositoryLocator,
        ISelectedDataSets selectedDataSet)
    {
        return new FlatFileReleasePotential(repositoryLocator, selectedDataSet);
    }

    public override FixedReleaseSource<ReleaseAudit> GetReleaseSource(ICatalogueRepository catalogueRepository)
    {
        return new FlatFileReleaseSource();
    }

    public override GlobalReleasePotential GetGlobalReleasabilityEvaluator(
        IRDMPPlatformRepositoryServiceLocator repositoryLocator, ISupplementalExtractionResults globalResult,
        IMapsDirectlyToDatabaseTable globalToCheck)
    {
        return new FlatFileGlobalsReleasePotential(repositoryLocator, globalResult, globalToCheck);
    }

    public override void Check(ICheckNotifier notifier)
    {
        if (_request == ExtractDatasetCommand.EmptyCommand)
        {
            notifier.OnCheckPerformed(new CheckEventArgs(
                "Request is ExtractDatasetCommand.EmptyCommand, checking will not be carried out",
                CheckResult.Warning));
            return;
        }

        try
        {
            var result = DateTime.Now.ToString(DateFormat);
            notifier.OnCheckPerformed(new CheckEventArgs(
                $"DateFormat '{DateFormat}' is valid, dates will look like:{result}", CheckResult.Success));
        }
        catch (Exception e)
        {
            notifier.OnCheckPerformed(new CheckEventArgs($"DateFormat '{DateFormat}' was invalid", CheckResult.Fail,
                e));
        }

        if (UseAcronymForFileNaming && _request is ExtractDatasetCommand dsRequest)
            if (string.IsNullOrWhiteSpace(dsRequest.Catalogue.Acronym))
                notifier.OnCheckPerformed(new CheckEventArgs(
                    $"Catalogue '{dsRequest.Catalogue}' does not have an Acronym but UseAcronymForFileNaming is true",
                    CheckResult.Fail));

        if (CleanExtractionFolderBeforeExtraction)
        {
            var rootDir = _request.GetExtractionDirectory();
            var contents = rootDir.GetFileSystemInfos();

            if (contents.Length > 0
                &&
                notifier.OnCheckPerformed(new CheckEventArgs(
                    $"Extraction directory '{rootDir.FullName}' contained {contents.Length} files/folders:\r\n {string.Join(Environment.NewLine, contents.Take(100).Select(e => e.Name))}",
                    CheckResult.Warning, null, "Delete Files"))
               )
            {
                rootDir.Delete(true);
                rootDir.Create();
            }
        }

        base.Check(notifier);
    }
}
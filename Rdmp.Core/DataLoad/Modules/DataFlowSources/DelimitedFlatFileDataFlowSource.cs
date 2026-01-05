// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using CsvHelper.Configuration;
using FAnsi.Discovery;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataFlowPipeline.Requirements;
using Rdmp.Core.DataLoad.Engine.Attachers;
using Rdmp.Core.DataLoad.Modules.DataFlowSources.SubComponents;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.DataLoad.Modules.DataFlowSources;

/// <summary>
/// Pipeline component (source) for reading from a flat file delimited by by a specific character (or string) e.g. csv.  The file is batch processed into
/// DataTables of size MaxBatchSize (to avoid memory problems in large files).
/// 
/// <para>Values read are fed into the pipeline as a DataTable with the Name of the DataTable being the name of the file being read.  Example usage would
/// be setting the separator to , to read CSV files.</para>
/// </summary>
public class DelimitedFlatFileDataFlowSource : IPluginDataFlowSource<DataTable>, IPipelineRequirement<FlatFileToLoad>
{
    private CsvReader _reader;

    private bool _dataAvailable;
    private IDataLoadEventListener _listener;

    /// <summary>
    /// The minimum value to allow the user to specify for <see cref="StronglyTypeInputBatchSize"/>
    /// </summary>
    public const int MinimumStronglyTypeInputBatchSize = 500;

    #region User viewable descriptions of what properties do (used to help wrapper classes have consistent definitions

    public const string ForceHeaders_DemandDescription =
        "Forces specific headers to be interpreted for columns, this is a string that will effectively be appended to the front of the file when it is read.  WARNING: Use this argument only when the file does not have any headers (Note that you must use the appropriate separator for your file)";

    public const string ForceHeadersReplacesFirstLineInFile_Description =
        "Only used when ForceHeaders is specified, if true then the line will replace the first line of the file.  If left as false (default) then the line will be appended to the file.  Use true if you want to replace existing headers in the file and false if the file doesn't have any headers in it at all.";

    public const string IgnoreQuotes_DemandDescription =
        "True if the parser should treat double quotes as normal characters";

    public const string IgnoreBlankLines_DemandDescription = "True if the parser should skip over blank lines";

    public const string MakeHeaderNamesSane_DemandDescription =
        "True (recommended) if you want to fix columns that have crazy names e.g. 'my column #1' would become 'mycolumn1'";

    public const string BadDataHandlingStrategy_DemandDescription =
        @"Determines system behaviour when unprocessable rows are found in the file being loaded:
ThrowException - Stop the loading process with an error
IgnoreRows - Step over the line in the file and carry on loading
DivertRows - As IgnoreRows but write all unloadable lines to X_Errors.txt where X is the file name being loaded";

    public const string IgnoreBadReads_DemandDescription =
        @"True - Ignore read warnings from CSVHelper (e.g. when a quote appears in the middle of a cell)
False - Treat read warnings from CSVHelper according to the BadDataHandlingStrategy";

    public const string ThrowOnEmptyFiles_DemandDescription =
        @"Determines system behaviour when a file is empty or has only a header row";

    public const string AttemptToResolveNewLinesInRecords_DemandDescription =
        @"Determines system behaviour when a line has too few cells compared to the header count.  
True - Attempt to read more lines to make a complete record
False - Treat the line as bad data (See BadDataHandlingStrategy)";

    public const string MaximumErrorsToReport_DemandDescription =
        "The maximum number of file report before suppressing logging.  This is important if you have a large file e.g. 80 million rows and you have a bug/configuration problem that results in lots of bad rows.  Specify 0 for no limit.  Negatives also result in no limit";

    public const string IgnoreColumns_Description =
        @"By default all columns from the source (file) will be read.  Set this to a list of headers (separated with the correct separator for your file) to ignore the specified columns.

This will not help you avoid bad data as the full file structure must still be read regardless.";

    #endregion

    [DemandsInitialization("The separator that delimits the file", Mandatory = true)]
    public string Separator
    {
        get => _separator;
        set => _separator =
            value == "\\t"
                ? "\t"
                : value; //automatically switch \\t into \t (user inputs \t it turns to whitespace tab when executing)
    }

    [DemandsInitialization(ForceHeaders_DemandDescription)]
    public string ForceHeaders { get; set; }

    [DemandsInitialization(ForceHeadersReplacesFirstLineInFile_Description)]
    public bool ForceHeadersReplacesFirstLineInFile { get; set; }

    [DemandsInitialization(IgnoreQuotes_DemandDescription)]
    public bool IgnoreQuotes { get; set; }

    [DemandsInitialization(IgnoreQuotes_DemandDescription)]
    public bool IgnoreBlankLines { get; set; }

    [DemandsInitialization(MakeHeaderNamesSane_DemandDescription, DemandType.Unspecified, true)]
    public bool MakeHeaderNamesSane { get; set; }

    [DemandsInitialization(
        "True (recommended) if you want to impute the datatypes from the data being loaded, False if you want to load everything as strings",
        DemandType.Unspecified, true)]
    public bool StronglyTypeInput { get; set; }

    [DemandsInitialization(
        "BatchSize to use when predicting datatypes i.e. if you set this to 1000 then the first 1000 rows have int field then the 5000th row has a string you will get an error.  Set to 0 to use MaxBatchSize.  Set to -1 to load the entire file before computing datatypes (can result in out of memory for super large files)")]
    public int StronglyTypeInputBatchSize { get; set; }

    [DemandsInitialization(
        "Number of rows to read at once from the input file in each go (after the first - See StronglyTypeInputBatchSize)",
        DefaultValue = 100000)]
    public int MaxBatchSize { get; set; }

    [DemandsInitialization(
        "A collection of column names that are expected to be found in the input file which you want to specify as explicit types (e.g. you load barcodes like 0110 and 1111 and want these all loaded as char(4) instead of int)")]
    public ExplicitTypingCollection ExplicitlyTypedColumns { get; set; }

    [DemandsInitialization(BadDataHandlingStrategy_DemandDescription,
        DefaultValue = BadDataHandlingStrategy.ThrowException)]
    public BadDataHandlingStrategy BadDataHandlingStrategy { get; set; }

    [DemandsInitialization(IgnoreBadReads_DemandDescription, DefaultValue = true)]
    public bool IgnoreBadReads { get; set; }

    [DemandsInitialization(ThrowOnEmptyFiles_DemandDescription, DefaultValue = true)]
    public bool ThrowOnEmptyFiles { get; set; }

    [DemandsInitialization(AttemptToResolveNewLinesInRecords_DemandDescription, DefaultValue = false)]
    public bool AttemptToResolveNewLinesInRecords { get; set; }

    [DemandsInitialization(MaximumErrorsToReport_DemandDescription, DefaultValue = 100)]
    public int MaximumErrorsToReport { get; set; }

    [DemandsInitialization(IgnoreColumns_Description)]
    public string IgnoreColumns { get; set; }

    private CultureInfo _culture;

    [DemandsInitialization("The culture to use for dates")]
    public CultureInfo Culture
    {
        get => _culture ?? CultureInfo.CurrentCulture;
        set => _culture = value;
    }

    [DemandsInitialization(Attacher.ExplicitDateTimeFormat_DemandDescription)]
    public string ExplicitDateTimeFormat { get; set; }

    /// <summary>
    /// The database table we are trying to load
    /// </summary>
    private DataTable _workingTable;

    /// <summary>
    /// File we are trying to load
    /// </summary>
    private FlatFileToLoad _fileToLoad;

    public FlatFileColumnCollection Headers { get; private set; }
    public FlatFileEventHandlers EventHandlers { get; private set; }
    public FlatFileToDataTablePusher DataPusher { get; private set; }

    /// <summary>
    /// things we know we definitely cannot load!
    /// </summary>
    private string[] _prohibitedExtensions =
    {
        ".xls", ".xlsx", ".doc", ".docx"
    };

    private string _separator;

    /// <summary>
    /// Used to split the records read into chunks to avoid running out of memory
    /// </summary>
    private int _lineNumberBatch;


    private void InitializeComponents()
    {
        Headers = new FlatFileColumnCollection(_fileToLoad, MakeHeaderNamesSane, ExplicitlyTypedColumns, ForceHeaders,
            ForceHeadersReplacesFirstLineInFile, IgnoreColumns);
        DataPusher = new FlatFileToDataTablePusher(_fileToLoad, Headers, HackValueReadFromFile,
            AttemptToResolveNewLinesInRecords, Culture, ExplicitDateTimeFormat);
        EventHandlers = new FlatFileEventHandlers(_fileToLoad, DataPusher, ThrowOnEmptyFiles, BadDataHandlingStrategy,
            _listener, MaximumErrorsToReport <= 0 ? int.MaxValue : MaximumErrorsToReport, IgnoreBadReads);
    }


    public DataTable GetChunk(IDataLoadEventListener listener, GracefulCancellationToken cancellationToken)
    {
        try
        {
            _listener = listener;

            var rowsRead = 0;

            if (_fileToLoad == null)
                throw new Exception(
                    "_fileToLoad was not set, it is supposed to be set because of IPipelineRequirement<FlatFileToLoad> - maybe this PreInitialize method was not called?");

            if (Headers == null)
            {
                InitializeComponents();

                //open the file
                OpenFile(_fileToLoad.File);

                if (Headers.FileIsEmpty)
                {
                    EventHandlers.FileIsEmpty();
                    return null;
                }
            }

            //if we do not yet have a data table to load
            if (_workingTable == null)
            {
                //create a table with the name of the file
                _workingTable = Headers.GetDataTableWithHeaders(_listener);
                _workingTable.TableName =
                    QuerySyntaxHelper.MakeHeaderNameSensible(Path.GetFileNameWithoutExtension(_fileToLoad.File.Name));

                //set the data table to the new untyped but correctly headered table
                SetDataTable(_workingTable);

                //Now we must read some data
                if (StronglyTypeInput && StronglyTypeInputBatchSize != 0)
                {
                    var batchSizeToLoad = StronglyTypeInputBatchSize == -1
                        ? int.MaxValue
                        : StronglyTypeInputBatchSize;

                    if (batchSizeToLoad < MinimumStronglyTypeInputBatchSize)
                        listener.OnNotify(this,
                            new NotifyEventArgs(ProgressEventType.Warning,
                                $"You set StronglyTypeInputBatchSize to {batchSizeToLoad} this may be too small!",
                                null));

                    //user wants to strongly type input with a custom batch size
                    rowsRead = IterativelyBatchLoadDataIntoDataTable(_workingTable, batchSizeToLoad);
                }
                else
                //user does not want to strongly type or is strongly typing with regular batch size
                {
                    rowsRead = IterativelyBatchLoadDataIntoDataTable(_workingTable, MaxBatchSize);
                }

                if (StronglyTypeInput)
                    _workingTable = DataPusher.StronglyTypeTable(_workingTable, ExplicitlyTypedColumns);

                if (rowsRead == 0)
                    EventHandlers.FileIsEmpty();
            }
            else
            {
                //this isn't the first pass, so we have everything set up and can just read more data

                //data table has been set so has a good schema or no schema depending on what user wanted, at least it has all the headers etc setup correctly
                //so just clear the rows we loaded last chunk and load more
                _workingTable.Rows.Clear();

                //get more rows
                rowsRead = IterativelyBatchLoadDataIntoDataTable(_workingTable, MaxBatchSize);
            }

            //however we read

            //if rows were not read
            if (rowsRead == 0)
                return null; //we are done

            //rows were read so return a copy of the DataTable, because we will continually reload the same DataTable schema throughout the file we don't want to give up our reference to good headers in case someone mutlates it
            var copy = _workingTable.Copy();

            foreach (var unnamed in Headers.UnamedColumns)
                copy.Columns.Remove(unnamed.ColumnName);

            return copy;
        }
        catch (Exception)
        {
            //make sure file is closed if it crashes
            _reader?.Dispose();
            throw;
        }
    }


    public void Dispose(IDataLoadEventListener listener, Exception pipelineFailureExceptionIfAny)
    {
        CloseReader();
    }

    public void Abort(IDataLoadEventListener listener)
    {
        CloseReader();
    }

    private void CloseReader()
    {
        if (_reader != null)
        {
            _reader.Dispose();
            _reader = null;
        }
    }

    public DataTable TryGetPreview()
    {
        //there is already a data table in memory
        if (_workingTable != null)
            return _workingTable;

        //we have not loaded anything yet
        if (Headers == null)
        {
            //get a chunk
            var toReturn = GetChunk(ThrowImmediatelyDataLoadEventListener.Quiet, new GracefulCancellationToken());

            //clear these to close the file and reset state to 'I need to open the file again state'
            CloseReader();

            Headers = null;
            EventHandlers = null;
            DataPusher = null;

            _workingTable = null;
            _reader = null;

            return toReturn;
        }

        throw new NotSupportedException(
            "Cannot generate preview because _headers has already been set which likely means it is already loading / didn't cleanup properly after last preview attempt?");
    }

    public void Check(ICheckNotifier notifier)
    {
        notifier.OnCheckPerformed(Separator == null
            ? new CheckEventArgs($"Separator argument has not been set on {GetType().Name}", CheckResult.Fail)
            : new CheckEventArgs($"Separator on {GetType().Name} is {Separator}", CheckResult.Success));

        if (!StronglyTypeInput)
            notifier.OnCheckPerformed(
                new CheckEventArgs("StronglyTypeInput is false, this feature is highly recommended",
                    CheckResult.Warning));

        if (StronglyTypeInput && StronglyTypeInputBatchSize < 500)
            notifier.OnCheckPerformed(
                new CheckEventArgs(
                    "StronglyTypeInputBatchSize is less than the recommended 500: this may cause errors when determining the best data type from the source file.",
                    CheckResult.Warning));

        if (_fileToLoad.File == null)
        {
            notifier.OnCheckPerformed(
                new CheckEventArgs(
                    "Input object FlatFileToLoad had a null .File property.  This means the FlatFileToLoad is not known.  This is only valid at DesignTime and only if the source file is unknown.  It means we can't check our compatibility with the file",
                    CheckResult.Warning));
            return;
        }

        if (_fileToLoad != null)
            CheckExpectedFileExtensions(notifier, _fileToLoad.File.Extension);
    }

    private void CheckExpectedFileExtensions(ICheckNotifier notifier, string extension)
    {
        if (_prohibitedExtensions.Contains(extension))
        {
            notifier.OnCheckPerformed(
                new CheckEventArgs(
                    $"File {_fileToLoad.File.Name} has a prohibited file extension {_fileToLoad.File.Extension} (this class is designed to handle .csv, .tsv, .txt etc - basically anything that is delimited by characters and not some freaky binary/fixed width file type",
                    CheckResult.Fail));
            return;
        }

        if (Separator == ",")
            ExpectFileExtension(notifier, ".csv", extension);

        if (Separator == "\t")
            ExpectFileExtension(notifier, ".tsv", extension);
    }

    private static void ExpectFileExtension(ICheckNotifier notifier, string expectedExtension, string actualExtension)
    {
        if (expectedExtension.Equals(actualExtension))
            notifier.OnCheckPerformed(new CheckEventArgs($"File extension matched expectations ({expectedExtension})",
                CheckResult.Success));
        else
            notifier.OnCheckPerformed(new CheckEventArgs(
                $"Unexpected file extension '{actualExtension}' (expected {expectedExtension}) ", CheckResult.Warning));
    }

    protected void OpenFile(FileInfo fileToLoad)
    {
        _dataAvailable = true;
        _lineNumberBatch = 0;

        //if it is blank or null (although tab is allowed)
        if (string.IsNullOrWhiteSpace(Separator) && Separator != "\t")
            throw new Exception(
                $"Could not open file {fileToLoad.FullName} because the file Separator has not been set yet, make sure to set all relevant [DemandsInitialization] properties");

        var sr = new StreamReader(fileToLoad.FullName);
        _reader = new CsvReader(sr, new CsvConfiguration(Culture)
        {
            Delimiter = Separator,
            HasHeaderRecord = string.IsNullOrWhiteSpace(ForceHeaders),
            ShouldSkipRecord = ShouldSkipRecord,
            IgnoreBlankLines = IgnoreBlankLines,
            Mode = IgnoreQuotes ? CsvMode.NoEscape : CsvMode.RFC4180,
            BadDataFound = s => EventHandlers.BadDataFound(new FlatFileLine(s), true),
            ReadingExceptionOccurred = EventHandlers.ReadingExceptionOccurred
        });

        Headers.GetHeadersFromFile(_reader);
    }


    private bool ShouldSkipRecord(ShouldSkipRecordArgs args)
    {
        if (_reader.Context.Parser.RawRow == 1 //first line of file
            && !string.IsNullOrWhiteSpace(ForceHeaders) //and we are forcing headers
            && ForceHeadersReplacesFirstLineInFile) //and those headers replace the first line of the file
        {
            Headers.ShowForceHeadersAsciiArt(args.Row, _listener);

            //skip the line
            return true;
        }

        //otherwise don't skip lines
        return false;
    }

    protected int IterativelyBatchLoadDataIntoDataTable(DataTable dt, int batchSize)
    {
        if (!_dataAvailable)
            return 0;

        if (Headers == null)
            throw new Exception("headers was null, how did that happen?");

        _lineNumberBatch = 0;

        //read from the peek first if there is anything otherwise read from the reader
        while
            (_dataAvailable =
             DataPusher.PeekedRecord != null ||
             _reader.Read()) //while we can read data -- also record whether the data was exhausted by this Read() because  CSVReader blows up if you ask it to Read() after Read() has already returned a false once
        {
            FlatFileLine currentRow;

            if (DataPusher.PeekedRecord != null)
            {
                currentRow = DataPusher.PeekedRecord;
                DataPusher.PeekedRecord = null;
            }
            else
            {
                currentRow = new FlatFileLine(_reader.Context);

                //if there is bad data on the current row just read the next
                if (DataPusher.BadLines.Contains(_reader.Context.Parser.RawRow))
                    continue;
            }

            _lineNumberBatch += DataPusher.PushCurrentLine(_reader, currentRow, dt, _listener, EventHandlers);

            if (!_dataAvailable)
                break;

            //if we have enough required rows for this batch, break out of reading loop
            if (_lineNumberBatch >= batchSize)
                break;
        }

        return _lineNumberBatch;
    }


    public void PreInitialize(FlatFileToLoad value, IDataLoadEventListener listener)
    {
        //we have been given a new file we no longer know the headers.
        Headers = null;

        _fileToLoad = value;
        _listener = listener;
    }

    /// <summary>
    /// Override this if you want to mess with values as they are read from the source file in some freaky way.
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    protected virtual object HackValueReadFromFile(string s) => s;

    /// <summary>
    /// Sets the target DataTable that we are loading from the csv/tsv etc
    /// </summary>
    /// <param name="dt"></param>
    public void SetDataTable(DataTable dt)
    {
        if (Headers == null)
        {
            InitializeComponents();
            OpenFile(_fileToLoad.File);

            Headers.MakeDataTableFitHeaders(dt, _listener);
        }

        _workingTable = dt;
    }
}

public enum BadDataHandlingStrategy
{
    ThrowException,
    IgnoreRows,
    DivertRows
}
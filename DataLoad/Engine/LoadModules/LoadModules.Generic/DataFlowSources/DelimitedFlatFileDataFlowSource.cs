using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using CatalogueLibrary.Data;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.DataFlowPipeline.Requirements;
using CsvHelper;
using CsvHelper.Configuration;
using LoadModules.Generic.Exceptions;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using ReusableLibraryCode.DatabaseHelpers.Discovery.TypeTranslation;
using ReusableLibraryCode.Progress;

namespace LoadModules.Generic.DataFlowSources
{
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

        public const string ForceHeaders_DemandDescription = "Forces specific headers to be interpreted for columns, this is a string that will effectively be appended to the front of the file when it is read.  WARNING: Use this argument only when the file does not have any headers (Note that you must use the appropriate separator for your file)";
        public const string ForceHeadersReplacesFirstLineInFile_Description = "Only used when ForceHeaders is specified, if true then the line will replace the first line of the file.  If left as false (default) then the line will be appended to the file.  Use true if you want to replace existing headers in the file and false if hte file doesn't have any headers in it at all.";
        public const string IgnoreQuotes_DemandDescription = "True if the parser should treat double quotes as normal characters";
        public const string IgnoreBlankLines_DemandDescription = "True if the parser should skip over blank lines";
        public const string MakeHeaderNamesSane_DemandDescription = "True (recommended) if you want to fix columns that have crazy names e.g. 'my column #1' would become 'mycolumn1'";


        [DemandsInitialization("The separator that delimits the file", Mandatory = true)]
        public string Separator
        {
            get { return _separator; }
            set { _separator = value == "\\t"?"\t":value; } //automatically switch \\t into \t (user inputs \t it turns to whitespace tab when executing)
        }

        [DemandsInitialization(ForceHeaders_DemandDescription)]
        public string ForceHeaders { get; set; }

        [DemandsInitialization(ForceHeadersReplacesFirstLineInFile_Description)]
        public bool ForceHeadersReplacesFirstLineInFile { get; set; }

        [DemandsInitialization(IgnoreQuotes_DemandDescription)]
        public bool IgnoreQuotes { get; set; }

        [DemandsInitialization(IgnoreQuotes_DemandDescription)]
        public bool IgnoreBlankLines { get; set; }

        [DemandsInitialization(MakeHeaderNamesSane_DemandDescription,DemandType.Unspecified,true)]
        public bool MakeHeaderNamesSane { get; set; }

        [DemandsInitialization("True (recommended) if you want to impute the datatypes from the data being loaded, False if you want to load everything as strings", DemandType.Unspecified,true)]
        public bool StronglyTypeInput { get; set; }
        
        [DemandsInitialization("BatchSize to use when predicting datatypes i.e. if you set this to 1000 then the first 1000 rows have int field then the 5000th row has a string you will get an error.  Set to 0 to use MaxBatchSize.  Set to -1 to load the entire file before computing datatypes (can result in out of memory for super large files)")]
        public int StronglyTypeInputBatchSize { get; set; }

        [DemandsInitialization("Number of rows to read at once from the input file in each go (after the first - See StronglyTypeInputBatchSize)",DefaultValue=10000)]
        public int MaxBatchSize {get;set;}

        [DemandsInitialization("A collection of column names that are expected to be found in the input file which you want to specify as explicit types (e.g. you load barcodes like 0110 and 1111 and want these all loaded as char(4) instead of int)")]
        public ExplicitTypingCollection ExplicitlyTypedColumns { get; set; }

        [DemandsInitialization("Bad Data handling strategy")]
        public BadDataHandlingStrategy BadDataHandlingStrategy { get; set; }

        [DemandsInitialization(@"Determines system behaviour when a file is empty or has only a header row")]
        public bool ThrowOnEmptyFiles { get; set; }

        /// <summary>
        /// The database table we are trying to load
        /// </summary>
        private DataTable _workingTable;

        /// <summary>
        /// File we are trying to load
        /// </summary>
        private FlatFileToLoad _fileToLoad;

        /// <summary>
        /// File where we put error rows
        /// </summary>
        public FileInfo DivertErrorsFile;

        /// <summary>
        /// The Headers found in the file / overridden by ForceHeaders
        /// </summary>
        private string[] _headers = null;

        private bool _haveComplainedAboutColumnMismatch = false;

        /// <summary>
        /// This is incremented when too many values are read from the file to match the header count BUT the values read were null/empty
        /// </summary>
        private long _bufferOverrunsWhereColumnValueWasBlank = 0;

        /// <summary>
        /// things we know we definetly cannot load!
        /// </summary>
        private string[] _prohibitedExtensions = 
        {
            ".xls",".xlsx",".doc",".docx"
        };

        /// <summary>
        /// used to advise user if he has selected the wrong separator
        /// </summary>
        private string[] _commonSeparators = new[] { "|", ",", "    ", "#" };
        private string _separator;

        /// <summary>
        /// Used to split the records read into chunks to avoid running out of memory
        /// </summary>
        private int _lineNumberBatch;

        /// <summary>
        /// All line numbers of the source file being read that could not be processed.  Allows BadDataFound etc to be called multiple times without skipping
        /// records by accident.
        /// </summary>
        HashSet<int> _badLines = new HashSet<int>();

        public DataTable GetChunk(IDataLoadEventListener listener, GracefulCancellationToken cancellationToken)
        {
            try
            {
                _listener = listener;

                int rowsRead = 0;

                if (_fileToLoad == null)
                    throw new Exception(
                        "_fileToLoad was not set, it is supposed to be set because of IPipelineRequirement<FlatFileToLoad> - maybe this PreInitialize method was not called?");

                if (_headers == null)
                    if (!LoadHeaders()) //load headers
                        return null; //if load headers failed but didn't throw then we have an empty file

                //if we do not yet have a data table to load
                if (_workingTable == null)
                {
                    //create a table with the name of the file
                    _workingTable = new DataTable();
                    _workingTable.TableName = QuerySyntaxHelper.MakeHeaderNameSane(Path.GetFileNameWithoutExtension(_fileToLoad.File.Name));

                    List<string> duplicateHeaders = new List<string>();

                    //create a string column for each header - these will change type once we have read some data
                    foreach (string header in _headers)
                    {
                        string h = header;

                        //watch for duplicate columns
                        if (_workingTable.Columns.Contains(header))
                            if (MakeHeaderNamesSane)
                                h = MakeHeaderUnique(header, _workingTable.Columns, listener, this);
                            else
                            {
                                duplicateHeaders.Add(header);
                                continue;
                            }
                        
                        //override type
                        if (ExplicitlyTypedColumns != null && ExplicitlyTypedColumns.ExplicitTypesCSharp.ContainsKey(h))
                            _workingTable.Columns.Add(h, ExplicitlyTypedColumns.ExplicitTypesCSharp[h]);
                        else
                            _workingTable.Columns.Add(h);
                    }
          
                    if (duplicateHeaders.Any())
                        throw new FlatFileLoadException("Found the following duplicate headers in file '" + _fileToLoad.File + "':" + string.Join(",", duplicateHeaders));
          
                    //set the data table to the new untyped but correctly headered table
                    SetDataTable(_workingTable);

                    //Now we must read some data
                    if (StronglyTypeInput && StronglyTypeInputBatchSize != 0)
                    {
                        int batchSizeToLoad = StronglyTypeInputBatchSize == -1
                            ? int.MaxValue
                            : StronglyTypeInputBatchSize;

                        if(batchSizeToLoad < 500)
                        {
                            listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Warning, "You set StronglyTypeInputBatchSize to " + batchSizeToLoad + " this will be increased to 500 because that number is too small!",null));
                            batchSizeToLoad = 500;
                        }

                        //user want's to strongly type input with a custom batch size
                        rowsRead = IterativelyBatchLoadDataIntoDataTable(_workingTable, batchSizeToLoad);
                    }
                    else
                        //user does not want to strongly type or is strongly typing with regular batch size
                        rowsRead = IterativelyBatchLoadDataIntoDataTable(_workingTable, MaxBatchSize);

                    if (StronglyTypeInput)
                        StronglyTypeWorkingTable();

                    if (rowsRead == 0 && ThrowOnEmptyFiles)
                        FileIsEmpty();
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
                    return null;//we are done

                //rows were read so return a copy of the DataTable, because we will continually reload the same DataTable schema throughout the file we don't want to give up our reference to good headers incase someone mutlates it
                return _workingTable.Copy();
            }
            catch (Exception )
            {
                //make sure file is closed if it crashes
                if(_reader != null)
                    _reader.Dispose();
                throw;
            }

        }

        private void StronglyTypeWorkingTable()
        {
            DataTable dtCloned = _workingTable.Clone();

            bool typeChangeNeeded = false;

            foreach (DataColumn col in _workingTable.Columns)
            {
                //if we have already handled it
                if (ExplicitlyTypedColumns != null && ExplicitlyTypedColumns.ExplicitTypesCSharp.ContainsKey(col.ColumnName))
                    continue;

                //let's make a decision about the data type to use based on the contents
                var computedType = new DataTypeComputer(col);

                //Type based on the contents of the column 
                if (computedType.ShouldDowngradeColumnTypeToMatchCurrentEstimate(col))
                {
                    dtCloned.Columns[col.ColumnName].DataType = computedType.CurrentEstimate;
                    typeChangeNeeded = true;
                }
            }

            if (typeChangeNeeded)
            {
                foreach (DataRow row in _workingTable.Rows)
                    dtCloned.ImportRow(row);

                _workingTable = dtCloned;
            }

        }

        private bool LoadHeaders()
        {
            if(_headers != null)
                throw new Exception("Headers have already been loaded from the flat file");

            //open the file
            OpenFile(_fileToLoad.File);

            //get the headers - either ForceHeaders or the ones read from the first line of the file
            return GetHeadersFromFile();
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
            if(_headers == null)
            {
                //get a chunk
                DataTable toReturn = GetChunk(new ThrowImmediatelyDataLoadEventListener(), new GracefulCancellationToken());

                //clear these to close the file and reset state to 'I need to open the file again state'
                CloseReader();
                
                _headers = null;
                _workingTable = null;
                _reader = null;
                _haveComplainedAboutColumnMismatch = false;
                DivertErrorsFile = null;

                return toReturn;
            }
            
            throw new NotSupportedException("Cannot generate preview because _headers has already been set which likely means it is already loading / didn't cleanup properly after last preview attempt?");
        }

        public void Check(ICheckNotifier notifier)
        {
            if (Separator == null)
                notifier.OnCheckPerformed(new CheckEventArgs("Separator argument has not been set on " + GetType().Name, CheckResult.Fail));
            else
                notifier.OnCheckPerformed(new CheckEventArgs("Separator on " + GetType().Name + " is " + Separator, CheckResult.Success));

            if (!StronglyTypeInput)
                notifier.OnCheckPerformed(
                    new CheckEventArgs("StronglyTypeInput is false, this feature is highly recommended",CheckResult.Warning));

            if (_fileToLoad.File == null)
            {
                notifier.OnCheckPerformed(
                    new CheckEventArgs(
                        "Input object FlatFileToLoad had a null .File property.  This means the FlatFileToLoad is not known.  This is only valid at DesignTime and only if the source file is unknown.  It means we can't check our compatibility with the file",
                        CheckResult.Warning));
                return;
            }

            if (_fileToLoad != null)
                CheckExpectedFileExtensions(notifier,_fileToLoad.File.Extension);
            
        }

        private void CheckExpectedFileExtensions(ICheckNotifier notifier, string extension)
        {
             if (_prohibitedExtensions.Contains(extension))
             {
                 notifier.OnCheckPerformed(
                     new CheckEventArgs(
                         "File " + _fileToLoad.File.Name + " has a prohibitted file extension " +
                         _fileToLoad.File.Extension +
                         " (this class is designed to handle .csv, .tsv, .txt etc - basically anything that is delimitted by characters and not some freaky binary/fixed width file type",
                         CheckResult.Fail));
                 return;
             }

            if (Separator == ",")
                ExpectFileExtension(notifier, ".csv",extension);

            if (Separator == "\t")
                ExpectFileExtension(notifier, ".tsv", extension);
        }

        private void ExpectFileExtension(ICheckNotifier notifier, string expectedExtension, string actualExtension)
        {
            if (expectedExtension.Equals(actualExtension))
                notifier.OnCheckPerformed(new CheckEventArgs("File extension matched expectations (" + expectedExtension + ")",CheckResult.Success));
            else
                notifier.OnCheckPerformed(new CheckEventArgs("Unexpected file extension '"+actualExtension+"' (expected " + expectedExtension + ") ", CheckResult.Warning));
        }
        
        protected void OpenFile(FileInfo fileToLoad)
        {
            _dataAvailable = true;
            _lineNumberBatch = 0;

            //if it is blank or null (although tab is allowed)
            if(string.IsNullOrWhiteSpace(Separator)  && Separator != "\t")
                throw new Exception("Could not open file " + fileToLoad.FullName + " because the file Separator has not been set yet, make sure to set all relevant [DemandsInitialization] properties");

            StreamReader sr = new StreamReader(fileToLoad.FullName);
            _reader = new CsvReader(sr, new Configuration()
            {
                Delimiter = Separator,
                HasHeaderRecord = string.IsNullOrWhiteSpace(ForceHeaders),
                ShouldSkipRecord = ShouldSkipRecord,
                BadDataFound = BadDataFound,
                ReadingExceptionOccurred = ReadingExceptionOccurred
            });

            _reader.Configuration.IgnoreBlankLines = IgnoreBlankLines;
            _reader.Configuration.IgnoreQuotes = IgnoreQuotes;
        }

        
        private void ReadingExceptionOccurred(CsvHelperException obj)
        {
            switch (BadDataHandlingStrategy)
            {
                case BadDataHandlingStrategy.IgnoreRows:
                    _listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Warning, "Ignored ReadingException on line " + obj.ReadingContext.RawRow,obj));
                    //move to next line
                    _badLines.Add(obj.ReadingContext.RawRow);

                    break;
                case BadDataHandlingStrategy.DivertRows:

                    DivertErrorRow(obj.ReadingContext,obj);
                    break;

                case BadDataHandlingStrategy.ThrowException:
                    throw new FlatFileLoadException("Bad data found on line " + obj.ReadingContext.RawRow,obj);

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        private void BadDataFound(ReadingContext obj)
        {
            switch (BadDataHandlingStrategy)
            {
                case BadDataHandlingStrategy.IgnoreRows:
                    _listener.OnNotify(this,new NotifyEventArgs(ProgressEventType.Warning, "Ignored BadData on line " + obj.RawRow));
                    
                    //move to next line
                    _badLines.Add(obj.RawRow);

                    break;
                case BadDataHandlingStrategy.DivertRows:
                    DivertErrorRow(obj, null);
                    break;
                
                case BadDataHandlingStrategy.ThrowException:
                    throw new FlatFileLoadException("Bad data found on line "+ obj.RawRow);


                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void DivertErrorRow(ReadingContext context, Exception ex)
        {
            if (DivertErrorsFile == null)
            {
                DivertErrorsFile = new FileInfo(Path.Combine(_fileToLoad.File.Directory.FullName, Path.GetFileNameWithoutExtension(_fileToLoad.File.Name) + "_Errors.txt"));

                //delete any old version
                if (DivertErrorsFile.Exists)
                    DivertErrorsFile.Delete();
            }

            _listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Warning, "Diverting Error on line " + context.RawRow + " to '" + DivertErrorsFile.FullName + "'", ex));

            File.AppendAllText(DivertErrorsFile.FullName, context.RawRecord);

            //move to next line
            _badLines.Add(context.RawRow);
        }

        private bool ShouldSkipRecord(string[] strings)
        {
            if (_reader.Context.RawRow == 1 //first line of file
                && !string.IsNullOrWhiteSpace(ForceHeaders) //and we are forcing headers
                && ForceHeadersReplacesFirstLineInFile) //and those headers replace the first line of the file
            {
                //create an ascii art representation of the headers being replaced in the format
                //[0]MySensibleCol>>>My Silly Coll#
                StringBuilder asciiArt = new StringBuilder();
                for (int i = 0; i < _headers.Length; i++)
                {
                    asciiArt.Append("[" + i + "]" + _headers[i] + ">>>");
                    asciiArt.AppendLine(i < strings.Length ? strings[i] : "???");
                }

                for (int i = _headers.Length; i < strings.Length; i++)
                    asciiArt.AppendLine("[" + i + "]???>>>" + strings[i]);

                _listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "Your attacher has ForceHeaders and ForceHeadersReplacesFirstLineInFile=true, I will now tell you about the first line of data in the file that you skipped (and how it related to your forced headers).  Replacement headers are " + Environment.NewLine + Environment.NewLine + asciiArt));

                if (strings.Length != _headers.Length)
                    _listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Warning, "The number of ForceHeader replacement headers specified does not match the number of headers in the file (being replaced)"));
                
                _listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "Skipped first line of file because there are forced replacement headers, we discarded" + string.Join(",", strings)));
                
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

            if (_headers == null)
                throw new Exception("headers was null, how did that happen?");
            
            _lineNumberBatch = 0;


            while (_dataAvailable = _reader.Read()) //while we can read data -- also record whether the data was exhausted by this Read() because  CSVReader blows up if you ask it to Read() after Read() has already returned a false once
            {
                //if there is bad data on the current row just read the next
                if (_badLines.Contains(_reader.Context.RawRow))
                    continue;
                
                FillUpDataTable(dt, _reader.Context.Record, _headers);

                if (!_dataAvailable)
                    break;

                //if we have enough required rows for this batch, break out of reading loop
                if (_lineNumberBatch >= batchSize)
                    break;
            }

            return _lineNumberBatch;

        }

        protected bool GetHeadersFromFile()
        {
            if (string.IsNullOrWhiteSpace(ForceHeaders))
            {
                bool empty = !_reader.Read();

                if (empty)
                {
                    FileIsEmpty();
                    return false;
                }
                    
                //get headers from first line of the file
                _reader.ReadHeader();
                _headers = _reader.Context.HeaderRecord;
            }
            else
            {
                //user has some specific headers he wants to override with
                _headers = ForceHeaders.Split(new[] { Separator }, StringSplitOptions.RemoveEmptyEntries);
                _reader.Configuration.HasHeaderRecord = false;
            }

            //at least trim them
            for (int i = 0; i < _headers.Length; i++)
                if (!string.IsNullOrWhiteSpace(_headers[i]))
                    _headers[i] =_headers[i].Trim();

            //throw away trailing null headers
            var trailingNullHeaders = _headers.Reverse().TakeWhile(IsNull).Count();

            if (trailingNullHeaders > 0)
                _headers = _headers.Take(_headers.Length - trailingNullHeaders).ToArray();

            //and maybe also help them out with a bit of sanity fixing
            if(MakeHeaderNamesSane)
                for (int i = 0; i < _headers.Length; i++)
                    _headers[i] = QuerySyntaxHelper.MakeHeaderNameSane(_headers[i]);

            

            return true;
        }

        private void FileIsEmpty()
        {
            if (ThrowOnEmptyFiles)
                throw new FlatFileLoadException("File " + _fileToLoad + " is empty");
            
            _listener.OnNotify(this,new NotifyEventArgs(ProgressEventType.Warning, "File " + _fileToLoad + " is empty"));
        }

        public static string MakeHeaderUnique(string newColumnName, DataColumnCollection columnsSoFar, IDataLoadEventListener listener, object sender)
        {
            //if it is already unique then that's fine
            if (!columnsSoFar.Contains(newColumnName))
                return newColumnName;
            
            //otherwise issue a rename
            int number = 2;
            while (columnsSoFar.Contains(newColumnName + "_" + number))
                number++;

            var newName = newColumnName + "_" + number;

            //found a novel number
            listener.OnNotify(sender, new NotifyEventArgs(ProgressEventType.Warning, "Renamed duplicate column '" + newColumnName + "' to '" + newName + "'"));
            return newName;
        }
        
        private void FillUpDataTable(DataTable dt, string[] splitUpInputLine, string[] headers)
        {
            //skip the blank lines
            if (splitUpInputLine.Length == 0 || splitUpInputLine.All(IsNull))
                return;

            int headerCount = headers.Count(h => !IsNull(h));
            
            //if the number of not empty headers doesn't match the headers in the data table
            if (dt.Columns.Count != headerCount)
                if (!_haveComplainedAboutColumnMismatch)
                {
                    _listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Warning, "Flat file '" + _fileToLoad.File.Name + "' line number '" + _reader.Context.RawRow + "' had  " + headerCount + " columns while the destination DataTable had " + dt.Columns.Count + " columns.  This message apperas only once per file"));
                    _haveComplainedAboutColumnMismatch = true;
                }

            Dictionary<string, object> rowValues = new Dictionary<string, object>();

            if (splitUpInputLine.Length < headerCount)
            {
                _listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Warning, "Too few columns on line " + _reader.Context.RawRow + " of file '" + dt.TableName + "', it has too many columns (expected " + headers.Length + " columns but line had " + splitUpInputLine.Length + ")." + (_bufferOverrunsWhereColumnValueWasBlank > 0 ? "( " + _bufferOverrunsWhereColumnValueWasBlank + " Previously lines also suffered from buffer overruns but the overrunning values were empty so we had ignored them up until now)" : "")));
                BadDataFound(_reader.Context);
                return;
            }

            bool haveIncremented_bufferOverrunsWhereColumnValueWasBlank = false;


            for (int i = 0; i < splitUpInputLine.Length; i++)
            {
                //about to do a buffer overrun
                if (i >= headers.Length)
                    if (IsNull(splitUpInputLine[i]))
                    {
                        if (!haveIncremented_bufferOverrunsWhereColumnValueWasBlank)
                        {
                            _bufferOverrunsWhereColumnValueWasBlank++;
                            haveIncremented_bufferOverrunsWhereColumnValueWasBlank = true;
                        }

                        continue; //do not bother buffer overruning with null whitespace stuff
                    }
                    else
                    {
                        _listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Warning, "Column mismatch on line " + _reader.Context.RawRow + " of file '" + dt.TableName + "', it has too many columns (expected " + headers.Length + " columns but line had " + splitUpInputLine.Length + ")." + (_bufferOverrunsWhereColumnValueWasBlank > 0 ? "( " + _bufferOverrunsWhereColumnValueWasBlank + " Previously lines also suffered from buffer overruns but the overrunning values were empty so we had ignored them up until now)" : "")));
                        BadDataFound(_reader.Context);
                        break;
                    }

                //its an empty header, dont bother populating it
                if (IsNull(headers[i]))
                    if (!IsNull(splitUpInputLine[i]))
                        throw new FileLoadException("The header at index " + i + " in flat file '" +dt.TableName+ "' had no name but there was a value in the data column (on Line number " + _reader.Context.RawRow + ")");
                    else
                        continue;

                //sometimes flat files have ,NULL,NULL,"bob" in instead of ,,"bob"
                if (IsNull(splitUpInputLine[i]))
                    rowValues.Add(headers[i], DBNull.Value);
                else
                {
                    object hackedValue = HackValueReadFromFile(splitUpInputLine[i]);

                    if (hackedValue is string)
                        hackedValue = ((string)hackedValue).Trim();

                    try
                    {
                        //if we are trying to load a boolean value out of the flat file into the strongly typed C# data type
                        if (dt.Columns[headers[i]].DataType == typeof (Boolean))
                        {
                            bool boolean;
                            int integer;
                            if (hackedValue is string)
                            {
                                if (Boolean.TryParse((string) hackedValue, out boolean)) //could be the text "true"
                                    hackedValue = boolean;
                                else
                                    if (int.TryParse((string)hackedValue, out integer)) //could be the number string "1" or "0"
                                        hackedValue = integer;

                                //else god knows what it is as a datatype, hopefully Convert.ChangeType will handle it
                            }
                            else if (int.TryParse(hackedValue.ToString(), out integer)) //could be the number 1 or 0 or something else that ToStrings into a legit value
                                hackedValue = integer;

                        }
                        //make it an int because apparently C# is too stupid to convert "1" into a bool but is smart enough to turn 1 into a bool.... seriously?!!?

                        rowValues.Add(headers[i], Convert.ChangeType(hackedValue, dt.Columns[headers[i]].DataType));
                        //convert to correct datatype (datatype was setup in SetupTypes)
                    }
                    catch (Exception e)
                    {
                        throw new FileLoadException("Error reading file '" + dt.TableName + "'.  Problem loading value " + splitUpInputLine[i] + " into data table (on Line number " + _reader.Context.RawRow + ") the header we were trying to populate was " + _headers[i] + " and was of datatype " + dt.Columns[headers[i]].DataType, e);
                    }
                }
            }

            if(!_badLines.Contains(_reader.Context.RawRow))
            {
                DataRow currentRow = dt.Rows.Add();
                foreach (KeyValuePair<string, object> kvp in rowValues)
                    currentRow[kvp.Key] = kvp.Value;
                
                _lineNumberBatch++;
            }

        }

        private bool IsNull(string s)
        {
            return string.IsNullOrWhiteSpace(s) || s.Trim().Equals("NULL", StringComparison.CurrentCultureIgnoreCase);
        }

        public void PreInitialize(FlatFileToLoad value, IDataLoadEventListener listener)
        {
            //we have been given a new file we no longer know the headers.
            _headers = null;
            
            _fileToLoad = value;
            _listener = listener;
        }

        /// <summary>
        /// Override this if you want to mess with values as they are read from the source file in some freaky way.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        protected virtual object HackValueReadFromFile(string s)
        {

            return s;
        }

        /// <summary>
        /// Sets the target DataTable that we are loading from the csv/tsv etc
        /// </summary>
        /// <param name="dt"></param>
        public void SetDataTable(DataTable dt)
        {
            if(_headers == null)
            {
                LoadHeaders();
                _headers = FixFlatFileNameToDatabaseTableMismatchedColumnNames(dt, _headers);
            }

            _workingTable = dt;
        }



        private string[] FixFlatFileNameToDatabaseTableMismatchedColumnNames(DataTable dt, string[] headers)
        {
            StringBuilder ASCIIArt = new StringBuilder();

            List<string> headersNotFound = new List<string>();
           
            for (int index = 0; index < headers.Length; index++)
            {
                ASCIIArt.Append("[" + index + "]");
                
                if (dt.Columns.Contains(headers[index]))    //exact match
                {
                    ASCIIArt.AppendLine(headers[index] + ">>>" + headers[index]);
                    continue;
                }
                
                if (string.IsNullOrWhiteSpace(headers[index])) //Empty column header, ignore it
                {
                    ASCIIArt.AppendLine("Blank Column>>>IGNORED" );
                    continue;
                }

                //try replacing spaces with underscores
                if (dt.Columns.Contains(headers[index].Replace(" ", "_")))
                {
                    string before = headers[index];
                    headers[index] = headers[index].Replace(" ", "_");

                    ASCIIArt.AppendLine(before + ">>>" + headers[index]);
                    continue;
                }

                //try replacing spaces with nothing
                if (dt.Columns.Contains(headers[index].Replace(" ", "")))
                {
                    string before = headers[index];
                    headers[index] = headers[index].Replace(" ", "");

                    ASCIIArt.AppendLine(before + ">>>" + headers[index]);
                    continue;
                }

                ASCIIArt.AppendLine(headers[index] + ">>>????" );
                headersNotFound.Add(headers[index]);
            }

            //now that we have adjusted the header names
            string[] unmatchedColumns =
                dt.Columns.Cast<DataColumn>()
                    .Where(c => !headers.Any(h => h != null && h.ToLower().Equals(c.ColumnName.ToLower())))//get all columns in data table where there are not any with the same name
                    .Select(c => c.ColumnName)
                    .ToArray();

            if (unmatchedColumns.Any())
                ASCIIArt.AppendLine(Environment.NewLine + "Unmatched Columns In DataTable:" + Environment.NewLine +
                                    string.Join(Environment.NewLine, unmatchedColumns));

            //if there is exactly 1 column found by the program and there are unmatched columns it is likely the user has selected the wrong separator
            if(headers.Length == 1 && unmatchedColumns.Any())
                foreach (string commonSeparator in _commonSeparators)
                    if(headers[0].Contains(commonSeparator))
                        _listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Error, "Your separator '" + Separator + "' does not appear in the headers line of your file ("+_fileToLoad.File.Name+") but the separator '"+commonSeparator+"' does... did you mean to set the Separator to '"+commonSeparator+"'? The headers line is:\"" + headers[0] +"\""));


            _listener.OnNotify(this, new NotifyEventArgs(
                headersNotFound.Any()?ProgressEventType.Error : ProgressEventType.Information, //information or warning if there are unrecognised field names
                "I will now tell you about how the columns in your file do or do not match the columns in your database, Matching flat file columns (or forced replacement headers) against database headers resulted in:" + Environment.NewLine + ASCIIArt)); //tell them about what columns match what


            if(headersNotFound.Any())
                throw new Exception("Could not find a suitable target column for flat file columns " +string.Join(",",headersNotFound)+ " amongst database data table columns (" + string.Join(",",from DataColumn col in dt.Columns select col.ColumnName) + ")");

            return headers;
        }
    }

    public enum BadDataHandlingStrategy 
    {
        ThrowException,
        IgnoreRows,
        DivertRows
    }
}

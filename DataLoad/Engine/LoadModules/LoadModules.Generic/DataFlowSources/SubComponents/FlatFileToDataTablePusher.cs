using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.DataFlowPipeline.Requirements;
using CsvHelper;
using ReusableLibraryCode.Progress;
using ReusableLibraryCode.Extensions;

namespace LoadModules.Generic.DataFlowSources.SubComponents
{
    /// <summary>
    /// This class is a sub component of <see cref="DelimitedFlatFileDataFlowSource"/>, it is responsible for adding rows read from the CSV file to
    /// the DataTable built by <see cref="FlatFileColumnCollection"/>.
    /// </summary>
    public class FlatFileToDataTablePusher
    {
        private readonly FlatFileToLoad _fileToLoad;
        private readonly FlatFileColumnCollection _headers;
        private readonly Func<string, object> _hackValuesFunc;

        /// <summary>
        /// All line numbers of the source file being read that could not be processed.  Allows BadDataFound etc to be called multiple times without skipping
        /// records by accident.
        /// </summary>
        public HashSet<int> BadLines = new HashSet<int>();


        /// <summary>
        /// This is incremented when too many values are read from the file to match the header count BUT the values read were null/empty
        /// </summary>
        private long _bufferOverrunsWhereColumnValueWasBlank = 0;

        /// <summary>
        /// We only complain once about headers not matching the number of cell values
        /// </summary>
        private bool _haveComplainedAboutColumnMismatch;

        public FlatFileToDataTablePusher(FlatFileToLoad fileToLoad,FlatFileColumnCollection headers, Func<string,object> hackValuesFunc)
        {
            _fileToLoad = fileToLoad;
            _headers = headers;
            _hackValuesFunc = hackValuesFunc;
        }

        public int PushCurrentLine(CsvReader reader, DataTable dt,IDataLoadEventListener listener, FlatFileEventHandlers eventHandlers)
        {
            var currentLine = reader.Context.Record;

            //skip the blank lines
            if (currentLine.Length == 0 || currentLine.All(h=>h.IsBasicallyNull()))
                return 0;

            int headerCount = _headers.CountNotNull;
            
            //if the number of not empty headers doesn't match the headers in the data table
            if (dt.Columns.Count != headerCount)
                if (!_haveComplainedAboutColumnMismatch)
                {
                    listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Warning, "Flat file '" + _fileToLoad.File.Name + "' line number '" + reader.Context.RawRow + "' had  " + headerCount + " columns while the destination DataTable had " + dt.Columns.Count + " columns.  This message apperas only once per file"));
                    _haveComplainedAboutColumnMismatch = true;
                }

            Dictionary<string, object> rowValues = new Dictionary<string, object>();

            if (currentLine.Length < headerCount)
            {
                listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Warning, "Too few columns on line " + reader.Context.RawRow + " of file '" + dt.TableName + "', it has too many columns (expected " + _headers.Length + " columns but line had " + currentLine.Length + ")." + (_bufferOverrunsWhereColumnValueWasBlank > 0 ? "( " + _bufferOverrunsWhereColumnValueWasBlank + " Previously lines also suffered from buffer overruns but the overrunning values were empty so we had ignored them up until now)" : "")));
                eventHandlers.BadDataFound(reader.Context);
                return 0;
            }

            bool haveIncremented_bufferOverrunsWhereColumnValueWasBlank = false;


            for (int i = 0; i < currentLine.Length; i++)
            {
                //about to do a buffer overrun
                if (i >= _headers.Length)
                    if (currentLine[i].IsBasicallyNull())
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
                        listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Warning, "Column mismatch on line " + reader.Context.RawRow + " of file '" + dt.TableName + "', it has too many columns (expected " + _headers.Length + " columns but line had " + currentLine.Length + ")." + (_bufferOverrunsWhereColumnValueWasBlank > 0 ? "( " + _bufferOverrunsWhereColumnValueWasBlank + " Previously lines also suffered from buffer overruns but the overrunning values were empty so we had ignored them up until now)" : "")));
                        eventHandlers.BadDataFound(reader.Context);
                        break;
                    }

                //its an empty header, dont bother populating it
                if (_headers[i].IsBasicallyNull())
                    if (!currentLine[i].IsBasicallyNull())
                        throw new FileLoadException("The header at index " + i + " in flat file '" +dt.TableName+ "' had no name but there was a value in the data column (on Line number " + reader.Context.RawRow + ")");
                    else
                        continue;

                //sometimes flat files have ,NULL,NULL,"bob" in instead of ,,"bob"
                if (currentLine[i].IsBasicallyNull())
                    rowValues.Add(_headers[i], DBNull.Value);
                else
                {
                    object hackedValue = _hackValuesFunc(currentLine[i]);

                    if (hackedValue is string)
                        hackedValue = ((string)hackedValue).Trim();

                    try
                    {
                        //if we are trying to load a boolean value out of the flat file into the strongly typed C# data type
                        if (dt.Columns[_headers[i]].DataType == typeof(Boolean))
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

                        rowValues.Add(_headers[i], Convert.ChangeType(hackedValue, dt.Columns[_headers[i]].DataType));
                        //convert to correct datatype (datatype was setup in SetupTypes)
                    }
                    catch (Exception e)
                    {
                        throw new FileLoadException("Error reading file '" + dt.TableName + "'.  Problem loading value " + currentLine[i] + " into data table (on Line number " + reader.Context.RawRow + ") the header we were trying to populate was " + _headers[i] + " and was of datatype " + dt.Columns[_headers[i]].DataType, e);
                    }
                }
            }

            if(!BadLines.Contains(reader.Context.RawRow))
            {
                DataRow currentRow = dt.Rows.Add();
                foreach (KeyValuePair<string, object> kvp in rowValues)
                    currentRow[kvp.Key] = kvp.Value;
                
                return 1;
            }

            return 0;
        }
    }
}

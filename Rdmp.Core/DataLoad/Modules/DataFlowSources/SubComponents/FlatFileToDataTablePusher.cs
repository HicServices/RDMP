// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using Rdmp.Core.DataFlowPipeline.Requirements;
using Rdmp.Core.ReusableLibraryCode;
using Rdmp.Core.ReusableLibraryCode.Progress;
using TypeGuesser;
using TypeGuesser.Deciders;

namespace Rdmp.Core.DataLoad.Modules.DataFlowSources.SubComponents;

/// <summary>
/// This class is a sub component of <see cref="DelimitedFlatFileDataFlowSource"/>, it is responsible for adding rows read from the CSV file to
/// the DataTable built by <see cref="FlatFileColumnCollection"/>.
/// </summary>
public class FlatFileToDataTablePusher
{
    private readonly FlatFileToLoad _fileToLoad;
    private readonly FlatFileColumnCollection _headers;
    private readonly Func<string, object> _hackValuesFunc;
    private readonly bool _attemptToResolveNewlinesInRecords;
    private readonly CultureInfo _culture;
    private readonly string _explicitDateTimeFormat;
    private TypeDeciderFactory typeDeciderFactory;

    /// <summary>
    /// Used in the event of reading too few cells for the current line.  The pusher will peek at the next lines to see if they
    /// make up a coherent row e.g. if a free text field is splitting up the document with newlines.  If the peeked lines do not
    /// resolve the problem then the line will be marked as BadData and the peeked records must be reprocessed by <see cref="DelimitedFlatFileDataFlowSource"/>
    /// </summary>
    public FlatFileLine PeekedRecord;

    /// <summary>
    /// All line numbers of the source file being read that could not be processed.  Allows BadDataFound etc to be called multiple times without skipping
    /// records by accident.
    /// </summary>
    public HashSet<int> BadLines = new();

    /// <summary>
    /// This is incremented when too many values are read from the file to match the header count BUT the values read were null/empty
    /// </summary>
    private long _bufferOverrunsWhereColumnValueWasBlank;

    /// <summary>
    /// We only complain once about headers not matching the number of cell values
    /// </summary>
    private bool _haveComplainedAboutColumnMismatch;

    public FlatFileToDataTablePusher(FlatFileToLoad fileToLoad, FlatFileColumnCollection headers,
        Func<string, object> hackValuesFunc, bool attemptToResolveNewlinesInRecords, CultureInfo culture,
        string explicitDateTimeFormat)
    {
        _fileToLoad = fileToLoad;
        _headers = headers;
        _hackValuesFunc = hackValuesFunc;
        _attemptToResolveNewlinesInRecords = attemptToResolveNewlinesInRecords;
        _culture = culture ?? CultureInfo.CurrentCulture;
        _explicitDateTimeFormat = explicitDateTimeFormat;
        typeDeciderFactory = new TypeDeciderFactory(_culture);

        if (!string.IsNullOrWhiteSpace(explicitDateTimeFormat))
            typeDeciderFactory.Settings.ExplicitDateFormats = new[] { explicitDateTimeFormat };
    }

    public int PushCurrentLine(CsvReader reader, FlatFileLine lineToPush, DataTable dt, IDataLoadEventListener listener,
        FlatFileEventHandlers eventHandlers)
    {
        //skip the blank lines
        if (lineToPush.Cells.Length == 0 || lineToPush.Cells.All(h => h.IsBasicallyNull()))
            return 0;

        var headerCount = _headers.CountNotNull;

        //if the number of not empty headers doesn't match the headers in the data table
        if (dt.Columns.Count != headerCount)
            if (!_haveComplainedAboutColumnMismatch)
            {
                listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Warning,
                    $"Flat file '{_fileToLoad.File.Name}' line number '{reader.Context.Parser.RawRow}' had  {headerCount} columns while the destination DataTable had {dt.Columns.Count} columns.  This message appears only once per file"));
                _haveComplainedAboutColumnMismatch = true;
            }

        var rowValues = new Dictionary<string, object>();

        if (lineToPush.Cells.Length < headerCount)
            if (!DealWithTooFewCellsOnCurrentLine(reader, lineToPush, listener, eventHandlers))
                return 0;

        var haveIncremented_bufferOverrunsWhereColumnValueWasBlank = false;


        for (var i = 0; i < lineToPush.Cells.Length; i++)
        {
            //about to do a buffer overrun
            if (i >= _headers.Length)
                if (lineToPush[i].IsBasicallyNull())
                {
                    if (!haveIncremented_bufferOverrunsWhereColumnValueWasBlank)
                    {
                        _bufferOverrunsWhereColumnValueWasBlank++;
                        haveIncremented_bufferOverrunsWhereColumnValueWasBlank = true;
                    }

                    continue; //do not bother buffer overrunning with null whitespace stuff
                }
                else
                {
                    var errorMessage =
                        $"Column mismatch on line {reader.Context.Parser.RawRow} of file '{dt.TableName}', it has too many columns (expected {_headers.Length} columns but line had  {lineToPush.Cells.Length})";

                    if (_bufferOverrunsWhereColumnValueWasBlank > 0)
                        errorMessage +=
                            $" ( {_bufferOverrunsWhereColumnValueWasBlank} Previously lines also suffered from buffer overruns but the overrunning values were empty so we had ignored them up until now)";

                    listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Warning, errorMessage));
                    eventHandlers.BadDataFound(lineToPush);
                    break;
                }

            //if we are ignoring this header
            if (_headers.IgnoreColumnsList.Contains(_headers[i]))
                continue;

            //it's an empty header, don't bother populating it
            if (_headers[i].IsBasicallyNull())
                if (!lineToPush[i].IsBasicallyNull())
                    throw new FileLoadException(
                        $"The header at index {i} in flat file '{dt.TableName}' had no name but there was a value in the data column (on Line number {reader.Context.Parser.RawRow})");
                else
                    continue;

            //sometimes flat files have ,NULL,NULL,"bob" in instead of ,,"bob"
            if (lineToPush[i].IsBasicallyNull())
            {
                rowValues.Add(_headers[i], DBNull.Value);
            }
            else
            {
                var hackedValue = _hackValuesFunc(lineToPush[i]);

                if (hackedValue is string value)
                    hackedValue = value.Trim();

                try
                {
                    if (hackedValue is string s &&
                        typeDeciderFactory.Dictionary.TryGetValue(dt.Columns[_headers[i]].DataType, out var decider))
                        hackedValue = decider.Parse(s);

                    rowValues.Add(_headers[i], hackedValue);
                }
                catch (Exception e)
                {
                    throw new FileLoadException(
                        $"Error reading file '{dt.TableName}'.  Problem loading value {lineToPush[i]} into data table (on Line number {reader.Context.Parser.RawRow}) the header we were trying to populate was {_headers[i]} and was of datatype {dt.Columns[_headers[i]].DataType}",
                        e);
                }
            }
        }

        if (!BadLines.Contains(reader.Context.Parser.RawRow))
        {
            var currentRow = dt.Rows.Add();
            foreach (var kvp in rowValues)
                currentRow[kvp.Key] = kvp.Value;

            return 1;
        }

        return 0;
    }

    private bool DealWithTooFewCellsOnCurrentLine(CsvReader reader, FlatFileLine lineToPush,
        IDataLoadEventListener listener, FlatFileEventHandlers eventHandlers)
    {
        if (!_attemptToResolveNewlinesInRecords)
        {
            //we read too little cell count but we don't want to solve the problem
            listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Warning,
                $"Too few columns on line {reader.Context.Parser.RawRow} of file '{_fileToLoad}', it has too many columns (expected {_headers.Length} columns but line had {lineToPush.Cells.Length}).{(_bufferOverrunsWhereColumnValueWasBlank > 0 ? $"( {_bufferOverrunsWhereColumnValueWasBlank} Previously lines also suffered from buffer overruns but the overrunning values were empty so we had ignored them up until now)" : "")}"));
            eventHandlers.BadDataFound(lineToPush);

            //didn't bother trying to fix the problem
            return false;
        }

        //We want to try to fix the problem by reading more data

        //Create a composite row
        var newCells = new List<string>(lineToPush.Cells);

        //track what we are Reading in case it doesn't work
        var allPeekedLines = new List<FlatFileLine>();

        do
        {
            FlatFileLine peekedLine;

            //try adding the next row
            if (reader.Read())
            {
                peekedLine = new FlatFileLine(reader.Context);

                //peeked line was 'valid' on its own
                if (peekedLine.Cells.Length >= _headers.Length)
                {
                    //queue it for reprocessing
                    PeekedRecord = peekedLine;

                    //and mark everything else as bad
                    AllBad(lineToPush, allPeekedLines, eventHandlers);
                    return false;
                }

                //peeked line was invalid (too short) so we can add it onto ourselves
                allPeekedLines.Add(peekedLine);
            }
            else
            {
                //Ran out of space in the file without fixing the problem so it's all bad
                AllBad(lineToPush, allPeekedLines, eventHandlers);

                //couldn't fix the problem
                return false;
            }

            //add the peeked line to the current cells
            //add the first record as an extension of the last cell in current row
            if (peekedLine.Cells.Length != 0)
                newCells[^1] += Environment.NewLine + peekedLine.Cells[0];
            else
                newCells[^1] += Environment.NewLine; //the next line was completely blank! just add a new line

            //add any further cells on after that
            newCells.AddRange(peekedLine.Cells.Skip(1));
        } while (newCells.Count < _headers.Length);


        //if we read too much or reached the end of the file
        if (newCells.Count > _headers.Length)
        {
            AllBadExceptLastSoRequeueThatOne(lineToPush, allPeekedLines, eventHandlers);
            return false;
        }

        if (newCells.Count != _headers.Length)
            throw new Exception("We didn't over read or reach end of file, how did we get here?");

        //we managed to create a full row
        lineToPush.Cells = newCells.ToArray();

        //problem was fixed
        return true;
    }


    public DataTable StronglyTypeTable(DataTable workingTable, ExplicitTypingCollection explicitTypingCollection)
    {
        var deciders = new Dictionary<int, IDecideTypesForStrings>();
        var factory = new TypeDeciderFactory(_culture);

        if (!string.IsNullOrWhiteSpace(_explicitDateTimeFormat))
            factory.Settings.ExplicitDateFormats = new[] { _explicitDateTimeFormat };

        var dtCloned = workingTable.Clone();
        dtCloned.BeginLoadData();
        var typeChangeNeeded = false;

        foreach (DataColumn col in workingTable.Columns)
        {
            //if we have already handled it
            if (explicitTypingCollection != null &&
                explicitTypingCollection.ExplicitTypesCSharp.ContainsKey(col.ColumnName))
                continue;

            //let's make a decision about the data type to use based on the contents
            var computedType = new Guesser();
            computedType.AdjustToCompensateForValues(col);

            //Type based on the contents of the column
            if (computedType.ShouldDowngradeColumnTypeToMatchCurrentEstimate(col))
            {
                dtCloned.Columns[col.ColumnName].DataType = computedType.Guess.CSharpType;

                //if we have a type decider to parse this data type
                if (factory.IsSupported(computedType.Guess.CSharpType))
                    deciders.Add(col.Ordinal,
                        factory.Create(computedType.Guess.CSharpType)); //record column index and parser

                typeChangeNeeded = true;
            }
        }

        if (typeChangeNeeded)
        {
            foreach (DataRow row in workingTable.Rows)
                dtCloned.Rows.Add(row.ItemArray.Select((v, idx) =>
                    deciders.TryGetValue(idx, out var decider) && v is string s ? decider.Parse(s) : v).ToArray());

            return dtCloned;
        }

        return workingTable;
    }

    private void AllBadExceptLastSoRequeueThatOne(FlatFileLine lineToPush, List<FlatFileLine> allPeekedLines,
        FlatFileEventHandlers eventHandlers)
    {
        //the current line is bad
        eventHandlers.BadDataFound(lineToPush);

        //last line resulted in the overrun so requeue it
        PeekedRecord = allPeekedLines.Last();

        //but throw away everything else we read
        foreach (var line in allPeekedLines.Take(allPeekedLines.Count - 1))
            eventHandlers.BadDataFound(line);
    }

    private static void AllBad(FlatFileLine lineToPush, List<FlatFileLine> allPeekedLines,
        FlatFileEventHandlers eventHandlers)
    {
        //the current line is bad
        eventHandlers.BadDataFound(lineToPush);

        foreach (var line in allPeekedLines)
            eventHandlers.BadDataFound(line);
    }
}
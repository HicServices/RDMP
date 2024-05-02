// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using CsvHelper;
using FAnsi.Discovery;
using FAnsi.Extensions;
using Rdmp.Core.DataFlowPipeline.Requirements;
using Rdmp.Core.DataLoad.Modules.Exceptions;
using Rdmp.Core.ReusableLibraryCode;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.DataLoad.Modules.DataFlowSources.SubComponents;

/// <summary>
///     This class is a sub component of <see cref="DelimitedFlatFileDataFlowSource" />, it is responsible for processing
///     the headers (or overriding headers)
///     of a CSV (TSV etc) file.
///     <para>
///         The component has two main operational modes after it has read headers:
///         <see cref="GetDataTableWithHeaders" /> and  <see cref="MakeDataTableFitHeaders" />
///     </para>
/// </summary>
public class FlatFileColumnCollection
{
    /// <summary>
    ///     Text to display in ASCII art of column matches when a column from the source could not be matched
    ///     with the destination.
    /// </summary>
    public const string UnmatchedText = "????";

    private readonly FlatFileToLoad _toLoad;
    private readonly bool _makeHeaderNamesSane;
    private readonly ExplicitTypingCollection _explicitlyTypedColumns;
    private readonly string _forceHeaders;
    private readonly bool _forceHeadersReplacesFirstLineInFile;
    private readonly string _ignoreColumns;

    /// <summary>
    ///     The columns from the file the user does not want to load into the destination (this will not help
    ///     you avoid bad data).
    /// </summary>
    public HashSet<string> IgnoreColumnsList { get; private set; }

    public FlatFileColumnCollection(FlatFileToLoad toLoad, bool makeHeaderNamesSane,
        ExplicitTypingCollection explicitlyTypedColumns, string forceHeaders, bool forceHeadersReplacesFirstLineInFile,
        string ignoreColumns)
    {
        _toLoad = toLoad;
        _makeHeaderNamesSane = makeHeaderNamesSane;
        _explicitlyTypedColumns = explicitlyTypedColumns;
        _forceHeaders = forceHeaders;
        _forceHeadersReplacesFirstLineInFile = forceHeadersReplacesFirstLineInFile;
        _ignoreColumns = ignoreColumns;
    }

    public string this[int index] => _headers[index];

    private enum State
    {
        Start,
        AfterHeadersRead,
        AfterTableGenerated
    }

    private State _state = State.Start;

    /// <summary>
    ///     The Headers found in the file / overridden by ForceHeaders
    /// </summary>
    private string[] _headers;

    /// <summary>
    ///     Column headers that appear in the middle of the file (i.e. not trailing) but that don't have a header name.  These
    ///     get thrown away
    ///     and they must never have data in them.  This lets you have a full blank column in the middle of your file e.g. if
    ///     you have inserted
    ///     it via Excel
    /// </summary>
    public ReadOnlyCollection<DataColumn> UnamedColumns = new(Array.Empty<DataColumn>()); //start off with none

    public bool FileIsEmpty;

    /// <summary>
    ///     used to advise user if he has selected the wrong separator
    /// </summary>
    private readonly string[] _commonSeparators = { "|", ",", "    ", "#" };

    /// <summary>
    ///     Counts the number of headers that are not null
    /// </summary>
    public int CountNotNull
    {
        get { return _headers.Except(IgnoreColumnsList).Count(h => !h.IsBasicallyNull()); }
    }

    /// <summary>
    ///     The number of headers including null ones (but not trailing null headers)
    /// </summary>
    public int Length => _headers.Length;


    public void GetHeadersFromFile(CsvReader r)
    {
        //check state
        if (_state != State.Start)
            throw new Exception($"Illegal state, headers cannot be read at state {_state}");

        _state = State.AfterHeadersRead;


        //if we are not forcing headers we must get them from the file
        if (string.IsNullOrWhiteSpace(_forceHeaders))
        {
            //read the first record from the file (this will read the header and first row
            var empty = !r.Read();

            if (empty)
            {
                FileIsEmpty = true;
                return;
            }

            //get headers from first line of the file
            r.ReadHeader();
            _headers = r.HeaderRecord;
        }
        else
        {
            //user has some specific headers he wants to override with
            _headers = _forceHeaders.Split(new[] { r.Configuration.Delimiter }, StringSplitOptions.None);
        }

        //ignore these columns (trimmed and ignoring case)
        if (!string.IsNullOrWhiteSpace(_ignoreColumns))
            IgnoreColumnsList = new HashSet<string>(
                _ignoreColumns.Split(new[] { r.Configuration.Delimiter }, StringSplitOptions.None)
                    .Select(h => h.Trim())
                , StringComparer.CurrentCultureIgnoreCase);
        else
            IgnoreColumnsList = new HashSet<string>();

        //Make adjustments to the headers (trim etc)

        //trim them
        for (var i = 0; i < _headers.Length; i++)
            if (!string.IsNullOrWhiteSpace(_headers[i]))
                _headers[i] = _headers[i].Trim();

        //throw away trailing null headers e.g. the header line "Name,Date,,,"
        var trailingNullHeaders = _headers.Reverse().TakeWhile(s => s.IsBasicallyNull()).Count();

        if (trailingNullHeaders > 0)
            _headers = _headers.Take(_headers.Length - trailingNullHeaders).ToArray();

        //and maybe also help them out with a bit of sanity fixing
        if (_makeHeaderNamesSane)
            for (var i = 0; i < _headers.Length; i++)
                _headers[i] = QuerySyntaxHelper.MakeHeaderNameSensible(_headers[i]);
    }


    /// <summary>
    ///     Creates a new empty DataTable has only the columns found in the headers that were read during
    ///     <see cref="GetHeadersFromFile" />
    /// </summary>
    /// <param name="listener"></param>
    /// <returns></returns>
    public DataTable GetDataTableWithHeaders(IDataLoadEventListener listener)
    {
        if (_state != State.AfterHeadersRead)
            throw new Exception($"Illegal state, data table cannot be created at state {_state}");

        _state = State.AfterTableGenerated;

        var dt = new DataTable();

        var duplicateHeaders = new List<string>();
        var unamedColumns = new List<DataColumn>();

        //create a string column for each header - these will change type once we have read some data
        foreach (var header in _headers)
        {
            var h = header;

            //if we are ignoring this column
            if (h != null && IgnoreColumnsList.Contains(h.Trim()))
                continue; //skip adding to dt

            //watch for duplicate columns
            if (dt.Columns.Contains(header))
                if (_makeHeaderNamesSane)
                {
                    h = MakeHeaderUnique(header, dt.Columns, listener, this);
                }
                else
                {
                    duplicateHeaders.Add(header);
                    continue;
                }

            if (h.IsBasicallyNull())
            {
                unamedColumns.Add(dt.Columns.Add(h));
            }
            else
                //override type
            if (_explicitlyTypedColumns?.ExplicitTypesCSharp.TryGetValue(h, out var t) == true)
            {
                var c = dt.Columns.Add(h, t);

                //if the user wants a string don't let downstream components pick a different Type (by assuming it is is untyped)
                if (c.DataType == typeof(string))
                    c.SetDoNotReType(true);
            }
            else
            {
                dt.Columns.Add(h);
            }
        }

        UnamedColumns = new ReadOnlyCollection<DataColumn>(unamedColumns);

        return duplicateHeaders.Any()
            ? throw new FlatFileLoadException(
                $"Found the following duplicate headers in file '{_toLoad.File}':{string.Join(",", duplicateHeaders)}")
            : dt;
    }

    /// <summary>
    ///     Takes an existing DataTable with a fixed schema and validates the columns read during
    ///     <see cref="GetHeadersFromFile" /> against it making minor changes
    ///     where appropriate to match the schema
    /// </summary>
    /// <param name="dt"></param>
    /// <param name="listener"></param>
    /// <returns></returns>
    public void MakeDataTableFitHeaders(DataTable dt, IDataLoadEventListener listener)
    {
        if (_state != State.AfterHeadersRead)
            throw new Exception($"Illegal state, data table cannot be created at state {_state}");

        _state = State.AfterTableGenerated;

        var ASCIIArt = new StringBuilder();

        var headersNotFound = new List<string>();

        for (var index = 0; index < _headers.Length; index++)
        {
            ASCIIArt.Append($"[{index}]");

            if (dt.Columns.Contains(_headers[index])) //exact match
            {
                ASCIIArt.AppendLine($"{_headers[index]}>>>{_headers[index]}");
                continue;
            }

            if (string.IsNullOrWhiteSpace(_headers[index])) //Empty column header, ignore it
            {
                ASCIIArt.AppendLine("Blank Column>>>IGNORED");
                continue;
            }

            //if we are ignoring the header
            if (IgnoreColumnsList.Contains(_headers[index]))
            {
                ASCIIArt.AppendLine($"{_headers[index]}>>>IGNORED");
                continue;
            }

            //try replacing spaces with underscores
            if (dt.Columns.Contains(_headers[index].Replace(" ", "_")))
            {
                var before = _headers[index];
                _headers[index] = _headers[index].Replace(" ", "_");

                ASCIIArt.AppendLine($"{before}>>>{_headers[index]}");
                continue;
            }

            //try replacing spaces with nothing
            if (dt.Columns.Contains(_headers[index].Replace(" ", "")))
            {
                var before = _headers[index];
                _headers[index] = _headers[index].Replace(" ", "");

                ASCIIArt.AppendLine($"{before}>>>{_headers[index]}");
                continue;
            }

            ASCIIArt.AppendLine($"{_headers[index]}>>>{UnmatchedText}");
            headersNotFound.Add(_headers[index]);
        }

        //now that we have adjusted the header names
        var unmatchedColumns =
            dt.Columns.Cast<DataColumn>()
                .Where(c => !_headers.Any(h =>
                    h != null &&
                    h.ToLower().Equals(c.ColumnName
                        .ToLower()))) //get all columns in data table where there are not any with the same name
                .Select(c => c.ColumnName)
                .ToArray();

        if (unmatchedColumns.Any())
            ASCIIArt.AppendLine(
                $"{Environment.NewLine}Unmatched Columns In DataTable:{Environment.NewLine}{string.Join(Environment.NewLine, unmatchedColumns)}");

        //if there is exactly 1 column found by the program and there are unmatched columns it is likely the user has selected the wrong separator
        if (_headers.Length == 1 && unmatchedColumns.Any())
            foreach (var commonSeparator in _commonSeparators)
                if (_headers[0].Contains(commonSeparator))
                    listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Error,
                        $"Your separator does not appear in the headers line of your file ({_toLoad.File.Name}) but the separator '{commonSeparator}' does... did you mean to set the Separator to '{commonSeparator}'? The headers line is:\"{_headers[0]}\""));

        listener.OnNotify(this, new NotifyEventArgs(
            headersNotFound.Any()
                ? ProgressEventType.Error
                : ProgressEventType.Information, //information or warning if there are unrecognised field names
            $"I will now tell you about how the columns in your file do or do not match the columns in your database, Matching flat file columns (or forced replacement headers) against database headers resulted in:{Environment.NewLine}{ASCIIArt}")); //tell them about what columns match what


        if (headersNotFound.Any())
            throw new Exception(
                $"Could not find a suitable target column for flat file columns {string.Join(",", headersNotFound)} amongst database data table columns ({string.Join(",", from DataColumn col in dt.Columns select col.ColumnName)})");
    }

    public static string MakeHeaderUnique(string newColumnName, DataColumnCollection columnsSoFar,
        IDataLoadEventListener listener, object sender)
    {
        //if it is already unique then that's fine
        if (!columnsSoFar.Contains(newColumnName))
            return newColumnName;

        //otherwise issue a rename
        var number = 2;
        while (columnsSoFar.Contains($"{newColumnName}_{number}"))
            number++;

        var newName = $"{newColumnName}_{number}";

        //found a novel number
        listener.OnNotify(sender, new NotifyEventArgs(ProgressEventType.Warning,
            $"Renamed duplicate column '{newColumnName}' to '{newName}'"));
        return newName;
    }

    /// <summary>
    ///     Use only when ForceHeaders is on and ForceHeadersReplacesFirstLineInFile is true.  Pass the header line that was
    ///     read from the file
    ///     that will be ignored (<paramref name="row" />).  This method will show the user what replacements were made.
    /// </summary>
    /// <param name="row"></param>
    /// <param name="listener"></param>
    public void ShowForceHeadersAsciiArt(IReaderRow row, IDataLoadEventListener listener)
    {
        if (_state < State.AfterHeadersRead)
            throw new Exception($"Illegal state:{_state}");

        if (string.IsNullOrWhiteSpace(_forceHeaders))
            throw new Exception("There are no force headers! how did we get here");

        if (!_forceHeadersReplacesFirstLineInFile)
            throw new Exception("Headers do not replace the first line in the file, how did we get here!");

        //create an ascii art representation of the headers being replaced in the format
        //[0]MySensibleCol>>>My Silly Coll#
        var asciiArt = new StringBuilder();
        for (var i = 0; i < _headers.Length; i++)
        {
            asciiArt.Append($"[{i}]{_headers[i]}>>>");
            asciiArt.AppendLine(i < row.ColumnCount ? row[i] : "???");
        }

        for (var i = _headers.Length; i < row.ColumnCount; i++)
            asciiArt.AppendLine($"[{i}]???>>>{row[i]}");

        listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information,
            $"Your attacher has ForceHeaders and ForceHeadersReplacesFirstLineInFile=true, I will now tell you about the first line of data in the file that you skipped (and how it related to your forced headers).  Replacement headers are {Environment.NewLine}{Environment.NewLine}{asciiArt}"));

        if (row.ColumnCount != _headers.Length)
            listener.OnNotify(this,
                new NotifyEventArgs(ProgressEventType.Warning,
                    "The number of ForceHeader replacement headers specified does not match the number of headers in the file (being replaced)"));

        var discarded = new StringBuilder();
        for (var i = 0; i < row.ColumnCount; i++)
        {
            if (i > 0)
                discarded.Append(',');
            discarded.Append(row[i]);
        }

        listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information,
            $"Skipped first line of file because there are forced replacement headers, we discarded: {discarded}"));
    }
}
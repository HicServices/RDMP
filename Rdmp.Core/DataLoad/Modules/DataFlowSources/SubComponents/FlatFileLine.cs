// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using CsvHelper;

namespace Rdmp.Core.DataLoad.Modules.DataFlowSources.SubComponents;

/// <summary>
///     Point in time record of a line read from CsvHelper including ReadingContext information such as
///     <see cref="LineNumber" />.  Can include multiple lines
///     of the underlying file if there is proper qualifying quotes and newlines in the csv e.g. when including free text
///     columns.
/// </summary>
public class FlatFileLine
{
    /// <summary>
    ///     The RAW file line number that this line reflects.  Where a record spans multiple lines (e.g. when it has newlines
    ///     in quote qualified fields) it
    ///     seems to be the last line number in the record
    /// </summary>
    public int LineNumber { get; }

    /// <summary>
    ///     The values as interpreted by CsvHelper for the current line
    /// </summary>
    public string[] Cells { get; set; }

    /// <summary>
    ///     The absolute text as it appears in the flat file being read for this 'line'
    /// </summary>
    public string RawRecord { get; set; }

    /// <summary>
    ///     The state of the CSVReader when the line was read
    /// </summary>
    public CsvContext ReadingContext { get; set; }

    public FlatFileLine(CsvContext context)
    {
        LineNumber = context.Parser.RawRow;
        Cells = context.Parser.Record;
        RawRecord = context.Parser.RawRecord;
        ReadingContext = context;

        //Doesn't seem to be correct:  StartPosition = context.RawRecordStartPosition;
    }

    public FlatFileLine(BadDataFoundArgs bad)
    {
        LineNumber = bad.Context.Parser.RawRow;
        Cells = Array.Empty<string>();
        RawRecord = bad.RawRecord;
        ReadingContext = bad.Context;
    }

    public string this[int i] => Cells[i];

    public string GetLineDescription()
    {
        return $"line {LineNumber}";
    }
}
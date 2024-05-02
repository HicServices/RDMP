// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Data;
using System.IO;
using System.Linq;
using Rdmp.Core.DataLoad.Modules.Exceptions;

namespace Rdmp.Core.DataLoad.Modules.Attachers;

/// <summary>
///     User generated file describing the layout of a fixed width file (See FixedWidthAttacher).  Includes the character
///     positions of each named field and date
///     format (where applicable).
/// </summary>
public class FixedWidthFormatFile
{
    private readonly FileInfo _pathToFormatFile;

    public FixedWidthColumn[] FormatColumns { get; }

    public FixedWidthFormatFile(FileInfo pathToFormatFile)
    {
        _pathToFormatFile = pathToFormatFile;
        var readAllLines = File.ReadAllLines(_pathToFormatFile.FullName);

        var headers = readAllLines[0];

        EnsureHeaderIntact(headers);

        //get rid of blank lines
        readAllLines = readAllLines.Where(l => !string.IsNullOrWhiteSpace(l)).ToArray();

        //create a format field for each line in the format file
        FormatColumns = new FixedWidthColumn[readAllLines.Length - 1];

        //now add values
        for (var index = 0; index < readAllLines.Length - 1; index++)
        {
            //skip header line
            var cellsOnRowAsSplitString = readAllLines[index + 1].Split(',');

            FormatColumns[index].From = int.Parse(cellsOnRowAsSplitString[0]);
            FormatColumns[index].To = int.Parse(cellsOnRowAsSplitString[1]);
            FormatColumns[index].Field = cellsOnRowAsSplitString[2];
            FormatColumns[index].Size = int.Parse(cellsOnRowAsSplitString[3]);

            //It's ok to omit this column for specific rows (that aren't dates)
            if (cellsOnRowAsSplitString.Length > 4)
                FormatColumns[index].DateFormat =
                    cellsOnRowAsSplitString[4]
                        .Replace("ccyy",
                            "yyyy"); //some people think that ccyy is a valid way of expressing year formats... they are wrong

            if (FormatColumns[index].From + FormatColumns[index].Size - 1 != FormatColumns[index].To)
                throw new FlatFileLoadException(
                    $"Problem with format of field {FormatColumns[index].Field} From + Size -1 does not equal To");

            if (!string.IsNullOrWhiteSpace(FormatColumns[index].DateFormat))
                try
                {
                    DateTime.Now.ToString(FormatColumns[index].DateFormat);
                }
                catch (Exception e)
                {
                    throw new FlatFileLoadException(
                        $"Problem with flat file format which announced the date format as {FormatColumns[index].DateFormat} which C# says isn't a valid format",
                        e);
                }
        }
    }


    public DataTable GetDataTableFromFlatFile(FileInfo f)
    {
        //setup the table
        var toReturn = new DataTable();

        toReturn.BeginLoadData();
        foreach (var fixedWidthColumn in FormatColumns)
        {
            var dataColumn = toReturn.Columns.Add(fixedWidthColumn.Field);

            if (!string.IsNullOrWhiteSpace(fixedWidthColumn.DateFormat))
                dataColumn.DataType = typeof(DateTime);

            dataColumn.AllowDBNull = true;
        }

        var lineNumber = 0;

        //populate the table
        //foreach line in file
        foreach (var readAllLine in File.ReadLines(f.FullName))
        {
            lineNumber++;

            //add a new row to data table
            var dataRow = toReturn.Rows.Add();

            //foreach expected fixed width column
            foreach (var fixedWidthColumn in FormatColumns)
            {
                if (readAllLine.Length < fixedWidthColumn.To)
                    throw new FlatFileLoadException(
                        $"Error on line {lineNumber} of file {f.Name}, the format file ({_pathToFormatFile.FullName}) specified that a column {fixedWidthColumn.Field} would be found between character positions {fixedWidthColumn.From} and {fixedWidthColumn.To} but the current line is only {readAllLine.Length} characters long");

                //substring in order to get cell data
                var value = readAllLine.Substring(fixedWidthColumn.From - 1, fixedWidthColumn.Size);

                //if it's a null
                if (string.IsNullOrWhiteSpace(value))
                    dataRow[fixedWidthColumn.Field] = DBNull.Value;
                else
                    //it is a date column
                if (!string.IsNullOrWhiteSpace(fixedWidthColumn.DateFormat))
                    try
                    {
                        dataRow[fixedWidthColumn.Field] = DateTime.ParseExact(value, fixedWidthColumn.DateFormat, null);
                    }
                    catch (Exception e)
                    {
                        throw new Exception(
                            $"The value '{value}' was rejected by DateTime.ParseExact using the listed date time format '{fixedWidthColumn.DateFormat}'",
                            e);
                    }
                else //it's not a date
                    dataRow[fixedWidthColumn.Field] = value.Trim();
            }
        }

        toReturn.EndLoadData();
        return toReturn;
    }

    private void EnsureHeaderIntact(string header)
    {
        //From	To	Field	Size	DateFormat
        var expected = string.Join(",", typeof(FixedWidthColumn).GetFields().Select(f => f.Name));

        if (!header.TrimEnd().Equals(expected))
            throw new FlatFileLoadException(
                $"Format file headers in file {_pathToFormatFile.FullName} WAS: {header} WE EXPECTED: {expected}");
    }
}
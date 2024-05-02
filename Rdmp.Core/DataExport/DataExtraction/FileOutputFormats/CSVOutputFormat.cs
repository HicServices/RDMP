// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Rdmp.Core.DataExport.DataExtraction.FileOutputFormats;

/// <summary>
///     Helper class for writing data to CSV files.  This is a simplified version of Rfc4180Writer in that it simply strips
///     out all problem fields rather
///     than applying proper escaping etc.  This is done because some researcher end point tools / scripts do not support
///     the full specification of CSV and
///     it is easier to provide them with a file where problem symbols are not present than explain that they have to join
///     multiple lines together when it is
///     bounded by quotes.
/// </summary>
public class CSVOutputFormat : FileOutputFormat
{
    public string Separator { get; set; }
    public string DateFormat { get; set; }

    public int SeparatorsStrippedOut { get; private set; }
    private static readonly string[] ThingsToStripOut = { "\r", "\n", "\t", "\"" };
    private StreamWriter _sw;
    private StringBuilder _sbWriteOutLinesBuffer;
    private const string _illegalCharactersReplacement = " ";

    public CSVOutputFormat(string outputFilename, string separator, string dateFormat) : base(outputFilename)
    {
        Separator = separator;
        DateFormat = dateFormat;
    }

    public override string GetFileExtension()
    {
        return ".csv";
    }

    public override void Open()
    {
        _sw = new StreamWriter(OutputFilename);
        _sbWriteOutLinesBuffer = new StringBuilder();
    }

    public override void Open(bool append)
    {
        _sw = new StreamWriter(OutputFilename, append);
        _sbWriteOutLinesBuffer = new StringBuilder();
    }

    public override void WriteHeaders(DataTable t)
    {
        //write headers separated by separator
        _sw.Write(string.Join(Separator, t.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToArray()));

        _sw.WriteLine();
    }

    public override void Append(DataRow r)
    {
        //write headers separated by separator
        _sbWriteOutLinesBuffer.Append(string.Join(Separator, r.ItemArray.Select(CleanString)));

        //write the new line
        _sbWriteOutLinesBuffer.Append(Environment.NewLine);
    }

    public override void Flush()
    {
        _sw.Write(_sbWriteOutLinesBuffer.ToString());
        _sbWriteOutLinesBuffer.Clear();
        _sw.Flush();
    }

    public override void Close()
    {
        _sw?.Flush();
        _sw?.Close();
        _sw?.Dispose();
    }

    public string CleanString(object o)
    {
        var toReturn = CleanString(o, Separator, out var numberOfSeparatorsStrippedOutThisPass, DateFormat,
            RoundFloatsTo);

        SeparatorsStrippedOut += numberOfSeparatorsStrippedOutThisPass;

        return toReturn;
    }


    public static string CleanString(object o, string separator, out int separatorsStrippedOut, string dateFormat,
        int? roundFloatsTo)
    {
        if (o is DateTime dateTime)
        {
            separatorsStrippedOut = 0;
            return dateTime.ToString(dateFormat);
        }

        if (roundFloatsTo.HasValue)
        {
            separatorsStrippedOut = 0;
            switch (o)
            {
                case float f: return f.ToString($"N{roundFloatsTo.Value}");
                case decimal dec: return dec.ToString($"N{roundFloatsTo.Value}");
                case double d: return d.ToString($"N{roundFloatsTo.Value}");
            }
        }

        //in order to kep a count
        var regexReplace = new Regex(Regex.Escape(separator));

        separatorsStrippedOut = 0;

        while (o.ToString().Contains(separator))
        {
            o = regexReplace.Replace(o.ToString(), _illegalCharactersReplacement, 1);
            separatorsStrippedOut++;
        }

        return o is string s
            ? ThingsToStripOut.Aggregate(s,
                (current, cToStripOut) => current.Replace(cToStripOut, _illegalCharactersReplacement)).Trim()
            : o.ToString().Trim();
    }
}
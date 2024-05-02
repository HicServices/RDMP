// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Data;
using System.IO;
using System.Linq;
using FAnsi.Discovery;
using TypeGuesser;

namespace Rdmp.Core.ReusableLibraryCode;

public static class Rfc4180Writer
{
    public static void WriteDataTable(DataTable sourceTable, TextWriter writer, bool includeHeaders,
        QuerySyntaxHelper escaper = null)
    {
        if (includeHeaders)
        {
            var headerValues = sourceTable.Columns
                .OfType<DataColumn>()
                .Select(column => QuoteValue(column.ColumnName));

            writer.WriteLine(string.Join(",", headerValues));
        }

        var typeDictionary = sourceTable.Columns.Cast<DataColumn>().ToDictionary(c => c, c => new Guesser());
        foreach (var kvp in typeDictionary)
            kvp.Value.AdjustToCompensateForValues(kvp.Key);

        foreach (DataRow row in sourceTable.Rows)
        {
            var line = (from DataColumn col in sourceTable.Columns
                select QuoteValue(GetStringRepresentation(row[col],
                    typeDictionary[col].Guess.CSharpType == typeof(DateTime), escaper))).ToList();
            writer.WriteLine(string.Join(",", line));
        }

        writer.Flush();
    }

    private static string GetStringRepresentation(object o, bool allowDates, QuerySyntaxHelper escaper = null)
    {
        if (o == null || o == DBNull.Value)
            return null;

        switch (o)
        {
            case string s when allowDates && DateTime.TryParse(s, out var dt):
                return GetStringRepresentation(dt);
            case DateTime dateTime:
                return GetStringRepresentation(dateTime);
        }

        var str = o.ToString();

        str = escaper != null ? escaper.Escape(str) : str.Replace("\"", "\"\"");

        return str;
    }

    private static string GetStringRepresentation(DateTime dt)
    {
        return dt.ToString(dt.TimeOfDay == TimeSpan.Zero ? "yyyy-MM-dd" : "yyyy-MM-dd HH:mm:ss.fff");
    }

    private static string QuoteValue(string value)
    {
        return value == null ? "NULL" : $"\"{value}\"";
    }
}
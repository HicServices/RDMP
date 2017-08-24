using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

namespace ReusableLibraryCode
{
    public static class Rfc4180Writer
    {
        public static void WriteDataTable(DataTable sourceTable, TextWriter writer, bool includeHeaders)
        {
            if (includeHeaders)
            {
                IEnumerable<string> headerValues = sourceTable.Columns
                    .OfType<DataColumn>()
                    .Select(column => QuoteValue(column.ColumnName));

                writer.WriteLine(String.Join(",", headerValues));
            }

            IEnumerable<string> items = null;

            foreach (DataRow row in sourceTable.Rows)
            {
                items = row.ItemArray.Select(o =>
                    QuoteValue(GetStringRepresentation(o)));

                writer.WriteLine(String.Join(",", items));
            }

            writer.Flush();
        }

        private static string GetStringRepresentation(object o)
        {
            if (o == null || o == DBNull.Value)
                return null;

            if (o is DateTime)
            {
                DateTime dt = (DateTime)o;

                return dt.ToString("yyyy-MM-dd HH:mm:ss.fff");
            }

            return o.ToString();
        }

        private static string QuoteValue(string value)
        {
            if (value == null)
                return "NULL";

            return String.Concat("\"",
                value.Replace("\"", "\"\""), "\"");
        }
    }
}
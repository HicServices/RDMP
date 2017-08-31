using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using ReusableLibraryCode.DataTableExtension;

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
            
            var helper = new DataTableHelper(sourceTable);
            var typeDictionary = helper.GetTypeDictionary();
            
            foreach (DataRow row in sourceTable.Rows)
            {
                var line = new List<string>();
                
                foreach (DataColumn col in sourceTable.Columns)
                    line.Add(QuoteValue(GetStringRepresentation(row[col], typeDictionary[col.ColumnName].CurrentEstimate == typeof(DateTime))));
                
                writer.WriteLine(String.Join(",", line));
            }

            writer.Flush();
        }

        private static string GetStringRepresentation(object o, bool allowDates)
        {
            if (o == null || o == DBNull.Value)
                return null;

            var s = o as string;
            if (s != null && allowDates)
            {
                DateTime dt;
                if (DateTime.TryParse(s, out dt))
                    return GetStringRepresentation(dt);
            }

            if (o is DateTime)
                GetStringRepresentation((DateTime) o);

            return o.ToString();
        }

        private static string GetStringRepresentation(DateTime dt)
        {
            if (dt.TimeOfDay == TimeSpan.Zero)
                return dt.ToString("yyyy-MM-dd");

            return dt.ToString("yyyy-MM-dd HH:mm:ss");
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
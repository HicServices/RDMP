using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using ReusableLibraryCode.DatabaseHelpers.Discovery.TypeTranslation;

namespace ReusableLibraryCode
{
    public static class Rfc4180Writer
    {
        public static void WriteDataTable(DataTable sourceTable, TextWriter writer, bool includeHeaders, QuerySyntaxHelper escaper = null)
        {
            if (includeHeaders)
            {
                IEnumerable<string> headerValues = sourceTable.Columns
                    .OfType<DataColumn>()
                    .Select(column => QuoteValue(column.ColumnName));

                writer.WriteLine(String.Join(",", headerValues));
            }
            
            var typeDictionary = sourceTable.Columns.Cast<DataColumn>().ToDictionary(c => c, c => new DataTypeComputer(c));

            foreach (DataRow row in sourceTable.Rows)
            {
                var line = new List<string>();
                
                foreach (DataColumn col in sourceTable.Columns)
                    line.Add(QuoteValue(GetStringRepresentation(row[col], typeDictionary[col].CurrentEstimate == typeof(DateTime), escaper)));
                
                writer.WriteLine(String.Join(",", line));
            }

            writer.Flush();
        }

        private static string GetStringRepresentation(object o, bool allowDates, QuerySyntaxHelper escaper = null)
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
                return GetStringRepresentation((DateTime) o);

            var str = o.ToString();

            if (escaper != null)
                str = escaper.Escape(str);
            else
                str = str.Replace("\"", "\"\"");

            return str;
        }

        private static string GetStringRepresentation(DateTime dt)
        {
            if (dt.TimeOfDay == TimeSpan.Zero)
                return dt.ToString("yyyy-MM-dd");

            return dt.ToString("yyyy-MM-dd HH:mm:ss.fff");
        }

        private static string QuoteValue(string value)
        {
            if (value == null)
                return "NULL";

            return String.Concat("\"", value, "\"");

        }
    }
}
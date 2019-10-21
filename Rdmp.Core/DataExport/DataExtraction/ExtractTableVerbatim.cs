// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Data.Common;
using System.IO;
using System.Linq;
using FAnsi.Discovery;
using Rdmp.Core.DataExport.DataExtraction.FileOutputFormats;

namespace Rdmp.Core.DataExport.DataExtraction
{
    /// <summary>
    /// Helper class for fetching entire tables from a database and writing them to CSV.  It uses CSVOutputFormat.CleanString to strip out problem characters.
    /// Records are read one at a time rather than downloading as a DataTable to allow any size of table to be processed without running out of memory.
    /// </summary>
    public class ExtractTableVerbatim
    {
        private readonly string[] _tableNames;
        private readonly string _specificSQL;

        private readonly DirectoryInfo _outputDirectory;
        private readonly string _separator;
        private readonly string _dateTimeFormat;
        private string _specificSQLTableName;
        private DiscoveredServer _server;

        public string OutputFilename { get; private set; }

        public ExtractTableVerbatim(DiscoveredServer server, string[] tableNames, DirectoryInfo outputDirectory, string separator, string dateTimeFormat)
        {
            if(tableNames.Length == 0)
                throw new ArgumentException("You must select at least one table to extract");

            _tableNames = tableNames;
            _outputDirectory = outputDirectory;
            _separator = separator;
            _dateTimeFormat = dateTimeFormat;
            _server = server;
        }

        public ExtractTableVerbatim(DirectoryInfo outputDirectory, string separator, string dateTimeFormat,params DiscoveredTable[] tables)
            :this(tables.Select(t=>t.Database.Server).Distinct().Single(),
                tables.Select(t=>t.GetFullyQualifiedName()).ToArray(),
                outputDirectory,
                separator,
                dateTimeFormat)
        {
        }

        /// <summary>
        /// Runs the supplied SQL and puts it out to the file specified (in the outputDirectory), will deal with stripping separators etc automatically
        /// </summary>
        /// <param name="server"></param>
        /// <param name="sql">Some SQL you want to run (instead of a specific table)</param>
        /// <param name="outputName">The name of the csv file you would like to create in the outputDirectory.  Do not include.csv in your string it will be put on automatically</param>
        /// <param name="outputDirectory"></param>
        /// <param name="separator"></param>
        /// <param name="dateTimeFormat"></param>
        public ExtractTableVerbatim(DiscoveredServer server, string sql,string outputName, DirectoryInfo outputDirectory, string separator,string dateTimeFormat)
        {
            _specificSQL = sql;
            _specificSQLTableName = outputName;
            _outputDirectory = outputDirectory;
            _separator = separator;
            _dateTimeFormat = dateTimeFormat;
            _server = server;
        }

        public int DoExtraction()
        {
            int linesWritten = 0;

            using (var con = _server.GetConnection())
            {
                con.Open();

                if (_specificSQL != null)
                {
                    linesWritten += ExtractSQL(_specificSQL,_specificSQLTableName,con);
                }
            
                if(_tableNames != null)
                    foreach (string table in _tableNames)
                        linesWritten += ExtractSQL("select * from " + table, table,con);

                con.Close();
            }

            return linesWritten;
        }

        private int ExtractSQL(string sql, string tableName, DbConnection con)
        {
            DbCommand cmdExtract = _server.GetCommand( sql, con);

            if (!Directory.Exists(_outputDirectory.FullName))
                Directory.CreateDirectory(_outputDirectory.FullName);

            string filename = tableName.Replace("[", "").Replace("]", "").ToLower().Trim();

            if (!filename.EndsWith(".csv"))
                filename += ".csv";

            OutputFilename = Path.Combine(_outputDirectory.FullName , filename);

            StreamWriter sw = new StreamWriter(OutputFilename);

            cmdExtract.CommandTimeout = 500000;

            DbDataReader r = cmdExtract.ExecuteReader();
            WriteHeader(sw, r, _separator, _dateTimeFormat);
            int linesWritten = WriteBody(sw, r, _separator, _dateTimeFormat);

            r.Close();
            sw.Flush();
            sw.Close();

            return linesWritten;
        }

        public static void WriteHeader(StreamWriter sw, DbDataReader r, string separator, string dateTimeFormat)
        {
            int whoCares = -999;

            //write headers
            for (int i = 0; i < r.FieldCount; i++)
            {
                sw.Write(CSVOutputFormat.CleanString(r.GetName(i), separator, out whoCares, dateTimeFormat));
                if (i < r.FieldCount - 1)
                    sw.Write(separator);
                else
                    sw.WriteLine();
            }
        }
        public static int WriteBody(StreamWriter sw, DbDataReader r, string separator, string dateTimeFormat)
        {
            int whoCares = -999;
            int linesWritten = 0;

            while (r.Read())
            {
                //write values
                for (int i = 0; i < r.FieldCount; i++)
                {
                    //clean string
                    sw.Write(CSVOutputFormat.CleanString(r[i], separator, out whoCares, dateTimeFormat));
                    if (i < r.FieldCount - 1)
                        sw.Write(separator); //if not the last element add a ','
                    else
                    {
                        sw.WriteLine();
                    }
                }
                linesWritten++;
            }

            return linesWritten;
        }
    }
}

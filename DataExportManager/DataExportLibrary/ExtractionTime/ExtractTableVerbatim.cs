using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using DataExportLibrary.ExtractionTime.ExtractionPipeline;
using DataExportLibrary.ExtractionTime.ExtractionPipeline.Destinations;
using DataExportLibrary.ExtractionTime.FileOutputFormats;

using ReusableLibraryCode.DatabaseHelpers.Discovery;

namespace DataExportLibrary.ExtractionTime
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
            _tableNames = tableNames;
            _outputDirectory = outputDirectory;
            _separator = separator;
            _dateTimeFormat = dateTimeFormat;
            _server = server;
        }

        /// <summary>
        /// Runs the supplied SQL and puts it out to the file specified (in the outputDirectory), will deal with stripping separators etc automatically
        /// </summary>
        /// <param name="server"></param>
        /// <param name="sql">Some SQL you want to run (instead of a specific table)</param>
        /// <param name="outputName">The name of the csv file you would like to create in the outputDirectory.  Do not include.csv in your string it will be put on automatically</param>
        /// <param name="outputDirectory"></param>
        /// <param name="separator"></param>
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


            OutputFilename = _outputDirectory.FullName + "\\" +
                                    tableName.Replace("[", "").Replace("]", "").ToLower().Trim() +
                                    ".csv";

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

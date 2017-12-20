using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using DataExportLibrary.Data.DataTables;

namespace DataExportLibrary.ExtractionTime.FileOutputFormats
{
    public class CSVOutputFormat : FileOutputFormat
    {
        public string Separator { get; set; }
        public string DateFormat { get; set; }
        public bool IncludeValidation { get; private set; }


        public int SeparatorsStrippedOut { get; private set; }
        private static readonly string[] ThingsToStripOut = { "\r", "\n", "\t","\""};
        private StreamWriter _sw;
        private StringBuilder _sbWriteOutLinesBuffer;
        private const string _illegalCharactersReplacement = " ";

        public CSVOutputFormat(string outputFilename,string separator, string dateFormat, bool includeValidation): base(outputFilename)
        {
            Separator = separator;
            DateFormat = dateFormat;
            IncludeValidation = includeValidation;
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
            _sw.Flush();
            _sw.Close();
            _sw.Dispose();
        }

        public string CleanString(object o)
        {
            int numberOfSeparatorsStrippedOutThisPass = 0;

            string toReturn = CleanString(o, Separator, out numberOfSeparatorsStrippedOutThisPass, DateFormat);

            SeparatorsStrippedOut += numberOfSeparatorsStrippedOutThisPass;

            return toReturn;
        }


        public static string CleanString(object o, string separator, out int separatorsStrippedOut, string dateFormat)
        {
            if (o is DateTime)
            {
                DateTime dt = (DateTime)o;
                separatorsStrippedOut = 0;
                return dt.ToString(dateFormat);
            }

            //in order to kep a count 
            Regex regexReplace = new Regex(Regex.Escape(separator));

            separatorsStrippedOut = 0;

            while (o.ToString().Contains(separator))
            {
                o = regexReplace.Replace(o.ToString(), _illegalCharactersReplacement, 1);
                separatorsStrippedOut++;
            }

            if (o is string)
                foreach (string cToStripOut in ThingsToStripOut)
                    o = o.ToString().Replace(cToStripOut, _illegalCharactersReplacement);

            return o.ToString().Trim();
        }

    }
}

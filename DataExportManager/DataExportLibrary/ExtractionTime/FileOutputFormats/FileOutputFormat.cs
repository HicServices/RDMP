using System.Data;
using System.Data.SqlClient;

namespace DataExportLibrary.ExtractionTime.FileOutputFormats
{
    public abstract class FileOutputFormat:IFileOutputFormat
    {
        public abstract string GetFileExtension();

        public string OutputFilename { get; private set; }
        public abstract void Open();
        public abstract void WriteHeaders(DataTable t);
        public abstract void Append(DataRow r);
        public abstract void Flush();
        public abstract void Close();

        protected FileOutputFormat(string outputFilename)
        {
            OutputFilename = outputFilename;
        }
    }
}

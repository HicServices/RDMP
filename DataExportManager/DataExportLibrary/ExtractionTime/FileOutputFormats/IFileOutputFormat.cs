using System.Data;
using System.Data.SqlClient;

namespace DataExportLibrary.ExtractionTime.FileOutputFormats
{
    interface IFileOutputFormat
    {

        string GetFileExtension();
        string OutputFilename { get; }
        void Open();
        void WriteHeaders(DataTable t);
        void Append(DataRow r);
        void Flush();
        void Close();
    }
}

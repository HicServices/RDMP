using System.IO;
using System.Security.Cryptography.X509Certificates;
using DataExportLibrary.Interfaces.Data.DataTables;

namespace DataExportLibrary.Interfaces.ExtractionTime.Commands
{
    public interface IExtractCommand
    {
        DirectoryInfo GetExtractionDirectory();
        IExtractionConfiguration Configuration { get; set; }
        string DescribeExtractionImplementation();

        ExtractCommandState State { get; set; }
        string Name { get; }
    }
}
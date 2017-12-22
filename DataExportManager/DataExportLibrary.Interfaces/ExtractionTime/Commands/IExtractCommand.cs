using System.IO;
using System.Security.Cryptography.X509Certificates;
using DataExportLibrary.Interfaces.Data.DataTables;

namespace DataExportLibrary.Interfaces.ExtractionTime.Commands
{
    /// <summary>
    /// Input object to Extraction Pipelines.  Typically this is a dataset that needs to be linked with a cohort and extracted into the ExtractionDirectory. 
    /// Also includes the ongoing ExtractCommandState that the IExtractCommand is in in the Pipeline e.g. WaitingForSQLServer etc.
    /// </summary>
    public interface IExtractCommand
    {
        DirectoryInfo GetExtractionDirectory();
        IExtractionConfiguration Configuration { get; set; }
        string DescribeExtractionImplementation();

        ExtractCommandState State { get; set; }
        string Name { get; }
    }
}
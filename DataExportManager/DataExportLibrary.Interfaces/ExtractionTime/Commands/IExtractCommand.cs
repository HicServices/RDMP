using System.IO;
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
        IExtractionConfiguration Configuration { get; }
        string DescribeExtractionImplementation();

        ExtractCommandState State { get; }
        void ElevateState(ExtractCommandState newState);
    }
}
using System.IO;
using DataExportLibrary.Interfaces.Data.DataTables;

namespace DataExportLibrary.Interfaces.ExtractionTime
{
    /// <summary>
    /// See ExtractionDirectory
    /// </summary>
    public interface IExtractionDirectory
    {
        DirectoryInfo GetDirectoryForDataset(IExtractableDataSet dataset);
        DirectoryInfo GetGlobalsDirectory();
        DirectoryInfo GetDirectoryForCohortCustomData();
    }
}